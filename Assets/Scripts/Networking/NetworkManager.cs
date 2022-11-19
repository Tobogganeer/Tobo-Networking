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

        [Space]
        public GameObject playerPrefab;
        public GameObject localPlayerPrefab;

        internal Server server;
        internal Client client;

        //internal Backend backend;



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
        }

        private void RegisterInternalPackets()
        {
            // Internal packets here

            RegisterPackets();
        }
        protected virtual void RegisterPackets() { }
        protected internal virtual void AddConnectData(ByteBuffer buf) { }
        protected internal virtual bool AllowConnection(Client c, ByteBuffer connectData) => true;
        #endregion

        #region Send Packets
        public static void SendToServer(Packet message, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.client == null || !Instance.client.IsConnected) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                SendBuffer(buf, size, Instance.client.connection, Instance.client.socketInterface, sendMode);
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

        public static void SendToAll(Packet message, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.server == null || !Instance.server.Started) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                foreach (Client c in Instance.server.clients.Values)
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

        public static void SendTo(Packet message, Client client, SendMode sendMode = SendMode.Reliable)
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

        public static void SendToAll(Packet message, Client except, SendMode sendMode = SendMode.Reliable)
        {
            if (Instance.server == null || !Instance.server.Started) return;

            (IntPtr buf, int size) = Prepare(message.GetBuffer());
            try
            {
                foreach (Client c in Instance.server.clients.Values)
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

        public static void Join(string username, string ip)
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
        private void Start()
        {
            server = new Server();
            server.ClientConnected += ClientJoined;

            client = new Client();

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

        private void Update()
        {
            server?.Update();
            client?.Update();
        }

        private void OnDestroy()
        {
            server?.Stop();
            client?.Disconnect();
            Library.Deinitialize();
        }
        #endregion


        void ClientJoined(Client client)
        {

        }

        void Connected()
        {

        }

        void ConnectFailed()
        {

        }

        void PlayerLeft()
        {

        }

        void DidDisconnect()
        {

        }
    }
}
