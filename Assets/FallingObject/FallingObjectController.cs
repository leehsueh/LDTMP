using UnityEngine;
using System.Collections;

public class FallingObjectController : MonoBehaviour {
	private GameObject floor;
	private GameObject player;
	public GameObject particleExplosionEffect;
	
	// Use this for initialization
	void Start () {
		floor = GameObject.Find("Floor");
		player = GameObject.Find ("Player");
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 curPosition = gameObject.transform.position;
		//gameObject.transform.position = new Vector3(curPosition.x, curPosition.y - fallSpeed, curPosition.z);
		if (floor.transform.position.y > curPosition.y + gameObject.transform.localScale.y) {
			Destroy (gameObject);	
		}
	}
	
	void OnTriggerEnter(Collider collider) {
		//print ("Ball collision detected with " + collider.gameObject.name);
		if (collider.gameObject.Equals(player)) {
			explode();
		}
	}
	
	public void explode() {
		// show an explosion and destroy the game object
			Vector3 explosionPos = gameObject.transform.position;
			Color color = gameObject.renderer.material.color;
			Destroy (gameObject);
			
			GameObject explosion = (GameObject)Instantiate(particleExplosionEffect, explosionPos, Quaternion.identity);
			ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
			ps.startColor = color;
			ps.Emit(0);
	}
}
