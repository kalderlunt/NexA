using DG.Tweening;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Services;
using Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Écran de gestion des amis:
    /// - Liste des amis avec statut (online/offline/in-game)
    /// - Recherche d'utilisateurs
    /// - Demandes d'amis (envoi/acceptation/refus)
    /// - Suppression d'amis
    /// </summary>
    public class FriendsScreen : ScreenBase
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private RectTransform contentTransform;
        
        [Header("Tabs")]
        [SerializeField] private Button friendsTabButton;
        [SerializeField] private Button requestsTabButton;
        [SerializeField] private GameObject friendsTab;
        [SerializeField] private GameObject requestsTab;

        [Header("Friends List")]
        [SerializeField] private Transform friendsListContainer;
        [SerializeField] private GameObject friendItemPrefab;
        [SerializeField] private GameObject emptyFriendsText;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;
        [SerializeField] private Transform searchResultsContainer;
        [SerializeField] private GameObject searchResultItemPrefab;

        [Header("Friend Requests")]
        [SerializeField] private Transform incomingRequestsContainer;
        [SerializeField] private Transform outgoingRequestsContainer;
        [SerializeField] private GameObject requestItemPrefab;
        [SerializeField] private GameObject noRequestsText;

        [Header("Loading/Error")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        public override ScreenType ScreenType => ScreenType.Friends;

        private List<Friend> _cachedFriends = new();
        private bool _isSearching = false;

        private void Start()
        {
            // Bind events
            friendsTabButton.onClick.AddListener(() => ShowTab(true));
            requestsTabButton.onClick.AddListener(() => ShowTab(false));
            searchButton.onClick.AddListener(OnSearchClicked);
            searchInput.onSubmit.AddListener(_ => OnSearchClicked());
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);

            // Animation d'entrée
            await AnimationHelper.FadeInScreen(mainCanvasGroup, contentTransform).AsyncWaitForCompletion();

            // Charger les données
            await LoadFriendsAsync();
            ShowTab(true); // Afficher l'onglet amis par défaut
        }

        public override async Task HideAsync()
        {
            await AnimationHelper.FadeOutScreen(mainCanvasGroup, contentTransform).AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }

        #region Friends List

        private async Task LoadFriendsAsync(bool forceRefresh = false)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                // Vérifier le cache
                if (!forceRefresh)
                {
                    var cached = CacheManager.Instance.Get<List<Friend>>(CacheKeys.FRIENDS_LIST);
                    if (cached != null)
                    {
                        _cachedFriends = cached;
                        DisplayFriends(_cachedFriends);
                        return;
                    }
                }

                // Appel API
                _cachedFriends = await APIService.Instance.GetFriendsAsync();

                // Mettre en cache (1 minute)
                CacheManager.Instance.Set(CacheKeys.FRIENDS_LIST, _cachedFriends, ttlSeconds: 60);

                // Afficher
                DisplayFriends(_cachedFriends);
            });
        }

        private void DisplayFriends(List<Friend> friends)
        {
            // Clear existing
            foreach (Transform child in friendsListContainer)
            {
                Destroy(child.gameObject);
            }

            if (friends.Count == 0)
            {
                emptyFriendsText.SetActive(true);
                return;
            }

            emptyFriendsText.SetActive(false);

            // Créer les items (triés: online > in-game > offline)
            friends.Sort((a, b) =>
            {
                int aPriority = GetStatusPriority(a.status);
                int bPriority = GetStatusPriority(b.status);
                if (aPriority != bPriority)
                    return aPriority.CompareTo(bPriority);
                return a.username.CompareTo(b.username);
            });

            foreach (var friend in friends)
            {
                GameObject itemObj = Instantiate(friendItemPrefab, friendsListContainer);
                FriendListItem item = itemObj.GetComponent<FriendListItem>();
                
                if (item != null)
                {
                    item.Setup(friend, OnRemoveFriendClicked);
                }

                // Animation d'apparition échelonnée
                itemObj.transform.localScale = Vector3.zero;
                itemObj.transform.DOScale(1f, AnimationHelper.FAST)
                    .SetEase(AnimationHelper.IN_BACK)
                    .SetDelay(itemObj.transform.GetSiblingIndex() * 0.05f);
            }
        }

        private int GetStatusPriority(string status)
        {
            return status switch
            {
                "online" => 1,
                "in-game" => 2,
                "offline" => 3,
                _ => 4
            };
        }

        private async void OnRemoveFriendClicked(string friendId)
        {
            // Confirmation popup (TODO)
            bool confirmed = true; // Pour l'instant, pas de popup

            if (!confirmed) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await APIService.Instance.RemoveFriendAsync(friendId);

                // Invalider le cache
                CacheManager.Instance.Invalidate(CacheKeys.FRIENDS_LIST);

                // Recharger
                await LoadFriendsAsync(forceRefresh: true);

                ToastManager.Show("Ami supprimé", ToastType.Info);
            });
        }

        #endregion

        #region Search

        private async void OnSearchClicked()
        {
            string query = searchInput.text.Trim();

            if (string.IsNullOrEmpty(query) || query.Length < 3)
            {
                ToastManager.Show("Recherche: minimum 3 caractères", ToastType.Warning);
                return;
            }

            if (_isSearching) return;

            _isSearching = true;

            await ExecuteWithLoadingAsync(async () =>
            {
                var users = await APIService.Instance.SearchUsersAsync(query);

                DisplaySearchResults(users);
            });

            _isSearching = false;
        }

        private void DisplaySearchResults(List<User> users)
        {
            // Clear existing
            foreach (Transform child in searchResultsContainer)
            {
                Destroy(child.gameObject);
            }

            if (users.Count == 0)
            {
                ToastManager.Show("Aucun résultat", ToastType.Info);
                return;
            }

            foreach (var user in users)
            {
                GameObject itemObj = Instantiate(searchResultItemPrefab, searchResultsContainer);
                SearchResultItem item = itemObj.GetComponent<SearchResultItem>();

                if (item != null)
                {
                    item.Setup(user, OnSendFriendRequestClicked);
                }

                // Animation
                itemObj.transform.localScale = Vector3.zero;
                itemObj.transform.DOScale(1f, AnimationHelper.FAST)
                    .SetEase(AnimationHelper.IN_BACK)
                    .SetDelay(itemObj.transform.GetSiblingIndex() * 0.05f);
            }
        }

        private async void OnSendFriendRequestClicked(string userId)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await APIService.Instance.SendFriendRequestAsync(userId);

                // Invalider le cache des requests
                CacheManager.Instance.Invalidate(CacheKeys.FRIEND_REQUESTS);

                ToastManager.Show("Demande d'ami envoyée !", ToastType.Success);

                // Recharger la recherche pour mettre à jour le bouton
                OnSearchClicked();
            });
        }

        #endregion

        #region Friend Requests

        private async Task LoadFriendRequestsAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var requests = await APIService.Instance.GetFriendRequestsAsync();

                // Séparer incoming et outgoing (à ajuster selon l'API réelle)
                // Pour l'instant on affiche tout comme incoming
                DisplayFriendRequests(requests, new List<FriendRequest>());
            });
        }

        private void DisplayFriendRequests(List<FriendRequest> incoming, List<FriendRequest> outgoing)
        {
            // Clear existing
            foreach (Transform child in incomingRequestsContainer)
                Destroy(child.gameObject);
            foreach (Transform child in outgoingRequestsContainer)
                Destroy(child.gameObject);

            // Incoming
            if (incoming.Count == 0)
            {
                // Afficher "Aucune demande"
            }
            else
            {
                foreach (var request in incoming)
                {
                    GameObject itemObj = Instantiate(requestItemPrefab, incomingRequestsContainer);
                    FriendRequestItem item = itemObj.GetComponent<FriendRequestItem>();

                    if (item != null)
                    {
                        item.Setup(request, true, OnAcceptRequestClicked, OnDeclineRequestClicked);
                    }
                }
            }

            // Outgoing
            if (outgoing.Count > 0)
            {
                foreach (var request in outgoing)
                {
                    GameObject itemObj = Instantiate(requestItemPrefab, outgoingRequestsContainer);
                    FriendRequestItem item = itemObj.GetComponent<FriendRequestItem>();

                    if (item != null)
                    {
                        item.Setup(request, false, null, null);
                    }
                }
            }
        }

        private async void OnAcceptRequestClicked(string requestId)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await APIService.Instance.AcceptFriendRequestAsync(requestId);

                // Invalider les caches
                CacheManager.Instance.Invalidate(CacheKeys.FRIEND_REQUESTS);
                CacheManager.Instance.Invalidate(CacheKeys.FRIENDS_LIST);

                // Recharger
                await LoadFriendRequestsAsync();
                await LoadFriendsAsync(forceRefresh: true);

                ToastManager.Show("Demande acceptée !", ToastType.Success);
            });
        }

        private async void OnDeclineRequestClicked(string requestId)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await APIService.Instance.DeclineFriendRequestAsync(requestId);

                // Invalider le cache
                CacheManager.Instance.Invalidate(CacheKeys.FRIEND_REQUESTS);

                // Recharger
                await LoadFriendRequestsAsync();

                ToastManager.Show("Demande refusée", ToastType.Info);
            });
        }

        #endregion

        #region Tabs

        private void ShowTab(bool showFriends)
        {
            friendsTab.SetActive(showFriends);
            requestsTab.SetActive(!showFriends);

            // Update button states (TODO: visual feedback)

            if (!showFriends)
            {
                // Charger les requests si on affiche l'onglet
                _ = LoadFriendRequestsAsync();
            }
        }

        #endregion

        #region Loading/Error

        protected override void ShowLoading(bool show)
        {
            loadingPanel.SetActive(show);
            mainCanvasGroup.interactable = !show;
        }

        protected override void ShowError(string message)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
            AnimationHelper.ShakeError(errorPanel.transform);

            DOVirtual.DelayedCall(5f, () => errorPanel.SetActive(false));
        }

        #endregion

        private void OnDestroy()
        {
            friendsTabButton.onClick.RemoveAllListeners();
            requestsTabButton.onClick.RemoveAllListeners();
            searchButton.onClick.RemoveAllListeners();
        }
    }

    // ============================================
    // Components (à créer dans des fichiers séparés)
    // ============================================

    /// <summary>
    /// Item de la liste d'amis
    /// </summary>
    public class FriendListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image statusIndicator;
        [SerializeField] private Button removeButton;

        private string _friendId;
        private Action<string> _onRemoveCallback;

        public void Setup(Friend friend, Action<string> onRemove)
        {
            _friendId = friend.id;
            _onRemoveCallback = onRemove;

            usernameText.text = friend.username;
            levelText.text = $"Niv. {friend.level}";

            // Status color
            statusIndicator.color = friend.status switch
            {
                "online" => Color.green,
                "in-game" => new Color(1f, 0.7f, 0.2f), // Orange
                _ => Color.gray
            };

            removeButton.onClick.AddListener(() => _onRemoveCallback?.Invoke(_friendId));
        }
    }

    /// <summary>
    /// Item de résultat de recherche
    /// </summary>
    public class SearchResultItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button addButton;
        [SerializeField] private GameObject alreadyFriendIndicator;

        private string _userId;
        private Action<string> _onAddCallback;

        public void Setup(User user, Action<string> onAdd)
        {
            _userId = user.id;
            _onAddCallback = onAdd;

            usernameText.text = user.username;
            levelText.text = $"Niv. {user.level}";

            // Si déjà ami, désactiver le bouton
            if (user.isFriend)
            {
                addButton.gameObject.SetActive(false);
                alreadyFriendIndicator.SetActive(true);
            }
            else
            {
                addButton.onClick.AddListener(() => _onAddCallback?.Invoke(_userId));
            }
        }
    }

    /// <summary>
    /// Item de demande d'ami
    /// </summary>
    public class FriendRequestItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        private string _requestId;

        public void Setup(FriendRequest request, bool isIncoming, Action<string> onAccept, Action<string> onDecline)
        {
            _requestId = request.id;

            User displayUser = isIncoming ? request.from : request.to;
            usernameText.text = displayUser.username;

            if (isIncoming)
            {
                acceptButton.gameObject.SetActive(true);
                declineButton.gameObject.SetActive(true);

                acceptButton.onClick.AddListener(() => onAccept?.Invoke(_requestId));
                declineButton.onClick.AddListener(() => onDecline?.Invoke(_requestId));
            }
            else
            {
                // Outgoing request, pas de boutons d'action
                acceptButton.gameObject.SetActive(false);
                declineButton.gameObject.SetActive(false);
            }
        }
    }
}

