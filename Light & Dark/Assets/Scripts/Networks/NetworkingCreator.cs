using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkingCreator : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject serverPrefab;

    public void CreateClient()
    {
        GameObject client = Instantiate(clientPrefab);

        DontDestroyOnLoad(client);

        client.GetComponent<Client>().StartClient();

        SceneManager.LoadScene("Playtesting");
    }

	public void CreateServer()
	{
		GameObject server = Instantiate(serverPrefab);
		DontDestroyOnLoad(server);

		server.GetComponent<Server>().StartServer();

		SceneManager.LoadScene("Playtesting");
	}

}
