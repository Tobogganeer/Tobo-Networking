using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tobo.Net;

[CustomEditor(typeof(NetworkManager), true)]
public class NetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //NetworkManager networkManager = (NetworkManager)target;

        if (GUILayout.Button("Host"))
            NetworkManager.Host("Host");
        if (GUILayout.Button("Join"))
            NetworkManager.Join("Client");
        if (GUILayout.Button("Leave"))
            NetworkManager.Disconnect();
    }
}
