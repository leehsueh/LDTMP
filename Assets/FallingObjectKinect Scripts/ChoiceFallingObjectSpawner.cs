using UnityEngine;
using System.Collections;

public class ChoiceFallingObjectSpawner : MonoBehaviour {
	public Texture[] faceTextures;
	public Color[] colors = {Color.red, Color.blue, Color.green, Color.yellow, Color.gray, Color.black, Color.magenta};
	public float drag;
	public float lowDrag;
	public GameObject facePrefab;
	public GameObject spherePrefab;
	public float yStartHeight;
	public float xLeftBound, xRightBound;
	private FallingObjectGameManager mManager;
	
	// Use this for initialization
	void Start () {
		mManager = (FallingObjectGameManager)FindObjectOfType(typeof(FallingObjectGameManager));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public GameObject[] spawnTwoProblem() {
		// generate a random face
		int index = Random.Range(0, faceTextures.Length);
		float xFace;
		float xSphere;
		
		if (Random.Range (0f,1f) < 0.5) {
			xFace = xLeftBound;
			xSphere = xRightBound;
		} else {
			xFace = xRightBound;
			xSphere = xLeftBound;
		}
		Vector3 position = new Vector3(xFace, yStartHeight, 0);
		GameObject faceObj = spawnPrefab(facePrefab, position);
		faceObj.renderer.material.mainTexture = faceTextures[index];
		faceObj.rigidbody.drag = drag;
		
		// generate a random color sphere
		position.x = xSphere;
		GameObject sphereObj = spawnPrefab(spherePrefab, position);
		sphereObj.rigidbody.drag = drag;
		
		return new GameObject[] {sphereObj, faceObj};
	}
	
	public void spawnThreeProblem() {
		
	}
	
	GameObject spawnPrefab(GameObject prefab, Vector3 position) {
		return (GameObject)Instantiate(prefab, position, facePrefab.transform.rotation);
	}
}
