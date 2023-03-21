using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOneShot : MonoBehaviour
{
    public string clip;
    public float range = AudioManager.Defaults.MaxDistance;
    public AudioCategory category = AudioManager.Defaults.Category;
    public float volume = AudioManager.Defaults.Volume;
    public float minPitch = AudioManager.Defaults.MinPitch;
    public float maxPitch = AudioManager.Defaults.MaxPitch;

    public bool parentToThis = false;

    private void Start()
    {
        Transform parent = parentToThis ? transform : null;
        if (AudioManager.GetClipIndex(clip) != -1)
            AudioManager.Play(new Audio(clip).SetDistance(range).SetCategory(category).SetVolume(volume).SetPitch(minPitch, maxPitch).SetParent(parent));
    }
}
