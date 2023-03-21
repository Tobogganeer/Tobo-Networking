using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Bob")]
    public float scale = 0.1f;
    public float vertMult = 1.5f;

    [Header("Rot")]
    public float rotAmount = 1f;

    [Space]
    public float smooth = 5f;

    void Update()
    {
        Bob();
        Rot();
    }

    void Bob()
    {
        Vector3 bob = new Vector3(Footsteps.SinValue, -Mathf.Abs(Footsteps.SinValue) * vertMult);
        bob *= scale;
        transform.localPosition = Vector3.Lerp(transform.localPosition, bob, Time.deltaTime * smooth);
    }

    void Rot()
    {
        Vector3 vel = Vector3.ClampMagnitude(PlayerMovement.LocalVelocity, 1f);
        Quaternion rot = Quaternion.Euler(0, 0, -vel.x * rotAmount);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rot, Time.deltaTime * smooth);
    }
}
