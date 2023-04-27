using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VisualLobby;

public class ConnectionTerminator : MonoBehaviour
{
    public void CallDisconnect()
    {
        AdditiveLevelsRoomClient.Disconnect();
    }
}
