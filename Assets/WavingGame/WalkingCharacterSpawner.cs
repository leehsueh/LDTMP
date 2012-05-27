using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkingCharacterSpawner : MonoBehaviour {
	public WavingGameManager mManager;
	public GameObject[] CharacterPrefabs;
	public Vector2 rangeXOrigin;
	private Vector2 rangeYOrigin;
	private Vector2 rangeZOrigin;
	public bool spawnAtFixedRate;	// mostly for testing purposes
	private float rateInterval = 8f;	// 4 seconds between spawns, if spawning at fixed rate
	private float lastSpawnTime;
	private int mTargetIndex = -1;
	public string TargetName {
		get {
			if (mTargetIndex < 0) {
				return "<target not set>";
			} else {
				return CharacterPrefabs[mTargetIndex].name;
			}
		}
	}
	private float defaultSpeed = 0.02f;
	public float DefaultSpeed {
		get { return defaultSpeed; }
		set { defaultSpeed = value; }
	}
	
	private LinkedList<WavingGameCharacterController> mWalkers;
	public LinkedList<WavingGameCharacterController> walkers {
		get { return mWalkers; }
	}
	
	private bool mSpawning;
	public bool spawning {
		get { return mSpawning; }	
	}
	
	public void startSpawning() {
		mSpawning = true;
	}
	
	public void stopSpawning() {
		mSpawning = false;
	}
	
	public void spawnCharacter() {
		// pick a random #
		int random = Random.Range(0, CharacterPrefabs.Length);
		spawnCharacter(random);
	}
	
	public void spawnTargetCharacter() {
		spawnCharacter(mTargetIndex);
	}
	
	public void spawnCharacterButNotTarget() {
		spawnCharacterButNotIndex(mTargetIndex);
	}
	
	// spawn a character excluding the one at the given index
	void spawnCharacterButNotIndex(int index) {
		int random = index;
		do {
			random = Random.Range(0, CharacterPrefabs.Length);
		} while (random == index);
		
		spawnCharacter(random);
	}
	
	void spawnCharacter(int index) {
		if (spawning) {
			GameObject objectPrefab = CharacterPrefabs[index];
			float xStart = Random.Range(rangeXOrigin.x, rangeXOrigin.y);
			float yStart = Random.Range(rangeYOrigin.x, rangeYOrigin.y);
			float zStart = Random.Range (rangeZOrigin.x, rangeZOrigin.y);
			Vector3 position = new Vector3(xStart, yStart, zStart);
			GameObject obj = (GameObject)Instantiate(objectPrefab, position, objectPrefab.transform.rotation);
			mWalkers.AddFirst(obj.GetComponent<WavingGameCharacterController>());
			
			// check if it's the target character
			if (index == mTargetIndex) {
				mManager.TargetWalker = obj.GetComponent<WavingGameCharacterController>();
			}
//			if (mWalkers.Count == 1) {
//				mManager.MainWalker = obj.GetComponent<WavingGameCharacterController>();
//			}
		}
	}
	
	public int selectRandomTarget() {
		//TODO: make this random
		//mTargetIndex = Random.Range(0, CharacterPrefabs.Length);	
		mTargetIndex = 0;
		return mTargetIndex;
	}
	
	public WavingGameCharacterController leastRecentlyAddedWalker() {
		return mWalkers.Last.Value;	
	}
	
	public void removeWalker(WavingGameCharacterController walker) {
		if (mWalkers.Contains(walker)) {
			mWalkers.Remove(walker);
		}
		print ("walker removed");
		//mManager.MainWalker = mWalkers.Last.Value;
	}
	
	public void killAll() {
		foreach (WavingGameCharacterController walker in mWalkers) {
			mWalkers.Remove(walker);
			Destroy(walker.gameObject);
		}
	}
	
	// Use this for initialization
	void Start () {
		if (rangeXOrigin == Vector2.zero) {
			rangeXOrigin = new Vector2(0, 1.4f);	
		}
		if (rangeYOrigin == Vector2.zero) {
			rangeYOrigin = new Vector2(0.55f, 0.55f);	
		}
		if (rangeZOrigin == Vector2.zero) {
			rangeZOrigin = new Vector2(4.55f, 4.55f);	
		}
		lastSpawnTime = -rateInterval;
		mWalkers = new LinkedList<WavingGameCharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		if (spawnAtFixedRate && spawning) {
			float time = Time.time;
			if (time - lastSpawnTime > rateInterval) {
				spawnCharacter();
				lastSpawnTime = time;
			}
		}
	}
}
