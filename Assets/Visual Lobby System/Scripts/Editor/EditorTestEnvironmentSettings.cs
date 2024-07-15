#if UNITY_EDITOR
using MasterServerToolkit.Bridges.MirrorNetworking;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Utils;
using System;
using UnityEngine;

namespace VisualLobby
{
    public class EditorTestEnvironmentSettings : SingletonBehaviour<EditorTestEnvironmentSettings>
    {
        [Header("Environment Settings")]
        public bool useEditorDebugMode;
        public RoomServerManager roomServerManager;

        [Header("Room Server Settings")]
        public EditorEnvironmentRoomOptions roomOptions = new EditorEnvironmentRoomOptions()
        {
            Name = "TestEnviroment",
            Password = "",
            RoomIp = "127.0.0.1",
            RoomPort = 7777,
            IsPublic = true,
            MaxConnections = 4,
            AccessTimeoutPeriod = 5,
            Region = "International",
            Type = GameInfoType.Room,
            CustomOptions = new MstProperties() { }
        };
        public string roomOnlineScene;

        [Serializable]
        public class EditorEnvironmentRoomOptions
        {
            public string Name = "Unnamed";
            public string RoomIp = string.Empty;
            public ushort RoomPort = 0;
            public bool IsPublic = false;
            public ushort MaxConnections = 0;
            public string Password = string.Empty;
            public float AccessTimeoutPeriod = 10;
            public string Region = string.Empty;
            public GameInfoType Type = GameInfoType.Unknown;
            public MstProperties CustomOptions = new MstProperties();
        }

        public void OverrideRoomOptionsInTest(RoomOptions roomOptions)
        {
#if UNITY_EDITOR
            if(!useEditorDebugMode) return;
#else
            return; 
#endif
            roomOptions.Name = this.roomOptions.Name;
            roomOptions.RoomIp = this.roomOptions.RoomIp;
            roomOptions.RoomPort = this.roomOptions.RoomPort;
            roomOptions.IsPublic = this.roomOptions.IsPublic;
            roomOptions.MaxConnections = this.roomOptions.MaxConnections;
            roomOptions.Password = this.roomOptions.Password;
            roomOptions.AccessTimeoutPeriod = this.roomOptions.AccessTimeoutPeriod;
            roomOptions.Region = this.roomOptions.Region;
            roomOptions.Type = this.roomOptions.Type;

            string[] keys = Mst.Args.FindKeys("-room.");
            var properties = new MstProperties();

            foreach (string key in keys)
            {
                if (Mst.Args.IsProvided(key))
                    properties.Set(key, Mst.Args.AsString(key));
            }
            roomOptions.CustomOptions = new MstProperties();
            roomOptions.CustomOptions.Append(properties.UnescapeValues());

            roomServerManager.RoomOptions = roomOptions;
        } 
    }
}
#endif