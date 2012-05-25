using UnityEngine;
using System.Collections;

public class GenerateMenu : MonoBehaviour {
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
	private bool rightOptionHighlighted;
	private bool leftOptionHighlighted;
	
	enum MenuState {
		WaitForPresence,
		WaitForHandRaise,
		RightHandRaised,
		LeftHandRaised,
		OptionSelected,
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
		rightOptionHighlighted = false;
		leftOptionHighlighted = false;
		CustomPlayerManager mPlayerManager = (CustomPlayerManager)FindObjectOfType(typeof(CustomPlayerManager));
		//mPlayerManager.MaxPlayers = 2;
	}
	
	void Update() {
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			if (isSkeletonPresent()) NextState = MenuState.WaitForHandRaise;
			else NextState = MenuState.WaitForPresence;
			break;
		case MenuState.WaitForHandRaise:
			rightOptionHighlighted = false;
			leftOptionHighlighted = false;
			if (isSkeletonPresent()) {
				if (isHandRaised("left")) {
					NextState = MenuState.LeftHandRaised;
					raiseTime = Time.time;
				} else if (isHandRaised("right")) {
					NextState = MenuState.RightHandRaised;
					raiseTime = Time.time;
				} else {
					NextState = MenuState.WaitForHandRaise;
				}
			} else {
				NextState = MenuState.WaitForPresence;
			}
			break;
		case MenuState.LeftHandRaised:
			if (isSkeletonPresent()) {
				if (isHandRaised("left")) {
					leftOptionHighlighted = true;
					rightOptionHighlighted = false;
					float curTime = Time.time;
					if (curTime - raiseTime > raiseTimeDuration) {
						NextState = MenuState.OptionSelected;
						selectionConfirmTime = Time.time;
					} else {
						NextState = MenuState.LeftHandRaised;	
					}
				} else {
					NextState = MenuState.WaitForHandRaise;	
				}
			} else {
				NextState = MenuState.WaitForPresence;
			}
			break;
		case MenuState.RightHandRaised:
			if (isSkeletonPresent()) {
				if (isHandRaised("right")) {
					rightOptionHighlighted = true;
					leftOptionHighlighted = false;
					float curTime = Time.time;
					if (curTime - raiseTime > raiseTimeDuration) {
						NextState = MenuState.OptionSelected;
						selectionConfirmTime = Time.time;
					} else {
						NextState = MenuState.RightHandRaised;	
					}
				} else {
					NextState = MenuState.WaitForHandRaise;	
				}
			} else {
				NextState = MenuState.WaitForPresence;
			}
			break;
		case MenuState.OptionSelected:
			float curTime = Time.time;
			if (curTime - selectionConfirmTime > selectionConfirmTimeDuration) {
				NextState = MenuState.PerformSceneChange;	
			} else {
				NextState = MenuState.OptionSelected;	
			}
			break;
		case MenuState.PerformSceneChange:
			if (rightOptionHighlighted) {
				Application.LoadLevel(2);
			} else if (leftOptionHighlighted) {
				Application.LoadLevel(1);
			} else {
				NextState = MenuState.WaitForPresence;	
			}
			break;
		}
		CurrentState = NextState;
	}
	
	void OnGUI() {
		GUI.Box (new Rect (100,100,200,50), "Raise Left Hand for the Waving Game");
		GUI.Box (new Rect (Screen.width - 200-100,100,200,50), "Raise Right Hand for the Falling Object Game");
		
		string statusMessage = "<nothing>";
		// show status message based on the state
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			statusMessage = "We can't find you!";
			break;
		case MenuState.WaitForHandRaise:
			statusMessage = "Raise hand to select.";
			break;
		case MenuState.LeftHandRaised:
			statusMessage = "Keep Holding!";
			break;
		case MenuState.RightHandRaised:
			statusMessage = "Keep Holding";
			break;
		case MenuState.OptionSelected:
			statusMessage = "Selection Made!";
			break;
		}
		int labelWidth = 500;
		int labelHeight = 100;
		GUIStyle style = new GUIStyle();
		style.fontSize = 24;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.black;
		GUI.Label(new Rect(Screen.width/2 - labelWidth/2, 300, labelWidth, labelHeight), statusMessage, style);
	}
}
