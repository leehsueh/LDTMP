//-----------------------------------------------------------------------------
// PlayerManager.cs
//
// Managers Kinect-based players via Gak2Unity interface
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;


/// <summary>
/// Script that handles player enrollment 
/// </summary>
public class PlayerManager : MonoBehaviour
{
    #region Enums
    /// <summary>
    /// Enum for describing the current state of an enrolled player 
    /// </summary>
    public enum PlayerEnrollmentState
    {
        NotInPosition = 0,
        WaitingForHandRaise,
        Enrolled,
        NUM
    };

    /// <summary>
    /// Enum for describing the player's location 
    /// </summary>
    public enum OutOfPositionState
    {
        NotCentered = 0,
        TooClose,
        TooFar,
        Centered,
        NUM
    };

#endregion


    #region Structs
    /// <summary>
    /// Structure describing a player's state and skeleton 
    /// </summary>
    public struct EnrolledPlayer
    {
        public bool IsActive;
        public UInt32 PlayerID;
        public UInt32 SkeletonID;
        public PlayerEnrollmentState EnrollmentState;
        public OutOfPositionState PositionState;
    };
    #endregion

    /// <summary>
	/// Maximum number of players we want to track 
	/// </summary>
	private uint maxPlayers = 1;
	
	/// <summary>
	/// Number of players currently tracked 
	/// </summary>
	public uint TotalPlayers = 0;
	
	/// <summary>
	/// Array of managed players 
	/// </summary>
	private EnrolledPlayer[] m_Players = new EnrolledPlayer[KinectInterface.NUI_SKELETON_MAX_TRACKED_COUNT];
	
	/// <summary>
	/// Handle to the KinectManager class 
	/// </summary>
	private KinectManager Manager;
	
	/// <summary>
	/// Get the current players 
	/// </summary>
	public EnrolledPlayer[] Players
	{
		get{ return m_Players;}
	}
	
	/// <summary>
	/// Get/set the number of players we want to track 
	/// </summary>
	public uint MaxPlayers
	{
		get{ return maxPlayers;}
		set
		{ 
			if(value <= KinectInterface.NUI_SKELETON_MAX_TRACKED_COUNT)
			{
				maxPlayers = value;
                HRESULT.ThrowOnFailure(KinectInterface.GakSetPlayerMaxCount(maxPlayers));
			}
		}
	}
	
	/// <summary>
	/// Initialize player state 
	/// </summary>
	void Start () {
		
		for(int I = 0; I < KinectInterface.NUI_SKELETON_MAX_TRACKED_COUNT; ++I)
		{
			m_Players[I].IsActive = false;
			m_Players[I].EnrollmentState = PlayerEnrollmentState.NotInPosition;
			m_Players[I].PlayerID = 0;
		}
		
		TotalPlayers = 0;
		
	}
	
	/// <summary>
	/// Check to see if a player's skeleton is in an acceptable area to interact,
	/// updating their state as appropriate
	/// </summary>
	/// <param name="Player">
	/// A <see cref="EnrolledPlayer"/>
	/// </param>
	private void CheckForCenteredSkeleton(ref EnrolledPlayer Player)
	{
		if(SkeletonInView(ref Player))
		{
			Player.EnrollmentState = PlayerEnrollmentState.WaitingForHandRaise;
		}
	}
	
	/// <summary>
	/// Function that classifies the position of a skeleton as too far, too close, not 
	/// centered, or acceptable for play.
	/// </summary>
	/// <param name="Player">
	/// The player to check <see cref="EnrolledPlayer"/>
	/// </param>
	/// <returns>
	/// True if we deem the player to be in an acceptable area to interact <see cref="System.Boolean"/>
	/// </returns>
	bool SkeletonInView(ref EnrolledPlayer Player)
	{
		
		const float X_Min = -0.7f;
		const float X_Max = 0.7f;
		const float Z_Min = 1.0f;
		const float Z_Max = 3.0f;
		KinectInterface.Vector4 Position = Manager.frameData.SkeletonData[Player.SkeletonID].Position;
		
		if(Position.X < X_Min || Position.X > X_Max)
		{
			Player.PositionState = OutOfPositionState.NotCentered;
		}
	
		else if(Position.Z < Z_Min)
		{
			Player.PositionState = OutOfPositionState.TooClose;
		}
		else if(Position.Z > Z_Max)
		{
			Player.PositionState = OutOfPositionState.TooFar;
		}
	
		else
		{
			Player.PositionState = OutOfPositionState.Centered;
			return true;
		}
	
		return false;
	}
	
