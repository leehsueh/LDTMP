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
	private float[] anglesOfLights;	// calculated based on the positions of the lights
	
	// state-related variables and methods
	bool isSkeletonPresent() {
		return skeletonRoot.GetEnrolledPlayer().PlayerID != 0;
	}
	
	bool isSkeletonCalibrated() {
		return skeletonRoot.isCalibrated();	
	}
	
	// whichHand should be "left" or "right"
	int optionHandIsPointedTo(string whichHand) {
		int optionNumber = -1;
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		if (skeletonRoot.GetSkeletonDataForPlayer(out skeletonData)) {
			Vector3 Hand, Shoulder;
			if (whichHand.Equals("left")) {
				Hand = skeletonRoot.leftHand.transform.position;
				Shoulder = skeletonRoot.leftShoulder.transform.position;
			} else if (whichHand.Equals("right")) {
				Hand = skeletonRoot.rightHand.transform.position;
				Shoulder = skeletonRoot.rightShoulder.transform.position;
			} else {
				Debug.LogError("Invalid argument: " + whichHand + " in optionHandIsPointedTo");
				return optionNumber;
			}
			if (Hand.y > Shoulder.y) {
				float angleOfHand = angleFromPlayerPosition(
					Hand
				);
				float delta = 10f;
				float[] diffs = new float[anglesOfLights.Length];
				for (int i = 0; i < anglesOfLights.Length; i++) {
					diffs[i] = Mathf.Abs(angleOfHand - anglesOfLights[i]);
				}
				int minIndex = 0;
				float minDiff = diffs[0];
				for (int i = 0; i < diffs.Length; i++) {
					if (diffs[i] < minDiff) {
						minIndex = i;
						minDiff = diffs[i];
					}
				}
				optionNumber = minIndex;
			}
		}
		
		return optionNumber;
	}
	
	/**
	 * the angle measured from positive x axis of the option text
	 * corresponding to the given optionNumber
	 */
	float angleOfMenuOption(int optionNumber) {
		Vector3 optionPosition = lights[optionNumber].transform.position;
		return angleFromPlayerPosition(optionPosition);
	}
	
	float angleFromPlayerPosition(Vector3 otherPosition) {
		Vector3 playerPosition = skeletonRoot.gameObject.transform.position;
		
		float deltaX = otherPosition.x - playerPosition.x;
		float deltaY = otherPosition.y - playerPosition.y;
		float theta = Mathf.Atan(deltaY/deltaX);
		float thetaDeg = theta * 180/Mathf.PI;
		return thetaDeg;
	}
	
	private float raiseTimeDuration = 3f;	// 3 seconds to make a selection
	private float raiseTime;
	private float selectionConfirmTimeDuration = 1f;	// 1 second to show a message saying what was selected
	private float selectionConfirmTime;
	private int optionHighlighted = -1;	// 0 for first option, 1 for second option, etc
	private bool rightOptionHighlighted;
	private bool leftOptionHighlighted;
	
	enum MenuState {
		WaitForPresence,
		WaitForCalibration,
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
		optionHighlighted = -1;
		rightOptionHighlighted = false;
		leftOptionHighlighted = false;
		CustomPlayerManager mPlayerManager = (CustomPlayerManager)FindObjectOfType(typeof(CustomPlayerManager));
		//mPlayerManager.MaxPlayers = 2;
		anglesOfLights = new float[lights.Length];
		foreach (GameObject light in lights) {
			light.active = false;
		}
	}
	
	void Update() {
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			if (isSkeletonPresent()) NextState = MenuState.WaitForCalibration;
			else NextState = MenuState.WaitForPresence;
			break;
		case MenuState.WaitForCalibration:
			if (isSkeletonPresent()) {
				if (isSkeletonCalibrated()) {
					NextState = MenuState.WaitForHandRaise;	
				} else {
					NextState = MenuState.WaitForCalibration;	
				}
			} else {
				NextState = MenuState.WaitForPresence;	
			}
			break;
		case MenuState.WaitForHandRaise:
			optionHighlighted = -1;
			rightOptionHighlighted = false;
			leftOptionHighlighted = false;
			if (isSkeletonPresent()) {
				if (optionHandIsPointedTo("left") >= 0) {
					optionHighlighted = optionHandIsPointedTo("left");
					NextState = MenuState.LeftHandRaised;
					raiseTime = Time.time;
				} else if (optionHandIsPointedTo("right") >= 0) {
					optionHighlighted = optionHandIsPointedTo("right");
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
				if (optionHighlighted == optionHandIsPointedTo("left")) {
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
				if (optionHighlighted == optionHandIsPointedTo("right")) {
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
			FallingFacesInfoScript info = (FallingFacesInfoScript)FindObjectOfType(typeof(FallingFacesInfoScript));
			info.LevelSelected = optionHighlighted;
			if (optionHighlighted >= 0 && optionHighlighted < 3) {
				Application.LoadLevel(1);
			} else if (optionHighlighted == 3) {
				Application.LoadLevel(2);
			} else {
				NextState = MenuState.WaitForPresence;	
			}
			break;
		}
		CurrentState = NextState;
	}
	
	void OnGUI() {
		for (int i=0; i < sizes.Length; i++) {
			if (anglesOfLights[i] == 0) {
				anglesOfLights[i] = angleOfMenuOption(i);
			}
			Vector3 position = mainCamera.WorldToScreenPoint(lights[i].transform.position);
			float width = sizes[i].x;
			float height = sizes[i].y;
			GUI.Box(new Rect(position.x - width/2, Screen.height - (position.y + height), width, height), itemTexts[i], itemStyle);
		}
		
		
		string statusMessage = "";
		// show status message based on the state
		switch (CurrentState) {
		case MenuState.WaitForPresence:
			statusMessage = "Make the Pose Below";
			break;
		case MenuState.WaitForCalibration:
			statusMessage = "Hold still...";
			break;
		case MenuState.WaitForHandRaise:
			break;
		case MenuState.LeftHandRaised:
			statusMessage = "Hold for " + (int)(raiseTimeDuration - (Time.time - raiseTime));
			break;
		case MenuState.RightHandRaised:
			statusMessage = "Hold for " + (int)(raiseTimeDuration - (Time.time - raiseTime));
			break;
		case MenuState.OptionSelected:
			statusMessage = "Selection Made!";
			lights[optionHighlighted].light.color = Color.red;
			break;
		}
		
		for (int i = 0; i < lights.Length; i++) {
			if (i == optionHighlighted) lights[i].active = true;
			else lights[i].active = false;
		}
		int labelWidth = 500;
		int labelHeight = 100;
		GUIStyle style = new GUIStyle();
		style.fontSize = 28;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.black;
		GUI.Label(new Rect(Screen.width/2 - labelWidth/2, 150, labelWidth, labelHeight), statusMessage, style);
	}
}
