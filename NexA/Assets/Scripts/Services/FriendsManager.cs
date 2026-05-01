using System;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Gestionnaire temps réel des statuts d'amis via WebSocket/STOMP.
    ///
    /// Flow :
    ///  1. Après login, appeler ConnectAsync(token, userId)
    ///  2. S'abonne automatiquement à /user/{userId}/queue/friend-status
    ///  3. Reçoit les changements de statut (ONLINE / OFFLINE / IN_GAME) en temps réel
    ///  4. Déclenche l'événement OnFriendStatusChanged pour que l'UI se mette à jour
    ///  5. Au logout, appeler DisconnectAsync() — le backend met aussi OFFLINE si le WS se ferme
    /// </summary>
    public class FriendsManager : MonoBehaviour
    {
        #region Singleton

        public static FriendsManager Instance { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Déclenché quand un ami change de statut.
        /// Les composants UI (SocialPanel, etc.) s'abonnent à cet événement.
        /// </summary>
        public event Action<FriendStatusUpdate> OnFriendStatusChanged;

        /// <summary>
        /// Déclenché quand une demande d'ami est acceptée (les deux côtés reçoivent cet event).
        /// Émis après confirmation BDD côté backend via STOMP /topic/friend-request.{userId}.
        /// </summary>
        public event Action<FriendAcceptedNotification> OnFriendAccepted;

        #endregion

        #region Configuration

        [Header("WebSocket")]
        [SerializeField] private string wsPath = "/ws";

        [Header("Reconnexion")]
        [SerializeField] private float reconnectBaseDelay = 2f;
        [SerializeField] private float reconnectMaxDelay = 30f;
        [SerializeField] private int maxReconnectAttempts = 10;

        #endregion

        #region Private Fields

        private WebSocket _ws;
        private string _token;
        private string _userId;
        private bool _connected;
        private bool _shouldReconnect;
        private int _reconnectAttempts;
        private bool _stompConnected;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // NativeWebSocket nécessite un dispatch des messages sur le main thread
#if !UNITY_WEBGL || UNITY_EDITOR
            _ws?.DispatchMessageQueue();
#endif
        }

        private void OnApplicationQuit()
        {
            _shouldReconnect = false;
            _ = DisconnectAsync();
        }

        private void OnDestroy()
        {
            _shouldReconnect = false;
            _ = DisconnectAsync();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Connecte le WebSocket STOMP et s'abonne aux notifications de statut.
        /// Appeler après un login réussi.
        /// </summary>
        public async Task ConnectAsync(string token, string userId)
        {
            _token = token;
            _userId = userId;
            _shouldReconnect = true;
            _reconnectAttempts = 0;

            await ConnectWebSocketAsync();
        }

        /// <summary>
        /// Ferme proprement la connexion STOMP puis le WebSocket.
        /// Appeler au logout.
        /// </summary>
        public async Task DisconnectAsync()
        {
            _shouldReconnect = false;
            _stompConnected = false;

            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    // Envoyer STOMP DISCONNECT
                    await _ws.SendText(StompClient.BuildDisconnectFrame());
                    await _ws.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[FriendsManager] Erreur fermeture WS : {ex.Message}");
                }
            }

            _ws = null;
            _connected = false;
            Debug.Log("[FriendsManager] WebSocket déconnecté");
        }

        /// <summary>Vrai si le WebSocket est ouvert et STOMP connecté.</summary>
        public bool IsConnected => _connected && _stompConnected;

        #endregion

        #region Private — Connection

        private async Task ConnectWebSocketAsync()
        {
            // Construire l'URL WS à partir de la config centralisée dans APIService
            string baseUrl = APIService.Instance.BaseURL;
            string port = APIService.Instance.Port;

            // Extraire le host sans le schéma http(s)://
            string host = baseUrl.Replace("https://", "").Replace("http://", "").TrimEnd('/');
            string scheme = baseUrl.StartsWith("https") ? "wss" : "ws";
            string portPart = string.IsNullOrEmpty(port) ? "" : $":{port}";
            string url = $"{scheme}://{host}{portPart}{wsPath}";

            Debug.Log($"[FriendsManager] Connexion WebSocket vers : {url}");


            _ws = new WebSocket(url);

            _ws.OnOpen += OnWebSocketOpen;
            _ws.OnMessage += OnWebSocketMessage;
            _ws.OnError += OnWebSocketError;
            _ws.OnClose += OnWebSocketClose;

            await _ws.Connect();
        }

        private async void OnWebSocketOpen()
        {
            Debug.Log("[FriendsManager] WebSocket ouvert — envoi STOMP CONNECT");
            _connected = true;
            _reconnectAttempts = 0;

            // Envoyer la frame STOMP CONNECT avec le JWT
            string connectFrame = StompClient.BuildConnectFrame(_token);
            await _ws.SendText(connectFrame);
        }

        private void OnWebSocketMessage(byte[] data)
        {
            string raw = System.Text.Encoding.UTF8.GetString(data);

            var frame = StompClient.Parse(raw);
            if (frame == null) return;

            switch (frame.Command)
            {
                case "CONNECTED":
                    OnStompConnected(frame);
                    break;

                case "MESSAGE":
                    OnStompMessage(frame);
                    break;

                case "ERROR":
                    Debug.LogError($"[FriendsManager] STOMP ERROR : {frame.Body}");
                    break;

                case "HEARTBEAT":
                    // Répondre au heartbeat (frame vide = \n)
                    _ = _ws.SendText("\n");
                    break;

                default:
                    Debug.Log($"[FriendsManager] Frame STOMP ignorée : {frame.Command}");
                    break;
            }
        }

        private async void OnStompConnected(StompClient.StompFrame frame)
        {
            _stompConnected = true;
            Debug.Log("[FriendsManager] STOMP connecté — abonnement aux statuts d'amis");

            // Statuts d'amis (présence)
            string presenceDest = $"/topic/presence.{_userId}";
            await _ws.SendText(StompClient.BuildSubscribeFrame("sub-friend-status", presenceDest));
            Debug.Log($"[FriendsManager] Abonné à {presenceDest}");

            // Acceptations de demandes d'ami (les deux côtés)
            string friendReqDest = $"/topic/friend-request.{_userId}";
            await _ws.SendText(StompClient.BuildSubscribeFrame("sub-friend-request", friendReqDest));
            Debug.Log($"[FriendsManager] Abonné à {friendReqDest}");
        }

        private void OnStompMessage(StompClient.StompFrame frame)
        {
            if (string.IsNullOrEmpty(frame.Body)) return;

            frame.Headers.TryGetValue("subscription", out string subId);

            try
            {
                if (subId == "sub-friend-request")
                {
                    var notification = JsonConvert.DeserializeObject<FriendAcceptedNotification>(frame.Body);
                    if (notification != null)
                    {
                        Debug.Log($"[FriendsManager] Demande acceptée — nouvel ami : {notification.username}");
                        OnFriendAccepted?.Invoke(notification);
                    }
                    return;
                }

                // Par défaut : changement de statut d'un ami
                var update = JsonConvert.DeserializeObject<FriendStatusUpdate>(frame.Body);
                if (update == null) return;

                if (update.status == "ONLINE")
                    Debug.Log($"[FriendsManager] {update.username} vient de se connecter");
                else if (update.status == "OFFLINE")
                    Debug.Log($"[FriendsManager] {update.username} vient de se déconnecter");
                else
                    Debug.Log($"[FriendsManager] {update.username} → {update.status}");

                OnFriendStatusChanged?.Invoke(update);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FriendsManager] Erreur parsing message STOMP : {ex.Message}");
            }
        }

        private void OnWebSocketError(string error)
        {
            Debug.LogError($"[FriendsManager] WebSocket erreur : {error}\n" +
                           $"URL tentée : {APIService.Instance.BaseURL} | wsPath={wsPath}");
        }

        private async void OnWebSocketClose(WebSocketCloseCode closeCode)
        {
            Debug.Log($"[FriendsManager] WebSocket fermé (code: {closeCode})");
            _connected = false;
            _stompConnected = false;

            if (_shouldReconnect && _reconnectAttempts < maxReconnectAttempts)
            {
                _reconnectAttempts++;
                float delay = Mathf.Min(reconnectBaseDelay * Mathf.Pow(2, _reconnectAttempts - 1), reconnectMaxDelay);
                Debug.Log($"[FriendsManager] Reconnexion dans {delay:F1}s (tentative {_reconnectAttempts}/{maxReconnectAttempts})");

                await Task.Delay((int)(delay * 1000));

                if (_shouldReconnect)
                {
                    // Rafraîchir le token avant de reconnecter
                    try
                    {
                        _token = await AuthManager.Instance.GetValidAccessTokenAsync();
                        await ConnectWebSocketAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FriendsManager] Échec reconnexion : {ex.Message}");
                    }
                }
            }
            else if (_reconnectAttempts >= maxReconnectAttempts)
            {
                Debug.LogError("[FriendsManager] Nombre max de tentatives de reconnexion atteint");
            }
        }

        #endregion
    }

    // ── Modèle ─────────────────────────────────────────────────────────

    /// <summary>
    /// Message reçu via STOMP quand un ami change de statut.
    /// Format backend : { "userId": "...", "username": "...", "status": "ONLINE|OFFLINE|IN_GAME" }
    /// </summary>
    [Serializable]
    public class FriendStatusUpdate
    {
        public string userId;
        public string username;
        public string status; // "ONLINE", "OFFLINE", "IN_GAME"
    }

    /// <summary>
    /// Message reçu via STOMP quand une demande d'ami est acceptée.
    /// Émis par le backend sur /topic/friend-request.{userId} pour les deux parties.
    /// Format backend : { "type": "FRIEND_REQUEST_ACCEPTED", "friendshipId": "...", "userId": "...", "username": "..." }
    /// </summary>
    [Serializable]
    public class FriendAcceptedNotification
    {
        public string type;        // "FRIEND_REQUEST_ACCEPTED"
        public string friendshipId;
        public string userId;      // ID du nouvel ami
        public string username;    // Pseudo du nouvel ami
    }
}