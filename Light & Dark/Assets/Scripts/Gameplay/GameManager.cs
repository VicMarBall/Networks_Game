using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameplayEnvironmentMode
{
	LIGHT,
	DARK
}

public class GameManager : MonoBehaviour
{
	// singleton
	public static GameManager instance { get; private set; }

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public GameplayEnvironmentMode gameplayEnvironmentMode { get; private set; }
}
