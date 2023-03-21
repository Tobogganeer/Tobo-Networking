using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//https://answers.unity.com/questions/1358491/character-controller-slide-down-slope.html

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    private void Awake()
    {
        instance = this;
    }

    private CharacterController controller;

    [Space]
    public float moveSpeed = 3.5f;
    public float slopeLimit = 60f;
    public float gravity = 10f;
    public float groundAcceleration = 8f;
    public float airAcceleration = 1f;
    public float accelLerpSpeed = 5f;
    public float pushPower = 3f;

    public LayerMask groundLayerMask;

    private bool grounded;
    private bool wasGrounded;

    private float y;

    const float DOWNFORCE = 10f;
    //float slopeMult;

    Vector3 desiredVelocity;
    Vector3 moveVelocity;
    Vector3 actualVelocity;
    Vector3 groundNormal;

    #region Constants

    //Crouch
    //const float CrouchRaySize = 0.4f;
    //const float CrouchRayLength = 1f;
    //const float StandingHeight = 2f;
    //const float CrouchingHeight = 1f;
    //const float CrouchHeightDif = StandingHeight - CrouchingHeight;

    //Grounded
    const float GroundedSphereRadius = 0.475f;
    const float GroundedSphereDist = 0.7f;
    const float GroundNearDist = 1.8f;
    const float NearSurfaceDist = 0.8f;
    const float NearSurfaceRadius = 0.55f;
    const float GroundedRayDist = 1.2f; // backup for sphere


    // Other
    const float SlideSpeedDecreaseMult = 2.65f;
    const float SlideMoveDirInfluence = 0.6f;

    #endregion

    private float cur_speed;
    private float cur_accel;

    private float airtime;

    public static event Action<float> OnLand;

    //Vector3 lastAirshipPos;
    //Vector3 lastAirshipRot;

    //private bool slidingFromSpeed;

    public static bool Grounded => instance.grounded;
    public static bool Moving { get; private set; }
    public static bool Sliding { get; private set; }
    public static Vector3 Position => instance.transform.position;

    public static Vector3 LocalVelocity { get; private set; }
    public static Vector3 WorldVelocity { get; private set; }

    public static float NormalizedSpeed { get; private set; }
    public static float AirTime { get; private set; }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.slopeLimit = 80;
    }

    private void Update()
    {
        cur_speed = moveSpeed;
        UpdateAcceleration();

        Move();

        UpdateGrounded();

        actualVelocity = moveVelocity;

        SetProperties();
    }

    private void SetProperties()
    {
        WorldVelocity = actualVelocity;
        LocalVelocity = transform.InverseTransformVector(WorldVelocity);
        Moving = desiredVelocity.sqrMagnitude > 0.1f && WorldVelocity.Flattened().sqrMagnitude > 0.1f;
        NormalizedSpeed = 1;
        AirTime = airtime;
    }

    private void Move()
    {
        Vector2 input = PlayerInputs.Movement;

        desiredVelocity = transform.right * input.x + transform.forward * input.y;

        desiredVelocity *= cur_speed;

        y -= gravity * Time.deltaTime;

        if (grounded)
        {
            y = -DOWNFORCE;

            if (!wasGrounded)
            {
                // Just landed
                OnLand?.Invoke(airtime);
                FPSCamera.VerticalDip += Mathf.Lerp(0.0f, 2f, airtime * 0.6f);

                airtime = 0;
            }
        }
        else airtime += Time.deltaTime;


        if (wasGrounded && !grounded)
        {
            // Left ground (not from a jump, otherwise why cancel y velocity)
            //y += DOWNFORCE; // counteract downforce, set y to 0
            y = 0; // didn't work as downforce was set multiplied with downforce earlier, just set to 0
        }

        Vector3 flatVel = moveVelocity.Flattened();

        moveVelocity = Vector3.Lerp(flatVel, desiredVelocity, Time.deltaTime * cur_accel).WithY(0);

        moveVelocity.y = y;

        controller.Move(moveVelocity * Time.deltaTime);

    }

    private void UpdateAcceleration()
    {
        float target = grounded ? groundAcceleration : airAcceleration;

        cur_accel = Mathf.Lerp(cur_accel, target, Time.deltaTime * accelLerpSpeed);
    }

    private void UpdateGrounded()
    {
        wasGrounded = grounded;
        RaycastHit hit;
        grounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), GroundedSphereRadius, out hit, GroundedSphereDist, groundLayerMask)
            || Physics.Raycast(transform.position, Vector3.down, out hit, GroundedRayDist, groundLayerMask);

        if (grounded)
            groundNormal = hit.normal;

        if (!Physics.CheckSphere(transform.position + Vector3.down * NearSurfaceDist, NearSurfaceRadius, groundLayerMask))
            groundNormal = Vector3.up;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.down * GroundedSphereDist, GroundedSphereRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * GroundedRayDist);
    }
}