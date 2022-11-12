using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.Sockets;

namespace Tobo.Net
{
    // https://github.com/nxrighthere/ValveSockets-CSharp

    public class SocketBackend : Backend
    {
        protected override void Init()
        {
            Library.Initialize();
        }

        protected override void OnStart()
        {
			/*
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
			*/
		}

        protected override void OnUpdate()
        {
			//server.RunCallbacks();

			//server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
		}

        protected override void CleanUp()
        {
			//server.DestroyPollGroup(pollGroup);
			Library.Deinitialize();
        }
    }
}