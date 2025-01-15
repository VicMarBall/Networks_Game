using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{

    bool menuOpen = false;
    public GameObject serverScreen;
    public GameObject clientScreen;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!menuOpen) {
                menuOpen = true;
                if (NetworkingEnd.instance.IsServer())
                {
                    serverScreen.SetActive(true);
                }
                else
                {
                    clientScreen.SetActive(true);
                } 
            }
            else
            {
                menuOpen = false;
                if (NetworkingEnd.instance.IsServer())
                {
                    serverScreen.SetActive(false);
                }
                else
                {
                    clientScreen.SetActive(false);
                }
            }
        }
    }
}
