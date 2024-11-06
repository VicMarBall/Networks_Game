using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{

    Dictionary<int, GameObject> networkObjects;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DeserializeWelcomePacket(byte[] welcomePacket)
    {

    }

    // TO IMPLEMENT
    void UpdateNetworkObject(int netID, byte[] data)
    {

    }
}
