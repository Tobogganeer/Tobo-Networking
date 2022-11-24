using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

    // THIS ITSELF RIPPED FROM VirtualVoid.Net / VVSteamNetworking
    // Most of this code is ripped straight from Mirror. https://github.com/vis2k/Mirror
    // The people working on it are smarter than I am and they have figured out all this complex stuff so :/

[DisallowMultipleComponent]
public class NetID : MonoBehaviour
{
    /*
    private bool copyOfSceneObj;
    [SerializeField, HideInInspector] private bool hasSpawned;
    public uint netID; //{ get; private set; }
    public uint sceneID; //{ get; private set; }

    private bool destroyed;

    internal static readonly Dictionary<uint, NetID> networkIDs = new Dictionary<uint, NetID>();
    internal static readonly Dictionary<uint, NetID> sceneIDs = new Dictionary<uint, NetID>();

    private static uint nextNetworkId = 1;
    internal static uint NextNetID() => nextNetworkId++;

    public static void ResetNetIDs()
    {
        nextNetworkId = 1;
        networkIDs.Clear();
    }

    public Guid assetID
    {
        get
        {
#if UNITY_EDITOR
            // This is important because sometimes OnValidate does not run (like when adding view to prefab with no child links)
            if (string.IsNullOrEmpty(assetIDString))
                AssignIDs();
#endif
            // convert string to Guid and use .Empty to avoid exception if
            // we would use 'new Guid("")'
            return string.IsNullOrEmpty(assetIDString) ? Guid.Empty : new Guid(assetIDString);
        }
        internal set
        {
            string newAssetIdString = value == Guid.Empty ? string.Empty : value.ToString("N");
            string oldAssetIdSrting = assetIDString;

            // they are the same, do nothing
            if (oldAssetIdSrting == newAssetIdString)
            {
                return;
            }

            // new is empty
            if (string.IsNullOrEmpty(newAssetIdString))
            {
                Debug.LogError($"Can not set AssetId to empty guid on NetworkID '{name}', old assetId '{oldAssetIdSrting}'");
                return;
            }

            // old not empty
            if (!string.IsNullOrEmpty(oldAssetIdSrting))
            {
                Debug.LogError($"Can not Set AssetId on NetworkIdentity '{name}' because it already had an assetId, current assetId '{oldAssetIdSrting}', attempted new assetId '{newAssetIdString}'");
                return;
            }

            // old is empty
            assetIDString = newAssetIdString;
            // Debug.Log($"Settings AssetId on NetworkIdentity '{name}', new assetId '{newAssetIdString}'");
        }
    }
    [SerializeField, HideInInspector] string assetIDString;

    public static bool IsServer
    {
        get
        {
            return NetworkManager.IsServer;
        }
    }

    private void Awake()
    {
        if (hasSpawned)
        {
            Debug.LogError($"{name} has already spawned. Don't call Instantiate for NetworkIDs that were in the scene since the beginning (aka scene objects). Destroying...");
            copyOfSceneObj = true;
            Destroy(gameObject);

            return;
        }
        hasSpawned = true;

        //SteamManager.OnAllClientsSceneLoaded += SteamManager_OnAllClientsSceneLoaded;
        sceneIDs[sceneID] = this;
    }

    private void SteamManager_OnAllClientsSceneLoaded()
    {
        if (!IsServer) return;

        if (netID == 0)
        {
            netID = NextNetID();

            networkIDs[netID] = this;

            //Debug.Log("Spawning " + gameObject.name + " after all clients loaded.");
            SteamManager.SpawnObject(this);
        }
    }

    private void Start()
    {
        if (!IsServer || sceneID != 0) return;
        // Only spawn if runtime ID, ie sceneID is 0

        if (netID == 0)
        {
            netID = NextNetID();

            networkIDs[netID] = this;

            //Debug.Log("Spawning " + gameObject.name + " after in start. SceneID: " + sceneID);
            SteamManager.SpawnObject(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            Debug.Log("Num scene ID " + sceneIDs.Count);
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        hasSpawned = false; // OnValidate not called from Instantiate()

#if UNITY_EDITOR
        AssignIDs();
#endif
    }

#if UNITY_EDITOR
    private void AssignAssetID(string path) => assetIDString = AssetDatabase.AssetPathToGUID(path);
    private void AssignAssetID(GameObject prefab) => AssignAssetID(AssetDatabase.GetAssetPath(prefab));

    private void AssignIDs()
    {
        if (Util.IsGameObjectPrefab(gameObject))
        {
            // force 0 for prefabs
            sceneID = 0;
            AssignAssetID(gameObject);
        }

        else if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null)
            {
                // force 0 for prefabs
                sceneID = 0;

                string path = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().assetPath;

                AssignAssetID(path);
            }
        }
        else if (Util.IsSceneObjectWithPrefabParent(gameObject, out GameObject prefab))
        {
            AssignSceneID();
            AssignAssetID(prefab);
        }
        else
        {
            AssignSceneID();

            if (!EditorApplication.isPlaying)
            {
                assetIDString = "";
            }
        }
    }

    private void AssignSceneID()
    {
        bool duplicate = sceneIDs.TryGetValue(sceneID, out NetID existing) && existing != null && existing != this;

        if (sceneID == 0 || duplicate)
        {
            sceneID = 0;

            if (BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Scene " + gameObject.scene.path + " needs to be opened and resaved before building, because the scene object " + name + " has no valid sceneId yet.");

            Undo.RecordObject(this, "Generated SceneID");

            uint newID = Util.GetRandomUInt();

            duplicate = sceneIDs.TryGetValue(newID, out existing) && existing != null && existing != this;

            if (!duplicate)
            {
                sceneID = newID;
            }
            else
            {
                Debug.LogWarning("Retry sceneID for " + name + " as duplicated twice");
            }
        }

        sceneIDs[sceneID] = this;
    }
#endif

    void OnDestroy()
    {
        if (copyOfSceneObj)
            return;

        if (IsServer && !destroyed)
        {
            SteamManager.OnAllClientsSceneLoaded -= SteamManager_OnAllClientsSceneLoaded;
            SteamManager.DestroyObject(this);
            destroyed = true;
        }

        if (networkIDs.ContainsKey(netID)) networkIDs.Remove(netID);
    }

    [ContextMenu("Log IDs")]
    public void LogIDs()
    {
        Debug.Log($"IDs for GameObject {name}\n-SceneID: {sceneID}\n-AssetID: {assetID}\n-NetID: {netID}");
    }
    */
}
