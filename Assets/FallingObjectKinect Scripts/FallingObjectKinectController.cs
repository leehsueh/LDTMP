using UnityEngine;
using System.Collections;

public class FallingObjectKinectController : MonoBehaviour {
	private GameObject floor;
	private GameObject player;
	public GameObject particleExplosionEffect;
	private GameObject gameManager;
	
	private bool collidedWithPlayer = false;
	
	
	
	// Use this for initialization
	void Start () {
		floor = GameObject.Find("Floor");
		player = GameObject.Find ("Player");
		gameManager = GameObject.Find("GameManager");
		collidedWithPlayer = false;
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
		Transform parentCollidee = collider.gameObject.transform.parent;
		if (parentCollidee != null && parentCollidee.gameObject.Equals(player) && !collidedWithPlayer) {
			explode();
			gameManager.GetComponent<FallingObjectGameStatus>().checkCollision(gameObject.collider);
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
