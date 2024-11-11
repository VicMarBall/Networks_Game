using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStarter : MonoBehaviour
{
	// called first
	void Awake()
	{
		Debug.Log("Awake");
	}

	// called second
	void OnEnable()
	{
		Debug.Log("OnEnable called");
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// called third
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("OnSceneLoaded: " + scene.name);
		Debug.Log(mode);
	}

	// called fourth
	void Start()
	{
		Debug.Log("Start");
		NetObjectsManager.instance.CreateLocalPlayer();
	}

	// called when the game is terminated
	void OnDisable()
	{
		Debug.Log("OnDisable");
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
