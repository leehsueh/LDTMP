using UnityEngine;
using System.Collections;

// implements the game logic state machine and facilitates communication between all game elements
public class WavingGameManager : MotionSandboxManager {
	// Game components
	#region game components
	public WalkingCharacterSpawner characterSpawner;
	public CalibratedNodeRoot skeletonRoot;
	public LabelFlash flashLabel;
	public BoxFlash flashBox;
	public BoxFlash flashBoxStatus;
	private float probabilityOfTarget = 0.3f;
	public float defaultSpeed;
	public float fasterSpeed;
	#endregion
	
	#region State machine parameters
	public float promptDuration;
	public float victoryMessageDuration;
	public float restartCountdownDuration;
	public float minDistanceFromTarget;
	public float pointOfPassingZ;
	public bool showStates;
	#endregion
	
	#region game state variables
	WavingGameCharacterController mMainWalker;
	WavingGameCharacterController mTargetWalker;
	public WavingGameCharacterController MainWalker {
		get { return mMainWalker; }
		set { mMainWalker = value; }
	}
	public WavingGameCharacterController TargetWalker {
		get { return mTargetWalker; }
		set { mTargetWalker = value; }
	}
	float promptTime;
	float victoryMessageTime;
	float restartCountdownTime;
	bool handWaveDetected;
	public bool HandWaveDetected {
		get { return handWaveDetected; }
		set { handWaveDetected = value; }
	}
	#endregion
	
	#region utility checking methods
	bool promptDone(float curTime) {
		return curTime - promptTime >= promptDuration;
	}
	bool victoryMessageDone(float curTime) {
		return curTime - victoryMessageTime >= victoryMessageDuration;	
	}
	bool restartCountdownDone(float curTime) {
		return curTime - restartCountdownTime >= restartCountdownDuration;	
	}
	bool skeletonPresent() {
		return skeletonRoot.GetEnrolledPlayer().PlayerID != 0;
	}
	bool skeletonCalibrated() {
		return skeletonRoot.isCalibrated();	
	}
	
	float playerDistanceFromWalker(WavingGameCharacterController walker) {
		return Vector3.Distance(skeletonRoot.gameObject.transform.position, walker.gameObject.transform.position);	
	}
	
	bool mainWalkerAlive() {
		if (mMainWalker) {
			return mMainWalker.gameObject.transform.position.z > pointOfPassingZ;
		} else {
			return false;
		}
	}
	#endregion
	
	#region state machine actions
	void startPromptTimer() {
		promptTime = Time.time;
		flashBox.Message = "Wave Hello to " + characterSpawner.TargetName;
		flashBox.Image = characterSpawner.TargetPicture;
		flashBox.Duration = promptDuration;
		flashBox.Center = new Vector2(Screen.width/2, 200);
		flashBox.WidthHeight = new Vector2(400, 300);
		flashBox.show();
		
	}
	void startVictoryMessageTimer() {
		victoryMessageTime = Time.time;	
	}
	void startRestartCountdownTimer() {
		restartCountdownTime = Time.time;	
	}
	void selectRandomTarget() {
		int possibilities = characterSpawner.CharacterPrefabs.Length;
	}
	void flashMessage(string message, float duration) {
		//TODO: implement this
//		flashLabel.updateText(message);
//		flashLabel.show(duration);
		flashBoxStatus.gameObject.active = true;
		flashBoxStatus.Message = message;
		flashBoxStatus.Duration = duration;
		flashBoxStatus.show();
	}
	void killExistingGameAndReset() {
		//TODO: implement this
		// kill all the walking characters
		print ("killing the game!");
	}
	
	#endregion
	
	#region State machine stuff
	enum GameState {
		WaitForPresence,
		WaitForCalibration,
		ShowPrompt,
		GameStart,
		Spawning,
		WaitForWave,
		WaveMissed,
		WaveRecognized,
		PerformAnimation,
		ShowVictoryMessage,
		RestartCountdown
	}
	
	GameState CurrentState;
	GameState NextState;
	
	#endregion
	
