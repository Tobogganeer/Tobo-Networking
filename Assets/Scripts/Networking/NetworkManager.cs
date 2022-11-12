using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;
//using UnityEngine;

namespace Tobo.Net
{
    public abstract class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public ushort port = 26950;

        Server server;
        Client client;
        Client client2;

        //internal Backend backend;



        #region Setup
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
        protected abstract void RegisterPackets();
        #endregion



        // From Socket Backend
        /*
        protected void Init()
        {
            Library.Initialize();
        }

        protected void OnStart()
        {
            NetworkingSockets server = new NetworkingSockets();

            uint pollGroup = server.CreatePollGroup();

            StatusCallback status = (ref StatusInfo info) => {
                switch (info.connectionInfo.state)
                {
                    case ConnectionState.None:
                        break;

                    case ConnectionState.Connecting:
                        server.AcceptConnection(info.connection);
                        server.SetConnectionPollGroup(pollGroup, info.connection);
                        break;

                    case ConnectionState.Connected:
                        Console.WriteLine("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;

                    case ConnectionState.ClosedByPeer:
                    case ConnectionState.ProblemDetectedLocally:
                        server.CloseConnection(info.connection);
                        Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;
                }
            };

            utils.SetStatusCallback(status);

            Address address = new Address();

            address.SetAddress("::0", port);

            uint listenSocket = server.CreateListenSocket(ref address);

            MessageCallback message = (in NetworkingMessage netMessage) => {
                Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            };
        }

        protected void OnUpdate()
        {
            server.RunCallbacks();

            server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
        }

        protected void CleanUp()
        {
            server.DestroyPollGroup(pollGroup);
            Library.Deinitialize();
        }
        */


        #region Send
        public static void SendBufferToServer(ByteBuffer buffer)
        {
            Packet.Handle(buffer);
        }

        public static void SendBufferToAllClients(ByteBuffer buffer)
        {

        }

        public static void SendBufferToClient(ByteBuffer buffer, Client client)
        {

        }

        public static void SendBufferToAllClients(ByteBuffer buffer, Client except)
        {

        }
        #endregion

        public static void Host()
        {
            Instance.server = new Server();
            Instance.client = new Client();
            Instance.client2 = new Client();

            Instance.server.Run(Instance.port);
            Instance.client.Connect("127.0.0.1", Instance.port);
            Instance.client2.Connect("127.0.0.1", Instance.port);
        }

        private void Update()
        {
            server?.Update();
            client?.Update();
            client2?.Update();
        }

        private void OnDestroy()
        {
            server?.Destroy();
            client?.Destroy();
            client2?.Destroy();
            Library.Deinitialize();
        }

        public static void DisconnectClient()
        {
            Instance.client.Destroy();
            Instance.client = null;
        }
    }
}
