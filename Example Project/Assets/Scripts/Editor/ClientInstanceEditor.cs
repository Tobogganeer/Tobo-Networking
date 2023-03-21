using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tobo.Net;

[CustomEditor(typeof(ClientInstance))]
public class ClientInstanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ClientInstance clientInstance = (ClientInstance)target;

        if (GUILayout.Button("Join"))
            clientInstance.Join();
        if (GUILayout.Button("Leave"))
            clientInstance.Leave();
    }
}
