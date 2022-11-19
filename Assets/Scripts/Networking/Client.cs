using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.Sockets;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public class Client
    {
        public static Dictionary<ushort, Client> All = new Dictionary<ushort, Client>();

        public ushort ID { get; internal set; }
        public string Username { get; internal set; }
        public bool IsConnected => connection != 0;

        internal NetworkingSockets socketInterface;
        internal uint connection;

        public event Action Connected;
        public event Action ConnectionFailed;
        public event Action<ByteBuffer> MessageReceived;
        public event Action Disconnected;
        public event Action<Client> ClientConnected;
        public event Action<Client> ClientDisconnected;

        Dictionary<uint, Action<ByteBuffer>> internalHandle;

        internal Client()
        {
            // Client constructor
            internalHandle = new Dictionary<uint, Action<ByteBuffer>>()
            {
                { Packet.HashCache<S_Handshake>.ID, S_Handshake},
                { Packet.HashCache<S_Welcome>.ID, S_Welcome},
                { Packet.HashCache<S_ClientConnected>.ID, S_ClientConnected},
                { Packet.HashCache<S_ClientDisconnected>.ID, S_ClientDisconnected},
                { Packet.HashCache<Ping>.ID, Ping},
            };
        }
        private Client(ushort id, string name)
        {
            ID = id;
            Username = name;
        }


        public void Connect(string username, string ip = "::0", ushort port = 26950)
        {
            if (IsConnected)
                Disconnect();

            this.Username = username;
            socketInterface = new NetworkingSockets();

            Configuration cfg = new Configuration();
            cfg.dataType = ConfigurationDataType.FunctionPtr;
            cfg.value = ConfigurationValue.ConnectionStatusChanged;
            cfg.data.FunctionPtr = Marshal.GetFunctionPointerForDelegate<StatusCallback>(StatusChanged);

            Address address = new Address();

            address.SetAddress(ip, port);

            connection = socketInterface.Connect(ref address, new Configuration[] { cfg });
            if (connection == 0)
                Debug.LogWarning("Failed to connect!");
        }

        public void Update()
        {
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnConnection(connection, OnMessage, 20);
        }

        public void Disconnect()
        {
            if (connection != 0)
                socketInterface?.CloseConnection(connection, 0, "Disconnect", true);
        }

        void StatusChanged(ref StatusInfo info)
        {
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    Debug.Log("Connecting to server");
                    break;

                case ConnectionState.Connected:
                    Debug.Log("Connected to server");
                    //Connected?.Invoke();
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connecting)
                    {
                        Debug.Log("Could not connect to server: " + info.connectionInfo.endDebug);
                        ConnectionFailed?.Invoke();
                    }
                    else if (info.oldState == ConnectionState.ProblemDetectedLocally)
                    {
                        Debug.Log("Lost contact with server: " + info.connectionInfo.endDebug);
                        Disconnected?.Invoke();
                    }
                    else
                    {
                        Debug.Log("Disconnected: " + info.connectionInfo.endDebug);
                        Disconnected?.Invoke();
                    }

                    //Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    connection = 0;
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            //Debug.Log("Message received from SERVER: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);

            if (internalHandle.TryGetValue(buf.Peek<uint>(), out var action))
            {
                buf.ReadPosition += 4;
                action(buf);
            }
            else
            {
                MessageReceived?.Invoke(buf);
                Packet.Handle(buf);
            }
            //MessageReceived?.Invoke(buf);
        }


        void S_Handshake(ByteBuffer buf)
        {
            // Server handshake has no contents
            C_Handshake handshake = new C_Handshake(Username);
            handshake.Send();
        }

        void S_Welcome(ByteBuffer buf)
        {
            S_Welcome welcome = new S_Welcome();
            welcome.Deserialize(buf, default);
            ID = welcome.id;
            All = new Dictionary<ushort, Client>();
            All.Add(ID, this);
            for (int i = 0; i < welcome.otherClientIDs.Length; i++)
            {
                All.Add(welcome.otherClientIDs[i], new Client(
                    welcome.otherClientIDs[i], welcome.otherClientNames[i]));
            }

            Connected?.Invoke();

            foreach (Client c in All.Values)
            {
                ClientConnected?.Invoke(c);
            }
        }

        void S_ClientConnected(ByteBuffer buf)
        {
            S_ClientConnected packet = new S_ClientConnected();
            packet.Deserialize(buf, default);

            Client c = new Client(packet.id, packet.name);
            All.Add(packet.id, c);
            ClientConnected?.Invoke(c);
        }

        void S_ClientDisconnected(ByteBuffer buf)
        {
            S_ClientDisconnected packet = new S_ClientDisconnected();
            packet.Deserialize(buf, default);

            if (All.TryGetValue(packet.id, out Client c))
                ClientDisconnected?.Invoke(c);
        }

        void Ping(ByteBuffer buf)
        {
            // Ping stuff
        }
    }

    public class S_Client
    {
        public static Dictionary<ushort, S_Client> All = new Dictionary<ushort, S_Client>();

        public ushort ID { get; internal set; }
        public string Username { get; internal set; }

        internal uint connection;

        internal S_Client(ushort ID, uint connection)
        {
            this.connection = connection;
            this.ID = ID;
        }

        public void Kick()
        {
            NetworkManager.Instance.server.socketInterface.CloseConnection(connection, 0, "Kicked", true);
        }
    }
}