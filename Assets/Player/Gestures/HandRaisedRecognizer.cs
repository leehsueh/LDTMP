using UnityEngine;
using System.Collections;

public class HandRaisedRecognizer : GestureRecognizer {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	bool handRaised() {
		return body.rightHand.gameObject.transform.position.y > body.rightShoulder.gameObject.transform.position.y;	
	}
}
