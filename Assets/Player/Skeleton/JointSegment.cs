using UnityEngine;
using System.Collections;

public class JointSegment : MonoBehaviour {
	public GameObject joint1, joint2;
	public float segmentWidth = 0.1f;
	private CalibratedNodeRoot m_Root;
	
	// Use this for initialization
	void Start () {
		Transform currParent = gameObject.transform.parent;
		while (m_Root == null && currParent != null) {
			m_Root = (CalibratedNodeRoot)currParent.gameObject.GetComponent<CalibratedNodeRoot>();
			currParent = currParent.gameObject.transform.parent;
		}
		
		// set up LineRenderer
		LineRenderer lr = (LineRenderer)gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(2);
		lr.SetWidth(segmentWidth, segmentWidth);
	}
	
	// Update is called once per frame
	void Update () {
		if (!m_Root.enableSkeleton)
		{
			return;
		}
		
		// check if skeleton is done calibrating
		if (m_Root.isCalibrated()) {
			print ("segment active!");
			Vector3 position1 = joint1.transform.localPosition;
			Vector3 position2 = joint2.transform.localPosition;
			print ("joint1: " + position1 + ", joint2: " + position2);
			LineRenderer lr = (LineRenderer)gameObject.GetComponent<LineRenderer>();
			lr.SetPosition(0, position1);
			lr.SetPosition(1, position2);
		} else {
			return;
		}
	}
}
