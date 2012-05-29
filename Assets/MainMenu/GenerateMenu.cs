using UnityEngine;
using System.Collections;

public class GenerateMenu : MonoBehaviour {
	private CalibratedNodeRoot skeletonRoot;
	public Camera mainCamera;
	public GUIStyle itemStyle;
	public GUIStyle itemStyleHighlighted;
	public string[] itemTexts;
	public Vector2[] sizes;
	public GameObject[] lights;
	
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
		foreach (GameObject light in lights) {
			light.active = false;	
		}
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
//		GUI.Box (new Rect (100,100,200,50), "Falling Faces");
//		GUI.Box (new Rect (Screen.width - 200-100,100,200,50), "Hello Park");
		for (int i=0; i < sizes.Length; i++) {
			Vector3 position = mainCamera.WorldToScreenPoint(lights[i].transform.position);
			float width = sizes[i].x;
			float height = sizes[i].y;
			GUI.Box(new Rect(position.x - width/2, Screen.height - (position.y + height), width, height), itemTexts[i], itemStyle);
			print (itemTexts[i]);
		}
		
		
		string statusMessage = "";
		// show status message based on the state
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			statusMessage = "We can't find you!";
			break;
		case MenuState.WaitForHandRaise:
			break;
		case MenuState.LeftHandRaised:
			statusMessage = "Keep Holding!";
			break;
		case MenuState.RightHandRaised:
			statusMessage = "Keep Holding";
			break;
		case MenuState.OptionSelected:
			statusMessage = "Selection Made!";
			if (rightOptionHighlighted) {
				lights[1].light.color = Color.red;	
			} else if (leftOptionHighlighted) {
				lights[0].light.color = Color.red;	
			}
			break;
		}
		
		// update the lights
		if (rightOptionHighlighted) {
			lights[1].active = true;	
		} else {
			lights[1].active = false;	
		}
		if (leftOptionHighlighted) {
			lights[0].active = true;	
		} else {
			lights[0].active = false;	
		}
		int labelWidth = 500;
		int labelHeight = 100;
		GUIStyle style = new GUIStyle();
		style.fontSize = 24;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.black;
		GUI.Label(new Rect(Screen.width/2 - labelWidth/2, 150, labelWidth, labelHeight), statusMessage, style);
	}
}
