using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> All = new Dictionary<ushort, Player>();

    public ushort ID { get; private set; }
    public string Username { get; private set; }

    public float speed = 3f;
    public Rigidbody rb;
    public NetTransform netTransform;

    bool LocalPlayer => ID == NetworkManager.MyID;



    private void OnDestroy()
    {
        if (All.ContainsKey(ID))
            All.Remove(ID);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (LocalPlayer)
        {
            float x = 0;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            float y = 0;
            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;

            rb.velocity = (transform.right * x * speed + transform.forward * y * speed).WithY(rb.velocity.y);
        }
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.MyID)
            player = Instantiate(ToboNetManager.TNMInstance.localPlayer, position, Quaternion.identity).GetComponent<Player>();
        else
            player = Instantiate(ToboNetManager.TNMInstance.remotePlayer, position, Quaternion.identity).GetComponent<Player>();

        player.ID = id;
        player.Username = username;
        player.name = $"Player {username} ({id})";

        All.Add(id, player);
    }
}
