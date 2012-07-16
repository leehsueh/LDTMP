using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChoiceFallingObjectSpawner : MonoBehaviour {
	#region enums
	public enum Emotion {
		Happy, Sad, Angry, Scared, Disgusted
	}
	#endregion
	
	private Texture[] faceTextures;
	
	// below arrays, make elements ordered by increasing degree
	public Texture[] happyTextures;
	public Texture[] sadTextures;
	public Texture[] angryTextures;
	public Texture[] scaredTextures;
	public Texture[] disgustedTextures;
	
	public Texture[] bwhappyTextures;
	public Texture[] bwsadTextures;
	public Texture[] bwangryTextures;
	public Texture[] bwscaredTextures;
	public Texture[] bwdisgustedTextures;
	
	// below arrays
	private Vector3[] happyRotations = new Vector3[] {
		new Vector3(7.124344f, -125.8637f, 9.476044f),
		new Vector3(8.238678f, -132.9525f, 8.527267f),
		new Vector3(8.996735f, -123.6133f, 14.07755f)
	};
	private Vector3[] sadRotations = new Vector3[] {
		new Vector3(7.124344f, -125.8637f, 9.476044f),
		new Vector3(8.945145f, -135.3699f, 9.804001f),
		new Vector3(6.116547f, -134.9838f, 6.706696f)
	};
	private Vector3[] angryRotations = new Vector3[] {
		new Vector3(9.627121f, -139.5111f, 9.135483f),
		new Vector3(8.238678f, -132.9525f, 8.527267f),
		new Vector3(8.178421f, -147.9693f, 5.410385f)
	};
	private Vector3[] scaredRotations = new Vector3[] {
		new Vector3(-5.41156f, -125.8095f, 17.18231f),
		new Vector3(6.675919f, -122.3968f, 6.495911f),
		new Vector3(6.675919f, -122.3968f, 6.495911f)
	};
	private Vector3[] disgustedRotations = new Vector3[] {
		new Vector3(4.141586f, -116.4201f, 8.885071f),
		new Vector3(6.706161f, -134.5473f, 7.156906f),
		new Vector3(8.045425f, -125.8485f, 10.30664f)
	};
	
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
		allTextures.AddRange(disgustedTextures);
		faceTextures = allTextures.ToArray();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public GameObject[] spawnOneFaceOneSphere() {
		// generate a random face
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
		// pick random emotion different from the target emotion
		Emotion randomEmotion = pickRandomEmotion();
		
		// get the textures and spawn the two face prefabs
		Texture randomTexture;
		Vector3 randomRotation;
		Texture[] randomChoices = choicesForEmotion(randomEmotion, false);
		int randomIndex = Random.Range(0, randomChoices.Length);
		randomTexture = randomChoices[randomIndex];
		randomRotation = rotationsForEmotion(randomEmotion)[randomIndex];
		GameObject faceObj = spawnFallingFace(facePrefab, position, randomTexture, randomRotation);
		faceObj.tag = mManager.targetTag;
		
		// generate a random color sphere
		position.x = xSphere;
		GameObject sphereObj = spawnPrefab(spherePrefab, position);
		sphereObj.renderer.material.color = colors[Random.Range(0, colors.Length)];
		sphereObj.rigidbody.drag = drag;
				
		return new GameObject[] {sphereObj, faceObj};
	}
	
	public GameObject[] spawnTwoEmotions(Emotion targetEmotion, bool bw) {
		// pick random emotion different from the target emotion
		Emotion randomEmotion;
		do {
			randomEmotion = pickRandomEmotion();
		} while (randomEmotion == targetEmotion);
		
		// get the textures and spawn the two face prefabs
		Texture randomTexture, targetTexture;
		Vector3 randomRotation, targetRotation;
		Texture[] randomChoices = choicesForEmotion(randomEmotion, bw);
		Texture[] targetChoices = choicesForEmotion(targetEmotion, bw);
		int randomIndex = Random.Range(0, randomChoices.Length);
		int targetIndex = Random.Range(0, targetChoices.Length);
		randomTexture = randomChoices[randomIndex];
		targetTexture = targetChoices[targetIndex];
		randomRotation = rotationsForEmotion(randomEmotion)[randomIndex];
		targetRotation = rotationsForEmotion(targetEmotion)[targetIndex];
		
		Vector3 positionTarget, positionRandom;
		if (Random.Range (0f,1f) < 0.5) {
			positionTarget = new Vector3(xRightBound, yStartHeight, 0);
			positionRandom = new Vector3(xLeftBound, yStartHeight, 0);
		} else {
			positionTarget = new Vector3(xLeftBound, yStartHeight, 0);
			positionRandom = new Vector3(xRightBound, yStartHeight, 0);
		}
		GameObject targetObj = spawnFallingFace(facePrefab, positionTarget, targetTexture, targetRotation);		
		GameObject randomObj = spawnFallingFace(facePrefab, positionRandom, randomTexture, randomRotation);
		targetObj.renderer.material.color = Color.white;
		randomObj.renderer.material.color = Color.white;
		
		// set the tag of the target emotion object to be targetTag
		targetObj.tag = mManager.targetTag;
		
		return new GameObject[] {targetObj, randomObj};
	}
	
	public GameObject[] spawnTwoDegrees(int targetDegree) {
		GameObject targetObj, randomObj;
		Texture targetTexture, randomTexture;
		Vector3 positionTarget, positionRandom;
		Vector3 targetRotation, randomRotation;
		Emotion randomEmotion = pickRandomEmotion();
		Texture[] choices = choicesForEmotion(randomEmotion, false);
		Vector3[] rotations = rotationsForEmotion(randomEmotion);
		if (targetDegree == -1) {
			targetTexture = choices[0];
			targetRotation = rotations[0];
			randomTexture = choices[choices.Length-1];
			randomRotation = rotations[rotations.Length-1];
		} else {
			targetTexture = choices[choices.Length-1];
			targetRotation = rotations[rotations.Length-1];
			randomTexture = choices[0];
			randomRotation = rotations[0];
		}
		
		if (Random.Range (0f,1f) < 0.5) {
			positionTarget = new Vector3(xRightBound, yStartHeight, 0);
			positionRandom = new Vector3(xLeftBound, yStartHeight, 0);
		} else {
			positionTarget = new Vector3(xLeftBound, yStartHeight, 0);
			positionRandom = new Vector3(xRightBound, yStartHeight, 0);
		}
		
		targetObj = spawnFallingFace(facePrefab, positionTarget, targetTexture, targetRotation);
		targetObj.tag = mManager.targetTag;
		randomObj = spawnFallingFace(facePrefab, positionRandom, randomTexture, randomRotation);
		return new GameObject[] {targetObj, randomObj};
	}
	
	Texture[] choicesForEmotion(Emotion emotion, bool bw) {
		switch (emotion) {
		case Emotion.Happy:
			if (bw) return bwhappyTextures;
			return happyTextures;
		case Emotion.Sad:
			if (bw) return bwsadTextures;
			return sadTextures;
		case Emotion.Angry:
			if (bw) return bwangryTextures;
			return angryTextures;
		case Emotion.Scared:
			if (bw) return bwscaredTextures;
			return scaredTextures;
		case Emotion.Disgusted:
			if (bw) return bwdisgustedTextures;
			return disgustedTextures;
		default:
			return happyTextures;
		}
	}
	
	Vector3[] rotationsForEmotion(Emotion emotion) {
		switch (emotion) {
		case Emotion.Happy:
			return happyRotations;
		case Emotion.Sad:
			return sadRotations;
		case Emotion.Angry:
			return angryRotations;
		case Emotion.Scared:
			return scaredRotations;
		case Emotion.Disgusted:
			return disgustedRotations;
		default:
			return happyRotations;
		}
	}
	
	GameObject spawnPrefab(GameObject prefab, Vector3 position) {
		return (GameObject)Instantiate(prefab, position, facePrefab.transform.rotation);
	}
	
	GameObject spawnFallingFace(GameObject prefab, Vector3 position, Texture texture, Vector3 rotation) {
		GameObject go = spawnPrefab(prefab, position);
		go.renderer.material.mainTexture = texture;
		go.transform.rotation = Quaternion.Euler(rotation);
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
