using UnityEngine;
using System.Collections;

public class WavingGameCharacterController : WalkingCharacterController {

	// Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
	}
	
	public override bool feedbackTriggerMet() {
		return gameObject.transform.position.z < 1;
	}
}
