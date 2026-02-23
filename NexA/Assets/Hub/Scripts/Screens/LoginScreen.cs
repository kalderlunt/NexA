using DG.Tweening;
using NexA.Hub.Services;
using Utils;
using System.Collections;
using System.Threading.Tasks;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Utils;
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
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Animation")]
        [SerializeField] private CanvasGroup formCanvasGroup;
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private Transform spinner;
        
        [Header("Screens")]
        [SerializeField] private ScreenType registerScreen = ScreenType.RegisterMultiStep;
        [SerializeField] private ScreenType homeScreen = ScreenType.Home;
        
        public override ScreenType ScreenType => ScreenType.Login;

        private void Start()
        {
            // Bind events
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);

            // Input validation en temps réel
            usernameInput.onValueChanged.AddListener(_ => ValidateInputs());
            passwordInput.onValueChanged.AddListener(_ => ValidateInputs());

            ValidateInputs();
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);
            errorPanel.SetActive(false);

            // Animation d'entrée
            Sequence sequence = DOTween.Sequence();
            
            // Form fade in
            sequence.Append(formCanvasGroup.DOFade(1f, AnimationHelper.NORMAL).SetEase(AnimationHelper.IN_SMOOTH).From(0));
            formCanvasGroup.interactable = true; 
            
            //sequence.AppendInterval(0.1f);
            
            // Logo bounce
            logoTransform.localScale = Vector3.zero;
            sequence.Append(logoTransform.DOScale(1f, AnimationHelper.MEDIUM).SetEase(AnimationHelper.IN_BACK));

            await sequence.AsyncWaitForCompletion();
            
            // Si un username est passé en paramètre (depuis l'inscription), l'animer dans le champ
            if (data is string username && !string.IsNullOrEmpty(username))
            {
                usernameInput.text = ""; // Start empty
                AnimationHelper.TypewriterEffect(usernameInput.textComponent as TextMeshProUGUI, username, AnimationHelper.MEDIUM, () =>
                {
                    usernameInput.text = username; // Ensure it's set
                    ValidateInputs(); // Update button state
                });
            }
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
            // Prevent multiple login attempts
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            await ExecuteWithLoadingAsync(async () =>
            {
                User user = await AuthManager.Instance.LoginAsync(username, password);
                
                // Success
                Debug.Log($"[Login] Success! Welcome {user.username}");
                ToastManager.Show($"Bienvenue, {user.username} !", ToastType.Success);
                
                // Transition vers Home
                UIManager.Instance.ShowScreen(homeScreen);
            });
        }

        private void OnRegisterClicked()
        {
            UIManager.Instance.ShowScreen(registerScreen);
        }

        private void ValidateInputs()
        {
            bool hasUsername = !string.IsNullOrWhiteSpace(usernameInput.text);
            bool hasPassword = !string.IsNullOrWhiteSpace(passwordInput.text);
            bool isPasswordLongEnough = passwordInput.text.Length >= ValidationConstants.MinPasswordLength;
            
            bool isValid = hasUsername && hasPassword && isPasswordLongEnough;
            
            loginButton.interactable = isValid;
        }

        protected override void ShowLoading(bool show)
        {
            loadingPanel.SetActive(show);
            formCanvasGroup.interactable = !show;
            
            if (show)
            {
                // Animer le spinner
                if (!spinner)
                    AnimationHelper.StartLoadingSpinner(spinner);
                else
                    Debug.LogError("Loading spinner animation");
            }
        }

        protected override void ShowError(string message)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
            
            AnimationHelper.ShakeError(errorPanel.transform);
            
            StopAllCoroutines();
            StartCoroutine(HideErrorAfterDelay(5f));
        }

        private IEnumerator HideErrorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            errorPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            loginButton.onClick.RemoveAllListeners();
            registerButton.onClick.RemoveAllListeners();
        }
    }
}

