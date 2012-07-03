using UnityEngine;
using System.Collections;

public class FallingObjectGameManager : MonoBehaviour {
	
	#region game components
	public ChoiceFallingObjectSpawner objectSpawner;
	public BoxFlash flashBox;
	public LabelFlash flashLabel;
	public CalibratedNodeRoot skeletonRoot;
	public Texture2D placeHolderTexture;
	public string targetTag;
	public Texture2D facePicture;
	#endregion
	
	#region state machine parameters
	public float promptDuration;
	public float gameOverMessageDuration;
	public float restartCountdownDuration;
	public int goalNumberCorrect;
	public int maxNumberIncorrect;
	public bool showStates;
	#endregion
	
	#region state machine variables
	private int numCorrect;
	private int numIncorrect;
	private float promptTime;
	private float gameOverMessageTime;
	private float restartCountdownTime;
	private GameObject[] currentFallingObjects;
	private GameObject collidedObject;
	private bool collisionDetected;
	public GameObject CollidedObject {
		set {
			collidedObject = value;
			collisionDetected = true;
		}	
	}
	
	#endregion
	#region state machine checks
	bool promptDone() {
		return Time.time - promptTime >= promptDuration;
	}
	bool gameOverMessageDone() {
		return Time.time - gameOverMessageTime >= gameOverMessageDuration;	
	}
	bool restartCountdownDone() {
		return Time.time - restartCountdownTime >= restartCountdownDuration;	
	}
	bool skeletonPresent() {
		return skeletonRoot.GetEnrolledPlayer().PlayerID != 0;
	}
	bool skeletonCalibrated() {
		return skeletonRoot.isCalibrated();	
	}
	bool objectsStillOnScreen() {
		return currentFallingObjects[0] != null || currentFallingObjects[1] != null;
	}
	bool collisionObjectIsTarget() {
		return collidedObject != null ? collidedObject.tag.Equals(targetTag) : false;
	}
	bool goalNumberMet() {
		return numCorrect >= goalNumberCorrect;	
	}
	bool maxIncorrectMet() {
		return numIncorrect >= maxNumberIncorrect;	
	}
	#endregion
	
	#region state machine actions
	void startPromptTimer() {
		promptTime = Time.time;
		flashBox.Message = "Burst the Faces!";
		flashBox.Image = facePicture;
		flashBox.Duration = promptDuration;
		flashBox.Center = new Vector2(Screen.width/2, 200);
		flashBox.WidthHeight = new Vector2(400, 300);
		flashBox.show();
	}
	void startGameOverMessageTimer() {
		gameOverMessageTime = Time.time;	
	}
	void startRestartCountdownTimer() {
		restartCountdownTime = Time.time;	
	}
	void incrementCorrect() {
		numCorrect++;	
	}
	void incrementIncorrect() {
		numIncorrect++;
	}
	void dismissProblem() {
		foreach (GameObject obj in currentFallingObjects) {
			if (obj != null) obj.rigidbody.drag = objectSpawner.lowDrag;
		}
		print ("dismissing problem");
	}
	void flashMessage(string message, float duration) {
		flashLabel.updateText(message);
		flashLabel.show(duration);
	}
	void resetState() {
		numCorrect = 0;
		numIncorrect = 0;
		collisionDetected = false;
	}
	#endregion
	
	#region state machine states
	enum GameState {
		WaitForPresence,
		WaitForCalibration,
		ShowPrompt,
		GameStart,
		SpawnFallingObjects,
		WaitForChoice,
		ChoicePass,
		ChoiceMade,
		CheckChoice,
		WaitForObjectsDisappear,
		GameOver,
		RestartCounter
	}
	
	GameState CurrentState;
	GameState NextState;
	#endregion

	// Use this for initialization
	void Start () {
		CurrentState = GameState.WaitForPresence;
		skeletonRoot.ConstrainZMovement = true;
		resetState();
		FallingFacesInfoScript info = (FallingFacesInfoScript)FindObjectOfType(typeof(FallingFacesInfoScript));
		Debug.Log("Level: " + info.LevelSelected);
	}
	
