//-----------------------------------------------------------------------------
// DebugSkeletonNodeJoint.cs
//
// Joint node for Kinect player skeleton data
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class DebugSkeletonNodeJoint : MonoBehaviour 
{
	
	private DebugSkeletonNodeRoot m_Root;
	public KinectInterface.NUI_SKELETON_POSITION_INDEX BodyPart;
	public KinectManager m_Manager;
	
	// Use this for initialization
	void Start () {
		if(m_Manager == null)
		{
			m_Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		}
		m_Root = (DebugSkeletonNodeRoot)gameObject.transform.parent.gameObject.GetComponent<DebugSkeletonNodeRoot>();
	}
	
	// Update is called once per frame
	void Update () {
		if(!m_Root.enableSkeleton)
		{
			gameObject.transform.localPosition = Vector3.zero;
			return;
		}	
		
		KinectInterface.NUI_SKELETON_DATA skeletonData;
		if(m_Manager.GetFirstTrackedSkeleton(out skeletonData))
		{
			Vector3 vector = new Vector3(
				skeletonData.SkeletonPositions[(int)BodyPart].X,
		    	skeletonData.SkeletonPositions[(int)BodyPart].Y,
				skeletonData.SkeletonPositions[(int)BodyPart].Z);
				
			gameObject.transform.localPosition = vector;
		}
	}
}
