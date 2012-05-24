using UnityEngine;
using System.Collections;

public class PlayerMovementController : MonoBehaviour {
	public float movementDelta = 0.5f;
	public float xLeftBound = -3.0f, xRightBound = 3.0f;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.LeftArrow)) {
			Vector3 position = this.gameObject.transform.position;
			if (position.x > xLeftBound) {
				this.gameObject.transform.position = new Vector3(position.x - movementDelta, position.y, position.z);
			}
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			Vector3 position = this.gameObject.transform.position;
			if (position.x < xRightBound) {
				this.gameObject.transform.position = new Vector3(position.x + movementDelta, position.y, position.z);
			}
		}
	}
	
	void OnTriggerEnter(Collider collider) {
		if (collider.gameObject.tag.Equals("FallingCube")) {
			Color objColor = collider.gameObject.renderer.material.color;
			print("Collision with " + objColor.ToString() + " cube!");
		}
		else if (collider.gameObject.tag.Equals("FallingSphere")) {
			Color objColor = collider.gameObject.renderer.material.color;
			print("Collision with " + objColor.ToString() + " sphere!");
		}
	}
}
