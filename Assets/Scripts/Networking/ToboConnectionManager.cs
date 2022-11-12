using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    public class ToboConnectionManager : ConnectionManager
    {
        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
            //NetworkManager.Instance.backend.OnConnectingToServer(info);

            /*
            base.OnConnecting(info);
            //Debug.Log($"Connecting to {new Friend(info.Identity.SteamId).Name}");
            */
        }

        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            //NetworkManager.Instance.backend.OnConnectedToServer(info);

            /*
            base.OnConnected(info);
            SteamManager.OnConnConnectedToServer(info);
            */
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);
            //NetworkManager.Instance.backend.OnDisconnectedFromServer(info);

            /*
            base.OnDisconnected(info);
            Debug.Log($"Disconnected from server");
            SteamManager.ConnectionClosed();
            SteamManager.Leave();
            */
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(data, size, messageNum, recvTime, channel);
            //NetworkManager.Instance.backend.OnMessageFromServer(data, size, messageNum, recvTime, channel);

            /*
            SteamManager.HandleDataFromServer(data, size);
            */
        }
    }
}
