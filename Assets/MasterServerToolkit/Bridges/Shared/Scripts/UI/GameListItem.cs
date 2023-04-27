﻿using MasterServerToolkit.MasterServer;
using MasterServerToolkit.UI;
using TMPro;
using UnityEngine;

namespace MasterServerToolkit.Bridges
{
    public class GameListItem : MonoBehaviour
    {
        #region INSPECTOR

        [SerializeField]
        private TextMeshProUGUI gameNameText;

        [SerializeField]
        private TextMeshProUGUI gameAddressText;

        [SerializeField]
        private TextMeshProUGUI gameRegionText;

        [SerializeField]
        private TextMeshProUGUI gamePlayersText;

        [SerializeField]
        private UIButton connectButton;

        #endregion

        public void SetInfo(GameInfoPacket gameInfo, GamesListView owner)
        {
            if (gameNameText)
            {
                gameNameText.text = gameInfo.IsPasswordProtected ? $"{gameInfo.Name} <color=yellow>[Password]</color>" : gameInfo.Name;
            }

            if (gameAddressText)
            {
                gameAddressText.text = gameInfo.Address;
            }

            if (gameRegionText)
            {
                string region = string.IsNullOrEmpty(gameInfo.Region) ? "International" : gameInfo.Region;
                gameRegionText.text = $"Region: <color=yellow>{region}</color>";
            }

            if (gamePlayersText)
            {
                string maxPleyers = gameInfo.MaxPlayers <= 0 ? "∞" : gameInfo.MaxPlayers.ToString();
                gamePlayersText.text = $"Players: <color=yellow>{gameInfo.OnlinePlayers} / {maxPleyers}</color>";
            }

            if (connectButton)
            {
                connectButton.AddOnClickListener(() =>
                {
                    MatchmakingBehaviour.Instance.StartMatch(gameInfo);
                });
            }
        }
    }
}