using UnityEngine;
using System.Collections;

// implements the game logic state machine and facilitates communication between all game elements
public class WavingGameManager : MotionSandboxManager {
	// Game components
	#region game components
	public WalkingCharacterSpawner characterSpawner;
	public CalibratedNodeRoot skeletonRoot;
	#endregion
	
	#region State machine parameters
	public float promptDuration;
	public float victoryMessageDuration;
	public float restartCountdownDuration;
	public float minDistanceFromTarget;
	public float pointOfPassingZ;
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
		if (mMainWalker)
			return mMainWalker.gameObject.transform.position.z < pointOfPassingZ;
		else
			return false;
	}
	#endregion
	
	#region state machine actions
	void startPromptTimer() {
		promptTime = Time.time;	
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
	void flashMessage(string message) {
		//TODO: implement this
		print(message);
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
			characterSpawner.startSpawning();
		}
		if (skeletonRoot == null) {
			skeletonRoot = (CalibratedNodeRoot)FindObjectOfType(typeof(CalibratedNodeRoot));
		}
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
					startPromptTimer();
					characterSpawner.selectRandomTarget();
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
			}
			break;
		case GameState.Spawning:
			if (skeletonPresent()) {
				characterSpawner.spawnCharacter();
				mMainWalker = characterSpawner.leastRecentlyAddedWalker();
				// mTargetWalker will get set by the spawner when it spawns an instance of the target
				NextState = GameState.WaitForWave;
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.WaitForWave:
			if (skeletonPresent()) {
				if (handWaveDetected && mainWalkerAlive()) {
					NextState = GameState.WaveRecognized;
				} else if (!mainWalkerAlive()) {
					mMainWalker = characterSpawner.leastRecentlyAddedWalker();
					NextState = GameState.WaveMissed;
				} else {
					NextState = GameState.WaitForWave;	
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.WaveMissed:
			flashMessage("Target Missed!");
			NextState = GameState.Spawning;
			break;
		case GameState.WaveRecognized:
			if (skeletonPresent()) {
				handWaveDetected = false;
				flashMessage("You waved.");
				if (mMainWalker == mTargetWalker) {
					//TODO: also check for distance from target!
					mTargetWalker.giveFeedback();
					NextState = GameState.PerformAnimation;
					print ("you waved to the target!");
				} else {
					NextState = GameState.WaitForWave;	
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.PerformAnimation:
			if (skeletonPresent()) {
				if (mTargetWalker.animation.IsPlaying(mTargetWalker.feedbackMotionName)) {
					NextState = GameState.PerformAnimation;	
				} else {
					NextState = GameState.ShowVictoryMessage;
					startVictoryMessageTimer();
				}
			} else {
				NextState = GameState.WaitForPresence;	
			}
			break;
		case GameState.ShowVictoryMessage:
			if (victoryMessageDone(Time.time)) {
				NextState = GameState.RestartCountdown;
				startRestartCountdownTimer();
			} else {
				NextState = GameState.ShowVictoryMessage;	
			}
			break;
		case GameState.RestartCountdown:
			if (restartCountdownDone(Time.time)) {
				NextState = GameState.ShowPrompt;
				startRestartCountdownTimer();
				characterSpawner.selectRandomTarget();
			} else {
				NextState = GameState.ShowVictoryMessage;	
			}
			break;
		}
		
		CurrentState = NextState;
	}

	void OnGUI() {
		switch (CurrentState) {
		case GameState.WaitForPresence:
			
			break;
		case GameState.WaitForCalibration:
			
			break;
		case GameState.ShowPrompt:
			
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
			
			break;
		case GameState.RestartCountdown:
			
			break;
		}
		
		string statusMessage = CurrentState.ToString();
		int labelWidth = 500;
		int labelHeight = 100;
		GUIStyle style = new GUIStyle();
		style.fontSize = 36;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.black;
		GUI.Box(new Rect(Screen.width/2 - labelWidth/2, 100, labelWidth, labelHeight), statusMessage, style);
	}
}
