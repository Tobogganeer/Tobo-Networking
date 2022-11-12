using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System;

namespace Tobo.Net
{
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            try
            {
                SteamClient.Init(appID, false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Couldn't log onto steam! " + ex);
                return;
            }

            if (SteamClient.IsValid)
            {
                Debug.Log($"Successfully logged into steam as {SteamName} ({SteamID})");
            }
            else
            {
#if !UNITY_EDITOR
                bool launchedThroughSteam = SteamClient.RestartAppIfNecessary(appID);
                Debug.Log("Launched through steam? " + launchedThroughSteam);

                if (!launchedThroughSteam)
                {
                    Debug.Log("Attempting restart through steam...");
                    Application.Quit();
                }
#endif
            }
        }

        [SerializeField] private uint appID = 480;
        public static uint AppID => instance.appID;

        private static SteamId steamID = 0;
        public static SteamId SteamID
        {
            get
            {
                if (steamID == 0)
                    steamID = SteamClient.SteamId;

                return steamID;
            }
        }

        public static string SteamName => SteamID.SteamName();


        public static void OpenOverlay(SteamOverlayOpenType openType = SteamOverlayOpenType.Friends)
        {
            SteamFriends.OpenOverlay(GetValidOverlayStringFromEnum(openType));
        }

        private static string GetValidOverlayStringFromEnum(SteamOverlayOpenType type)
        {
            return type switch
            {
                SteamOverlayOpenType.Friends => "friends",
                SteamOverlayOpenType.Community => "community",
                SteamOverlayOpenType.Players => "players",
                SteamOverlayOpenType.Settings => "settings",
                SteamOverlayOpenType.OfficialGameGroup => "officalgamegroup",
                SteamOverlayOpenType.Stats => "stats",
                SteamOverlayOpenType.Achievements => "achievements",
                _ => throw new NotImplementedException(),
            };
        }



        private void Update()
        {
            if (!SteamClient.IsValid)
            {
                Debug.LogWarning("Re-initializing steam client...");
                SteamClient.Init(AppID, false);
            }

            SteamClient.RunCallbacks();
        }
    }

    public enum SteamOverlayOpenType
    {
        Friends,
        Community,
        Players,
        Settings,
        OfficialGameGroup,
        Stats,
        Achievements
    }

    public static class SteamIDUtils
    {
        public static string SteamName(this SteamId id) => new Friend(id).Name;
    }
}
