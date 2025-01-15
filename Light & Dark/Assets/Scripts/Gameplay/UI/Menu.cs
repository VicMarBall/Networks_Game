using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{

    bool menuOpen = false;
    public GameObject menuScreen;
    public GameObject serverScreen;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!menuOpen) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menuScreen.SetActive(true);
                menuOpen = true;
                if (NetworkingEnd.instance.IsServer())
                {
                    serverScreen.SetActive(true);
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                menuOpen = false;
                if (NetworkingEnd.instance.IsServer())
                {
                    serverScreen.SetActive(false);
                }
                menuScreen.SetActive(false);
            }
        }
    }
}
