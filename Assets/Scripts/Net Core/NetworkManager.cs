using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public ushort port = 26950;
        public ushort maxPlayers;

        public static ushort Port => Instance.port;
        public static ushort MaxPlayers => Instance.maxPlayers;

        //[Space]
        //public GameObject playerPrefab;
        //public GameObject localPlayerPrefab;

        protected internal Server server;
        protected internal Client client;

        public static ushort MyID
        {
            get
            {
                if (Instance.client != null && Instance.client.IsConnected)
                    return Instance.client.ID;
                return 0;
            }
        }
        public static bool IsServer => Instance.server != null && Instance.server.Started;
        public static bool ConnectedToServer => Instance.client != null && Instance.client.IsConnected;
        public static bool Quitting { get; private set; }
        //internal Backend backend;


        //DebugCallback debug;

        #region Awake / Register Packets
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            //backend = GetComponent<Backend>();
            Library.Initialize(); // --
            RegisterInternalPackets();

            /*
            debug = (type, message) => {
                Debug.Log("SOCKET - Type: " + type + ", Message: " + message);
            };

            NetworkingUtils utils = new NetworkingUtils();

            utils.SetDebugCallback(DebugType.Everything, debug);
            */
        }

        private void RegisterInternalPackets()
        {
            // Internal packets here
            Packet.Register<S_Handshake>();
            Packet.Register<C_Handshake>();
            Packet.Register<S_Welcome>();
            Packet.Register<Ping>();
            Packet.Register<S_ClientConnected>();
            Packet.Register<S_ClientDisconnected>();

            RegisterPackets();
        }
        protected virtual void RegisterPackets() { }
        protected internal virtual void AddConnectData(ByteBuffer buf) { }
        protected internal virtual bool AllowConnection(S_Client c, ByteBuffer connectData, out string failReason)
        {
            failReason = "Server Rejected"; 
            return true;
        }
        #endregion

        #region Send Packets
        public static void SendToServer(Packet message, SendMode sendMode = SendMode.Reliable)
        {
            //if (Instance.client == null || !Instance.client.IsConnected) return;
            if (Instance.client == null) return;
            // ^^^ So handshakes can be sent

            Instance.client.Send(message, sendMode);
        }

        public static void SendToAll(Packet message, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.server == null || !Instance.server.Started) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                foreach (S_Client c in Instance.server.clients.Values)
                {
                    SendBuffer(buf, size, c.connection, Instance.server.socketInterface, sendMode);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Free(buf);
            }
        }

        public static void SendTo(Packet message, S_Client client, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.server == null || !Instance.server.Started) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                SendBuffer(buf, size, client.connection, Instance.server.socketInterface, sendMode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Free(buf);
            }
        }

        public static void SendToAll(Packet message, S_Client except, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.server == null || !Instance.server.Started) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                foreach (S_Client c in Instance.server.clients.Values)
                {
                    if (c != except)
                        SendBuffer(buf, size, c.connection, Instance.server.socketInterface, sendMode);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Free(buf);
            }
        }

        internal static (IntPtr, int) Prepare(ByteBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            int sizeOfMessage = buffer.WritePosition;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(buffer.Data, 0, intPtrMessage, sizeOfMessage);
            return (intPtrMessage, sizeOfMessage);
        }

        internal static void SendBuffer(IntPtr buffer, int size, uint connection, NetworkingSockets socketInterface, SendMode sendMode)
        {
            if (socketInterface == null)
                throw new ArgumentNullException(nameof(socketInterface));

            Result success = socketInterface.SendMessageToConnection(connection, buffer, size, (SendFlags)sendMode);
            if (success == Result.OK)
            {
                NetStats.OnPacketSent(size);
            }
            else
            {
                // RETRY
                Result retry = socketInterface.SendMessageToConnection(connection, buffer, size, (SendFlags)sendMode);
                if (retry == Result.OK)
                {
                    NetStats.OnPacketSent(size);
                    return;
                }
                Debug.LogWarning($"Failed to send message to conn ({connection})! Res: {retry}");
            }
        }

        internal static void Free(IntPtr buffer)
        {
            Marshal.FreeHGlobal(buffer); // Free up memory at pointer
        }

        /*
        public static void SendBuffer(ByteBuffer buffer, uint connection, NetworkingSockets socketInterface, SendMode sendMode)
        {
            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = buffer.WritePosition;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(buffer.Data, 0, intPtrMessage, sizeOfMessage);
            Result success = socketInterface.SendMessageToConnection(connection, intPtrMessage, sizeOfMessage, (SendFlags)sendMode);
            if (success == Result.OK)
            {
                Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                NetStats.OnPacketSent(sizeOfMessage);
            }
            else
            {
                // RETRY
                Result retry = socketInterface.SendMessageToConnection(connection, intPtrMessage, sizeOfMessage, (SendFlags)sendMode);
                Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                if (retry == Result.OK)
                {
                    NetStats.OnPacketSent(sizeOfMessage);
                    return;
                }
                Debug.LogWarning($"Failed to send message to conn ({connection})! Res: {retry}");
            }
        }
        */
        #endregion

        #region Host / Join / Disconnect
        public static void Host(string username)
        {
            Instance.server.Run(Instance.port);
            Instance.client.Connect(username, "127.0.0.1", Instance.port);
        }

        public static void Join(string username, string ip = "127.0.0.1")
        {
            Instance.client.Connect(username, ip, Instance.port);
        }

        public static void Disconnect()
        {
            Instance.server.Stop();
            Instance.client.Disconnect();
        }
        #endregion


        #region Start / Update / OnDestroy
        protected virtual void Start()
        {
            server = new Server();
            //server.ClientConnected += S_ClientConnected;
            //server.ClientDisconnected += S_ClientDisconnected;

            client = new Client();
            //client.Connected += C_Connected;
            //client.ConnectionFailed += C_ConnectionFailed;
            //client.ClientConnected += C_ClientConnected;
            //client.ClientDisconnected += C_ClientDisconnected;
            //client.Disconnected += C_Disconnected;

            /*
            Server = new Server();
            Server.ClientConnected += PlayerJoined;
            Server.RelayFilter = new MessageRelayFilter(typeof(MessageId), MessageId.SpawnPlayer, MessageId.PlayerMovement);

            Client = new Client();
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
            */
        }

        protected virtual void Update()
        {
            server?.Update();
            client?.Update();
            MainThread.UpdateMain();
        }

        protected virtual void OnDestroy()
        {
            try
            {
                client?.Destroy();
                client = null;

                server?.Destroy();
                server = null;

                //Debug.Log("SOCKETS: De-init");
                Library.Deinitialize();
                Debug.Log("[SOCKETS]: Shutdown");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error terminating! " + ex);
                throw;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            Quitting = true;
        }
        #endregion


        // https://www.youtube.com/watch?v=uYLCviZrZVs
        // ^^^ WATCH THIS

        /*
        void S_ClientConnected(S_Client client)
        {
            Debug.Log($"SERVER: {client.Username} ({client.ID}) connected");
        }

        void S_ClientDisconnected(S_Client client)
        {
            Debug.Log($"SERVER: {client.Username} ({client.ID}) disconnected");
        }


        void C_Connected()
        {
            Debug.Log($"CLIENT: Connected (id: {MyID})");
        }

        void C_ConnectionFailed()
        {
            Debug.Log($"CLIENT: Connect failed");
        }

        void C_ClientConnected(Client c)
        {
            Debug.Log($"CLIENT: {c.Username} ({c.ID}) connected");
        }

        void C_ClientDisconnected(Client c)
        {
            Debug.Log($"CLIENT: {c.Username} ({c.ID}) disconnected");
        }

        void C_Disconnected()
        {
            Debug.Log($"CLIENT: Disconnected");
        }
        */


        [ContextMenu("Dump Clients")]
        void DumpClients()
        {
            server?.DumpClients();
        }
    }
}
