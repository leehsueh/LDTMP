using UnityEngine;
using System.Collections;

public class FallingObjectGameStatus : MonoBehaviour {
	public bool constrainZMovement;
	public int ballsToExplode = 3;
	private int ballsExploded = 0;
	public int gameDuration = 10;	// in interval units
	public int timeInterval = 1;	// 1 second interval
	private float lastUpdateTime = 0;
	private bool gameOn = false;
	
	public float leftBound, rightBound, topBound;
	
	// game objects
	private GameObject fallingObjectSpawner;
	public ChoiceFallingObjectSpawner twoObjectSpawner;
	private GameObject kinectPlayer;
	
	// UI stuff
	public GameObject counterText;
	public GameObject statusText;
	
	public void setup() {
		statusText.GetComponent<LabelFlash>().updateText("Stand still...");
		statusText.GetComponent<LabelFlash>().show(3f);
		twoObjectSpawner.spawnTwoProblem();
	}
	
	// Use this for initialization
	void Start () {
		fallingObjectSpawner = GameObject.Find("FallingObjectSpawner");
		fallingObjectSpawner.GetComponent<FallingObjectSpawner>().stopSpawning();
		twoObjectSpawner = (ChoiceFallingObjectSpawner)FindObjectOfType(typeof(ChoiceFallingObjectSpawner));
		
		kinectPlayer = GameObject.Find ("Player");
		if (constrainZMovement) {
			kinectPlayer.GetComponent<CalibratedNodeRoot>().ConstrainZMovement = constrainZMovement;
		}
		setup ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void incrementCount() {
		ballsExploded++;
	}
	
		
	void OnGUI() {
		// update status
		if (!gameOn && gameDuration > 0 && kinectPlayer.GetComponent<CalibratedNodeRoot>().isCalibrated()) {
			gameOn = true;
			statusText.GetComponent<LabelFlash>().updateText("GO!");
			statusText.GetComponent<LabelFlash>().show(3f);
			fallingObjectSpawner.GetComponent<FallingObjectSpawner>().startSpawning();
		}
		// update timer
		if (gameOn && gameDuration > 0) {
			
			float time = Time.time;
			if (lastUpdateTime == 0) lastUpdateTime = time;
			if (time - lastUpdateTime >= timeInterval) {
				gameDuration--;
				lastUpdateTime = time;
				
				counterText.guiText.text = gameDuration + " seconds";
			}
		} else if (gameDuration == 0) {
			fallingObjectSpawner.GetComponent<FallingObjectSpawner>().stopSpawning();
			GUIStyle style = new GUIStyle();
			style.fontSize = 64;
			style.alignment = TextAnchor.MiddleCenter;
			style.normal.textColor = Color.black;
			Vector2 labelSize = new Vector2(400, 150);
			GUI.Label(new Rect(Screen.width/2 - labelSize.x/2, 100, labelSize.x, labelSize.y), "Game Over!\n" + ballsExploded + " balls!", style);
			
			gameOn = false;
		}
		
	}
	
	public void checkCollision(Collider collider) {
		if (gameOn) {
			if (collider.gameObject.tag.Equals("FallingCube")) {
				//Color objColor = collider.gameObject.renderer.material.color;
				//print("Collision with " + objColor.ToString() + " cube!");
			}
			else if (collider.gameObject.tag.Equals("FaceSphere")) {
				//Color objColor = collider.gameObject.renderer.material.color;
				//print("Collision with " + objColor.ToString() + " sphere!");
				incrementCount();
			}
		}
	}
}
