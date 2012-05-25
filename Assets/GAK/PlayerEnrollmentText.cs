//-----------------------------------------------------------------------------
// PlayerEnrollmentText.cs
//
// Displays GUI text related to Kinect player enrollment
//
// Copyright (C) Microsoft. All rights reserved.
//-----------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;

public class PlayerEnrollmentText : MonoBehaviour 
{
	
	PlayerManager Players;
	KinectManager Manager;
	
	// Use this for initialization
	void Start () {
		if(!Players)
		{
			Players = (PlayerManager)FindObjectOfType(typeof(PlayerManager));
		}
		if(!Manager)
		{
			Manager = (KinectManager)FindObjectOfType(typeof(KinectManager));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		float TextX = 200;
		float TextY = 500;
		String str = "";
		for(int I = 0; I < Players.TotalPlayers; ++I)
		{
			str += "Player " + (I+1).ToString() + ": ";
			
			switch(Players.Players[I].EnrollmentState)
			{
			case PlayerManager.PlayerEnrollmentState.NotInPosition:
				switch(Players.Players[I].PositionState)
				{
				case PlayerManager.OutOfPositionState.NotCentered:
					str += "Please move to the center of the play area";
					break;
					
				case PlayerManager.OutOfPositionState.TooClose:
					str += "Please move back";
					break;
					
				case PlayerManager.OutOfPositionState.TooFar:
					str += "Please move forward";
					break;
				}
				break;
				
			case PlayerManager.PlayerEnrollmentState.WaitingForHandRaise:
				str += "Raise your hand above your head to start the game";
				break;
				
			case PlayerManager.PlayerEnrollmentState.Enrolled:
				str += "Enrolled";
				break;
			}
			str += "\n";
		}
		
		GUI.Label(new Rect(TextX,TextY, 500, 500), str);
	}
}
