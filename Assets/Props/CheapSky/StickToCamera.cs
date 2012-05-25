using UnityEngine;
using System.Collections;

public class StickToCamera : MonoBehaviour {
	
	public Transform cam;
	
	void Awake () {
		if (!cam) {
			Debug.Log("ATTACH CAMERA TRANSFORM");
			enabled = false;
		}
	}
	
	void LateUpdate () {
		transform.position = cam.position;
	}
}