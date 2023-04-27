using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualLobby.UI
{
    public class GameListElement : MonoBehaviour
    {
        //KEEP PLAYER FROM BEING THROWN TO MAIN MENU ON DISCONNECT
        [SerializeField] private bool switchNotDisconnect;

        [SerializeField] private TMP_Text textNumber;
        [SerializeField] private TMP_Text textName;
        [SerializeField] private TMP_Text textAddress;
        [SerializeField] private TMP_Text textRegion;
        [SerializeField] private TMP_Text textPing;
        [SerializeField] private TMP_Text textPlayers;

        [SerializeField] private Button playerListButton;
        [SerializeField] private Button connectButton;

        private GameInfoPacket gameInfo;

        public void SetUpElement(GameInfoPacket game)
        {
            if (playerListButton) playerListButton.interactable = false;
            if (connectButton) connectButton.interactable = false;

            if (game == null)
            {
                Debug.Log("Game List Element is set up without Game info. At object " + gameObject.name);
                return;
            }

            gameInfo = game;

            textNumber.text = (transform.GetSiblingIndex() + 1).ToString();
            textName.text = game.Name;
            textAddress.text = game.Address;
            textRegion.text = game.Region;

            string maxPlayers = gameInfo.MaxPlayers <= 0 ? "?" : gameInfo.MaxPlayers.ToString();
            maxPlayers = $"{gameInfo.OnlinePlayers} / {maxPlayers}";

            textPlayers.text = maxPlayers;

            textPing.text = "...";

            var rx = new Regex(@":\d+");
            string ip = rx.Replace(gameInfo.Address.Trim(), "");

            MstTimer.WaitPing(ip, (time) =>
            {
                if (textPing) textPing.text = $"{time} ms.";
                if (playerListButton) playerListButton.interactable = true;
                if (connectButton && gameInfo.OnlinePlayers < gameInfo.MaxPlayers) connectButton.interactable = true;
            });   
        }

        public void ConnectToGame()
        {
            if (switchNotDisconnect)
            {
                //IF SHOULD - KEEP PLAYER IN GAME
                Mst.Events.Invoke(MstEventKeys.leaveRoom);
                Mst.Events.Invoke(MstEventKeys.goToZone, true);
            }

            MasterServerToolkit.Bridges.MatchmakingBehaviour.Instance.StartMatch(gameInfo);
        }

        public void ShowPlayerList()
        {
            Mst.Events.Invoke(MstEventKeys.showPlayersListView, gameInfo.Id);
            Mst.Events.Invoke(MstEventKeys.hideGamesListView);
        }
    }
}
