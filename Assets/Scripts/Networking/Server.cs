using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
using System.Runtime.InteropServices;
using System;

namespace Tobo.Net
{
    public class Server
    {
        public Dictionary<uint, S_Client> clients;
        internal NetworkingSockets socketInterface;
        uint pollGroup;
        uint listenSocket;
        ushort nextPlayerID = 1;

        public bool Started { get; private set; }

        public event Action<S_Client> ClientConnected;
        public event Action<S_Client, ByteBuffer> MessageReceived;
        public event Action<S_Client> ClientDisconnected;

        Dictionary<uint, Action<ByteBuffer, S_Client>> internalHandle;
        StatusCallback callback;
        IntPtr statusPtr;

        //public event MessageCallback OnMessage = delegate { };

        public Server()
        {
            internalHandle = new Dictionary<uint, Action<ByteBuffer, S_Client>>()
            {
                { Packet.HashCache<C_Handshake>.ID, C_Handshake},
                { Packet.HashCache<Ping>.ID, Ping},
            };
            callback = StatusChanged;
            statusPtr = Marshal.GetFunctionPointerForDelegate(callback);
        }

        public void Run(ushort port = 26950)
        {
            if (Started)
                Stop();

            nextPlayerID = 1;
            clients = new Dictionary<uint, S_Client>();
            socketInterface = new NetworkingSockets();
            pollGroup = socketInterface.CreatePollGroup();

            /* CPP
            
            https://github.com/ValveSoftware/GameNetworkingSockets/blob/master/examples/example_chat.cpp#L245

            SteamNetworkingConfigValue_t opt;
		    opt.SetPtr( k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged, (void*)SteamNetConnectionStatusChangedCallback );
		    m_hListenSock = m_pInterface->CreateListenSocketIP( serverLocalAddr, 1, &opt );

            */

            //utils.SetStatusCallback(status);

            Configuration cfg = new Configuration();
            cfg.dataType = ConfigurationDataType.FunctionPtr;
            cfg.value = ConfigurationValue.ConnectionStatusChanged;
            //cfg.data.FunctionPtr = Marshal.GetFunctionPointerForDelegate<StatusCallback>(StatusChanged);
            cfg.data.FunctionPtr = statusPtr;

            Address address = new Address();

            address.SetAddress("::0", port);

            listenSocket = socketInterface.CreateListenSocket(ref address, new Configuration[] { cfg });

            //OnMessage += (in NetworkingMessage netMessage) =>
            //{
            //    Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            //};

            Started = true;
        }

        public void Update()
        {
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnPollGroup(pollGroup, OnMessage, 20);
        }

        public void Stop()
        {
            if (clients != null)
                foreach (S_Client c in clients.Values)
                    c.Kick("Server closed");

            clients?.Clear();
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);

            Started = false;
        }

        public void Kick(S_Client c)
        {
            if (S_Client.All.ContainsKey(c.ID))
                S_Client.All.Remove(c.ID);

            c.Kick("Kicked");
        }

        internal void Destroy()
        {
            if (clients != null)
            {
                foreach (S_Client c in clients.Values)
                    c.Kick("Server Closed");
                clients.Clear();
            }
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);
            socketInterface = null;

