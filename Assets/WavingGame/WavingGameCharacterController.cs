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
		
		// make head bigger
//		GameObject head = GameObject.FindGameObjectWithTag("CharacterHead");
//		if (head.transform.localScale.x == 1.0f) {
//			head.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
//		}
	}
	
	public override bool feedbackTriggerMet() {
		return false;
	}
}
