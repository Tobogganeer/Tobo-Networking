using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//[DefaultExecutionOrder(-105)] // Input system is -100
public class PlayerInputs : MonoBehaviour
{
    /*
    public bool controllerViewPow = false;
    public float pow = 2;
    public bool controllerViewSelfMult = false;
    public float selfMult = 0.2f;
    */

    private Inputs inputs;
    private static Inputs.GameplayActions actions;

    public static Vector2 Movement { get; private set; }
    public static bool Primary => actions.Primary.WasPressedThisFrame();
    public static bool Secondary => actions.Secondary.WasPressedThisFrame();
    public static Vector2 Look { get; private set; }


    private void Awake()
    {
        inputs = new Inputs();
        actions = inputs.Gameplay;

        actions.Movement.performed += ctx => Movement = ctx.ReadValue<Vector2>();
        actions.Movement.canceled += ctx => Movement = Vector2.zero;

        actions.Look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        actions.Look.canceled += ctx => Look = Vector2.zero;

        //actions.Interact.performed += ctx => Interact = true;
    }

    private void OnEnable()
    {
        inputs.Gameplay.Enable();
    }

    private void OnDisable()
    {
        inputs.Gameplay.Disable();
    }
}
