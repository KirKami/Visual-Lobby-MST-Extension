using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;

using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace VisualLobby.UI
{
    public class RoomGamesView : UIView
    {
        public GameInfoType gameTypeToSearch;

        [Header("Components"), SerializeField]
        private GameListElement gameListElementPrefab;

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
                foreach (GameInfoPacket gameInfo in games)
                {
                    var gameListElement = Instantiate(gameListElementPrefab, listContainer, false);
                    gameListElement.SetUpElement(gameInfo);

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
