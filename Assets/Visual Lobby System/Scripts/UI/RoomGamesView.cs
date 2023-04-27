using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace VisualLobby.UI
{
    public class RoomGamesView : UIView
{
        //KEEP PLAYER FROM BEING THROWN TO MAIN MENU ON DISCONNECT
        //public bool keepInGame;

        public GameInfoType gameTypeToSearch;

        [Header("Components"), SerializeField]

        private GameListElement gameListElementPrefab;

        //[SerializeField]
        //private UILable uiLablePrefab;
        //[SerializeField]
        //private UILable uiColLablePrefab;
        //[SerializeField]
        //private UIButton uiButtonPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TMP_Text statusInfoText;

        public UnityEvent OnStartGameEvent;

        protected override void Awake()
        {
            base.Awake();

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showGamesListView, OnShowGamesListEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideGamesListView, OnHideGamesListEventHandler);
        }

        protected override void Start()
        {
            base.Start();

            if (listContainer)
            {
                foreach (Transform t in listContainer)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void OnShowGamesListEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideGamesListEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            FindGames();
        }

        private void DrawGamesList(IEnumerable<GameInfoPacket> games)
        {
            if (listContainer)
            {
                //int index = 0;

                //var gameNumberCol = Instantiate(uiColLablePrefab, listContainer, false);
                //gameNumberCol.Text = "#";
                //gameNumberCol.name = "gameNumberCol";

                //var gameNameCol = Instantiate(uiColLablePrefab, listContainer, false);
                //gameNameCol.Text = "Name";
                //gameNameCol.name = "gameNameCol";

                //var gameAddressCol = Instantiate(uiColLablePrefab, listContainer, false);
                //gameAddressCol.Text = "Address";
                //gameAddressCol.name = "gameAddressCol";

                //var gameRegionCol = Instantiate(uiColLablePrefab, listContainer, false);
                //gameRegionCol.Text = "Region";
                //gameRegionCol.name = "gameRegionCol";

                //var pingRegionCol = Instantiate(uiColLablePrefab, listContainer, false);
                //pingRegionCol.Text = "Ping";
                //pingRegionCol.name = "pingRegionCol";

                //var gamePlayersCol = Instantiate(uiColLablePrefab, listContainer, false);
                //gamePlayersCol.Text = "Players";
                //gamePlayersCol.name = "gamePlayersCol";

                //var connectBtnCol = Instantiate(uiColLablePrefab, listContainer, false);
                //connectBtnCol.Text = "Action";
                //connectBtnCol.name = "connectBtnCol";

                foreach (GameInfoPacket gameInfo in games)
                {
                    //var gameNumberLable = Instantiate(uiLablePrefab, listContainer, false);
                    //gameNumberLable.Text = $"{index + 1}";
                    //gameNumberLable.name = $"gameNumberLable_{index}";

                    //var gameNameLable = Instantiate(uiLablePrefab, listContainer, false);
                    //gameNameLable.Text = gameInfo.IsPasswordProtected ? $"{gameInfo.Name} <color=yellow>[Password]</color>" : gameInfo.Name;
                    //gameNameLable.name = $"gameNameLable_{index}";

                    //var gameAddressLable = Instantiate(uiLablePrefab, listContainer, false);
                    //gameAddressLable.Text = gameInfo.Address;
                    //gameAddressLable.name = $"gameAddressLable_{index}";

                    //var gameRegionLable = Instantiate(uiLablePrefab, listContainer, false);
                    //string region = string.IsNullOrEmpty(gameInfo.Region) ? "International" : gameInfo.Region;
                    //#region BF_MODIFIED
                    ////INCLUDE GAME TYPE WITH REGION
                    //gameRegionLable.Text = region + " / " + gameInfo.Type;
                    //#endregion
                    //gameRegionLable.name = $"gameRegionLable_{index}";

                    //var pingRegionLable = Instantiate(uiLablePrefab, listContainer, false);
                    //pingRegionLable.Text = $"...";

                    //var rx = new Regex(@":\d+");
                    //string ip = rx.Replace(gameInfo.Address.Trim(), "");

                    //MstTimer.WaitPing(ip, (time) =>
                    //{
                    //    pingRegionLable.Text = $"{time} ms.";
                    //});

                    //pingRegionLable.name = $"pingRegionLable_{index}";

                    //var gamePlayersBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    //string maxPleyers = gameInfo.MaxPlayers <= 0 ? "?" : gameInfo.MaxPlayers.ToString();
                    //gamePlayersBtn.SetLable($"{gameInfo.OnlinePlayers} / {maxPleyers} [Show]");
                    //gamePlayersBtn.name = $"gamePlayersLable_{index}";
                    //gamePlayersBtn.AddOnClickListener(() =>
                    //{
                    //    Mst.Events.Invoke(MstEventKeys.showPlayersListView, gameInfo.Id);
                    //    Hide();
                    //});

                    //var gameConnectBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    //gameConnectBtn.SetLable("Join");
                    //gameConnectBtn.AddOnClickListener(() =>
                    //{
                    //    #region BF_MODIFIED
                    //    if (requiredToLeaveRoom)
                    //    {
                    //        //IF SHOULD - KEEP PLAYER IN GAME
                    //        Mst.Events.Invoke(MstEventKeys.leaveRoom);
                    //        Mst.Events.Invoke(MstEventKeys.goToZone, true);
                    //    }
                    //    #endregion

                    //    MasterServerToolkit.Bridges.MatchmakingBehaviour.Instance.StartMatch(gameInfo);
                    //});
                    //gameConnectBtn.name = $"gameConnectBtn_{index}";

                    var gameListElement = Instantiate(gameListElementPrefab, listContainer, false);
                    gameListElement.SetUpElement(gameInfo);

                    //index++;

                    logger.Info(gameInfo);
                }
            }
            else
            {
                logger.Error("Not all components are setup");
            }
        }

        private void ClearGamesList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        public void ShowCreateNewRoomView()
        {
            Mst.Events.Invoke(MstEventKeys.showCreateNewRoomView);
        }

        /// <summary>
        /// Sends request to master server to find games list
        /// </summary>
        public void FindGames()
        {
            ClearGamesList();

            canvasGroup.interactable = false;

            if (statusInfoText)
            {
                statusInfoText.text = "Finding rooms... Please wait!";
                statusInfoText.gameObject.SetActive(true);
            }

            // GAME SESSION SEARCH FILTER
            var searchOptions = new MstProperties();
            searchOptions.Add(Mst.Args.Names.RoomType, (int)gameTypeToSearch);

            MstTimer.WaitForSeconds(0.2f, () =>
            {
                Mst.Client.Matchmaker.FindGames(searchOptions, (games) =>
                {
                    canvasGroup.interactable = true;

                    if (games.Count == 0)
                    {
                        statusInfoText.text = "No games found! Try to create your own one.";
                        return;
                    }

                    statusInfoText.gameObject.SetActive(false);
                    DrawGamesList(games);
                });
            });
        }
    }
}
