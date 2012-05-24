using UnityEngine;
using System.Collections;

public class WalkingCharacterController : MonoBehaviour {
	public float speedMultiplier = 0.01f;
	private Vector3 directionVector;
	private WavingGameManager gameManager;
	private float offscreenZ;	// the z coordinate indicating where the camera is; used to determine when character is off screen in the z direction
	
	// Use this for initialization
	void Start () {
		gameManager = (WavingGameManager)FindObjectOfType(typeof(WavingGameManager));
		float angleX = gameManager.ground.transform.rotation.eulerAngles.x;
		directionVector = new Vector3(0, Mathf.Tan(Mathf.Deg2Rad*angleX), -1).normalized;
		offscreenZ = gameManager.mainCamera.transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position += speedMultiplier * directionVector;
		
		// check if off screen
		if (gameObject.transform.position.z < offscreenZ) {
			gameObject.active = false;	
		}
	}
}
