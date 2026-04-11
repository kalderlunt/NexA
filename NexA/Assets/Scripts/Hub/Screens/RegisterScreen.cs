using DG.Tweening;
using Utils;
using System.Threading.Tasks;
using NexA.Hub.Components;
using NexA.Hub.Core;
using NexA.Hub.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Écran d'inscription avec validation complète, animations, et gestion d'erreurs
    /// </summary>
    public class RegisterScreen : ScreenBase
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField confirmPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button backToLoginButton;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Validation Feedback")]
        [SerializeField] private TextMeshProUGUI usernameValidationText;
        [SerializeField] private TextMeshProUGUI passwordValidationText;
        [SerializeField] private TextMeshProUGUI confirmPasswordValidationText;

        [Header("Animation")]
        [SerializeField] private CanvasGroup formCanvasGroup;
        [SerializeField] private RectTransform headerTransform;

        public override ScreenType ScreenType => ScreenType.Register;

        private void Start()
        {
            // Bind events
            registerButton.onClick.AddListener(OnRegisterClicked);
            backToLoginButton.onClick.AddListener(OnBackToLoginClicked);

            // Input validation en temps réel
            usernameInput.onValueChanged.AddListener(_ => ValidateInputs());
            emailInput.onValueChanged.AddListener(_ => ValidateInputs());
            passwordInput.onValueChanged.AddListener(_ => ValidateInputs());
            confirmPasswordInput.onValueChanged.AddListener(_ => ValidateInputs());

            // Hide validation texts by default
            HideValidationTexts();
            ValidateInputs();
        }

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);
            HideError();
            HideValidationTexts();

            // Animation d'entrée
            Sequence sequence = DOTween.Sequence();
            
            // Header slide in from top
            headerTransform.anchoredPosition = new Vector2(0, 100);
            sequence.Append(headerTransform.DOAnchorPosY(0, AnimationHelper.MEDIUM).SetEase(AnimationHelper.OUT_SMOOTH));
            
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

        private async void OnRegisterClicked()
        {
            string username = usernameInput.text.Trim();
            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            await ExecuteWithLoadingAsync(async () =>
            {
                //bool isRegister = await AuthManager.Instance.RegisterAsync(username, email, password);
                
                // Success
                Debug.Log($"[Register] Success! Welcome {username}");
                ToastManager.Show($"Compte créé avec succès ! Bienvenue, {username}", ToastType.Success);
                
                // Transition vers Home
                UIManager.Instance.ShowScreen(ScreenType.Login);
            });
        }

        private void OnBackToLoginClicked()
        {
            UIManager.Instance.ShowScreen(ScreenType.Login);
        }

        private void ValidateInputs()
        {
            // Validation username
            bool hasUsername = !string.IsNullOrWhiteSpace(usernameInput.text);
            bool isUsernameLongEnough = usernameInput.text.Length >= ValidationConstants.MinUsernameLength;
            bool isUsernameShortEnough = usernameInput.text.Length <= ValidationConstants.MaxUsernameLength;
            bool isUsernameValid = hasUsername && isUsernameLongEnough && isUsernameShortEnough;
            
            // Validation email
            bool hasEmail = !string.IsNullOrWhiteSpace(emailInput.text);
            bool isEmailValid = hasEmail && IsValidEmail(emailInput.text);
            
            // Validation password
            bool hasPassword = !string.IsNullOrWhiteSpace(passwordInput.text);
            bool isPasswordLongEnough = passwordInput.text.Length >= ValidationConstants.MinPasswordLength;
            
            // Validation confirm password
            bool hasConfirmPassword = !string.IsNullOrWhiteSpace(confirmPasswordInput.text);
            bool passwordsMatch = passwordInput.text == confirmPasswordInput.text;
            
            // Update validation feedback
            UpdateUsernameValidation(isUsernameValid, isUsernameLongEnough, isUsernameShortEnough);
            UpdatePasswordValidation(isPasswordLongEnough);
            UpdateConfirmPasswordValidation(passwordsMatch, hasConfirmPassword);
            
            // Enable button only if all validations pass
            bool isValid = isUsernameValid && isEmailValid && hasPassword && isPasswordLongEnough && hasConfirmPassword && passwordsMatch;
            registerButton.interactable = isValid;
        }

        private void UpdateUsernameValidation(bool isValid, bool longEnough, bool shortEnough)
        {
            if (string.IsNullOrWhiteSpace(usernameInput.text))
            {
                usernameValidationText.gameObject.SetActive(false);
                return;
            }

            if (!isValid)
            {
                usernameValidationText.gameObject.SetActive(true);
                if (!longEnough)
                    usernameValidationText.text = $"Minimum {ValidationConstants.MinUsernameLength} caractères";
                else if (!shortEnough)
                    usernameValidationText.text = $"Maximum {ValidationConstants.MaxUsernameLength} caractères";
                usernameValidationText.color = new Color(1f, 0.3f, 0.3f); // Rouge
            }
            else
            {
                usernameValidationText.gameObject.SetActive(true);
                usernameValidationText.text = "✓ Nom d'utilisateur valide";
                usernameValidationText.color = new Color(0.3f, 1f, 0.3f); // Vert
            }
        }

        private void UpdatePasswordValidation(bool isLongEnough)
        {
            if (string.IsNullOrWhiteSpace(passwordInput.text))
            {
                passwordValidationText.gameObject.SetActive(false);
                return;
            }

            passwordValidationText.gameObject.SetActive(true);
            if (!isLongEnough)
            {
                passwordValidationText.text = $"Minimum {ValidationConstants.MinPasswordLength} caractères";
                passwordValidationText.color = new Color(1f, 0.3f, 0.3f); // Rouge
            }
            else
            {
                passwordValidationText.text = "✓ Mot de passe valide";
                passwordValidationText.color = new Color(0.3f, 1f, 0.3f); // Vert
            }
        }

        private void UpdateConfirmPasswordValidation(bool passwordsMatch, bool hasConfirm)
        {
            if (string.IsNullOrWhiteSpace(confirmPasswordInput.text))
            {
                confirmPasswordValidationText.gameObject.SetActive(false);
                return;
            }

            confirmPasswordValidationText.gameObject.SetActive(true);
            if (!passwordsMatch)
            {
                confirmPasswordValidationText.text = "✗ Les mots de passe ne correspondent pas";
                confirmPasswordValidationText.color = new Color(1f, 0.3f, 0.3f); // Rouge
            }
            else
            {
                confirmPasswordValidationText.text = "✓ Les mots de passe correspondent";
                confirmPasswordValidationText.color = new Color(0.3f, 1f, 0.3f); // Vert
            }
        }

        private void HideValidationTexts()
        {
            if (usernameValidationText != null)
                usernameValidationText.gameObject.SetActive(false);
            if (passwordValidationText != null)
                passwordValidationText.gameObject.SetActive(false);
            if (confirmPasswordValidationText != null)
                confirmPasswordValidationText.gameObject.SetActive(false);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
            registerButton.onClick.RemoveAllListeners();
            backToLoginButton.onClick.RemoveAllListeners();
        }
    }
}