	// Update is called once per frame
	void Update () {
		switch (CurrentState) {
		case GameState.WaitForPresence:
			if (skeletonPresent()) {
				NextState = GameState.WaitForCalibration;
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.WaitForCalibration:
			if (skeletonPresent()) {
				if (skeletonCalibrated()) {
					NextState = GameState.ShowPrompt;
					startPromptTimer();
				} else {
					NextState = GameState.WaitForCalibration;
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.ShowPrompt:
			if (skeletonPresent()) {
				if (promptDone()) {
					NextState = GameState.GameStart;
				} else {
					NextState = GameState.ShowPrompt;
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.GameStart:
			flashMessage("Go!", 3f);
			NextState = GameState.SpawnFallingObjects;
			break;
		case GameState.SpawnFallingObjects:
			currentFallingObjects = objectSpawner.spawnTwoProblem();
			NextState = GameState.WaitForChoice;
			break;
		case GameState.WaitForChoice:
			if (skeletonPresent()) {
				if (collisionDetected) {
					NextState = GameState.ChoiceMade;
					collisionDetected = false;
				} else {
					if (objectsStillOnScreen()) {
						NextState = GameState.WaitForChoice;
					} else {
						NextState = GameState.ChoicePass;
					}
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.ChoicePass:
			incrementIncorrect();
			NextState = GameState.CheckChoice;
			break;
		case GameState.ChoiceMade:
			if (collisionObjectIsTarget()) {
				collidedObject.GetComponent<FallingObjectKinectController>().explode();
				incrementCorrect();
			} else {
				incrementIncorrect();
			}
			flashMessage(numCorrect + " faces, " + numIncorrect + " misses!", 2f);
			dismissProblem();
			NextState = GameState.CheckChoice;
			break;
		case GameState.CheckChoice:
			if (goalNumberMet() || maxIncorrectMet()) {
				NextState = GameState.GameOver;
				startGameOverMessageTimer();
				if (goalNumberMet()) flashMessage("Great! You burst " + goalNumberCorrect + " faces!", gameOverMessageDuration);
				else flashMessage("Sorry, you missed " + maxNumberIncorrect + " times.", gameOverMessageDuration);
			} else {
				NextState = GameState.WaitForObjectsDisappear;	
			}
			break;
		case GameState.WaitForObjectsDisappear:
			if (objectsStillOnScreen()) {
				NextState = GameState.WaitForObjectsDisappear;
			} else {
				NextState = GameState.SpawnFallingObjects;
			}
			break;
		case GameState.GameOver:
			if (gameOverMessageDone()) {
				NextState = GameState.GameOver;
			} else {
				NextState = GameState.RestartCounter;
				startRestartCountdownTimer();
			}
			break;
		case GameState.RestartCounter:
			if (restartCountdownDone()) {
				NextState = GameState.ShowPrompt;
				resetState();
				startPromptTimer();
			} else {
				NextState = GameState.RestartCounter;
			}
			break;
		}
		CurrentState = NextState;
	}
	
	// mostly for debugging purposes to see what state the game is in
	void OnGUI() {
		string statusMessage;
		if (showStates) statusMessage = CurrentState.ToString();
		else statusMessage = "";
		
		switch (CurrentState) {
		case GameState.WaitForPresence:
			statusMessage = "Make the Pose Below";
			break;
		case GameState.WaitForCalibration:
			statusMessage = "Hold Still...";
			break;
		case GameState.ShowPrompt:
			break;
		case GameState.GameStart:
			
			break;
		case GameState.SpawnFallingObjects:
			
			break;
		case GameState.WaitForChoice:
			
			break;
		case GameState.ChoicePass:
			break;
		case GameState.ChoiceMade:
			break;
		case GameState.CheckChoice:
			
			break;
		case GameState.WaitForObjectsDisappear:
			break;
		case GameState.GameOver:
			break;
		case GameState.RestartCounter:
			statusMessage = "Restarting in " + (int)(restartCountdownDuration - Time.time + restartCountdownTime);
			break;
		}
		
		int labelWidth = 500;
		int labelHeight = 100;
		GUIStyle style = new GUIStyle();
		style.fontSize = 36;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.black;
		GUI.Box(new Rect(Screen.width/2 - labelWidth/2, 80, labelWidth, labelHeight), statusMessage, style);
		
		GUIStyle scoreStyle = new GUIStyle();
		scoreStyle.alignment = TextAnchor.MiddleLeft;
		scoreStyle.fontSize = 24;
//		GUI.Label(new Rect(20, Screen.height/2 - 50, 150, 50), numCorrect + " faces", scoreStyle);
//		GUI.Label(new Rect(20, Screen.height/2, 150, 50), numIncorrect + " misses", scoreStyle);
	}
}
