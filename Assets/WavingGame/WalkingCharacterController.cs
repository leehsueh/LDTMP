using UnityEngine;
using System.Collections;

public class WalkingCharacterController : MonoBehaviour {
	private float speedMultiplier = 0.01f;
	public float Speed {
		get { return speedMultiplier; }	
		set { speedMultiplier = value; }
	}
	public string feedbackMotionName;
	protected bool didGiveFeedback;
	public bool DidGiveFeedback {
		get { return didGiveFeedback; }	
	}
	private Vector3 directionVector;
	private MotionSandboxManager gameManager;
	private float offscreenZ;	// the z coordinate indicating where the camera is; used to determine when character is off screen in the z direction
	
	// Use this for initialization
	public void Start () {
		gameManager = (MotionSandboxManager)FindObjectOfType(typeof(MotionSandboxManager));
		float angleX = gameManager.ground.transform.rotation.eulerAngles.x;
		directionVector = new Vector3(0, Mathf.Tan(Mathf.Deg2Rad*angleX), -1).normalized;
		offscreenZ = gameManager.mainCamera.transform.position.z;
		didGiveFeedback = false;
	}
	
	// Update is called once per frame
	public void Update () {
		gameObject.transform.position += speedMultiplier * Time.deltaTime * directionVector;
		if (feedbackTriggerMet() && !didGiveFeedback) {
			giveFeedback();
		} else {
			// check if feedback is done
			if (didGiveFeedback && !animation.IsPlaying(feedbackMotionName)) {
				animation.CrossFadeQueued(animation.clip.name, 0.3f,QueueMode.CompleteOthers);
				
			}
			if (!animation.IsPlaying(feedbackMotionName)) {
				gameObject.transform.position += speedMultiplier * directionVector;
			}
		}
		// check if off screen
		if (gameObject.transform.position.z < offscreenZ) {
			gameObject.active = false;	
		}
	}
	
	public virtual bool feedbackTriggerMet() {
		return gameObject.transform.position.z < 3;
	}
	
	public void giveFeedback() {
		animation.CrossFade(feedbackMotionName);
		print (feedbackMotionName);
		didGiveFeedback = true;
	}
}
