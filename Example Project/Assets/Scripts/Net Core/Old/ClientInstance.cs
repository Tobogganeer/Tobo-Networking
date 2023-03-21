using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tobo.Net
{
    public class ClientInstance : MonoBehaviour
    {
        public ushort port = 26950;
        public string username = "New Client";
        Client client;

        [ContextMenu("Join")]
        public void Join()
        {
            client.Connect(username, "127.0.0.1", port);
        }

        public void Leave()
        {
            client.Disconnect();
        }


        private void Start()
        {
            client = new Client();
            client.Connected += C_Connected;
            client.ConnectionFailed += C_ConnectionFailed;
            client.ClientConnected += C_ClientConnected;
            client.ClientDisconnected += C_ClientDisconnected;
            client.Disconnected += C_Disconnected;
        }

        private void Update()
        {
            client?.Update();
        }

        // To run before NetManager kills library
        private void OnApplicationQuit()
        {
            client?.Destroy();
        }

        void C_Connected()
        {
            Debug.Log($"CLIENT: Connected (id: {client.ID})");
        }

        void C_ConnectionFailed()
        {
            Debug.Log($"CLIENT: Connect failed");
        }

        void C_ClientConnected(Client c)
        {
            Debug.Log($"CLIENT: {c.Username} ({c.ID}) just connected");
        }

        void C_ClientDisconnected(Client c)
        {
            Debug.Log($"CLIENT: {c.Username} ({c.ID}) just disconnected");
        }

        void C_Disconnected()
        {
            Debug.Log($"CLIENT: Disconnected");
        }
    }
}
