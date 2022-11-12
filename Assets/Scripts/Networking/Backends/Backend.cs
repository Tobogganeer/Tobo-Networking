using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    public abstract class Backend : MonoBehaviour
    {
        private void Awake() => Init();
        protected abstract void Init();

        private void Start() => OnStart();
        protected abstract void OnStart();
        

        private void Update() => OnUpdate();
        protected abstract void OnUpdate();

        private void OnDestroy() => CleanUp();
        protected abstract void CleanUp();

        /*
        public abstract void OnConnectingToServer(ConnectionInfo info);
        public abstract void OnConnectedToServer(ConnectionInfo info);
        public abstract void OnDisconnectedFromServer(ConnectionInfo info);
        public abstract void OnMessageFromServer(IntPtr data, int size, long messageNum, long recvTime, int channel);

        public abstract void OnClientConnecting(Connection connection, ConnectionInfo data);
        public abstract void OnClientConnected(Connection connection, ConnectionInfo data);
        public abstract void OnClientDisconnected(Connection connection, ConnectionInfo data);
        public abstract void OnMessageFromClient(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel);
        */
    }
}
