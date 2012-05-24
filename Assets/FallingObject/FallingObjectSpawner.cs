using UnityEngine;
using System.Collections;

public class FallingObjectSpawner : MonoBehaviour {
	public Color[] colors = {Color.red, Color.blue, Color.green, Color.yellow, Color.gray, Color.black, Color.magenta};
	public float drag;
	public GameObject[] fallingObjectPrefabs = {};
	public float yStartHeight;
	public float xleftBound, xRightBound;
	public float minScale, maxScale;
	public GameObject objectPrefab;
	public float spawnRate = 2;
	private float lastSpawnTime = 0;
	
	private bool spawningOn = true;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float time = Time.time;
		if (time - lastSpawnTime > spawnRate && spawningOn) {
			spawnObject();
			lastSpawnTime = time;
		}
	}
	
	void spawnObject() {
		float x = Random.Range (xleftBound, xRightBound);
		int index = Random.Range(0, fallingObjectPrefabs.Length);
		GameObject objectPrefab = fallingObjectPrefabs[index];
		objectPrefab.rigidbody.drag = drag;
		Vector3 position = new Vector3(x, yStartHeight,0);
		float scaleFactor = Random.Range (minScale,maxScale);
		Vector3 scale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
		GameObject obj = (GameObject)Instantiate(objectPrefab, position, objectPrefab.transform.rotation);
		
		index = Random.Range(0, colors.Length);
		obj.renderer.material.color = colors[index];
		obj.transform.localScale = scale;
	}
	
	public void stopSpawning() {
		spawningOn = false;
	}
	
	public void startSpawning() {
		spawningOn = true;	
	}
}
