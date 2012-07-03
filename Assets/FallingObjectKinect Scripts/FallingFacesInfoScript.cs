using UnityEngine;
using System.Collections;

public class FallingFacesInfoScript : MonoBehaviour {
	private int levelSelected = -1;
	public int LevelSelected {
		get	{ return levelSelected; }
		set { levelSelected = value; }
	}
	
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
