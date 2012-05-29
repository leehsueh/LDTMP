using UnityEngine;
using System.Collections;

public class CalibratedNodeJoint : MonoBehaviour {
	private CalibratedNodeRoot m_Root;
	public KinectInterface.NUI_SKELETON_POSITION_INDEX BodyPart;
	public KinectManager m_Manager;
	public CustomPlayerManager mPlayerManager;
	
	private Vector3 basePosition;	// the position of this object as set in Unity
	private Vector3 initialPosition;	// the initial position of the tracked joint, to be "zeroed" at the base position
	private int calibrationStepsNeeded = 20;
	private int calibrationStepCount = 0;
	private bool mirrorMode;	// obtained from the NodeRoot
	
	// Use this for initialization
	void Start () {
		if(m_Manager == null)
		{
			m_Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		}
		if(mPlayerManager == null)
		{
			mPlayerManager = (CustomPlayerManager)FindObjectOfType(typeof(CustomPlayerManager));
		}
		basePosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
		
		Transform currParent = gameObject.transform.parent;
		while (m_Root == null && currParent != null) {
			m_Root = (CalibratedNodeRoot)currParent.gameObject.GetComponent<CalibratedNodeRoot>();
			currParent = currParent.gameObject.transform.parent;
			mirrorMode = m_Root.mirrorMode;
		}
		
		if (gameObject.Equals(m_Root.head) && !m_Root.mirrorMode) {
			Quaternion rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, 
				gameObject.transform.rotation.eulerAngles.y + 180, 
				1.72f);
			gameObject.transform.rotation = rotation;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!m_Root.enableSkeleton)
		{
			return;
		}	
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		
		if (m_Root.GetSkeletonDataForPlayer(out skeletonData))
		//if(m_Manager.GetFirstTrackedSkeleton(out skeletonData))
		{
			Vector3 jointPosition = new Vector3(
				skeletonData.SkeletonPositions[(int)BodyPart].X,
		    	skeletonData.SkeletonPositions[(int)BodyPart].Y,
				skeletonData.SkeletonPositions[(int)BodyPart].Z
			);
			if (calibrationStepCount < calibrationStepsNeeded) {

				if (initialPosition == Vector3.zero) {
					initialPosition = jointPosition;
				} else {
					initialPosition = (initialPosition + jointPosition)/2;
				}
				calibrationStepCount++;
				if (calibrationStepCount == calibrationStepsNeeded) {
					m_Root.jointCalibrated(gameObject);	
				}
			} else {
				Vector3 diffVector = jointPosition - initialPosition;
				if (m_Root.ConstrainZMovement) {
					diffVector.z = 0;	
				}
				if (!mirrorMode) diffVector = new Vector3(diffVector.x, diffVector.y, -diffVector.z);
				gameObject.transform.localPosition = basePosition + diffVector;
				
				if (gameObject.Equals(m_Root.GetComponent<CalibratedNodeRoot>().core)) {
					m_Root.GetComponent<CalibratedNodeRoot>().updateBodyPosition(diffVector);
				}
			}
		}
	}
	
	public void recalibrateJoint() {
		calibrationStepCount = 0;
		initialPosition = Vector3.zero;
	}
	
	public void resetToBasePosition() {
		gameObject.transform.localPosition = basePosition;
		initialPosition = Vector3.zero;
	}
}

