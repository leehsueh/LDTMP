using UnityEngine;
using System.Collections;

// should be placed on the game object that has CalibratedNodeRoot
public class HandWaveGesture : MonoBehaviour {
	enum GestureState {
		WaitForPresence,
		WaitForHandRaise,
		HandRaised,
		HandRaisedOut,
		HandRaisedIn,
		HandWaveRecognized,
		HandBackDown
	}
	
	protected CalibratedNodeRoot skeletonRoot;
	private GestureState CurrentState;
	private GestureState NextState;
	
	// state variables and checks
	enum Hand {
		None, Left, Right	
	}
	
	private Hand currentHandRaised = Hand.None;
	
	public int NumWavesNeeded = 3;
	private int numWavesCounted = 0;
	public float RestTimeNeeded = 1f;	// 1 second between full waves
	private float restTime;
	
	bool isSkeletonPresent() {
		return skeletonRoot.GetEnrolledPlayer().PlayerID != 0;
	}
	
	// whichHand should be "left" or "right"
	bool isHandRaised(Hand whichHand) {
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		if (skeletonRoot.GetSkeletonDataForPlayer(out skeletonData)) {
			KinectInterface.Vector4 LeftShoulder = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_SHOULDER_LEFT];
			KinectInterface.Vector4 RightShoulder = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_SHOULDER_RIGHT];
			KinectInterface.Vector4 RightHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT];
			KinectInterface.Vector4 LeftHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_LEFT];
			
			if (whichHand == Hand.Left) {
				return LeftHand.Y > LeftShoulder.Y;
			} else if (whichHand == Hand.Right) {
				return RightHand.Y > RightShoulder.Y;
			} else {
				return false;
			}
		} else {
			return false;
		}
	}
	
	// return -1 if hands not raised, 0 if hands raised in, 1 if hands raised out
	int handRaisedOutOrIn(Hand whichHand) {		
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		if (skeletonRoot.GetSkeletonDataForPlayer(out skeletonData) && isHandRaised(whichHand)) {
			KinectInterface.Vector4 LeftElbow = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_ELBOW_LEFT];
			KinectInterface.Vector4 RightElbow = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_ELBOW_RIGHT];
			KinectInterface.Vector4 RightHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT];
			KinectInterface.Vector4 LeftHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_LEFT];
			
			if (whichHand == Hand.Left) {
				return LeftHand.X >= LeftElbow.X ? 0 : 1;
			} else if (whichHand == Hand.Right) {
				return RightHand.X <= RightElbow.X ? 0 : 1;
			} else {
				return -1;
			}
		} else {
			return -1;
		}
	}
	
	public virtual void triggerAction() {
		print ("Hand wave recognized!");
	}
	
	// Use this for initialization
	protected void Start () {
		skeletonRoot = (CalibratedNodeRoot)gameObject.GetComponent<CalibratedNodeRoot>();
		CurrentState = GestureState.WaitForPresence;
		currentHandRaised = Hand.None;
		numWavesCounted = 0;
	}
	
	// Update is called once per frame
	protected void Update () {
		switch (CurrentState) {
		case GestureState.WaitForPresence:
			if (!isSkeletonPresent()) NextState = GestureState.WaitForPresence;
			else NextState = GestureState.WaitForHandRaise;
			break;
		case GestureState.WaitForHandRaise:
			if (isSkeletonPresent()) {
				// check right hand first, arbitrarily
				if (isHandRaised(Hand.Right)) {
					currentHandRaised = Hand.Right;
				} else if (isHandRaised(Hand.Left)) {
					currentHandRaised = Hand.Left;
				} else {
					currentHandRaised = Hand.None;
				}
				if (currentHandRaised != Hand.None) {	
					NextState = GestureState.HandRaised;
					numWavesCounted = 0;
				} else {
					NextState = GestureState.WaitForHandRaise;	
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		case GestureState.HandRaised:
			if (isSkeletonPresent()) {
				if (isHandRaised(currentHandRaised)) {
					int inOrOut = handRaisedOutOrIn(currentHandRaised);
					if (inOrOut == 0) {	// raised in
						NextState = GestureState.HandRaisedIn;
					} else if (inOrOut == 1) {	// raised out
						NextState = GestureState.HandRaisedOut;	
					}
				} else {
					NextState = GestureState.WaitForHandRaise;	
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		case GestureState.HandRaisedOut:
			if (isSkeletonPresent()) {
				int inOrOut = handRaisedOutOrIn(currentHandRaised);
				if (inOrOut >= 0 && numWavesCounted >= NumWavesNeeded) {
					NextState = GestureState.HandWaveRecognized;
					triggerAction();
				} else if (inOrOut == 0) {	// raised in
					NextState = GestureState.HandRaisedIn;
					numWavesCounted++;
					//print (numWavesCounted);
				} else if (inOrOut == 1) {	// raised out
					NextState = GestureState.HandRaisedOut;	
				} else {
					NextState = GestureState.WaitForHandRaise;	
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		case GestureState.HandRaisedIn:
			if (isSkeletonPresent()) {
				int inOrOut = handRaisedOutOrIn(currentHandRaised);
				if (inOrOut >= 0 && numWavesCounted >= NumWavesNeeded) {
					NextState = GestureState.HandWaveRecognized;
					triggerAction();
				} else if (inOrOut == 0) {	// raised in
					NextState = GestureState.HandRaisedIn;
				} else if (inOrOut == 1) {	// raised out
					NextState = GestureState.HandRaisedOut;	
					numWavesCounted++;
					//print (numWavesCounted);
				} else {
					NextState = GestureState.WaitForHandRaise;	
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		case GestureState.HandWaveRecognized:
			if (isSkeletonPresent()) {
				if (isHandRaised(currentHandRaised)) {
					NextState = GestureState.HandWaveRecognized;	
				} else {
					NextState = GestureState.HandBackDown;	
					restTime = Time.time;
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		case GestureState.HandBackDown:
			numWavesCounted = 0;
			if (isSkeletonPresent()) {
				float time = Time.time;
				if (time - restTime >= RestTimeNeeded) {
					NextState = GestureState.WaitForHandRaise;
				} else {
					NextState = GestureState.HandBackDown;	
				}
			} else {
				NextState = GestureState.WaitForPresence;
			}
			break;
		default:
			break;
		}
		
		CurrentState = NextState;
	}
	
	void OnGUI() {
		if (CurrentState == GestureState.HandWaveRecognized || CurrentState == GestureState.HandBackDown) {
			GUI.Box(new Rect(Screen.width - 100,100, 100, 50),"Wave recognized!");	
		}
	}
}
