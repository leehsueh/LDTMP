using UnityEngine;
using System.Collections;

public class PlayerStatusDisplay : MonoBehaviour {
	public int playerNum;	// the player to track
	private CustomPlayerManager mPlayerManager;
	private CustomPlayerManager.EnrolledPlayer mPlayer;
	private uint mNumberOfBodies;
	
	public GUIStyle style;
	public Rect positionAndSize;
	
	// Use this for initialization
	void Start () {
		mPlayerManager = (CustomPlayerManager)FindObjectOfType(typeof(CustomPlayerManager));
		if (playerNum == 0) playerNum = 1;
	}
	
	// Update is called once per frame
	void Update () {
		mPlayer = playerNum == 1 ? mPlayerManager.Player1 : mPlayerManager.Player2;
		mNumberOfBodies = mPlayerManager.TotalPlayers;
		//print (mNumberOfBodies + " bodies");
	}
	
	void OnGUI() {
		//GUI.Box (new Rect (100,100,400,50), "Raise Both Hands to Go Back");
		string playerStatus = "<nothing>";
		CustomPlayerManager.PlayerEnrollmentState playerState = mPlayer.EnrollmentState;
		
		// show status message based on the state
		switch (playerState) {
		case CustomPlayerManager.PlayerEnrollmentState.Enrolled:
			playerStatus = "Player " + playerNum + " enrolled.";
			break;
		default:
			playerStatus = "Player " + playerNum + " lost.";
			break;
		}
		string statusMessage = mNumberOfBodies + " bodies, " + playerStatus;
		GUI.Box(positionAndSize, statusMessage, style);	
	}
}
