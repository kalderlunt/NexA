using DG.Tweening;
using NexA.Hub.Services;
using NexA.Hub.Utils;
using System;
using System.Threading.Tasks;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Écran de login avec validation, animations, et gestion d'erreurs
    /// </summary>
    public class LoginScreen : ScreenBase
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Animation")]
        [SerializeField] private CanvasGroup formCanvasGroup;
        [SerializeField] private RectTransform logoTransform;

        public override ScreenType ScreenType => ScreenType.Login;

        private void Start()
        {
            // Bind events
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);

            // Input validation en temps réel
            emailInput.onValueChanged.AddListener(_ => ValidateInputs());
            passwordInput.onValueChanged.AddListener(_ => ValidateInputs());

            ValidateInputs();
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);
            HideError();

            // Animation d'entrée
            Sequence sequence = DOTween.Sequence();
            
            // Logo bounce
            logoTransform.localScale = Vector3.zero;
            sequence.Append(logoTransform.DOScale(1f, AnimationHelper.MEDIUM).SetEase(AnimationHelper.IN_BACK));
            
            // Form fade in
            sequence.AppendInterval(0.1f);
            sequence.Append(formCanvasGroup.DOFade(1f, AnimationHelper.NORMAL).SetEase(AnimationHelper.IN_SMOOTH).From(0));
            
            formCanvasGroup.interactable = true;

            await sequence.AsyncWaitForCompletion();
        }

        public override async Task HideAsync()
        {
            formCanvasGroup.interactable = false;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(formCanvasGroup.DOFade(0f, AnimationHelper.FAST));
            
            await sequence.AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }

        private async void OnLoginClicked()
        {
            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            await ExecuteWithLoadingAsync(async () =>
            {
                User user = await AuthManager.Instance.LoginAsync(email, password);
                
                // Success
                Debug.Log($"[Login] Success! Welcome {user.username}");
                ToastManager.Show($"Bienvenue, {user.username} !", ToastType.Success);
                
                // Transition vers Home
                UIManager.Instance.ShowScreen(ScreenType.Home);
            });
        }

        private void OnRegisterClicked()
        {
            UIManager.Instance.ShowScreen(ScreenType.Register);
        }

        private void ValidateInputs()
        {
            bool hasEmail = !string.IsNullOrWhiteSpace(emailInput.text);
            bool hasPassword = !string.IsNullOrWhiteSpace(passwordInput.text);
            bool isPasswordLongEnough = passwordInput.text.Length >= ValidationConstants.MinPasswordLength;
            
            bool isValid = hasEmail && hasPassword && isPasswordLongEnough;
            
            loginButton.interactable = isValid;
        }

        protected override void ShowLoading(bool show)
        {
            loadingPanel.SetActive(show);
            formCanvasGroup.interactable = !show;
            
            if (show)
            {
                // Animer le spinner
                Transform spinner = loadingPanel.transform.Find("Spinner");
                if (spinner != null)
                    AnimationHelper.StartLoadingSpinner(spinner);
            }
        }

        protected override void ShowError(string message)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
            
            // Shake l'error panel
            AnimationHelper.ShakeError(errorPanel.transform);
            
            // Auto-hide après 5s
            DOVirtual.DelayedCall(5f, () => HideError());
        }

        private void HideError()
        {
            errorPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            loginButton.onClick.RemoveAllListeners();
            registerButton.onClick.RemoveAllListeners();
        }
    }
}

