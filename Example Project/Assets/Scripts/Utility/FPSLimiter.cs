using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int target = 144;
    [Range(0, 2)]
    public int vsync = 0;

    void Start()
    {
	    QualitySettings.vSyncCount = vsync;
        Application.targetFrameRate = target;
    }
}
