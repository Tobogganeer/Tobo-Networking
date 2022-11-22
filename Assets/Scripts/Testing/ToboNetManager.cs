using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class ToboNetManager : NetworkManager
{
    public static ToboNetManager TNMInstance;
    protected override void Awake()
    {
        base.Awake();
        TNMInstance = this;
    }

    [Header("Prefabs")]
    public GameObject localPlayer;
    public GameObject remotePlayer;

    protected override void RegisterPackets()
    {
        Packet.Register<SpawnPlayerPacket>();
        Packet.Register<PlayerPositionPacket>();
        //throw new System.NotImplementedException();
    }

    protected override void Start()
    {
        base.Start();
        //client.ClientConnected += SpawnPlayer;
        //client.Connected += () => SpawnPlayer(client);
        client.ClientDisconnected += Client_ClientDisconnected;
        client.Disconnected += Client_Disconnected;
        client.Connected += Client_Connected;

        server.ClientConnected += Server_ClientConnected;
    }

    private void Server_ClientConnected(S_Client c)
    {
        foreach (S_Client other in server.clients.Values)
        {
            if (other.ID != c.ID)
            {
                SpawnPlayerPacket p = new SpawnPlayerPacket(other.ID, Vector3.zero);
                p.SendTo(c);
            }
        }
    }

    private void Client_Connected()
    {
        Player.Spawn(client.ID, client.Username, Vector3.zero);
        SpawnPlayerPacket p = new SpawnPlayerPacket(client.ID, Vector3.zero);
        client.Send(p);
    }

    private void Client_Disconnected()
    {
        Debug.Log("Disconnected");
        foreach (Player player in Player.All.Values)
            Destroy(player.gameObject);
    }

    private void Client_ClientDisconnected(Client c)
    {
        if (Player.All.TryGetValue(c.ID, out Player val))
            Destroy(val.gameObject);
    }
}

public class SpawnPlayerPacket : Packet
{
    public ushort id;
    public Vector3 position;

    public SpawnPlayerPacket() { }
    public SpawnPlayerPacket(ushort id, Vector3 position)
    {
        this.id = id;
        this.position = position;
    }

    public override void Serialize(ByteBuffer buf)
    {
        buf.Write(id);
        buf.Write(position);
    }

    public override void Deserialize(ByteBuffer buf, Args args)
    {
        id = buf.Read<ushort>();
        position = buf.Read<Vector3>();

        if (args.ServerSide)
        {
            SpawnPlayerPacket bounce = new SpawnPlayerPacket(id, position);
            bounce.SendTo(args.from, true);
        }
        else
        {
            Client c = Client.All[id];

            Player.Spawn(c.ID, c.Username, position);
            Debug.Log("Spawn player " + c);
        }
    }
}


/*

// https://github.com/RiptideNetworking/Riptide/tree/main/Demos/Unity/PlayerHostedDemo




*/