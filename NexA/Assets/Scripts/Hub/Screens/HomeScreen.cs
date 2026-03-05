using DG.Tweening;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Services;
using Utils;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Écran d'accueil "NEXA" — page centrale après connexion.
    /// La navigation (Play, Profil, Amis, etc.) est gérée par HubTopBar.
    /// Cet écran affiche : bannière centrale + stats rapides.
    /// </summary>
    public class HomeScreen : ScreenBase
    {
        [Header("Contenu central")]
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private RectTransform contentPanel;

        [Header("Stats rapides")]
        [SerializeField] private TextMeshProUGUI totalMatchesText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private TextMeshProUGUI eloText;

        [Header("Bannière / Message de bienvenue")]
        [SerializeField] private TextMeshProUGUI welcomeText;

        [Header("Bouton Déconnexion (optionnel)")]
        [SerializeField] private Button logoutButton;

        [Header("Panel Social (enfant de HomeScreen)")]
        [SerializeField] private SocialPanel socialPanel;

        public override ScreenType ScreenType => ScreenType.Home;

        private void Start()
        {
            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);

            socialPanel?.Show();   // ← panel social visible avec HomeScreen

            LoadUserData();

            // Animation d'entrée
            mainCanvasGroup.alpha = 0f;
            Sequence seq = DOTween.Sequence();
            seq.Append(mainCanvasGroup.DOFade(1f, AnimationHelper.NORMAL));

            if (contentPanel != null)
            {
                contentPanel.localScale = new Vector3(0.95f, 0.95f, 1f);
                seq.Join(contentPanel.DOScale(1f, AnimationHelper.MEDIUM).SetEase(Ease.OutBack));
            }

            await seq.AsyncWaitForCompletion();
        }

        public override async Task HideAsync()
        {
            socialPanel?.Hide(instant: true); // ← cacher immédiatement avec l'écran
            await mainCanvasGroup.DOFade(0f, AnimationHelper.FAST).AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }

        private void LoadUserData()
        {
            User user = AuthManager.Instance?.CurrentUser;
            if (user == null) return;

            if (welcomeText != null)
                welcomeText.text = $"Bienvenue, {user.username} !";

            if (eloText != null)
                eloText.text = $"Elo : {user.elo}";

            if (user.stats != null)
            {
                if (totalMatchesText != null)
                    totalMatchesText.text = $"{user.stats.totalMatches} parties";
                if (winRateText != null)
                    winRateText.text = $"{user.stats.winRate:F1}% victoires";
            }
        }

        private async void OnLogoutClicked()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await AuthManager.Instance.LogoutAsync();
                ToastManager.Show("Déconnexion réussie", ToastType.Success);
                UIManager.Instance.ShowScreen(ScreenType.Login);
            });
        }

        private void OnDestroy()
        {
            logoutButton?.onClick.RemoveAllListeners();
        }
    }
}
