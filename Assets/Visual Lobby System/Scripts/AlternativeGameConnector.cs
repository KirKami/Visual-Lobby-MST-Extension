using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualLobby
{
    public class AlternativeGameConnector : MonoBehaviour
    {
        public GameInfoType gameTypeToSearch;

        public static GameInfoPacket storedServerInfo;


        public bool tryConnectToRandomIfStoredNull;
        public bool disconnectOnStoredNull;

        public bool switchNotDisconnect;

        public void FindRandomGame()
        {
            // GAME SESSION SEARCH FILTER
            var searchOptions = new MstProperties();
            searchOptions.Add(Mst.Args.Names.RoomType, (int)gameTypeToSearch);

            MstTimer.WaitForSeconds(0.2f, () =>
            {
                Mst.Client.Matchmaker.FindRandomGame(searchOptions, (games) =>
                {
                    if (games.Count == 0)
                    {
                        AdditiveLevelsRoomClient.Disconnect();
                        return;
                    }

                    MasterServerToolkit.Bridges.MatchmakingBehaviour.Instance.StartMatch(games[0]);
                });
            });
        }
        public static void StoreServerInfo(GameInfoPacket gameInfo)
        {
            storedServerInfo = gameInfo;
        }
        public void ConnectToStoredServer()
        {
            if (switchNotDisconnect)
            {
                //IF SHOULD - KEEP PLAYER IN GAME
                Mst.Events.Invoke(MstEventKeys.leaveRoom);
                Mst.Events.Invoke(MstEventKeys.goToZone, true);
            }

            if (storedServerInfo == null)
            {
                if (tryConnectToRandomIfStoredNull)
                {
                    //Connect To Random
                    FindRandomGame();
                }
                if(disconnectOnStoredNull) AdditiveLevelsRoomClient.Disconnect();
            }
            else
            {
                //Connect To Stored
                MasterServerToolkit.Bridges.MatchmakingBehaviour.Instance.StartMatch(storedServerInfo);
            }
        }
    }
}
