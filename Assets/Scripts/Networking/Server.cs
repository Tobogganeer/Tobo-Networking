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
        internal Dictionary<uint, S_Client> clients;
        internal NetworkingSockets socketInterface;
        uint pollGroup;
        uint listenSocket;
        ushort nextPlayerID;

        public bool Started { get; private set; }

        public event Action<S_Client> ClientConnected;
        public event Action<S_Client, ByteBuffer> MessageReceived;
        public event Action<S_Client> ClientDisconnected;

        Dictionary<uint, Action<ByteBuffer, S_Client>> internalHandle;

        //public event MessageCallback OnMessage = delegate { };

        public Server()
        {
            internalHandle = new Dictionary<uint, Action<ByteBuffer, S_Client>>()
            {
                { Packet.HashCache<C_Handshake>.ID, C_Handshake},
                { Packet.HashCache<Ping>.ID, Ping},
            };
        }

        public void Run(ushort port = 26950)
        {
            if (Started)
                Stop();

            clients = new Dictionary<uint, Client>();
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
            cfg.data.FunctionPtr = Marshal.GetFunctionPointerForDelegate<StatusCallback>(StatusChanged);

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
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);

            Started = false;
        }

        public void Kick(S_Client c)
        {
            if (S_Client.All.ContainsKey(c.ID))
                S_Client.All.Remove(c.ID);

            c.Kick();
        }

        void StatusChanged(ref StatusInfo info)
        {
            // https://github.com/ValveSoftware/GameNetworkingSockets/blob/master/examples/example_chat.cpp
            // All just taken from here ^^^
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    Debug.Log("Connection from " + info.connectionInfo.connectionDescription);

                    if (clients.ContainsKey(info.connection))
                    {
                        Debug.LogError("Clients list already contains " + info.connection + "!");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                    }

                    if (clients.Count >= NetworkManager.Instance.maxPlayers)
                    {
                        Debug.Log("Rejecting " + info.connectionInfo.connectionDescription + ", max players reached.");
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        break;
                    }

                    if (socketInterface.AcceptConnection(info.connection) != Result.OK)
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        Debug.Log("Can't accept connection, already closed?");
                        break;
                    }

                    if (!socketInterface.SetConnectionPollGroup(pollGroup, info.connection))
                    {
                        socketInterface.CloseConnection(info.connection, 0, "", false);
                        Debug.Log("Failed to set poll group?");
                        break;
                    }

                    clients[info.connection] = new Client(unchecked(nextPlayerID++), info.connection);

                    break;

                case ConnectionState.Connected:
                    Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    ClientConnected?.Invoke(clients[info.connection]);
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connected)
                    {
                        Client client = clients[info.connection];
                        if (info.connectionInfo.state == ConnectionState.ProblemDetectedLocally)
                            Debug.Log("Client closed the connection - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        else
                            Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

                        ClientDisconnected?.Invoke(client);
                        clients.Remove(info.connection);
                    }

                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            //Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            //netMessage.CopyTo
            //netMessage.
            //Marshal.Copy(data, destination, 0, length);

            ByteBuffer buf = ByteBuffer.Get();
            buf.ReadData(netMessage.data, netMessage.length);
            Client c = clients[netMessage.connection];

            if (internalHandle.TryGetValue(buf.Peek<uint>(), out var action))
            {
                buf.ReadPosition += 4;
                action(buf, c);
            }
            else
            {
                MessageReceived?.Invoke(c, buf);
                Packet.Handle(buf, c);
            }
        }


        void C_Handshake(ByteBuffer buf, S_Client c)
        {
            C_Handshake packet = new C_Handshake();
            packet.Deserialize(buf, default);
            c.Username = packet.username;

            if (NetworkManager.Instance.AllowConnection(c, buf))
            {

            }
            else
            {
                c.Disconnect();
            }
        }

        void Ping(ByteBuffer buf, S_Client c)
        {
            new Ping().SendTo(c);
        }
    }
}
