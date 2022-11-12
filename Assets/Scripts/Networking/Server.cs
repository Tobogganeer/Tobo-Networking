using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public class Server
    {
        Dictionary<uint, Client> clients;
        NetworkingSockets socketInterface;
        uint pollGroup;
        uint listenSocket;

        //public event MessageCallback OnMessage = delegate { };

        public void Run(ushort port = 26950)
        {
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
        }

        public void Update()
        {
            socketInterface?.RunCallbacks();
            socketInterface?.ReceiveMessagesOnPollGroup(pollGroup, OnMessage, 20);
        }

        public void Destroy()
        {
            socketInterface?.CloseListenSocket(listenSocket);
            socketInterface?.DestroyPollGroup(pollGroup);
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
                    if (clients.ContainsKey(info.connection))
                        Debug.LogError("Clients list already contains " + info.connection + "!");

                    Debug.Log("Connection from " + info.connectionInfo.connectionDescription);

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

                    clients[info.connection] = new Client(info.connection);

                    break;

                case ConnectionState.Connected:
                    Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connected)
                    {
                        //Client client = clients[info.connection];
                        // Unneeded, but valid (not removed yet) ^^^
                        if (info.connectionInfo.state == ConnectionState.ProblemDetectedLocally)
                            Debug.Log("Client closed the connection - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        else
                            Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());

                        clients.Remove(info.connection);
                    }

                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
        }


        public void SendMessageToAllClients()
        {

        }
    }
}
