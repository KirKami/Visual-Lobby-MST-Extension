using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VisualLobby;

public class RandomGameConnector : MonoBehaviour
{
    public GameInfoType gameTypeToSearch;

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
}
