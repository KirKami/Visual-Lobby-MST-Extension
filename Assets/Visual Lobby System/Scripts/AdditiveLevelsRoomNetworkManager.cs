using kcp2k;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.Bridges.MirrorNetworking;

using Mirror;
using Mirror.SimpleWeb;
using Mirror.Examples.AdditiveLevels;

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VisualLobby
{
    public class AdditiveLevelsRoomNetworkManager : NetworkManager
    {
        #region INSPECTOR

        [Header("Mirror Network Manager Components"), SerializeField]
        protected RoomServerManager roomServerManager;

        /// <summary>
        /// Log levelof this module
        /// </summary>
        [Header("Mirror Network Manager Settings"), SerializeField]
        protected LogLevel logLevel = LogLevel.Info;

        [Header("Additive Scenes - First is start scene")]

        [Scene, Tooltip("Add additive scenes here.\nFirst entry will be players' start scene")]
        public string[] additiveScenes;

        [Header("Fade Control - See child FadeCanvas")]

        [Tooltip("Reference to FadeInOut script on child FadeCanvas")]
        public FadeInOut fadeInOut;

        #endregion

        public static new AdditiveLevelsRoomNetworkManager singleton { get; private set; }

        /// <summary>
        /// Logger assigned to this module
        /// </summary>
        protected MasterServerToolkit.Logging.Logger logger;

        /// <summary>
        /// Invokes when mirror server is started
        /// </summary>
        public event Action OnServerStartedEvent;

        /// <summary>
        /// Invokes when mirror server is stopped
        /// </summary>
        public event Action OnServerStoppedEvent;

        /// <summary>
        /// Invokes when mirror host is started
        /// </summary>
        public event Action OnHostStartedEvent;

        /// <summary>
        /// Invokes when mirror host is stopped
        /// </summary>
        public event Action OnHostStopEvent;

        /// <summary>
        /// Invokes when mirror client is started
        /// </summary>
        public event Action OnClientStartedEvent;

        /// <summary>
        /// Invokes when mirror client is stopped
        /// </summary>
        public event Action OnClientStoppedEvent;

        /// <summary>
        /// Called on clients when connected to a server
        /// </summary>
        public event Action<NetworkConnection> OnConnectedEvent;

        /// <summary>
        /// Called on clients when disconnected from a server
        /// </summary>
        public event Action<NetworkConnection> OnDisconnectedEvent;

        /// <summary>
        /// This is called on the Server when a Client connects the Server
        /// </summary>
        public event Action<NetworkConnection> OnClientConnectedEvent;

        /// <summary>
        /// This is called on the Server when a Client disconnects from the Server
        /// </summary>
        public event Action<NetworkConnection> OnClientDisconnectedEvent;

        /// <summary>
        /// 
        /// </summary>
        public event Action OnClientSceneChangedEvent;

        // This is set true after server loads all subscene instances
        bool subscenesLoaded;

        // This is managed in LoadAdditive, UnloadAdditive, and checked in OnClientSceneChanged
        bool isInTransition;

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            logger = Mst.Create.Logger(GetType().Name);
            logger.LogLevel = logLevel;

            // Prevent start network manager in headless mode automatically
            autoStartServerBuild = false;

        }

        public void StartRoomServer()
        {
            // Find room server if it is not assigned in inspector
            if (!roomServerManager) roomServerManager = GetComponent<RoomServerManager>();

            // Set online scene
            onlineScene = Mst.Args.AsString(Mst.Args.Names.RoomOnlineScene, SceneManager.GetActiveScene().name);

            // Set the max number of connections
            maxConnections = roomServerManager.RoomOptions.MaxConnections;

            // Set room IP just for information purpose only
            SetAddress(roomServerManager.RoomOptions.RoomIp);

            // Set room port
            SetPort(roomServerManager.RoomOptions.RoomPort);

            logger.Info($"Starting Room Server: {networkAddress}:{roomServerManager.RoomOptions.RoomPort}");
            logger.Info($"Online Scene: {onlineScene}");

#if UNITY_EDITOR
            //StartHost();
#else
            StartServer();
#endif
        }

        public void StopRoomServer()
        {
            StopServer();

            MstTimer.WaitForSeconds(1f, () =>
            {
                Mst.Runtime.Quit();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        public void SetAddress(string address)
        {
            networkAddress = address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port)
        {
            // Set room port
            if (Transport.active is KcpTransport kcpTransport)
            {
                kcpTransport.Port = (ushort)port;
            }
            else if (Transport.active is TelepathyTransport telepathyTransport)
            {
                telepathyTransport.port = (ushort)port;
            }
            else if (Transport.active is SimpleWebTransport swTransport)
            {
                swTransport.port = (ushort)port;
            }
        }

        #region MIRROR SERVER

        /// <summary>
        /// When mirror server is started
        /// </summary>
        public override void OnStartServer()
        {
            logger.Info($"Room Server started and listening to: {networkAddress}:{roomServerManager.RoomOptions.RoomPort}");

            base.OnStartServer();
            NetworkServer.RegisterHandler<ValidateRoomAccessRequestMessage>(ValidateRoomAccessRequestHandler, false);
            if (roomServerManager) roomServerManager.OnServerStarted();
            OnServerStartedEvent?.Invoke();
            StartCoroutine(ServerLoadSubScenes());
        }

        public override void OnStopServer()
        {
            logger.Info("Room Server stopped");

            base.OnStopServer();
            NetworkServer.UnregisterHandler<ValidateRoomAccessRequestMessage>();
            if (roomServerManager) roomServerManager.OnServerStopped();
            OnServerStoppedEvent?.Invoke();
        }

        public override void OnStartHost()
        {
            base.OnStartHost();
            OnHostStartedEvent?.Invoke();
        }

        public override void OnStopHost()
        {
            base.OnStopHost();
            OnHostStopEvent?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            logger.Info($"Client {conn.connectionId} has just joined the room");
            base.OnServerConnect(conn);
            OnClientConnectedEvent?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            logger.Info($"Client {conn.connectionId} has just left the room");
            base.OnServerDisconnect(conn);
            if (roomServerManager) roomServerManager.OnPeerDisconnected(conn.connectionId);
            OnClientDisconnectedEvent?.Invoke(conn);
        }

        #endregion

        #region MIRROR CLIENT

        public override void OnStartClient()
        {
            base.OnStartClient();

            logger.Info($"Ñlient started");
            OnClientStartedEvent?.Invoke();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            logger.Info($"Ñlient stopped");
            OnClientStoppedEvent?.Invoke();
        }

        public override void OnClientConnect()
        {
            logger.Info($"Ñlient connected");
            OnConnectedEvent?.Invoke(NetworkClient.connection);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            logger.Info($"You have left a room");
            OnDisconnectedEvent?.Invoke(NetworkClient.connection);
        }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            logger.Info($"Client is loading scene {newSceneName}");

            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene {newSceneName} {sceneOperation}");

            if (sceneOperation == SceneOperation.UnloadAdditive)
                StartCoroutine(UnloadAdditive(newSceneName));

            if (sceneOperation == SceneOperation.LoadAdditive)
                StartCoroutine(LoadAdditive(newSceneName));
        }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        public override void OnClientSceneChanged()
        {
            logger.Info($"Client scene loaded");

            Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientSceneChanged {isInTransition}");

            // Only call the base method if not in a transition.
            // This will be called from DoTransition after setting doingTransition to false
            // but will also be called first by Mirror when the scene loading finishes.
            if (!isInTransition)
            {
                base.OnClientSceneChanged();
                OnClientSceneChangedEvent?.Invoke();
            }
        }
        #endregion

        #region ROOM SERVER

        protected void ValidateRoomAccessRequestHandler(NetworkConnection conn, ValidateRoomAccessRequestMessage mess)
        {
            if (roomServerManager)
            {
                roomServerManager.ValidateRoomAccess(conn.connectionId, mess.Token, (isSuccess, error) =>
                {
                    try
                    {
                        if (!isSuccess)
                        {
                            logger.Error(error);

                            conn.Send(new ValidateRoomAccessResultMessage()
                            {
                                Error = error,
                                Status = ResponseStatus.Failed
                            });

                            MstTimer.WaitForSeconds(1f, () => conn.Disconnect());
                            return;
                        }

                        conn.Send(new ValidateRoomAccessResultMessage()
                        {
                            Error = string.Empty,
                            Status = ResponseStatus.Success
                        });
                    }
                    // If we got another exception
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        conn.Send(new ValidateRoomAccessResultMessage()
                        {
                            Error = e.Message,
                            Status = ResponseStatus.Error
                        });

                        MstTimer.WaitForSeconds(1f, () => conn.Disconnect());
                    }
                });
            }
        }

        #endregion
        #region Scene Management

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName)
        {
            // This fires after server fully changes scenes, e.g. offline to online
            // If server has just loaded the Container (online) scene, load the subscenes on server
            if (sceneName == onlineScene)
                StartCoroutine(ServerLoadSubScenes());
        }

        IEnumerator ServerLoadSubScenes()
        {
            foreach (string additiveScene in additiveScenes)
                yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
                });

            subscenesLoaded = true;
        }

        IEnumerator LoadAdditive(string sceneName)
        {
            isInTransition = true;

            // This will return immediately if already faded in
            // e.g. by UnloadAdditive above or by default startup state
            yield return fadeInOut.FadeIn();

            // host client is on server...don't load the additive scene again
            if (mode == NetworkManagerMode.ClientOnly)
            {
                // Start loading the additive subscene
                loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                    yield return null;
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            isInTransition = false;

            base.OnClientSceneChanged();

            yield return fadeInOut.FadeOut();
        }

        IEnumerator UnloadAdditive(string sceneName)
        {
            isInTransition = true;

            // This will return immediately if already faded in
            // e.g. by LoadAdditive above or by default startup state
            yield return fadeInOut.FadeIn();

            if (mode == NetworkManagerMode.ClientOnly)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
                yield return Resources.UnloadUnusedAssets();
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
            isInTransition = false;

            base.OnClientSceneChanged();

            // There is no call to FadeOut here on purpose.
            // Expectation is that a LoadAdditive will follow
            // that will call FadeOut after that scene loads.
        }
        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            Debug.Log($"OnServerReady {conn} {conn.identity}");

            // This fires from a Ready message client sends to server after loading the online scene
            base.OnServerReady(conn);

            if (conn.identity == null)
                StartCoroutine(AddPlayerDelayed(conn));
        }

        // This delay is mostly for the host player that loads too fast for the
        // server to have subscenes async loaded from OnServerSceneChanged ahead of it.
        IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // Wait for server to async load all subscenes for game instances
            while (!subscenesLoaded)
                yield return null;

            // Send Scene msg to client telling it to load the first additive scene
            conn.Send(new SceneMessage { sceneName = additiveScenes[0], sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            // We have Network Start Positions in first additive scene...pick one
            Transform start = GetStartPosition();

            // Instantiate player as child of start position - this will place it in the additive scene
            // This also lets player object "inherit" pos and rot from start position transform
            GameObject player = Instantiate(playerPrefab, start);
            // now set parent null to get it out from under the Start Position object
            player.transform.SetParent(null);

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            // Finally spawn the player object for this connection
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        #endregion
    }
}