            Started = false;
        }

        void StatusChanged(ref StatusInfo info)
        {
            if (NetworkManager.Quitting) return;

            // https://github.com/ValveSoftware/GameNetworkingSockets/blob/master/examples/example_chat.cpp
            // All just taken from here ^^^
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    //Debug.Log("Connection from " + info.connectionInfo.connectionDescription);
                    LogMessage("Incoming connection from " + info.connectionInfo.connectionDescription);

                    if (clients.ContainsKey(info.connection))
                    {
                        Debug.LogError("[SERVER]: Clients list already contains " + info.connection + "!");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        break;
                    }

                    if (clients.Count >= NetworkManager.Instance.maxPlayers)
                    {
                        LogMessage("Rejecting " + info.connectionInfo.connectionDescription + ", max players reached.");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        break;
                    }

                    if (socketInterface.AcceptConnection(info.connection) != Result.OK)
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        LogMessage("Can't accept connection, already closed?");
                        break;
                    }

                    if (!socketInterface.SetConnectionPollGroup(pollGroup, info.connection))
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        LogMessage("Failed to set poll group?");
                        break;
                    }

                    //Debug.Log("SERVER: Adding new client, conn " + info.connection);
                    //DumpClients();
                    clients[info.connection] = new S_Client(info.connection);
                    //Debug.Log("-----");
                    //DumpClients();

                    break;

                case ConnectionState.Connected:
                    //Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    new S_Handshake().SendTo(clients[info.connection]);
                    //ClientConnected?.Invoke(clients[info.connection]);
                    // Moved to after handshake
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connected || info.oldState == ConnectionState.Connecting)
                    {
                        S_Client client = clients[info.connection];
                        if (info.connectionInfo.state == ConnectionState.ProblemDetectedLocally)
                            LogMessage($"{client} closed the connection - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        else
                            LogMessage($"{client} disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

                        // VVV change so only called if client is full joined?
                        ClientDisconnected?.Invoke(client);
                        if (clients[info.connection].ID != 0)
                        {
                            S_ClientDisconnected disc = new S_ClientDisconnected(clients[info.connection].ID);
                            disc.SendTo(clients[info.connection], true);
                        }
                        clients.Remove(info.connection);
                    }

                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            if (NetworkManager.Quitting) return;

            //Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            //netMessage.CopyTo
            //netMessage.
            //Marshal.Copy(data, destination, 0, length);

            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);
            S_Client c = clients[netMessage.connection];
            //Debug.Log("Got message from " + c + ", conn " + netMessage.connection);

            if (internalHandle.TryGetValue(buf.Peek<uint>(), out var action))
            {
                // Imma be real, idk why I did it this way but yknow
                buf.ReadPosition += 4;
                action(buf, c);
            }
            else
            {
                if (c.ID == 0)
                {
                    Debug.LogWarning($"[SERVER]: Got message from limbo client {c}");
                    return;
                }
                MessageReceived?.Invoke(c, buf);
                Packet.Handle(buf, c);
            }
        }

        public void LogMessage(string message)
        {
            Debug.Log($"[SERVER]: {message}");
        }



        void C_Handshake(ByteBuffer buf, S_Client c)
        {
            //Debug.Log("SERVER: Got client handshake");
            C_Handshake packet = new C_Handshake();
            packet.Deserialize(buf, default);
            //Debug.Log("SERVER: Handshaking data for " + c);
            c.Username = packet.username;
            c.ID = nextPlayerID++;

            if (NetworkManager.Instance.AllowConnection(c, buf, out string failReason))
            {
                //Debug.Log("Authing " + c);
                //DumpClients();

                LogMessage($"{c} connected.");
                S_Welcome welcome = new S_Welcome(c.ID);
                //Debug.Log("-----");
                //DumpClients();
                //Debug.Log("SEND WELCOME: " + welcome.GetBuffer().Dump());
                welcome.SendTo(c);
                ClientConnected?.Invoke(c);
                S_ClientConnected conn = new S_ClientConnected(c.ID, c.Username);
                conn.SendTo(c, true);
            }
            else
            {
                nextPlayerID--;
                c.Kick(failReason);
                // Removed in status method
                //clients.Remove(c.connection);
                //clients.Remove(c.connection);
                //ClientDisconnected?.Invoke(c);
            }
        }

        void Ping(ByteBuffer buf, S_Client c)
        {
            new Ping().SendTo(c);
        }


        public void DumpClients()
        {
            foreach (S_Client c in clients.Values)
            {
                Debug.Log($"CLIENT DUMP: {c}");
            }
        }
    }
}