	// Use this for initialization
	void Start () {
		base.Start();
		if (characterSpawner == null) {
			characterSpawner = (WalkingCharacterSpawner)FindObjectOfType(typeof(WalkingCharacterSpawner));
			//characterSpawner.spawnAtFixedRate = true;
			
		}
		characterSpawner.DefaultSpeed = defaultSpeed;
		characterSpawner.startSpawning();
		if (skeletonRoot == null) {
			skeletonRoot = (CalibratedNodeRoot)FindObjectOfType(typeof(CalibratedNodeRoot));
		}
		
		flashBoxStatus.Image = null;
		flashBoxStatus.Center = new Vector2(Screen.width/2, 130);
		flashBoxStatus.Duration = promptDuration;
		flashBoxStatus.WidthHeight = new Vector2(Screen.width/1.5f, 100); 
		flashBoxStatus.gameObject.active = false;
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
				if (skeletonRoot.isCalibrated()) {
					NextState = GameState.ShowPrompt;
					characterSpawner.selectRandomTarget();
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
				if (promptDone(Time.time)) {
					NextState = GameState.GameStart;
				} else {
					NextState = GameState.ShowPrompt;	
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.GameStart:
			if (skeletonPresent()) {
				mTargetWalker = null;
				mMainWalker = null;
				//characterSpawner.spawnAtFixedRate = true;
				characterSpawner.startSpawning();
				NextState = GameState.Spawning;
			} else {
				NextState = GameState.WaitForPresence;
				characterSpawner.killAll();
			}
			break;
		case GameState.Spawning:
			if (skeletonPresent()) {
				if (Random.Range (0f,1f) < probabilityOfTarget) {
					characterSpawner.spawnTargetCharacter();
				} else {
					characterSpawner.spawnCharacterButNotTarget();
				}
				mMainWalker = characterSpawner.leastRecentlyAddedWalker();
				// mTargetWalker will get set by the spawner when it spawns an instance of the target
				NextState = GameState.WaitForWave;
			} else {
				NextState = GameState.WaitForPresence;
				characterSpawner.killAll();
			}
			break;
		case GameState.WaitForWave:
			if (skeletonPresent()) {
				if (handWaveDetected && mainWalkerAlive()) {
					NextState = GameState.WaveRecognized;
				} else if (!mainWalkerAlive()) {
					if (mMainWalker == mTargetWalker) {
						NextState = GameState.WaveMissed;	
					} else {
						NextState = GameState.Spawning;
					}
					mMainWalker.Speed = 2*fasterSpeed;
					characterSpawner.removeWalker(mMainWalker);
					print ("Spawning from WaitForWave");
				} else {
					NextState = GameState.WaitForWave;	
				}
			} else {
				NextState = GameState.WaitForPresence;	
				characterSpawner.killAll();
			}
			break;
		case GameState.WaveMissed:
			flashMessage("You missed " + characterSpawner.TargetName + "!", 2f);
			characterSpawner.removeWalker(mMainWalker);
			NextState = GameState.Spawning;
			print ("Spawning from WaveMissed");
			break;
		case GameState.WaveRecognized:
			if (skeletonPresent()) {
				handWaveDetected = false;
				if (mMainWalker == mTargetWalker && !mTargetWalker.DidGiveFeedback) {
					//TODO: also check for distance from target!
					mTargetWalker.Speed = 0f;
					mTargetWalker.giveFeedback();
					NextState = GameState.PerformAnimation;
//					flashMessage("You waved to " + characterSpawner.TargetName);
//					print ("you waved to the target!");
				} else {
					mMainWalker.Speed = fasterSpeed;
					NextState = GameState.WaitForWave;
					flashMessage("Oops! That's not " + characterSpawner.TargetName, 2f);
				}
			} else {
				NextState = GameState.WaitForPresence;
				characterSpawner.killAll();
			}
			break;
		case GameState.PerformAnimation:
			if (mTargetWalker.animation.IsPlaying(mTargetWalker.feedbackMotionName)) {
				NextState = GameState.PerformAnimation;	
			} else {
				NextState = GameState.ShowVictoryMessage;
				mTargetWalker.Speed = defaultSpeed;
				flashMessage("Good job! You waved to " + characterSpawner.TargetName, victoryMessageDuration);
				characterSpawner.stopSpawning();
				startVictoryMessageTimer();
			}
			break;
		case GameState.ShowVictoryMessage:
			if (victoryMessageDone(Time.time)) {
				NextState = GameState.RestartCountdown;
				characterSpawner.killAll();
				startRestartCountdownTimer();
			} else {
				NextState = GameState.ShowVictoryMessage;	
			}
			break;
		case GameState.RestartCountdown:
			if (restartCountdownDone(Time.time)) {
				NextState = GameState.ShowPrompt;
				startPromptTimer();
				characterSpawner.selectRandomTarget();
			} else {
				NextState = GameState.RestartCountdown;	
			}
			break;
		}
		CurrentState = NextState;
	}
	
	Texture2D backgroundColor;
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
			//statusMessage = "Wave Hello to " + characterSpawner.TargetName;
			break;
		case GameState.GameStart:
			
			break;
		case GameState.Spawning:
			
			break;
		case GameState.WaitForWave:
			
			break;
		case GameState.WaveMissed:
			break;
		case GameState.WaveRecognized:
			
			break;
		case GameState.PerformAnimation:
			
			break;
		case GameState.ShowVictoryMessage:
			//statusMessage = "You waved to " + characterSpawner.TargetName;
			break;
		case GameState.RestartCountdown:
			statusMessage = "Restarting in " + (int)(restartCountdownDuration - Time.time + restartCountdownTime);
			break;
		}
		
		int labelWidth = 500;
		int labelHeight = 100;
		if (backgroundColor == null) {
			Vector2 widthHeight = new Vector2(labelWidth, labelHeight);
			backgroundColor = new Texture2D((int)widthHeight.x, (int)widthHeight.y);
			for (int y = 0; y < backgroundColor.height; ++y)
	        {
	            for (int x = 0; x < backgroundColor.width; ++x)
	            {
	                //float r = Random.value;
					float r = 0.95f;
	                Color color = new Color(r, r, r, 0.8f);
	                backgroundColor.SetPixel(x, y, color);
	            }
	        }
	        backgroundColor.Apply();
		}
		if (statusMessage != null && !statusMessage.Equals("")) {
			GUIStyle style = new GUIStyle();
			style.fontSize = 36;
			style.alignment = TextAnchor.MiddleCenter;
			style.normal.background = backgroundColor;
			style.normal.textColor = Color.black;
			GUI.Box(new Rect(Screen.width/2 - labelWidth/2, 100, labelWidth, labelHeight), statusMessage, style);
		}
	}
}
