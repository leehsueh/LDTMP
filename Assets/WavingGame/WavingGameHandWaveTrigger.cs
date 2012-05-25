using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WavingGameHandWaveTrigger : HandWaveGesture {
	private WavingGameManager mManager;
	
	void Start() {
		base.Start();
		mManager = (WavingGameManager)FindObjectOfType(typeof(WavingGameManager));
	}
	
	public override void triggerAction() {
		print ("Action in the child!");
		WalkingCharacterController closest = closestWalker();
		closest.giveFeedback();
	}
	
	WavingGameCharacterController closestWalker() {
		HashSet<WavingGameCharacterController> walkers = mManager.characterSpawner.walkers;
		WavingGameCharacterController closestWalker = null;
		float minDistance = 20;
		foreach (WavingGameCharacterController walker in walkers) {
			if (walker == null) continue;
			if (closestWalker == null) {
				closestWalker = walker;	
				minDistance = Vector3.Distance(skeletonRoot.bodyPosition(), walker.gameObject.transform.position);
			} else {
				float distance = Vector3.Distance(skeletonRoot.bodyPosition(), walker.gameObject.transform.position);
				if (distance < minDistance) {
					closestWalker = walker;
					minDistance = distance;
				}
			}
		}
		return closestWalker;
	}
}
