using System;
using System.Collections.Generic;
using UnityEngine;

//namespace VirtualVoid.Util.ObjectPooling
//{
public abstract class PoolObject : MonoBehaviour
{
    /// <summary>
    /// Called when the object has been spawned from the object pool. Called after the object is set active.
    /// </summary>
    public abstract void OnObjectSpawn();

    /// <summary>
    /// Calling this will stop all processes on the object and return it to the pool. Call instead of Destroy();
    /// </summary>
    public void DestroyPoolObject()
    {
        gameObject.SetActive(false);
        StopAllCoroutines();
        CancelInvoke();
    }
}
//}
