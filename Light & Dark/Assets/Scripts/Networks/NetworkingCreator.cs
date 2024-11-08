using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingCreator : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject serverPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateClient()
    {
        GameObject client = Instantiate(clientPrefab);

        DontDestroyOnLoad(client);

        SceneManager.LoadScene("Playtesting");
    }

	public void CreateServer()
	{
		GameObject server = Instantiate(serverPrefab);

		DontDestroyOnLoad(server);

		SceneManager.LoadScene("Playtesting");
	}

}
