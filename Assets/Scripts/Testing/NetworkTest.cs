using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Tobo.Net;

public class NetworkTest : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            Debug.Log("Host");
            NetworkManager.Host();
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
            NetworkManager.DisconnectClient();
        //if (Keyboard.current.rKey.wasPressedThisFrame)
        //    NetworkManager.Run();
    }
}
