using UnityEngine;
using System.Collections;

public class LabelFlash : MonoBehaviour {
	public float displayTime;
	private int timeLeft;
	private int interval = 1;
	private float lastUpdateTime;
	private string text;
	private bool textShowing = false;
	
	// Use this for initialization
	void Start () {
		gameObject.guiText.text = "";
		gameObject.guiText.font.material.color = Color.black;
	}
	
	// Update is called once per frame
	void OnGUI () {
		if (textShowing && timeLeft > 0) {
			float time = Time.time;
			if (time - lastUpdateTime >= interval) {
				timeLeft--;
				lastUpdateTime = time;
				if (timeLeft == 0) {
					gameObject.guiText.text = "";
					textShowing = false;
				}
			}
		}
	}
	
	public void show(float duration) {
		gameObject.guiText.text = text;
		displayTime = duration;
		timeLeft = (int)displayTime;
		textShowing = true;
		lastUpdateTime = Time.time;
	}
	
	public void updateText(string newText) {
		text = newText;	
	}
	
	public void setStyle(FontStyle style) {
		gameObject.guiText.fontStyle = style;
	}
}
