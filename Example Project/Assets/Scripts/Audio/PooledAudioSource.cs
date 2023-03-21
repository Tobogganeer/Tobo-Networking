using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudioSource : MonoBehaviour
{
    private Transform originalParent;

    private void Start()
    {
        originalParent = transform.parent;
    }

    public void DisableAfterTime(float seconds)
    {
        if (!isActiveAndEnabled)
        {
            gameObject.SetActive(false);
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisableAfterSeconds(seconds));
    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        gameObject.SetActive(false);
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}
