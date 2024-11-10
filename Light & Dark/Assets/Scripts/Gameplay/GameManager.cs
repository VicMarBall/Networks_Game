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

	public int playerID {  get; private set; }
	public void SetPlayerID(int playerID) { this.playerID = playerID; }

	public GameplayEnvironmentMode gameplayEnvironmentMode { get; private set; }
}
