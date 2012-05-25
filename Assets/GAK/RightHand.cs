//-----------------------------------------------------------------------------
// RightHand.cs
//
// Binds associated GameObject to right-hand motion via Kinect
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class RightHand : MonoBehaviour 
{
	
	private KinectManager m_Manager;
	private PlayerManager m_Player;
	
	// Use this for initialization
	void Start () {
		m_Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		m_Player = (PlayerManager)FindObjectOfType(typeof(PlayerManager));
	}
	
	// Update is called once per frame
	void Update () {
		if(m_Manager != null)
		{
			if(m_Player.TotalPlayers >= 1)
			{
				{
					KinectInterface.NUI_SKELETON_DATA data = m_Manager.frameData.SkeletonData[m_Player.Players[0].SkeletonID];
					Vector3 vector = new Vector3(
					data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT].X * 5,
			    	data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT].Y * 5,
					(-data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT].Z * 2));
					
					vector.y = System.Math.Max(vector.y, 0);
					
					gameObject.transform.localPosition = vector + new Vector3(0,0,3.5f);
				}
			}
		}
	}
}
