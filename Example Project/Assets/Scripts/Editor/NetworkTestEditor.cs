using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkTest))]
public class NetworkTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NetworkTest obj = (NetworkTest)target;

        if (GUILayout.Button("Load Server"))
            obj.LoadServer();
        if (GUILayout.Button("Load Client"))
            obj.LoadClient();
        if (GUILayout.Button("Remove Server"))
            obj.RemoveServer();
        if (GUILayout.Button("Remove Client"))
            obj.RemoveClient();
        if (GUILayout.Button("Host"))
            obj.Host();
    }

    /*
    
    LoadServer()

    LoadClient()

    RemoveServer()

    RemoveClient()

    Host()

    */
}
