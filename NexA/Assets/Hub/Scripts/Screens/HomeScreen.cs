using DG.Tweening;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Services;
using NexA.Hub.Utils;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Écran d'accueil principal après connexion
    /// Affiche les infos du joueur, amis en ligne, et accès rapides
    /// </summary>
    public class HomeScreen : ScreenBase
    {
        [Header("User Info")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI eloText;
        [SerializeField] private Image avatarImage;

        [Header("Quick Actions")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button friendsButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button matchHistoryButton;
        [SerializeField] private Button logoutButton;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI onlineFriendsText;
        [SerializeField] private TextMeshProUGUI totalMatchesText;
        [SerializeField] private TextMeshProUGUI winRateText;

        [Header("Animation")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private RectTransform userInfoPanel;
        [SerializeField] private RectTransform quickActionsPanel;

        public override ScreenType ScreenType => ScreenType.Home;

        private void Start()
        {
            // Bind buttons
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (friendsButton != null) friendsButton.onClick.AddListener(OnFriendsClicked);
            if (profileButton != null) profileButton.onClick.AddListener(OnProfileClicked);
            if (matchHistoryButton != null) matchHistoryButton.onClick.AddListener(OnMatchHistoryClicked);
            if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);

            // Charger les données utilisateur
            LoadUserData();

            // Animation d'entrée
            Sequence sequence = DOTween.Sequence();

            // Fade in
            mainCanvasGroup.alpha = 0f;
            sequence.Append(mainCanvasGroup.DOFade(1f, AnimationHelper.NORMAL));

            // User info panel slide from left
            if (userInfoPanel != null)
            {
                userInfoPanel.anchoredPosition = new Vector2(-300f, userInfoPanel.anchoredPosition.y);
                sequence.Join(userInfoPanel.DOAnchorPosX(0f, AnimationHelper.MEDIUM).SetEase(AnimationHelper.IN_BACK));
            }

            // Quick actions panel slide from right
            if (quickActionsPanel != null)
            {
                quickActionsPanel.anchoredPosition = new Vector2(300f, quickActionsPanel.anchoredPosition.y);
                sequence.Join(quickActionsPanel.DOAnchorPosX(0f, AnimationHelper.MEDIUM).SetEase(AnimationHelper.IN_BACK));
            }

            await sequence.AsyncWaitForCompletion();

            Debug.Log("[HomeScreen] Écran affiché");
        }

        public override async Task HideAsync()
        {
            // Animation de sortie
            Sequence sequence = DOTween.Sequence();
            sequence.Append(mainCanvasGroup.DOFade(0f, AnimationHelper.FAST));

            await sequence.AsyncWaitForCompletion();

            gameObject.SetActive(false);
            Debug.Log("[HomeScreen] Écran caché");
        }

        /// <summary>
        /// Charge et affiche les données de l'utilisateur connecté
        /// </summary>
        private void LoadUserData()
        {
            User currentUser = AuthManager.Instance.CurrentUser;

            if (currentUser == null)
            {
                Debug.LogError("[HomeScreen] Aucun utilisateur connecté!");
                return;
            }

            // Afficher les infos utilisateur
            if (usernameText != null)
                usernameText.text = currentUser.username;

            if (levelText != null)
                levelText.text = $"Niveau {currentUser.level}";

            if (eloText != null)
                eloText.text = $"Elo: {currentUser.elo}";

            // Stats (si disponibles)
            if (currentUser.stats != null)
            {
                if (totalMatchesText != null)
                    totalMatchesText.text = $"{currentUser.stats.totalMatches} parties";

                if (winRateText != null)
                    winRateText.text = $"{currentUser.stats.winRate:F1}% victoires";
            }

            // TODO: Charger avatar depuis URL si disponible
            // if (!string.IsNullOrEmpty(currentUser.avatar))
            // {
            //     StartCoroutine(LoadAvatar(currentUser.avatar));
            // }

            Debug.Log($"[HomeScreen] Données utilisateur chargées pour {currentUser.username}");
        }

        #region Button Handlers

        private void OnPlayClicked()
        {
            Debug.Log("[HomeScreen] Bouton Play cliqué");
            ToastManager.Show("Matchmaking à venir !", ToastType.Info);
            // TODO: Implémenter le matchmaking
        }

        private void OnFriendsClicked()
        {
            Debug.Log("[HomeScreen] Navigation vers Friends");
            UIManager.Instance.ShowScreen(ScreenType.Friends);
        }

        private void OnProfileClicked()
        {
            Debug.Log("[HomeScreen] Navigation vers Profile");
            UIManager.Instance.ShowScreen(ScreenType.Profile);
        }

        private void OnMatchHistoryClicked()
        {
            Debug.Log("[HomeScreen] Navigation vers Match History");
            UIManager.Instance.ShowScreen(ScreenType.MatchHistory);
        }

        private async void OnLogoutClicked()
        {
            Debug.Log("[HomeScreen] Déconnexion demandée");

            // Confirmation
            // TODO: Ajouter un dialogue de confirmation

            await ExecuteWithLoadingAsync(async () =>
            {
                await AuthManager.Instance.LogoutAsync();
                
                ToastManager.Show("Déconnexion réussie", ToastType.Success);
                
                // Retour au login
                UIManager.Instance.ShowScreen(ScreenType.Login);
            });
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup
            if (playButton != null) playButton.onClick.RemoveAllListeners();
            if (friendsButton != null) friendsButton.onClick.RemoveAllListeners();
            if (profileButton != null) profileButton.onClick.RemoveAllListeners();
            if (matchHistoryButton != null) matchHistoryButton.onClick.RemoveAllListeners();
            if (logoutButton != null) logoutButton.onClick.RemoveAllListeners();
        }
    }
}
