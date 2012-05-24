//-----------------------------------------------------------------------------
// KinectInterface.cs
//
// Defines the types and methods needed to interface with Gak2Unity
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

public static class KinectInterface 
{
	
	#region Constants

    public static Guid GUID_SkeletonMirroringFilter = new Guid("{1ECF2087-BA49-4E7F-863B-937E5DBAF517}");
    public static Guid GUID_SkeletonSmoothingFilter = new Guid("{73D3D242-2BB9-47C7-BBA2-8DCCB89C55D5}");


	public const int NUI_SKELETON_COUNT = 6;
	public const int NUI_CAMERA_ELEVATION_MAXIMUM  = 27;
	public const int NUI_CAMERA_ELEVATION_MINIMUM = -27;
	public const int NUI_SKELETON_MAX_TRACKED_COUNT = 2;
	public const int PACK_BYTES = 8;
	
	private const int POS_COUNT = (int)NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_COUNT;
	
	#endregion

    #region Enums
    /// <summary>
    /// Enum for describing skeleton streams
    /// </summary>
    public enum NuiSkeletonStream
    {
	    NSM_SKELETON_STREAM_FULL,
	    NSM_SKELETON_STREAM_COUNT,
        NSM_SKELETON_STREAM_NONE // turn off skeleton stream
    };

    /// <summary>
	/// Enum for describing stream types
	/// </summary>
	public enum NuiImageStream
	{
		NSM_IMAGE_STREAM_DEPTH_640x480,
		NSM_IMAGE_STREAM_DEPTH_320x240,
	    NSM_IMAGE_STREAM_DEPTH_320x240_IN_COLOR_SPACE,
		NSM_IMAGE_STREAM_DEPTH_80x60,
		NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_640x480,
		NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_320x240,
	    NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_320x240_IN_COLOR_SPACE,
		NSM_IMAGE_STREAM_DEPTH_AND_PLAYERINDEX_80x60,
		NSM_IMAGE_STREAM_COLOR_640x480,
		NSM_IMAGE_STREAM_COLOR_IN_DEPTH_SPACE_640x480,
		NSM_IMAGE_STREAM_COLOR_YUV_640x480,
		NSM_IMAGE_STREAM_COUNT,
        NSM_IMAGE_STREAM_NONE // turn off image stream
	};
	
	/// <summary>
	/// Skeleton Joints
	/// </summary>
    public enum NUI_SKELETON_POSITION_INDEX
    {
        NUI_SKELETON_POSITION_HIP_CENTER,
        NUI_SKELETON_POSITION_SPINE,
        NUI_SKELETON_POSITION_SHOULDER_CENTER,
        NUI_SKELETON_POSITION_HEAD,
        NUI_SKELETON_POSITION_SHOULDER_LEFT,
        NUI_SKELETON_POSITION_ELBOW_LEFT,
        NUI_SKELETON_POSITION_WRIST_LEFT,
        NUI_SKELETON_POSITION_HAND_LEFT,
        NUI_SKELETON_POSITION_SHOULDER_RIGHT,
        NUI_SKELETON_POSITION_ELBOW_RIGHT,
        NUI_SKELETON_POSITION_WRIST_RIGHT,
        NUI_SKELETON_POSITION_HAND_RIGHT,
        NUI_SKELETON_POSITION_HIP_LEFT,
        NUI_SKELETON_POSITION_KNEE_LEFT,
        NUI_SKELETON_POSITION_ANKLE_LEFT,
        NUI_SKELETON_POSITION_FOOT_LEFT,
        NUI_SKELETON_POSITION_HIP_RIGHT,
        NUI_SKELETON_POSITION_KNEE_RIGHT,
        NUI_SKELETON_POSITION_ANKLE_RIGHT,
        NUI_SKELETON_POSITION_FOOT_RIGHT,
        NUI_SKELETON_POSITION_COUNT
    };
	
	/// <summary>
	/// Skeleton Tracking State 
	/// </summary>
	public enum NUI_SKELETON_TRACKING_STATE 
    {
        NUI_SKELETON_NOT_TRACKED = 0,
        NUI_SKELETON_POSITION_ONLY,
        NUI_SKELETON_TRACKED
    };
	
