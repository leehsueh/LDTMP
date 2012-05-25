using UnityEngine;
using System.Collections;

public class MotionSandboxManager : MonoBehaviour {
	public GameObject groundObject;
	public GameObject mainCameraObject;
	
	public GameObject ground {
		get { return groundObject; }
	}
	
	public GameObject mainCamera {
		get { return mainCameraObject; }
	}
	
	// Use this for initialization
	public void Start () {
		if (groundObject == null) {
			groundObject = GameObject.Find("Ground");
		}
		if (mainCameraObject == null) {
			mainCameraObject = GameObject.Find("Main Camera");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
