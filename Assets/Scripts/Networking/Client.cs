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
        public bool IsConnected { get; internal set; }
        bool connecting = false;
        //public bool IsConnected => connection != 0;
        //bool handshaken;

        internal NetworkingSockets socketInterface;
        internal uint connection;

        public event Action Connected;
        public event Action ConnectionFailed;
        public event Action<ByteBuffer> MessageReceived;
        public event Action Disconnected;
        public event Action<Client> ClientConnected;
        public event Action<Client> ClientDisconnected;

        Dictionary<uint, Action<ByteBuffer>> internalHandle;
        IntPtr statusPtr;
        StatusCallback callback;

        internal Client()
        {
            socketInterface = new NetworkingSockets();
            // Client constructor
            internalHandle = new Dictionary<uint, Action<ByteBuffer>>()
            {
                { Packet.HashCache<S_Handshake>.ID, S_Handshake},
                { Packet.HashCache<S_Welcome>.ID, S_Welcome},
                { Packet.HashCache<S_ClientConnected>.ID, S_ClientConnected},
                { Packet.HashCache<S_ClientDisconnected>.ID, S_ClientDisconnected},
                { Packet.HashCache<Ping>.ID, Ping},
            };
            callback = StatusChanged;
            statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }
        private Client(ushort id, string name)
        {
            ID = id;
            Username = name;
        }


        public void Connect(string username, string ip = "::0", ushort port = 26950)
        {
            socketInterface ??= new NetworkingSockets();

            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "", false);
                connection = 0;
            }

            IsConnected = false;
            connecting = false;
            ID = 0;
            this.Username = username;

            Configuration cfg = new Configuration();
            cfg.dataType = ConfigurationDataType.FunctionPtr;
            cfg.value = ConfigurationValue.ConnectionStatusChanged;
            cfg.data.FunctionPtr = statusPtr;

            Address address = new Address();

            address.SetAddress(ip, port);

            connection = socketInterface.Connect(ref address, new Configuration[] { cfg });
            if (connection == 0)
                LogMessage("Failed to connect!");
        }

        public void Update()
        {
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnConnection(connection, OnMessage, 20);
        }

        public void Disconnect()
        {
            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "Disconnect", true);
                connection = 0;
                Disconnected?.Invoke(); // Not being invoked by status callback
            }
            IsConnected = false;
            connecting = false;
        }

        internal void Destroy()
        {
            if (connection != 0)
            {
                socketInterface?.CloseConnection(connection, 0, "", false);
            }

            socketInterface = null;
            connection = 0;
        }

        void StatusChanged(ref StatusInfo info)
        {
            if (NetworkManager.Quitting) return;

            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    if (connecting)
                    {
                        LogMessage("Double connect?: " + info.connectionInfo.endDebug);
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        connection = 0;
                        IsConnected = false;
                        ConnectionFailed?.Invoke();
                        break;
                    }
                    connecting = true;
                    //Debug.Log("Connecting to server");
                    break;

                case ConnectionState.Connected:
                    //Debug.Log("Connected to server");
                    //Connected?.Invoke();
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connecting)
                    {
                        LogMessage("Could not connect to server: " + info.connectionInfo.endDebug);
                        ConnectionFailed?.Invoke();
                    }
                    else if (info.oldState == ConnectionState.ProblemDetectedLocally)
                    {
                        LogMessage("Lost contact with server: " + info.connectionInfo.endDebug);
                        Disconnected?.Invoke();
                    }
                    else
                    {
                        LogMessage("Connection closed: " + info.connectionInfo.endDebug);
                        Disconnected?.Invoke();
                    }

                    //Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    connection = 0;
                    IsConnected = false;
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            if (NetworkManager.Quitting) return;

            //Debug.Log("Message received from SERVER: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);
            //Debug.Log($"GOT SERVER MES: {buf.Peek<uint>()} -> {Packet.HashCache<S_Welcome>.ID}");

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

        public override string ToString()
        {
            return $"Client '{Username}' ({ID})";
        }

        public void LogMessage(string message)
        {
            Debug.Log($"[{(ID == 0 || ID == NetworkManager.MyID ? "LOCAL CLIENT" : "CLIENT " + ID)}]: {message}");
        }


        public void Send(Packet packet, SendMode mode = SendMode.Reliable)
        {
            if (connection == 0 || socketInterface == null)
            {
                Debug.LogWarning("Client send fail.");
                return;
            }

            (IntPtr buf, int size) = NetworkManager.Prepare(packet.GetBuffer());
            try
            {
                //Debug.Log($"Sending ");
                NetworkManager.SendBuffer(buf, size, connection, socketInterface, mode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                NetworkManager.Free(buf);
            }
        }


        void S_Handshake(ByteBuffer buf)
        {
            // Server handshake has no contents
            LogMessage("Negotiating with server...");
            C_Handshake handshake = new C_Handshake(Username);
            Send(handshake);
            //Debug.Log("CLIENT: Sent handshake back");
        }

        void S_Welcome(ByteBuffer buf)
        {
            //Debug.Log("CONNECTED YIPPEEEE");

            LogMessage($"Connected.");
            IsConnected = true;
            S_Welcome welcome = new S_Welcome();
            welcome.Deserialize(buf, default);
            //Debug.Log("GOT WELCOME:: " + welcome.GetBuffer().Dump());
            // 176 190 158 23 1 0 0 0 0 0     FIRST
            // 176 190 158 23 1 0 1 0 0 0 2 0 0 0    SECOND
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
                if (c != this)
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

        internal S_Client(uint connection)
        {
            this.connection = connection;
        }

        public void Kick(string reason)
        {
            NetworkManager.Instance.server.socketInterface.CloseConnection(connection, 0, reason, true);
            connection = 0;
        }

        public override string ToString()
        {
            return $"S_Client '{Username}' ({ID})";
        }
    }
}