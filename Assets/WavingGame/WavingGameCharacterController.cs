using UnityEngine;
using System.Collections;

public class WavingGameCharacterController : WalkingCharacterController {
	private WavingGameManager mManager;
	
	// Use this for initialization
	void Start () {
		base.Start();
		mManager = (WavingGameManager)FindObjectOfType(typeof(WavingGameManager));
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		if (!gameObject.active) {
			mManager.characterSpawner.removeWalker(gameObject.GetComponent<WavingGameCharacterController>());
		}
	}
	
	public override bool feedbackTriggerMet() {
		return false;
	}
}
