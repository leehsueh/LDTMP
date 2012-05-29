using UnityEngine;
using System.Collections;

public class BoxFlash : MonoBehaviour {
	public Texture2D backgroundColor;
	private Vector2 center;
	public Vector2 Center {
		set {center = value;}
	}
	private Vector2 widthHeight;
	public Vector2 WidthHeight {
		set {
			widthHeight = value;
			backgroundColor = new Texture2D((int)widthHeight.x, (int)widthHeight.y);
			for (int y = 0; y < backgroundColor.height; ++y)
	        {
	            for (int x = 0; x < backgroundColor.width; ++x)
	            {
	                //float r = Random.value;
					float r = 0.95f;
	                Color color = new Color(r, r, r, 0.8f);
	                backgroundColor.SetPixel(x, y, color);
	            }
	        }
	        backgroundColor.Apply();
		}
	}
	
	private Texture2D tex;
	public Texture2D Image {
		get { return tex; }
		set { tex = value; }
	}
	private string message;
	public string Message {
		get { return message; }
		set { message = value; }
	}
	
	private float duration;
	private float time;
	public float Duration {
		get { return duration; }
		set { duration = value > 0 ? value : 0; }
	}

	public void show() {
		time = Time.time;
	}
	
	void Start() {
		backgroundColor = new Texture2D((int)widthHeight.x, (int)widthHeight.y);
		for (int y = 0; y < backgroundColor.height; ++y)
        {
            for (int x = 0; x < backgroundColor.width; ++x)
            {
                //float r = Random.value;
				float r = 0.95f;
                Color color = new Color(r, r, r, 0.8f);
                backgroundColor.SetPixel(x, y, color);
            }
        }
        backgroundColor.Apply();
	}
	
    void OnGUI() {
        if (Time.time - time <= duration) {
	        GUIContent content = new GUIContent();
			content.text = message;
			content.image = tex;
			GUIStyle style = new GUIStyle();
			
			style.normal.background = backgroundColor;
			style.imagePosition = ImagePosition.ImageAbove;
			style.alignment = TextAnchor.MiddleCenter;
			style.fontSize = 32;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			//style.wordWrap = true;
			style.margin = new RectOffset(2, 2, 2, 2);
			style.padding = new RectOffset(5, 5, 5, 5);
			//style.stretchWidth = true;
			//style.stretchHeight = true;
			GUI.Box(new Rect(center.x - widthHeight.x/2, center.y - widthHeight.y/2, widthHeight.x, widthHeight.y), content, style);
		}
	}
}
