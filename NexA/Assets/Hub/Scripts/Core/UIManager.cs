using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using NexA.Hub.Screens;
using NexA.Hub.Utils;
using UnityEngine;

namespace NexA.Hub.Core
{
    /// <summary>
    /// Singleton - Gère la navigation entre les écrans avec transitions animées
    /// State machine pour assurer les transitions autorisées
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Configuration")]
        [Header("UI Configuration")]
        [SerializeField] private Transform screensContainer;
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private Transform toastContainer;

        [Header("Transition Settings")]
        [SerializeField] private float overlayFadeDuration = 0.2f;

        public Transform ToastContainer => toastContainer;

        private Dictionary<ScreenType, ScreenBase> screens = new();
        private ScreenType currentScreen = ScreenType.None;
        private bool isTransitioning = false;

        // Transitions autorisées
        private static readonly Dictionary<ScreenType, List<ScreenType>> AllowedTransitions = new()
        {
            { ScreenType.None, new() { ScreenType.Login } },
            { ScreenType.Login, new() { ScreenType.Register, ScreenType.Home } },
            { ScreenType.Register, new() { ScreenType.Login } },
            { ScreenType.Home, new() { ScreenType.Profile, ScreenType.Friends, ScreenType.MatchHistory, ScreenType.Login } },
            { ScreenType.Profile, new() { ScreenType.Home } },
            { ScreenType.Friends, new() { ScreenType.Home, ScreenType.Profile } },
            { ScreenType.MatchHistory, new() { ScreenType.Home, ScreenType.MatchDetails } },
            { ScreenType.MatchDetails, new() { ScreenType.MatchHistory } }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAllScreens();
            InitializeFadeOverlay();
        }

        private void Start()
        {
            // Démarrer sur l'écran de login
            ShowScreen(ScreenType.Login);
        }

        /// <summary>
        /// Charger tous les écrans enfants du container
        /// </summary>
        private void LoadAllScreens()
        {
            foreach (Transform child in screensContainer)
            {
                var screen = child.GetComponent<ScreenBase>();
                if (screen != null)
                {
                    screens[screen.ScreenType] = screen;
                    screen.gameObject.SetActive(false);
                    Debug.Log($"[UIManager] Loaded screen: {screen.ScreenType}");
                }
            }

            Debug.Log($"[UIManager] Total screens loaded: {screens.Count}");
        }

        private void InitializeFadeOverlay()
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0;
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Afficher un écran avec transition animée
        /// </summary>
        public async void ShowScreen(ScreenType screenType, object data = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[UIManager] Transition already in progress, ignoring request for {screenType}");
                return;
            }

            if (currentScreen == screenType)
            {
                Debug.LogWarning($"[UIManager] Already on screen {screenType}");
                return;
            }

            // Vérifier si transition autorisée
            if (!IsTransitionAllowed(currentScreen, screenType))
            {
                Debug.LogWarning($"[UIManager] Transition {currentScreen} → {screenType} not allowed");
                return;
            }

            await TransitionToScreen(screenType, data);
        }

        /// <summary>
        /// Revenir à l'écran précédent (si autorisé)
        /// </summary>
        public void GoBack()
        {
            ScreenType previousScreen = GetPreviousScreen(currentScreen);
            if (previousScreen != ScreenType.None)
            {
                ShowScreen(previousScreen);
            }
        }

        private async Task TransitionToScreen(ScreenType screenType, object data)
        {
            isTransitioning = true;

            try
            {
                // 1. Hide current screen
                if (currentScreen != ScreenType.None && screens.ContainsKey(currentScreen))
                {
                    Debug.Log($"[UIManager] Hiding screen: {currentScreen}");
                    await screens[currentScreen].HideAsync();
                }

                // 2. Fade overlay in
                await FadeOverlayAsync(true);

                // 3. Switch screens
                currentScreen = screenType;
                var newScreen = screens[screenType];

                // 4. Fade overlay out
                await FadeOverlayAsync(false);

                // 5. Show new screen
                Debug.Log($"[UIManager] Showing screen: {screenType}");
                await newScreen.ShowAsync(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIManager] Error during transition: {ex.Message}");
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private async Task FadeOverlayAsync(bool fadeIn)
        {
            if (fadeOverlay == null) return;

            float targetAlpha = fadeIn ? 1f : 0f;
            fadeOverlay.blocksRaycasts = fadeIn;

            await fadeOverlay.DOFade(targetAlpha, overlayFadeDuration)
                .SetEase(AnimationHelper.IN_SMOOTH)
                .AsyncWaitForCompletion();
        }

        private bool IsTransitionAllowed(ScreenType from, ScreenType to)
        {
            if (!AllowedTransitions.ContainsKey(from))
                return false;

            return AllowedTransitions[from].Contains(to);
        }

        private ScreenType GetPreviousScreen(ScreenType current)
        {
            // Logique simple de "back" (peut être amélioré avec une stack)
            return current switch
            {
                ScreenType.Register => ScreenType.Login,
                ScreenType.Profile => ScreenType.Home,
                ScreenType.Friends => ScreenType.Home,
                ScreenType.MatchHistory => ScreenType.Home,
                ScreenType.MatchDetails => ScreenType.MatchHistory,
                _ => ScreenType.None
            };
        }

        /// <summary>
        /// Obtenir l'écran actuel
        /// </summary>
        public ScreenType GetCurrentScreen() => currentScreen;

        /// <summary>
        /// Vérifier si un écran est chargé
        /// </summary>
        public bool IsScreenLoaded(ScreenType screenType) => screens.ContainsKey(screenType);

        private void OnDestroy()
        {
            // Cleanup DOTween
            DOTween.KillAll();
        }
    }
}

