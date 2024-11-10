using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GameplayEnvironmentMode
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

	GameplayEnvironmentMode gameplayEnvironmentMode;
}