	/// <summary>
	/// Skeleton Position Tracking state 
	/// </summary>
    public enum NUI_SKELETON_POSITION_TRACKING_STATE 
    {
        NUI_SKELETON_POSITION_NOT_TRACKED = 0,
        NUI_SKELETON_POSITION_INFERRED,
        NUI_SKELETON_POSITION_TRACKED
    };
	
	/// <summary>
	/// Should match SkeletonDataType enum in KinectPlugin.h
	/// </summary>
	public enum SkeletonDataType 
	{
		KP_SKELETON_DATA_TYPE_UNFILTERED = 0,
		KP_SKELETON_DATA_TYPE_FILTERED,
		KP_SKELETON_DATA_TYPE_COUNT,
	};
    #endregion

    #region Structs
    /// <summary>
	/// Vector4 defined for NuiStream 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=PACK_BYTES)]
    public struct Vector4
    {	
		public float X;
        public float Y;
        public float Z;
        public float W;
    };
	
	/// <summary>
	/// Skeleton data 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=PACK_BYTES)]
    public struct NUI_SKELETON_DATA
    {
        public NUI_SKELETON_TRACKING_STATE eTrackingState;
        public UInt32 dwTrackingID;
        public UInt32 dwEnrollmentIndex;
        public UInt32 dwUserIndex;
        public Vector4 Position;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = POS_COUNT)]
        public Vector4[] SkeletonPositions;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = POS_COUNT)]
        public NUI_SKELETON_POSITION_TRACKING_STATE[] eSkeletonPositionTrackingState;

        public UInt32 dwQualityFlags;

    };
	
	/// <summary>
	/// Skeleton frame data 
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=PACK_BYTES)]
    public struct NUI_SKELETON_FRAME
    {		
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public UInt32 liTimeStampLowPart;
        public Int32 liTimeStampHighPart;
#else
        public Int32 liTimeStampHighPart;
        public UInt32 liTimeStampLowPart;
#endif
        public UInt32 dwFrameNumber;
        public UInt32 dwFlags;
        public Vector4 vFloorClipPlane;
        public Vector4 vNormalToGravity;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = NUI_SKELETON_COUNT)]
        public NUI_SKELETON_DATA[] SkeletonData;
    };
	

	/// <summary>
	///Managed Player info 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=PACK_BYTES)]
	public struct ManagedPlayer
	{
		public UInt32 PersonID;
		public UInt32 SkeletonIndex;
	};
	
	/// <summary>
	/// Data returned from GAKGetPlayerManagerState 
	/// </summary>
	public struct PlayerManagerState
	{
		[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = NUI_SKELETON_MAX_TRACKED_COUNT)]
		public ManagedPlayer[] Players;
	};
    #endregion

    #region pInvoke Layer
    [DllImport("Gak2Unity")]
    public static extern uint GakInit(
        uint framesPerSecond,
        NuiSkeletonStream skeletonStream,
        NuiImageStream colorStream,
        NuiImageStream depthStream,
        bool startStreams);
	
    [DllImport("Gak2Unity")]
    public static extern uint GakUpdate();
	
    [DllImport("Gak2Unity")]
	public static extern uint GakClose();

    [DllImport("Gak2Unity")]
    public static extern uint GakSetStreamsPaused(bool isPaused);
	
	[DllImport("Gak2Unity")]
	public static extern uint GakCopyImageDataToTexturePtr(IntPtr TextureMemory, NuiImageStream dataStreamType);
	
	[DllImport("Gak2Unity")]
    public static extern uint GakGetSkeletonData(out NUI_SKELETON_FRAME skeletonFrameData);

    [DllImport("Gak2Unity")]
    public static extern uint GakAddFilter(string filterId, bool isEnabled, float priority, out uint handle);

    [DllImport("Gak2Unity")]
    public static extern uint GakRemoveFilter(uint handle);

    [DllImport("Gak2Unity")]
    public static extern uint GakGetFilterEnabled(uint handle, out bool isEnabled);

    [DllImport("Gak2Unity")]
    public static extern uint GakSetFilterEnabled(uint handle, bool isEnabled);
	
    [DllImport("Gak2Unity")]
    public static extern uint GakSetPlayerMaxCount(uint count);
	
	[DllImport("Gak2Unity")]
	public static extern uint GakGetPlayerManagerState(out PlayerManagerState State);
    #endregion

}
