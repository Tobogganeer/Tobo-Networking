using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Tobo.Net;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.SceneManagement;

public class NetworkTest : MonoBehaviour
{
    /*
    void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            Debug.Log("Host");
            NetworkManager.Host("Username weehee");
        }
        Scene scene = USceneManager.CreateScene("Test Scene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        PhysicsScene physicsScene = scene.GetPhysicsScene();
        //if (Keyboard.current.dKey.wasPressedThisFrame)
        //    NetworkManager.DisconnectClient();
        //if (Keyboard.current.rKey.wasPressedThisFrame)
        //    NetworkManager.Run();
    }
    */

    [ContextMenu("LoadServer")]
    public void LoadServer()
    {
        SceneInit.instance.SpawnServer();
        Invoke(nameof(Host), 1f);
    }

    [ContextMenu("LoadClient")]
    public void LoadClient()
    {
        SceneInit.instance.SpawnClient();
    }

    [ContextMenu("RemoveServer")]
    public void RemoveServer()
    {
        SceneInit.instance.RemoveServer();
    }

    [ContextMenu("RemoveClient")]
    public void RemoveClient()
    {
        SceneInit.instance.RemoveCurrentClient();
    }

    [ContextMenu("Host")]
    public void Host()
    {
        NetworkManager.Host("Host");
    }
}
