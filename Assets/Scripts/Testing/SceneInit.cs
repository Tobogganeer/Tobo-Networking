using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneInit : MonoBehaviour
{
    public static SceneInit instance;
    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        USceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SpawnClient()
    {
        LoadSceneParameters csp = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        Scene scene1 = USceneManager.LoadScene("ClientScene", csp);
    }
    public void SpawnServer()
    {
        LoadSceneParameters csp = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        Scene scene2 = USceneManager.LoadScene("ServerScene", csp);
    }

    public void RemoveServer()
    {
        USceneManager.UnloadSceneAsync("ServerScene");
    }

    public void RemoveCurrentClient()
    {
        if (USceneManager.GetActiveScene().name == "ClientScene")
        {
            USceneManager.UnloadSceneAsync(USceneManager.GetActiveScene());
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ClientScene")
        {
            USceneManager.SetActiveScene(scene);
            //  SceneManager.UnloadSceneAsync(gameObject.scene);
        }
    }
}