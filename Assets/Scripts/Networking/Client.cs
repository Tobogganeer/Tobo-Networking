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
        public static Dictionary<ulong, Client> All = new Dictionary<ulong, Client>();

        public ulong ID { get; private set; }

        NetworkingSockets socketInterface;
        uint connection;

        public Client(uint connection) => this.connection = connection;
        public Client() { }


        public void Connect(string ip = "::0", ushort port = 26950)
        {
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

        public void Destroy()
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
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    if (info.oldState == ConnectionState.Connecting)
                        Debug.Log("Could not connect to server: " + info.connectionInfo.endDebug);
                    else if (info.oldState == ConnectionState.ProblemDetectedLocally)
                        Debug.Log("Lost contact with server: " + info.connectionInfo.endDebug);
                    else
                        Debug.Log("Disconnected: " + info.connectionInfo.endDebug);

                    //Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                    socketInterface.CloseConnection(info.connection, 0, "", false);
                    connection = 0;
                    break;
            }
        }

        void OnMessage(in NetworkingMessage netMessage)
        {
            Debug.Log("Message received from SERVER: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
        }
    }
}