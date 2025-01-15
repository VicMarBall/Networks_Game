using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoUI : MonoBehaviour
{
    public TMP_Text PacketsSent;//
    public TMP_Text PacketsReceived;//
    public TMP_Text NetworkedObjects;//

    public TMP_Text PingInterval;//
    public TMP_Text DisconnectionTimeout;//
    public TMP_Text ClientAdress;//
    public TMP_Text ClientPort;//
    public TMP_Text ClientLastPacket; //no
    public TMP_Text ClientSinceReply;//


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PacketsSent.SetText(NetworkingEnd.instance.sentPacketCounter.ToString());
        PacketsReceived.SetText(NetworkingEnd.instance.receivedPacketCounter.ToString());
        NetworkedObjects.SetText(NetObjectsManager.instance.netObjects.Count.ToString());

        if (NetworkingEnd.instance.IsServer())
        {
            PingInterval.SetText(NetworkingEnd.instance.pingIntervalTime.ToString());
            DisconnectionTimeout.SetText(((Server)(NetworkingEnd.instance)).maxWaitingPongTime.ToString());
            //ClientAdress.SetText(((Server)(NetworkingEnd.instance)).userID.);
            //ClientPort.SetText();
            ClientSinceReply.SetText(((Server)(NetworkingEnd.instance)).usersLastPong.Values.ToString());
        }
    }
}
