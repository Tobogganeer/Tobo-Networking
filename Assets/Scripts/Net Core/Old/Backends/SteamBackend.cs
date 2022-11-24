using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    [RequireComponent(typeof(SteamManager))]
    public class SteamBackend : Backend
    {
        protected override void Init()
        {
            SteamNetworkingUtils.InitRelayNetworkAccess();

            //StartCoroutine(CheckForCMDJoins());
        }

        protected override void OnStart()
        {
            //SteamNetworkingSockets.
        }

        protected override void OnUpdate()
        {
            /*
            try
            {
                if (socketServer != null)
                    socketServer.Receive();

                if (connectionToServer != null)
                    connectionToServer.Receive();
            }
            catch
            {
                Debug.Log("Error receiving data on Steam socket/connection");
            }
            */
        }


        /*
        IEnumerator CheckForCMDJoins()
        {
            
            yield return new WaitForSeconds(1f);

            const string CONNECT_ARG = "+connect_lobby";
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == CONNECT_ARG)
                {
                    Lobby lobby = new Lobby(ulong.Parse(args[i + 1]));
                    SteamFriends_OnGameLobbyJoinRequested(lobby, 0);
                }
            }
            
        }

        public static void InviteFriends()
        {
            if (CurrentLobby.Id.IsValid)
                SteamFriends.OpenGameInviteOverlay(CurrentLobby.Id);
            else
                Debug.LogWarning("Tried to open overlay for a lobby invite, but no lobby has been joined!");

        }
        */

        protected override void CleanUp()
        {
            throw new System.NotImplementedException();
        }

    }

    public enum SteamLobbyPrivacyMode
    {
        Public,
        Private,
        FriendsOnly,
        Invisible,
    }
}