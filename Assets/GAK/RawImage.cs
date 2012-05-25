//-----------------------------------------------------------------------------
// RawImage.cs
//
// Copies the raw color image data from a Kinect sensor
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class RawImage : MonoBehaviour 
{
	
	public KinectManager m_Manager;
	private Texture m_Texture;
	// Use this for initialization
	void Start () {
		if(m_Manager == null)
		m_Manager = (KinectManager) FindObjectOfType(typeof(KinectManager));
	}
	
	// Update is called once per frame
	void Update () {
		m_Texture = m_Manager.ColorStream;
		gameObject.renderer.material.mainTexture = m_Texture;
	}
}
