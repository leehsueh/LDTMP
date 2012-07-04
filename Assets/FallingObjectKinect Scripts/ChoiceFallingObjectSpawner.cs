using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChoiceFallingObjectSpawner : MonoBehaviour {
	#region enums
	public enum Emotion {
		Happy, Sad, Angry, Surprised, Scared, Disgusted
	}
	#endregion
	
	private Texture[] faceTextures;
	
	// below arrays, make elements ordered by increasing degree
	public Texture[] happyTextures;
	public Texture[] sadTextures;
	public Texture[] angryTextures;
	public Texture[] scaredTextures;
	public Texture[] surprisedTextures;
	public Texture[] disgustedTextures;
	
	public Color[] colors = {Color.red, Color.blue, Color.green, Color.yellow, Color.gray, Color.magenta};
	public float drag;
	public float lowDrag;
	public GameObject facePrefab;
	public GameObject spherePrefab;
	public float yStartHeight;
	public float xLeftBound, xRightBound;
	private FallingObjectGameManager mManager;
	
	// Use this for initialization
	void Start () {
		mManager = (FallingObjectGameManager)FindObjectOfType(typeof(FallingObjectGameManager));
		
		List<Texture> allTextures = new List<Texture>();
		allTextures.AddRange(happyTextures);
		allTextures.AddRange(sadTextures);
		allTextures.AddRange(angryTextures);
		allTextures.AddRange(scaredTextures);
		allTextures.AddRange(surprisedTextures);
		allTextures.AddRange(disgustedTextures);
		faceTextures = allTextures.ToArray();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public GameObject[] spawnOneFaceOneSphere() {
		// generate a random face
		int index = Random.Range(0, faceTextures.Length);
		float xFace;
		float xSphere;
		
		if (Random.Range (0f,1f) < 0.5) {
			xFace = xLeftBound;
			xSphere = xRightBound;
		} else {
			xFace = xRightBound;
			xSphere = xLeftBound;
		}
		Vector3 position = new Vector3(xFace, yStartHeight, 0);
		GameObject faceObj = spawnPrefab(facePrefab, position);
		faceObj.renderer.material.mainTexture = faceTextures[index];
		faceObj.rigidbody.drag = drag;
		faceObj.tag = mManager.targetTag;
		
		// generate a random color sphere
		position.x = xSphere;
		GameObject sphereObj = spawnPrefab(spherePrefab, position);
		sphereObj.renderer.material.color = colors[Random.Range(0, colors.Length)];
		sphereObj.rigidbody.drag = drag;
				
		return new GameObject[] {sphereObj, faceObj};
	}
	
	public GameObject[] spawnTwoEmotions(Emotion targetEmotion) {
		// pick random emotion different from the target emotion
		Emotion randomEmotion;
		do {
			randomEmotion = pickRandomEmotion();
		} while (randomEmotion == targetEmotion);
		
		// get the textures and spawn the two face prefabs
		Texture randomTexture, targetTexture;
		randomTexture = randomTextureForEmotion(randomEmotion);
		targetTexture = randomTextureForEmotion(targetEmotion);
		
		Vector3 positionTarget, positionRandom;
		if (Random.Range (0f,1f) < 0.5) {
			positionTarget = new Vector3(xRightBound, yStartHeight, 0);
			positionRandom = new Vector3(xLeftBound, yStartHeight, 0);
		} else {
			positionTarget = new Vector3(xLeftBound, yStartHeight, 0);
			positionRandom = new Vector3(xRightBound, yStartHeight, 0);
		}
		GameObject targetObj = spawnFallingFace(facePrefab, positionTarget, targetTexture);		
		GameObject randomObj = spawnFallingFace(facePrefab, positionRandom, randomTexture);
		
		// set the tag of the target emotion object to be targetTag
		targetObj.tag = mManager.targetTag;
		
		return new GameObject[] {targetObj, randomObj};
	}
	
	public GameObject[] spawnTwoDegrees(int targetDegree) {
		GameObject targetObj, randomObj;
		Texture targetTexture, randomTexture;
		Vector3 positionTarget, positionRandom;
		Texture[] choices = choicesForEmotion(pickRandomEmotion());
		if (targetDegree == -1) {
			targetTexture = choices[0];
			randomTexture = choices[choices.Length-1];
		} else {
			targetTexture = choices[choices.Length-1];
			randomTexture = choices[0];
		}
		
		if (Random.Range (0f,1f) < 0.5) {
			positionTarget = new Vector3(xRightBound, yStartHeight, 0);
			positionRandom = new Vector3(xLeftBound, yStartHeight, 0);
		} else {
			positionTarget = new Vector3(xLeftBound, yStartHeight, 0);
			positionRandom = new Vector3(xRightBound, yStartHeight, 0);
		}
		
		targetObj = spawnFallingFace(facePrefab, positionTarget, targetTexture);
		targetObj.tag = mManager.targetTag;
		randomObj = spawnFallingFace(facePrefab, positionRandom, randomTexture);
		return new GameObject[] {targetObj, randomObj};
	}
	
	Texture randomTextureForEmotion(Emotion emotion) {
		Texture[] choices = choicesForEmotion(emotion);
		return choices[Random.Range(0, choices.Length)];
	}
	
	Texture[] choicesForEmotion(Emotion emotion) {
		switch (emotion) {
		case Emotion.Happy:
			return happyTextures;
		case Emotion.Sad:
			return sadTextures;
		case Emotion.Angry:
			return angryTextures;
		case Emotion.Surprised:
			return surprisedTextures;
		case Emotion.Scared:
			return scaredTextures;
		case Emotion.Disgusted:
			return disgustedTextures;
		default:
			return happyTextures;
		}
	}
	
	GameObject spawnPrefab(GameObject prefab, Vector3 position) {
		return (GameObject)Instantiate(prefab, position, facePrefab.transform.rotation);
	}
	
	GameObject spawnFallingFace(GameObject prefab, Vector3 position, Texture texture) {
		GameObject go = spawnPrefab(prefab, position);
		go.renderer.material.mainTexture = texture;
		go.rigidbody.drag = drag;
		return go;
	}
	
	public Emotion pickRandomEmotion() {
		System.Array values = System.Enum.GetValues(typeof(Emotion));
		System.Random random = new System.Random();
		Emotion randomEmotion = (Emotion)values.GetValue(random.Next(values.Length));
		return randomEmotion;
	}
}
