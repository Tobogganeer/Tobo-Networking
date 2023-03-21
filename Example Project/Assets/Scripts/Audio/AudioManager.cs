//#define MULTIPLAYER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MULTIPLAYER
using VirtualVoid.Net;
#endif

[RequireComponent(typeof(AudioMaster))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;

        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        Init();
    }

    private void Init()
    {
        clips.Clear();
        clipNameToIndex.Clear();

        int count = 0;

        for (int i = 0; i < singleClips.Length; i++)
        {
            clips.Add(singleClips[i]);
            if (clipNameToIndex.ContainsKey(clips[count].name))
                Debug.LogError("Duplicate single audio key: " + clips[count].name);
            clipNameToIndex.Add(clips[count].name, count++);
        }

        for (int i = 0; i < clipGroups.Length; i++)
        {
            int[] indices = new int[clipGroups[i].clips.Length];

            for (int j = 0; j < clipGroups[i].clips.Length; j++)
            {
                clips.Add(clipGroups[i].clips[j]);
                if (clipNameToIndex.ContainsKey(clips[count].name))
                    Debug.LogError("Duplicate group audio clip key: " + clips[count].name);
                clipNameToIndex.Add(clips[count].name, count);
                indices[j] = count++;
            }

            if (groupNameToIndices.ContainsKey(clipGroups[i].name))
                Debug.LogError("Duplicate group audio group key: " + clipGroups[i].name);
            groupNameToIndices.Add(clipGroups[i].name, indices);
        }
    }

    public AudioClip[] singleClips;
    public AudioClipGroup[] clipGroups;

    private static readonly List<AudioClip> clips = new List<AudioClip>();
    private static readonly Dictionary<string, int> clipNameToIndex = new Dictionary<string, int>();
    private static readonly Dictionary<string, int[]> groupNameToIndices = new Dictionary<string, int[]>();

    public static int GetClipIndex(string clipOrGroup)
    {
        if (groupNameToIndices.TryGetValue(clipOrGroup, out int[] indices))
            return indices[Random.Range(0, indices.Length)];
        else if (clipNameToIndex.TryGetValue(clipOrGroup, out int clip))
            return clip;
        else
        {
            Debug.LogWarning("Could not get clip for " + clipOrGroup);
            return -1;
        }
    }


    public static void Play(Audio audio)
    {
        // Send over network
#if MULTIPLAYER
        //AudioMessage message = new AudioMessage(audio);

        if (SteamManager.IsServer)
            ServerSend.SendAudio(audio, SteamManager.SteamID);
        else if (SteamManager.ConnectedToServer)
            ClientSend.SendAudio(audio);
#endif

        // Play on our side
        PlayLocal(audio);
    }

    public static void PlayLocal(Audio audio)
    {
        if (audio.ClipIndex < 0 || audio.ClipIndex >= clips.Count || clips[audio.ClipIndex] == null)
        {
            Debug.LogWarning($"Clip (index: {audio.ClipIndex}) was invalid or null.");
            return;
        }

        GameObject sourceObj = ObjectPoolManager.GetObject(PooledObject.AudioSource);
        if (sourceObj == null)
        {
            // Create pool
            Debug.Log("Creating audio source pool");
            ObjectPoolManager.CreatePool(PooledObject.AudioSource, AudioMaster.Instance.audioSourcePrefab, 16);
            sourceObj = ObjectPoolManager.GetObject(PooledObject.AudioSource);
        }

        if (audio.Parent != null && !audio.Parent.gameObject.activeInHierarchy)
        {
            // Parent is turned off
            Debug.Log($"Skipping audio played on disabled parent ({audio.Parent.name})");
            sourceObj.SetActive(false);
            return;
        }

        sourceObj.transform.SetParent(audio.Parent);
        sourceObj.transform.position = audio.Position;

        AudioSource source = sourceObj.GetComponent<AudioSource>();

        source.clip = clips[audio.ClipIndex];
        source.maxDistance = audio.MaxDistance;
        source.pitch = audio.Pitch;
        source.volume = audio.Volume;
        source.spatialBlend = audio.Flags.HasFlag(Audio.AudioFlags.Global) ? 0f : 1f; // 0 for 2d, 1 for 3d
        source.outputAudioMixerGroup = AudioMaster.GetGroup(audio.Category);
        source.Play();

        sourceObj.GetComponent<PooledAudioSource>().DisableAfterTime(source.clip.length / audio.Pitch + 0.25f); // 0.25 seconds extra for good measure
    }

    public static void OnNetworkAudio(Audio audio)
    {
        PlayLocal(audio);
    }


    public class Defaults
    {
        public const float Pitch = 1f;
        public const float MinPitch = 0.85f;
        public const float MaxPitch = 1.10f;
        public const float MaxDistance = 25f;
        public const float Volume = 1f;
        public const float _3dAmount = 1f;
        public const AudioCategory Category = AudioCategory.SFX;
    }

    [System.Serializable]
    public class AudioClipGroup
    {
        public string name;
        public AudioClip[] clips;
    }
}

