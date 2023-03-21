using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public static Footsteps instance;
    private void Awake()
    {
        instance = this;
    }

    public float stepSpeed = 2.5f;
    public GameObject smokeParticles;
    public GameObject dropParticles;
    public Transform footSource;

    public static Foot foot;
    public static float SinValue;
    private const float MIN_AIRTIME = 0.2f;
    public static event System.Action<Foot, float> OnFootstep;

    float time;

    private void OnEnable()
    {
        PlayerMovement.OnLand += PlayerMovement_OnLand;
    }

    private void OnDisable()
    {
        PlayerMovement.OnLand -= PlayerMovement_OnLand;
    }

    private void Footstep(Foot foot, float magnitude)
    {
        OnFootstep?.Invoke(foot, magnitude);

        float vol = 0.65f;
        float range = 25f;

        if (smokeParticles)
            Instantiate(smokeParticles, footSource.position, Quaternion.identity);
        AudioManager.Play(new Audio(GetSound(foot)).SetPosition(footSource.position).SetDistance(range).SetVolume(vol));
    }

    private void PlayerMovement_OnLand(float airtime)
    {
        if (airtime > MIN_AIRTIME)
        { 
            AudioManager.Play(new Audio("Drop").SetPosition(footSource.position).SetVolume(Mathf.Clamp01(airtime * 0.6f)));
            if (dropParticles)
                Instantiate(dropParticles, footSource.position, Quaternion.identity);
        }
            //AudioManager.Play(AudioArray.Drop, footSource.position, null, 35, AudioCategory.SFX, Mathf.Clamp01(airtime * 0.6f));
    }

    private string GetSound(Foot foot)
    {
        if (PlayerMovement.Sliding) return "Slide";

        return foot == Foot.Right ? "RightFoot" : "LeftFoot";
    }


    private void Update()
    {
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        Vector3 actualHorizontalVelocity = PlayerMovement.LocalVelocity.Flattened();
    
        float velocityMag = actualHorizontalVelocity.magnitude;
    
        time += Time.deltaTime * stepSpeed * velocityMag;
    
        SinValue = Mathf.Sin(time);
    
        CalculateFootstep(velocityMag);
    }

    //public static void Calculate(float sinValue, float magnitude, ref float time)
    //    => instance.CalculateFootstep(sinValue, magnitude, ref time);

    private void CalculateFootstep(float magnitude)
    {
        if (magnitude < 1f || !PlayerMovement.Grounded)// || PlayerMovement.Sliding)
        {
            time = 0;
            foot = Foot.Right;
        }

        if (SinValue > 0.5f && foot == Foot.Right && PlayerMovement.Grounded)
        {
            Footstep(Foot.Right, magnitude);
            foot = Foot.Left;
        }
        else if (SinValue < -0.5f && foot == Foot.Left && PlayerMovement.Grounded)
        {
            Footstep(Foot.Left, magnitude);
            foot = Foot.Right;
        }
    }
}
