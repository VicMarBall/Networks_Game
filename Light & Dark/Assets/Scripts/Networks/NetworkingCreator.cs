using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkingCreator : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject serverPrefab;

    [SerializeField] TMP_InputField ipInputField;

    public void CreateClient()
    {
        GameObject client = Instantiate(clientPrefab);
        DontDestroyOnLoad(client);

        if (ipInputField.text != "")
        {
			client.GetComponent<Client>().StartClient(ipInputField.text);
		}
		else
		{
			client.GetComponent<Client>().StartClient();
		}

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
