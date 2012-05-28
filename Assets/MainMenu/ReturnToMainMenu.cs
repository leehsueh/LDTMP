using UnityEngine;
using System.Collections;

public class ReturnToMainMenu : MonoBehaviour {
	public GUIStyle style;
	private CalibratedNodeRoot skeletonRoot;
	
	// state-related variables and methods
	bool isSkeletonPresent() {
		return skeletonRoot.GetEnrolledPlayer().PlayerID != 0;
	}
	
	// whichHand should be "left" or "right"
	bool isHandRaised(string whichHand) {
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		if (skeletonRoot.GetSkeletonDataForPlayer(out skeletonData)) {
			KinectInterface.Vector4 HeadPos = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HEAD];
			KinectInterface.Vector4 RightHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT];
			KinectInterface.Vector4 LeftHand = skeletonData.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_LEFT];
			
			if (whichHand.Equals("left")) {
				return LeftHand.Y > HeadPos.Y;
			} else if (whichHand.Equals("right")) {
				return RightHand.Y > HeadPos.Y;
			} else {
				return false;
			}
		} else {
			return false;
		}
		
	}
	
	private float raiseTimeDuration = 3f;	// 3 seconds to make a selection
	private float raiseTime;
	private float selectionConfirmTimeDuration = 1f;	// 1 second to show a message saying what was selected
	private float selectionConfirmTime;
	
	enum MenuState {
		WaitForPresence,
		WaitForPose,
		PoseHeld,
		ReturnRecognized,
		PerformSceneChange
	};
	
	MenuState CurrentState;
	MenuState NextState;
	
	void Start() {
		GameObject player = GameObject.Find("Player");
		if (player != null) {
			skeletonRoot = player.GetComponent<CalibratedNodeRoot>();	
		}
		CurrentState = MenuState.WaitForPresence;
	}
	
	void Update() {
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			if (isSkeletonPresent()) NextState = MenuState.WaitForPose;
			else NextState = MenuState.WaitForPresence;
			break;
		case MenuState.WaitForPose:
			if (isSkeletonPresent()) {
				if (isHandRaised("left") && isHandRaised("right")) {
					NextState = MenuState.PoseHeld;
					raiseTime = Time.time;
				} else {
					NextState = MenuState.WaitForPose;
				}
			} else {
				NextState = MenuState.WaitForPresence;
			}
			break;
		case MenuState.PoseHeld:
			if (isSkeletonPresent()) {
				if (isHandRaised("left") && isHandRaised("right")) {
					float curTime = Time.time;
					if (curTime - raiseTime > raiseTimeDuration) {
						NextState = MenuState.ReturnRecognized;
						selectionConfirmTime = Time.time;
					} else {
						NextState = MenuState.PoseHeld;	
					}
				} else {
					NextState = MenuState.WaitForPose;	
				}
			} else {
				NextState = MenuState.WaitForPresence;
			}
			break;
		case MenuState.ReturnRecognized:
			float curTime = Time.time;
			if (curTime - selectionConfirmTime > selectionConfirmTimeDuration) {
				NextState = MenuState.PerformSceneChange;	
			} else {
				NextState = MenuState.ReturnRecognized;	
			}
			break;
		case MenuState.PerformSceneChange:
			Application.LoadLevel(0);
			break;
		}
		CurrentState = NextState;
	}
	
	void OnGUI() {
		string statusMessage = "Raise Both Hands to Go Back";
		// show status message based on the state
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			//statusMessage = "We can't find you!";
			break;
		case MenuState.WaitForPose:
			//statusMessage = "Raise hand to select.";
			break;
		case MenuState.PoseHeld:
			//statusMessage = "Keep Holding!";
			break;
		case MenuState.ReturnRecognized:
			statusMessage = "Returning to Main Menu...";
			break;
		}

		GUI.Box (new Rect (20,50,250,30), statusMessage, style);
	}
}
