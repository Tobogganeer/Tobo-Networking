using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public static PopUp instance;
    private void Awake()
    {
        instance = this;
        transform.SetParent(null);
    }

    public TMPro.TMP_Text text;

    public static void Show(string message, float time = 3)
    {
        instance.text.text = message;
        instance.CancelInvoke();
        instance.Invoke(nameof(Cancel), time);
        //Debug.Log(message);
    }

    private void Cancel()
    {
        text.text = "";
    }
}