public class Audio
#if MULTIPLAYER
    : INetworkMessage
#endif
{
    public int ClipIndex { get; private set; }
    public Vector3 Position { get; private set; }
    public Transform Parent { get; private set; }
    public float MaxDistance { get; private set; }
    public AudioCategory Category { get; private set; }
    public float Volume { get; private set; }
    public float Pitch { get; private set; }
    public float MinPitch { get; private set; }
    public float MaxPitch { get; private set; }

    public AudioFlags Flags { get; private set; }

    #region Constructors

    public Audio()
    {
        SetDefault();
    }

    public Audio(string clipOrGroup)
    {
        SetDefault();
        SetClip(clipOrGroup);
    }

    public Audio(int clipIndex)
    {
        SetDefault();
        SetClip(clipIndex);
    }

    #endregion

    #region Args

    public Audio SetDefault()
    {
        ClipIndex = -1;
        Position = Vector3.zero;
        Parent = null;
        MaxDistance = AudioManager.Defaults.MaxDistance;
        Category = AudioManager.Defaults.Category;
        Volume = AudioManager.Defaults.Volume;
        MinPitch = AudioManager.Defaults.MinPitch;
        MaxPitch = AudioManager.Defaults.MaxPitch;
        Pitch = Random.Range(MinPitch, MaxPitch);

        Flags = AudioFlags.None;

        return this;
    }

    public Audio SetClip(string clipOrGroup)
    {
        ClipIndex = AudioManager.GetClipIndex(clipOrGroup);
        return this;
    }

    public Audio SetClip(int clipIndex)
    {
        ClipIndex = clipIndex;
        return this;
    }

    public Audio SetClip(AudioClip clip)
    {
        SetClip(clip.name);
        return this;
    }

    public Audio SetPosition(Vector3 position)
    {
        Position = position;
        return this;
    }

    public Audio SetParent(Transform parent)
    {
        Parent = parent;
        if (parent != null)
            Flags |= AudioFlags.Parent;
        return this;
    }

    public Audio SetDistance(float maxDistance)
    {
        MaxDistance = maxDistance;
        if (maxDistance != AudioManager.Defaults.MaxDistance)
            Flags |= AudioFlags.Distance;
        return this;
    }

    public Audio SetVolume(float volume)
    {
        Volume = volume;
        if (volume != AudioManager.Defaults.Volume)
            Flags |= AudioFlags.Volume;
        return this;
    }

    public Audio SetPitch(float min, float max)
    {
        MinPitch = min;
        MaxPitch = max;
        Pitch = Random.Range(min, max);
        return this;
    }

    public Audio SetPitch(float pitch)
    {
        Pitch = pitch;
        return this;
    }

    public Audio SetCategory(AudioCategory category)
    {
        Category = category;
        if (category != AudioManager.Defaults.Category)
            Flags |= AudioFlags.Category;
        return this;
    }

    public Audio SetGlobal()
    {
        Flags |= AudioFlags.Global;
        return this;
    }

    public Audio Set2D()
    {
        return SetGlobal();
    }

    #endregion

    #region Net
#if MULTIPLAYER
    public void AddToMessage(Message message)
    {
        message.Add((byte)Flags);
        message.Add(ClipIndex);
        message.Add(Pitch);

        if (!Flags.HasFlag(AudioFlags.Global))
        {
            message.Add(Position);

            NetworkID netObj = Parent != null ? Parent.GetComponent<NetworkID>() : null;
            if (Flags.HasFlag(AudioFlags.Parent) && netObj != null)
                message.Add(netObj);

            if (Flags.HasFlag(AudioFlags.Distance))
                message.Add(MaxDistance);
        }

        if (Flags.HasFlag(AudioFlags.Volume))
            message.Add(Volume);

        if (Flags.HasFlag(AudioFlags.Category))
            message.Add((byte)Category);
    }

    public void Deserialize(Message message)
    {
        Flags = (AudioFlags)message.GetByte();
        SetClip(message.GetInt());
        SetPitch(message.GetFloat());

        if (!Flags.HasFlag(AudioFlags.Global))
        {
            SetPosition(message.GetVector3());

            if (Flags.HasFlag(AudioFlags.Parent))
                SetParent(message.GetNetworkID()?.transform);

            if (Flags.HasFlag(AudioFlags.Distance))
                SetDistance(message.GetFloat());
        }

        if (Flags.HasFlag(AudioFlags.Volume))
            SetVolume(message.GetFloat());

        if (Flags.HasFlag(AudioFlags.Category))
            SetCategory((AudioCategory)message.GetByte());
    }
#endif
#endregion

    [System.Flags]
    public enum AudioFlags : byte
    {
        None = 0,
        Global = 1 << 0,
        Parent = 1 << 1,
        Distance = 1 << 2,
        Volume = 1 << 3,
        //Pitch = 1 << 4,
        Category = 1 << 5,
    }
}

public enum AudioCategory : byte
{
    Master,
    SFX,
    Ambient
}
