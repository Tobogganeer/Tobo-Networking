using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> All = new Dictionary<ushort, Player>();

    public ushort ID { get; private set; }
    private string username;

    public float speed = 3f;
    public Rigidbody rb;
    [Range(1, 10)]
    public int updateCycle = 8;
    [ReadOnly]
    public float updatesPerSecond;
    float syncDelay => 1f / updatesPerSecond;

    private float currentInterpolation
    {
        get
        {
            float difference = target.time - current.time;

            float elapsed = Time.time - target.time;
            return difference > 0 ? elapsed / difference : 0;
            // Thanks mirror for this useful bit of code
        }
    }
    bool LocalPlayer => ID == NetworkManager.MyID;


    private readonly TransformSnapshot current = new TransformSnapshot();
    private readonly TransformSnapshot target = new TransformSnapshot();



    private void OnDestroy()
    {
        if (All.ContainsKey(ID))
            All.Remove(ID);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!LocalPlayer)
        {
            current.Update(transform.position, transform.rotation, transform.localScale, Time.time - syncDelay);
            target.Update(transform.position, transform.rotation, transform.localScale, Time.time);
        }
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

    int count;
    private void FixedUpdate()
    {
        if (!LocalPlayer)
        {
            UpdateTransform();
            return;
        }

        count++;
        if (count >= updateCycle)
        {
            count = 0;
            PlayerPositionPacket send = new PlayerPositionPacket(ID, rb.position, rb.rotation);
            NetworkManager.SendToServer(send);
        }
    }

    void UpdateTransform()
    {
        transform.position = Vector3.Lerp(current.position, target.position, currentInterpolation);
    }

    public void NewTransformReceived(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        //if (ShouldSnap()) current.Update(target.position, target.rotation, target.scale, target.time);
        //else current.Update(settings.position.useGlobal ? transform.position : transform.localPosition,
        //    settings.rotation.useGlobal ? transform.rotation : transform.localRotation, transform.localScale, Time.time - syncDelay);
        current.Update(target.position, target.rotation, target.scale, Time.time - syncDelay);

        target.Update(position, rotation, scale, Time.time);
    }



    private void OnValidate()
    {
        updatesPerSecond = 1f / Time.fixedDeltaTime / updateCycle;
    }

    /*
    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;
        forward.y = 0;
        transform.forward = forward.normalized;
    }
    */

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.MyID)
            player = Instantiate(ToboNetManager.TNMInstance.localPlayer, position, Quaternion.identity).GetComponent<Player>();
        else
            player = Instantiate(ToboNetManager.TNMInstance.remotePlayer, position, Quaternion.identity).GetComponent<Player>();

        player.ID = id;
        player.username = username;
        player.name = $"Player {username} ({id})";

        All.Add(id, player);
    }

    /*
    private void SendSpawn()
    {
        Message message = Message.Create(MessageSendMode.Reliable, MessageId.SpawnPlayer);
        message.AddUShort(ID);
        message.AddString(username);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    internal void SendSpawn(ushort newPlayerId)
    {
        Message message = Message.Create(MessageSendMode.Reliable, MessageId.SpawnPlayer);
        message.AddUShort(ID);
        message.AddString(username);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.Send(message, newPlayerId);
    }
    #endregion
    */
}

public class TransformSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float time;

    public void Update(Vector3 position, Quaternion rotation, Vector3 scale, float time)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.time = time;
    }
}

public class PlayerPositionPacket : Packet
{
    public ushort id;
    public Vector3 position;
    public Quaternion rot;
    //public Vector3 velocity;

    public PlayerPositionPacket() { }
    //public PlayerPositionPacket(ushort id, Vector3 position, Vector3 velocity)
    public PlayerPositionPacket(ushort id, Vector3 position, Quaternion rot)
    {
        this.id = id;
        this.position = position;
        this.rot = rot;
    }

    public override void Serialize(ByteBuffer buf)
    {
        buf.Write(id);
        buf.Write(position);
        buf.Add(rot); // Compressed
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        id = buf.Read<ushort>();
        position = buf.Read<Vector3>();
        rot = buf.GetQuaternion();

        if (args.ServerSide)
        {
            PlayerPositionPacket bounce = new PlayerPositionPacket(id, position, rot);
            bounce.SendTo(args.from, true);
        }
        else
        {
            Player p = Player.All[id];
            p.NewTransformReceived(position, rot, Vector3.one);
            //p.rb.position = position;
            //p.rb.velocity = velocity;
        }
    }
}