	/// <summary>
	/// Check if a player's hand is above their head
	/// </summary>
	/// <param name="Data">
	/// The player's skeleton data <see cref="KinectInterface.NUI_SKELETON_DATA"/>
	/// </param>
	/// <returns>
	/// True if a hand joint is above their head, false otherwise <see cref="System.Boolean"/>
	/// </returns>
	bool HandRaised(KinectInterface.NUI_SKELETON_DATA Data)
	{
		KinectInterface.Vector4 HeadPos = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HEAD];
		KinectInterface.Vector4 RightHand = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_RIGHT];
		KinectInterface.Vector4 LeftHand = Data.SkeletonPositions[(int)KinectInterface.NUI_SKELETON_POSITION_INDEX.NUI_SKELETON_POSITION_HAND_LEFT];
	
		return RightHand.Y > HeadPos.Y || LeftHand.Y > HeadPos.Y;
	}
	
	/// <summary>
	/// Check to see if a player's hand is raised, updating their enrollment state accordingly
	/// </summary>
	/// <param name="Player">
	/// The information of the player we're checking <see cref="EnrolledPlayer"/>
	/// </param>
	void CheckForHandRaise(ref EnrolledPlayer Player)
	{
		if(SkeletonInView(ref Player))
		{
			if(HandRaised(Manager.frameData.SkeletonData[Player.SkeletonID]))
			{
				Player.EnrollmentState = PlayerEnrollmentState.Enrolled;
			}
		}
		else
		{
			Player.EnrollmentState = PlayerEnrollmentState.NotInPosition;
		}
	}

	
	/// <summary>
	/// Poll the plugin for the current player state 
	/// </summary>
	void UpdatePlayerState()
	{
		KinectInterface.PlayerManagerState CurrentState = new KinectInterface.PlayerManagerState();
		HRESULT.ThrowOnFailure( KinectInterface.GakGetPlayerManagerState(out CurrentState) );
		
		TotalPlayers = 0;
		
		for(int playerIndex = 0; playerIndex < KinectInterface.NUI_SKELETON_MAX_TRACKED_COUNT; ++playerIndex)
		{
			if(m_Players[playerIndex].PlayerID != CurrentState.Players[playerIndex].PersonID)
			{
				if(CurrentState.Players[playerIndex].PersonID == 0) //Lost Player
				{
					m_Players[playerIndex].IsActive = false;
					m_Players[playerIndex].PlayerID = 0;
				}
				else //New player
				{
					m_Players[playerIndex].IsActive = true;
					m_Players[playerIndex].PlayerID = CurrentState.Players[playerIndex].PersonID;
					m_Players[playerIndex].SkeletonID = CurrentState.Players[playerIndex].SkeletonIndex;
					m_Players[playerIndex].EnrollmentState = PlayerEnrollmentState.NotInPosition;
					m_Players[playerIndex].PositionState = OutOfPositionState.NotCentered;
				}
			}
			
			if(m_Players[playerIndex].IsActive)
			{
				TotalPlayers++;
			}
		}
	}
	
	/// <summary>
	/// Poll the plugin for the current player state, run a simple enrollment state machine 
	/// </summary>
	void Update () {		
		
		UpdatePlayerState();
		
		if(!Manager)
		{
			Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		}
		
		for(int I = 0; I < TotalPlayers; ++I)
		{
			if(m_Players[I].IsActive)
			{
				switch(m_Players[I].EnrollmentState)
				{
				case PlayerEnrollmentState.NotInPosition:
					CheckForCenteredSkeleton(ref m_Players[I]);
					break;
					
				case PlayerEnrollmentState.WaitingForHandRaise:
					CheckForHandRaise(ref m_Players[I]);
					break;
					
				case PlayerEnrollmentState.Enrolled:
					break;
					
				default:
					break;
				}
			}

		}
	}
}
