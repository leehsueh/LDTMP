using UnityEngine;
using System.Collections;

public class HandModelController : MonoBehaviour {
	
	public GameObject elbowJoint;
	public KinectInterface.NUI_SKELETON_POSITION_INDEX BodyPart;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	// rotate the hand wrt to the elbow to maintain a "straight" lower arm
	void Update () {
		float deltaX = gameObject.transform.localPosition.x - elbowJoint.transform.localPosition.x;
		float deltaY = gameObject.transform.localPosition.y - elbowJoint.transform.localPosition.y;
		float theta = Mathf.Atan(deltaY/deltaX);
		float thetaDeg = theta * 180/Mathf.PI - 60;
		if (deltaX < 0) thetaDeg += 180;
		
		//gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleDeg));
		gameObject.transform.localRotation = Quaternion.AngleAxis(thetaDeg, Vector3.forward);
	}
}
