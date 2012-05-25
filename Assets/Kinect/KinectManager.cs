//-----------------------------------------------------------------------------
// KinectManager.cs
//
// Manages the Kinect interface via Gak2Unity
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Script that handles the connection to the Kinect sensor
/// </summary>
public class KinectManager : MonoBehaviour, IDisposable 
{
	
	/// <summary>
	/// Texture data 
	/// </summary>
	Texture2D m_ColorTexture;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	Color32[] m_ColorArray;
	GCHandle m_ColorHandle;
#endif
	
	Texture2D m_DepthTexture;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	Color32[] m_DepthArray;
	GCHandle m_DepthHandle;
#endif
    KinectFilter m_SmoothingFilter;
	
	public KinectInterface.NUI_SKELETON_FRAME frameData = new KinectInterface.NUI_SKELETON_FRAME();
	
	/// <summary>
	/// Getter for the raw color stream texture 
	/// </summary>
	public Texture2D ColorStream
	{
		get
		{
			return m_ColorTexture;
		}
	}
	
	/// <summary>
	/// Getter for the depth stream texture 
	/// </summary>
	public Texture2D DepthStream
	{
		get
		{
			return m_DepthTexture;
		}
	}
	
	/// <summary>
	/// Initializes the Kinect and allocates resources
	/// </summary>
	void Start () 
	{
		//Start Kinect
		HRESULT.ThrowOnFailure(KinectInterface.GakInit(
            /* fps */30,
            KinectInterface.NuiSkeletonStream.NSM_SKELETON_STREAM_FULL,
            KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_COLOR_640x480,
            KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_320x240,
            /* startStreams */ true));
        this.m_SmoothingFilter = new KinectFilter(KinectInterface.GUID_SkeletonSmoothingFilter, /* isEnabled */ true);

		PlayerManager Manager = (PlayerManager)FindObjectOfType(typeof(PlayerManager));

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		m_ColorTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
		m_ColorArray = m_ColorTexture.GetPixels32(0); 
		m_ColorHandle = GCHandle.Alloc(m_ColorArray, GCHandleType.Pinned);

		m_DepthTexture = new Texture2D(320, 240, TextureFormat.RGBA32, false);
		m_DepthArray = m_DepthTexture.GetPixels32();
		m_DepthHandle = GCHandle.Alloc(m_DepthArray, GCHandleType.Pinned);
#else
		m_ColorTexture = new Texture2D(1024, 512, TextureFormat.ARGB32, false);
		m_DepthTexture = new Texture2D(1024, 512, TextureFormat.ARGB32, false);		
#endif
		Manager.MaxPlayers = 2;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Update Kinect
		HRESULT hr = HRESULT.ThrowOnFailure(KinectInterface.GakUpdate());
		// if new data or work was done the GakUpdate call, it will return S_OK, then do the work with it
		// if nothing happened during the update, it will return S_FALSE and there is no need to waist cycles
		if (hr == HRESULT.S_OK)
		{
			GetSkeletons();
			GetColorStream();
			GetDepthStream();
		}	
	}

	private bool m_Disposed = false;
	
	void Dispose(bool disposing)
	{
		if (!this.m_Disposed)
	    {
	        if (disposing)
	        {
               //Release Managed Resources
	        }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            //Release Unmanaged Resources
            if (m_ColorHandle.IsAllocated)
            {
                m_ColorHandle.Free();
            }
            if (m_DepthHandle.IsAllocated)
            {
                m_DepthHandle.Free();
            }
#endif
	    }
	    m_Disposed = true;

	}
		
	/// <summary>
	/// Dispose unmanaged resources 
	/// </summary>
	public void Dispose()
	{		
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	
	/// <summary>
	/// Shut down the plugin 
	/// </summary>
	void OnApplicationQuit()
    {
  		HRESULT.ThrowOnFailure(KinectInterface.GakClose());
		Dispose();
    }
	
	/// <summary>
	/// Query the plugin for a new color frame 
	/// </summary>
	/// <returns>
	/// A Unity Texture containing the raw color data, also stored as m_ColorTexture <see cref="Texture"/>
	/// </returns>
	public Texture GetColorStream()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		HRESULT.ThrowOnFailure(KinectInterface.GakCopyImageDataToTexturePtr(
                m_ColorHandle.AddrOfPinnedObject(), 
                KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_COLOR_640x480));
		m_ColorTexture.SetPixels32(m_ColorArray);
		m_ColorTexture.Apply();
#else
		HRESULT.ThrowOnFailure(KinectInterface.GakCopyImageDataToTexturePtr(
			X360Core.GetRawTexture(m_ColorTexture),
			KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_COLOR_640x480));
#endif
		return m_ColorTexture;
	}
	
	/// <summary>
	/// Query the plugin for a new depth frame
	/// </summary>
	/// <returns>
	/// A Unity Texture containing the depth frame data, also stored as m_DepthTexture<see cref="Texture"/>
	/// </returns>
	public Texture GetDepthStream()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN	
		HRESULT.ThrowOnFailure(KinectInterface.GakCopyImageDataToTexturePtr(
                m_DepthHandle.AddrOfPinnedObject(), 
                KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_320x240));
		m_DepthTexture.SetPixels32(m_DepthArray);
		m_DepthTexture.Apply();
#else
		HRESULT.ThrowOnFailure(KinectInterface.GakCopyImageDataToTexturePtr(
			X360Core.GetRawTexture(m_DepthTexture),
			KinectInterface.NuiImageStream.NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_320x240));
#endif
		return m_DepthTexture;
	}
	
	/// <summary>
	/// Query the plugin for a new skeleton frame 
	/// </summary>
	public void GetSkeletons()
	{
		HRESULT.ThrowOnFailure(KinectInterface.GakGetSkeletonData(out this.frameData));
	}
	
	/// <summary>
	/// Gets the skeleton data for the first tracked skeleton 
	/// </summary>
	/// <param name="skeletonData">
	/// The skeleton data for the first tracked skeleton<see cref="KinectInterface.NUI_SKELETON_DATA"/>
	/// </param>
	/// <returns>
	/// True if a skeleton was found, false if there are no skeletons tracked <see cref="System.Boolean"/>
	/// </returns>
	public bool GetFirstTrackedSkeleton(out KinectInterface.NUI_SKELETON_DATA skeletonData)
    {
		skeletonData = new KinectInterface.NUI_SKELETON_DATA();

		if (frameData.SkeletonData != null)
        {
            for (int skeletonIndex = 0; skeletonIndex < KinectInterface.NUI_SKELETON_COUNT; skeletonIndex++)
            {
                if (frameData.SkeletonData[skeletonIndex].eTrackingState == KinectInterface.NUI_SKELETON_TRACKING_STATE.NUI_SKELETON_TRACKED)
                {
                    skeletonData = frameData.SkeletonData[skeletonIndex];
                    return true;
                }
            }
        }
        return false;
    }	
}
