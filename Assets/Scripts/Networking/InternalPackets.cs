using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.Net
{
    /*
    
    - Client connects
    - Server handshake
    - Client handshake + password
    - Reject or accept
    - Welcome packet + other clients + id


    */

    public class S_Handshake : Packet
    {
        public override void Serialize(ByteBuffer buf) { }

        public override void Deserialize(ByteBuffer buf, Args args) { }
    }

    public class C_Handshake : Packet
    {
        public string username;
        const int MaxStringLength = 64;

        public C_Handshake() { }
        public C_Handshake(string username)
        {
            this.username = username;
        }

        public override void Serialize(ByteBuffer buf)
        {
            if (username.Length > MaxStringLength)
                throw new System.ArgumentException("Max username length: " + MaxStringLength, "username");

            buf.Write(username);
            NetworkManager.Instance.AddConnectData(buf);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            username = buf.Read();
            // Will then be passed to server to accept or reject
        }
    }

    public class S_Welcome : Packet
    {
        public ushort id;
        public ushort[] otherClientIDs;
        public string[] otherClientNames;

        public S_Welcome() { }
        public S_Welcome(ushort id)
        {
            this.id = id;

            int numOtherClients = NetworkManager.Instance.server.clients.Count - 1; // Don't include ourselves

            otherClientIDs = new ushort[numOtherClients];
            otherClientNames = new string[numOtherClients];

            int i = 0;
            foreach (var client in NetworkManager.Instance.server.clients.Values)
            {
                if (client.ID != id)
                {
                    otherClientIDs[i] = client.ID;
                    otherClientNames[i] = client.Username;
                    i++;
                }
            }
        }

        public override void Serialize(ByteBuffer buf)
        {
            buf.Write(id);
            buf.Write(otherClientIDs);
            buf.Write(otherClientNames);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            id = buf.Read<ushort>();
            otherClientIDs = buf.ReadArray<ushort>();
            otherClientNames = buf.ReadStrArray();
        }
    }

    public class Ping : Packet
    {
        public override void Serialize(ByteBuffer buf) { }

        public override void Deserialize(ByteBuffer buf, Args args) { }
    }

    public class S_ClientConnected : Packet
    {
        public ushort id;
        public string name;

        public S_ClientConnected() { }
        public S_ClientConnected(ushort id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override void Serialize(ByteBuffer buf)
        {
            buf.Write(id);
            buf.Write(name);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            id = buf.Read<ushort>();
            name = buf.Read();
        }
    }

    public class S_ClientDisconnected : Packet
    {
        public ushort id;

        public S_ClientDisconnected() { }
        public S_ClientDisconnected(ushort id)
        {
            this.id = id;
        }

        public override void Serialize(ByteBuffer buf)
        {
            buf.Write(id);
        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {
            id = buf.Read<ushort>();
        }
    }

    /*
    
    public class PACKET_NAME : Packet
    {
        public override void Serialize(ByteBuffer buf)
        {

        }

        public override void Deserialize(ByteBuffer buf, Args args)
        {

        }
    }
    
    */
}
