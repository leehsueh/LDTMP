using UnityEngine;
using System.Collections;

public class WavingGameManager : MotionSandboxManager {
	public WalkingCharacterSpawner characterSpawner;
	
	// Use this for initialization
	void Start () {
		base.Start();
		if (characterSpawner == null) {
			characterSpawner = (WalkingCharacterSpawner)FindObjectOfType(typeof(WalkingCharacterSpawner));
			characterSpawner.spawnAtFixedRate = true;
			characterSpawner.startSpawning();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
