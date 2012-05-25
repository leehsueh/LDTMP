using UnityEngine;
using System.Collections;

public class CalibratedNodeRoot : MonoBehaviour {

	public int frameIndex = 0;
	public bool enableSkeleton = true, enableSegments = false;
	public int playerNumber;	// should be 1 or 2 only; used to get the EnrolledPlayer associated with the player number from CustomPlayerManager
	private uint playerID;
	private bool playerSet;	// true if the player is an enrolled player
	public bool mMirrorMode;	// true if player is shown as if looking at a mirror
	
	// Body parts
	public GameObject core;
	public GameObject head;
	public GameObject leftShoulder;
	public GameObject rightShoulder;
	public GameObject leftElbow;
	public GameObject rightElbow;
	public GameObject leftHand;
	public GameObject rightHand;
	public GameObject leftHip;
	public GameObject rightHip;
	public GameObject leftKnee;
	public GameObject rightKnee;
	public GameObject leftFoot;
	public GameObject rightFoot;
	
	private GameObject[] bodyJoints;
	
	public Vector3 basePosition;
	public float movementXThreshold = 0.3f;
	public float movementMultiplier = 1f;
	
	private int numJoints, calibratedJoints = 0;
	private bool calibrationDone = false;
	private CustomPlayerManager mPlayerManager;
	
	public GameObject gameManager;
	
	public bool mirrorMode {
		get { return mMirrorMode; }	
	}
	
	// Use this for initialization
	void Start () {
		numJoints = gameObject.GetComponentsInChildren<CalibratedNodeJoint>().Length;
		if (!enableSegments) {
			JointSegment[] segments = gameObject.GetComponentsInChildren<JointSegment>();
			foreach (JointSegment s in segments) {
				s.gameObject.active = false;	
			}
		}
		if (mPlayerManager == null)
		{
			mPlayerManager = (CustomPlayerManager)FindObjectOfType(typeof(CustomPlayerManager));
		}
		basePosition = gameObject.transform.position;
		
	}
	
	// Update is called once per frame
	void Update () {
		if (bodyJoints == null) {
			bodyJoints = new GameObject[14] {core, 
				head, 
				leftShoulder, 
				rightShoulder,
				leftElbow,
				rightElbow,
				leftHand,
				rightHand,
				leftHip,
				rightHip,
				leftKnee,
				rightKnee,
				leftFoot,
				rightFoot
			};
		}
		CustomPlayerManager.EnrolledPlayer player = GetEnrolledPlayer();
		if (player.PlayerID != 0) {
			if (!playerSet) {
				playerID = player.PlayerID;
				playerSet = true;
				recalibrate();
			} else {
				
			}
		} else {
			if (playerSet) {
				// lost the player
				playerSet = !playerSet;
				playerID = 0;
				foreach (GameObject bodyPart in bodyJoints) {
					bodyPart.GetComponent<CalibratedNodeJoint>().resetToBasePosition();	
				}
				gameObject.transform.position = basePosition;
			}
		}
	}
	
	void OnGUI() {
	}
	
	public CustomPlayerManager.EnrolledPlayer GetEnrolledPlayer() {
		if (playerNumber == 1) {
			return mPlayerManager.Player1;
		} else if (playerNumber == 2) {
			return mPlayerManager.Player2;	
		} else {
			return new CustomPlayerManager.EnrolledPlayer();	
		}
	}
	
	public bool GetSkeletonDataForPlayer(out KinectInterface.NUI_SKELETON_DATA skeletonData) {
		return GetEnrolledPlayer().PlayerID != 0 && mPlayerManager.GetSkeletonDataForPlayer(GetEnrolledPlayer(), out skeletonData);
	}
	
	public void changePlayerColor(Color color) {
		if (bodyJoints != null) {
			foreach (GameObject bodyPart in bodyJoints) {
				if (bodyPart != null) {
					bodyPart.renderer.material.color = color;
				}
			}
		}
	}
	
	public void recalibrate() {
		calibratedJoints = 0;
		calibrationDone = false;
		CalibratedNodeJoint[] joints = gameObject.GetComponentsInChildren<CalibratedNodeJoint>();
		foreach (CalibratedNodeJoint joint in joints) {
			joint.recalibrateJoint();	
		}
	}
	
	/**
	 * called by child game objects representing joints
	 */
	public void jointCalibrated(GameObject go) {
		calibratedJoints++;
		calibrationDone = calibratedJoints == numJoints;
		if (calibrationDone) print("Calibration done!");
	}
	
	/**
	 * Called by segment game objects to poll state of calibration
	 */
	public bool isCalibrated() {
		return calibrationDone;	
	}
	
	public Vector3 bodyPosition() {
		// TODO: get position of the Core, rather than container gameObject
		//GameObject core = gameObject.GetComponentInChildren
		return core.transform.position;
	}
	
	public void updateBodyPosition(Vector3 diffVector) {
		Vector3 incrementalPosition = new Vector3(0, 0, diffVector.z);
		if (diffVector.x >= movementXThreshold) {
			incrementalPosition.x = movementMultiplier * diffVector.x;
		}
		Vector3 resultPos = basePosition + incrementalPosition;
		if (gameManager != null) {
			//if (resultPos.x > 0) resultPos.x = Mathf.Min(resultPos.x, gameManager.GetComponent<FallingObjectGameStatus>().rightBound);
			//else if (resultPos.x < 0) resultPos.x = Mathf.Max(resultPos.x, gameManager.GetComponent<FallingObjectGameStatus>().leftBound);
		}
		gameObject.transform.position = resultPos;
	}
}
