using DG.Tweening;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Services;
using NexA.Hub.Screens;
using Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Top bar persistante inspirée de League of Legends :
    /// [Logo] [NEXA] [HISTORIQUE] [   JOUER   ] | [avatar] [pseudo] [niveau]
    /// Le panel social droit est géré séparément (toujours visible).
    /// </summary>
    public class HubTopBar : MonoBehaviour
    {
        public static HubTopBar Instance { get; private set; }

        [Header("Bouton JOUER (central, proéminent)")]
        [SerializeField] private Button playButton;
        [SerializeField] private CanvasGroup playButtonGlow;

        [Header("Navigation (onglets)")]
        [SerializeField] private Button nexaTabButton;
        [SerializeField] private Button matchHistoryTabButton;

        [Header("Profil (haut droite)")]
        [SerializeField] private Button profileButton;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image avatarImage;

        [Header("Indicateurs d'onglet actif")]
        [SerializeField] private Image nexaTabIndicator;
        [SerializeField] private Image matchHistoryTabIndicator;

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform barTransform;

        private static readonly Color ActiveTabColor   = new(0.20f, 0.75f, 1.00f, 1f);
        private static readonly Color InactiveTabColor = new(0.55f, 0.55f, 0.60f, 1f);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            playButton.onClick.AddListener(OnPlayClicked);
            nexaTabButton.onClick.AddListener(OnNexaTabClicked);
            matchHistoryTabButton.onClick.AddListener(OnMatchHistoryClicked);
            profileButton.onClick.AddListener(OnProfileClicked);

            if (playButtonGlow != null)
            {
                playButtonGlow.DOFade(0.4f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            Hide(instant: true);
        }

        // ─── Affichage ────────────────────────────────────────────────

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshUserInfo();

            canvasGroup.alpha = 0f;
            if (barTransform != null)
                barTransform.anchoredPosition = new Vector2(barTransform.anchoredPosition.x, 80f);

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.35f).SetEase(Ease.OutCubic));
            if (barTransform != null)
                seq.Join(barTransform.DOAnchorPosY(0f, 0.35f).SetEase(Ease.OutBack));
        }

        public void Hide(bool instant = false)
        {
            if (instant)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
        }

        // ─── Données profil ───────────────────────────────────────────

        public void RefreshUserInfo()
        {
            User user = AuthManager.Instance?.CurrentUser;
            if (user == null) return;

            if (usernameText != null) usernameText.text = user.username;
            if (levelText != null)    levelText.text    = $"Niv. {user.level}";
        }

        // ─── Badge amis (affiché sur l'avatar ou ailleurs) ───────────

        public void SetFriendsOnlineBadge(int count)
        {
            // Optionnel : afficher le nb d'amis online quelque part dans la top bar
        }

        // ─── Highlight onglet actif ───────────────────────────────────

        public void SetActiveTab(ScreenType screen)
        {
            SetTabColor(nexaTabIndicator,         screen == ScreenType.Home);
            SetTabColor(matchHistoryTabIndicator, screen == ScreenType.MatchHistory);
        }

        private void SetTabColor(Image indicator, bool active)
        {
            if (indicator == null) return;
            indicator.DOColor(active ? ActiveTabColor : InactiveTabColor, 0.2f);
        }

        // ─── Handlers ─────────────────────────────────────────────────

        private void OnPlayClicked()
        {
            playButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.3f, 5);
            ToastManager.Show("Recherche d'une partie… (bientôt)", ToastType.Info);
        }

        private void OnNexaTabClicked()      => UIManager.Instance.ShowScreen(ScreenType.Home);
        private void OnMatchHistoryClicked() => UIManager.Instance.ShowScreen(ScreenType.MatchHistory);
        private void OnProfileClicked()      => UIManager.Instance.ShowScreen(ScreenType.Profile);

        private void OnDestroy()
        {
            playButton?.onClick.RemoveAllListeners();
            nexaTabButton?.onClick.RemoveAllListeners();
            matchHistoryTabButton?.onClick.RemoveAllListeners();
            profileButton?.onClick.RemoveAllListeners();
        }
    }
}
