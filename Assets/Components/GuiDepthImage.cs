//-----------------------------------------------------------------------------
// GuiDepthImage.cs
//
// Displays depth image data from Kinect sensor
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class GuiDepthImage : MonoBehaviour 
{	
	
	public KinectManager m_Manager;
	public Texture m_Texture;
	
	//Settable in Unity Editor
	public Vector2 m_ScreenPos = new Vector2(0,0);
	public Vector2 m_Scale = new Vector2(1,1);
	// Use this for initialization
	void Start () {
		if(m_Manager == null)
			m_Manager = (KinectManager) FindObjectOfType(typeof(KinectManager));
	}
	
	void Update()
	{
		m_Texture = m_Manager.DepthStream;
	}

	
	void OnGUI()
	{
		if(m_Texture != null)
		{
			GUIUtility.ScaleAroundPivot(-m_Scale, new Vector2(m_Texture.width / 2, m_Texture.height / 2));
			GUI.Label(new Rect(-m_ScreenPos.x,-m_ScreenPos.y,m_Texture.width,m_Texture.height),m_Texture);
		}
	}
}