using UnityEngine;
using System.Collections;

public class PlayerStatusController : MonoBehaviour {
	public GameObject targetPlayer;
	public GameObject counterText;
	public GameObject statusText;
	
	private CalibratedNodeRoot skeletonRoot;
	private KinectManager m_Manager;
	
	public enum PlayerProximityState
	{
        TooFar = 0,
        TooClose,
        JustRight,
		WaitingForHandRaise,
		HandRaised,
		WaveComplete,
        NUM
    };
	
	public enum PlayerStatusState
	{
		OffScreen = 0,
		Calibrating,
		Ready,
		NUM
	}
	
	private PlayerProximityState currentProximityState;
	private PlayerStatusState currentStatusState;
	private int helloCount = 0;
	
	// state-relevant variables
	private float updateStateInterval = (float)(1.0/60.0);	// roughly 60 hz
	private float lastUpdateTime = 0f;
	public float properDistance = 1.15f;
	public float properDistanceThreshold = 0.1f;
	private float handRaisedMinDuration = 1.0f;
	private float handRaisedTime;	// the time when hand was raised
	
	bool HandRaised(KinectInterface.NUI_SKELETON_DATA Data)
	{
		KinectInterface.Vector4 HeadPos = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HEAD];
		KinectInterface.Vector4 RightHand = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT];
		KinectInterface.Vector4 LeftHand = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_LEFT];
	
		return RightHand.Y > HeadPos.Y || LeftHand.Y > HeadPos.Y;
	}
	
	// Use this for initialization
	void Start () {
		m_Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		skeletonRoot = (CalibratedNodeRoot)gameObject.GetComponent<CalibratedNodeRoot>();
		currentProximityState = PlayerProximityState.NUM;
		currentStatusState = PlayerStatusState.NUM;
		if (!targetPlayer) {
			targetPlayer = GameObject.Find("StaticAvatar");
		}
	}

	// Update is called once per frame
	void Update () {
		float updateTime = Time.time;
		if (updateTime - lastUpdateTime > updateStateInterval) {
			KinectInterface.NUI_SKELETON_DATA skeletonData;
			if (!skeletonRoot.GetSkeletonDataForPlayer(out skeletonData)) {
				currentStatusState = PlayerStatusState.OffScreen;
				currentProximityState = PlayerProximityState.NUM;
			} else if (currentStatusState == PlayerStatusState.NUM || currentStatusState == PlayerStatusState.OffScreen) {
				currentStatusState = PlayerStatusState.Calibrating;
			} else if (currentStatusState == PlayerStatusState.Calibrating && skeletonRoot.isCalibrated()) {
				currentStatusState = PlayerStatusState.Ready;
				currentProximityState = distanceNextState();
			}
			if (currentStatusState == PlayerStatusState.Ready) {
				PlayerProximityState nextState = currentProximityState;
				switch (currentProximityState) {
				case PlayerProximityState.NUM:
				case PlayerProximityState.TooFar:
				case PlayerProximityState.TooClose:
					nextState = distanceNextState();
					break;
				case PlayerProximityState.JustRight:
					nextState = PlayerProximityState.WaitingForHandRaise;
					break;
				case PlayerProximityState.WaitingForHandRaise:
					// make sure we're still at the right distance
					nextState = distanceNextState();
					if (nextState == PlayerProximityState.JustRight) {
						// check for hand raise
						bool handRaised = HandRaised(skeletonData);
						if (handRaised) {
							nextState = PlayerProximityState.HandRaised;
							handRaisedTime = Time.time;
						} else {
							nextState = PlayerProximityState.WaitingForHandRaise;	
						}
					}
					break;
				case PlayerProximityState.HandRaised:
					// make sure we're still at the right distance
					nextState = distanceNextState();
					if (nextState == PlayerProximityState.JustRight) {
						if (HandRaised(skeletonData)) {
							// check how long it's been raised
							float currTime = Time.time;
							if (currTime - handRaisedTime > handRaisedMinDuration) {
								nextState = PlayerProximityState.WaveComplete;
								helloCount++;
							} else {
								nextState = PlayerProximityState.HandRaised;
							}
						} else {
							nextState = PlayerProximityState.WaitingForHandRaise;	
						}
					}
					break;
				case PlayerProximityState.WaveComplete:
					// wait till user moves away from target
					nextState = distanceNextState();
					nextState = nextState == PlayerProximityState.TooFar ? nextState : PlayerProximityState.WaveComplete;
					break;
				}
				
				// update the state
				currentProximityState = nextState;
				lastUpdateTime = updateTime;
			}
		}
	}
	
	float distanceFromTarget() {
		Vector3 localPosition = gameObject.GetComponent<CalibratedNodeRoot>().bodyPosition();
		Vector3 targetPosition = targetPlayer.GetComponent<CalibratedNodeRoot>().bodyPosition();
		return Vector3.Distance(localPosition, targetPosition);
	}
	
	PlayerProximityState distanceNextState() {
		if (distanceFromTarget() > properDistance + properDistanceThreshold) {
			return PlayerProximityState.TooFar;	
		} else if (distanceFromTarget() < properDistance - properDistanceThreshold) {
			return PlayerProximityState.TooClose;
		} else {
			return PlayerProximityState.JustRight;
		}
	}
	
	void OnGUI() {
		string message = "Default";
		
		//statusText.guiText.material.color = Color.yellow;
		
		switch (currentStatusState) {
		case PlayerStatusState.NUM:
		case PlayerStatusState.OffScreen:
			message = "We can't find you!";
			statusText.guiText.material.color = Color.red;
			break;
		case PlayerStatusState.Calibrating:
			message = "Stand still with arms at your side!";
			break;
		case PlayerStatusState.Ready:
			switch (currentProximityState) {
			case PlayerProximityState.TooFar:
				message = "Move closer to your neighbor.";
				changePlayerColor(targetPlayer, Color.white);
				break;
			case PlayerProximityState.TooClose:
				message = "Too close!";
				changePlayerColor(targetPlayer, Color.red);
				statusText.guiText.material.color = Color.red;
				break;
			case PlayerProximityState.JustRight:
				message = "Great! Now, raise your hand above your shoulder.";
				changePlayerColor(targetPlayer, Color.yellow);
				break;
			case PlayerProximityState.WaitingForHandRaise:
				message = "Great! Now, raise your hand above your shoulder.";
				break;
			case PlayerProximityState.HandRaised:
				message = "Wave your hand!";
				break;
			case PlayerProximityState.WaveComplete:
				message = "Great! You said hello! Let's do it again!";
				statusText.guiText.material.color = Color.green;
				changePlayerColor(targetPlayer, Color.green);
				counterText.guiText.text = helloCount + " times";
				break;
			default:
				message = "Uh oh...proximity stuck on " + currentProximityState;
				break;
			}
			break;
		default:
			message = "Uh oh...status stuck on " + currentStatusState;
			break;
		}
		//Vector2 labelSize = new Vector2(900, 200);
		//GUI.Label(new Rect(Screen.width/2 - labelSize.x/2, 40, labelSize.x, labelSize.y), message, style);
		statusText.guiText.text = message;
		
		// debug; show distance
		GUIStyle style = new GUIStyle();
		style.fontSize = 16;
		style.normal.textColor = Color.gray;
		GUI.Label(new Rect(10, 60, 200, 40), "distance: " + distanceFromTarget(), style);
	}
	
	void changePlayerColor(GameObject target, Color color) {
		Transform[] childrenTransforms = target.GetComponentsInChildren<Transform>();
		foreach (Transform childTransform in childrenTransforms) {
			childTransform.gameObject.renderer.material.color = color;	
		}
	}
	
}
