using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using NexA.Hub.Core;
using NexA.Hub.Components;
using Utils;
using System.Threading.Tasks;
using DG.Tweening;
using NexA.Hub.Models;
using NexA.Hub.Services;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Configuration d'une étape de l'inscription
    /// </summary>
    [System.Serializable]
    public class StepData
    {
        [Tooltip("Le panel GameObject de cette étape")]
        public GameObject panel;
        
        [Tooltip("Texte affiché dans l'indicateur de progression (ex: '1', 'Username', '👤')")]
        public string indicatorText = "1";
        
        [Tooltip("Sous-titre affiché en haut de l'écran (ex: 'Étape 1/4 : Choisissez votre pseudo')")]
        public string subtitle = "Étape 1/4";
        
        [HideInInspector]
        public CanvasGroup canvasGroup; // Cache automatique
    }

    /// <summary>
    /// Gère l'écran d'inscription multi-étapes (dynamique)
    /// Le nombre d'étapes est automatiquement détecté à partir du tableau 'steps'
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class RegisterScreenMultiStep : ScreenBase
    {
        public override ScreenType ScreenType => ScreenType.RegisterMultiStep;
        // ============================================
        // CONFIGURATION: Testing Mode
        [Header("Testing Mode")]
        [SerializeField] private bool CHECK_TO_BACKEND = true;  // Set to false to disable username check

        [Header("Progress Indicator")]
        [SerializeField] private Image[] stepIndicators;
        [SerializeField] private TextMeshProUGUI[] stepIndicatorTexts;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Step Configuration")]
        [Tooltip("Configure toutes les étapes ici. Le nombre d'étapes est automatiquement détecté.")]
        [SerializeField] private StepData[] steps = new StepData[]
        {
            new StepData { indicatorText = "1. Pseudo", subtitle = $"Étape 1/4 : Choisissez votre pseudo" },
            new StepData { indicatorText = "2. Email", subtitle = "Étape 2/4 : Entrez votre email" },
            new StepData { indicatorText = "3. Mot de Passe", subtitle = "Étape 3/4 : Créez votre mot de passe" },
            new StepData { indicatorText = "4. Vérification", subtitle = "Étape 4/4 : Vérifiez votre email" }
        };

        // Propriétés calculées automatiquement
        private int TotalSteps => steps?.Length ?? 0;
        private StepData CurrentStepData => (currentStep > 0 && currentStep <= TotalSteps) ? steps[currentStep - 1] : null;

        [Header("Step 1 - Username")]
        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private Image usernameValidationIcon;
        [SerializeField] private TextMeshProUGUI usernameValidationMessage;

        [Header("Step 2 - Email")]
        [SerializeField] private TMP_InputField emailField;
        [SerializeField] private Image emailValidationIcon;
        [SerializeField] private TextMeshProUGUI emailValidationMessage;
        [SerializeField] private TMP_InputField confirmEmailField;
        [SerializeField] private Image confirmEmailValidationIcon;
        [SerializeField] private TextMeshProUGUI confirmEmailValidationMessage;

        [Header("Step 3 - Password")]
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private Image passwordValidationIcon;
        [SerializeField] private TextMeshProUGUI passwordValidationMessage;
        [SerializeField] private TMP_InputField confirmPasswordField;
        [SerializeField] private Image confirmPasswordValidationIcon;
        [SerializeField] private TextMeshProUGUI confirmPasswordValidationMessage;
        [SerializeField] private Button passwordToggleButton;
        [SerializeField] private Button confirmPasswordToggleButton;

        [Header("Step 4 - Email Verification")]
        [SerializeField] private GameObject step4Panel;
        [SerializeField] private TextMeshProUGUI verificationInstructionText;
        [SerializeField] private TMP_InputField verificationCodeField;
        //[SerializeField] private GameObject validationFeedback; // Parent container for validation UI
        [SerializeField] private Image verificationCodeValidationIcon;
        [SerializeField] private TextMeshProUGUI verificationCodeValidationMessage;
        [SerializeField] private Button resendCodeButton;
        [SerializeField] private TextMeshProUGUI resendCodeButtonText;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI backButtonText;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;

        [Header("UI Panels")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Colors - Dark Theme (#1A1A1A)")]
        [SerializeField] private Color activeStepColor = new Color(0.2f, 0.6f, 0.86f); // #3498DB
        [SerializeField] private Color inactiveStepColor = new Color(0.31f, 0.31f, 0.31f); // #505050
        [SerializeField] private Color completedStepColor = new Color(0.18f, 0.8f, 0.44f); // #2ECC71
        [SerializeField] private Color validColor = new Color(0.18f, 0.8f, 0.44f); // #2ECC71
        [SerializeField] private Color invalidColor = new Color(0.91f, 0.3f, 0.24f); // #E74C3C
        [SerializeField] private Color textInactiveColor = new Color(0.5f, 0.5f, 0.5f); // #808080
        
        
        // Verification Code Delays
        private const float MIN_GENERATION_DELAY = 0.5f;  // Minimum code generation time
        private const float MAX_GENERATION_DELAY = 1.2f;  // Maximum code generation time
        private const float MIN_SENDING_DELAY = 0.8f;     // Minimum email sending time
        private const float MAX_SENDING_DELAY = 1.8f;     // Maximum email sending time
        private const int RESEND_COOLDOWN_SECONDS = 60;   // Cooldown before resend is allowed
        // ============================================

        // État
        private int currentStep;
        private bool isPasswordVisible;
        private bool isConfirmPasswordVisible;
        private CanvasGroup canvasGroup;

        // Validation states
        private bool isUsernameValid;
        private bool isEmailValid;
        private bool isConfirmEmailValid;
        private bool isPasswordValid;
        private bool isConfirmPasswordValid;
        private bool isVerificationCodeValid;
        
        // Async validation states
        private bool isUsernameAvailable = true;
        private bool isCheckingUsername;
        
        // Email verification
        private int resendCodeCooldown;
        private Coroutine resendCooldownCoroutine;
        private string generatedVerificationCode; // Code généré localement par Unity
        
        // Debounce timers
        private Coroutine usernameCheckCoroutine;
        private const float DEBOUNCE_DELAY = 0.5f;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Assert.IsNotNull(canvasGroup, "[RegisterScreenMultiStep] CanvasGroup is null");
            
            // Initialiser automatiquement les CanvasGroups pour tous les steps
            InitializeStepCanvasGroups();
        }

        /// <summary>
        /// Initialise automatiquement les CanvasGroups de tous les steps
        /// </summary>
        private void InitializeStepCanvasGroups()
        {
            if (steps == null || steps.Length == 0)
            {
                Debug.LogError("[RegisterScreenMultiStep] Steps array is null or empty!");
                return;
            }

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].panel == null)
                {
                    Debug.LogError($"[RegisterScreenMultiStep] Step {i + 1} panel is null!");
                    continue;
                }

                steps[i].canvasGroup = steps[i].panel.GetComponent<CanvasGroup>();
                
                if (steps[i].canvasGroup == null)
                {
                    Debug.LogError($"[RegisterScreenMultiStep] Step {i + 1} ({steps[i].panel.name}) is missing CanvasGroup component!");
                }
            }

            Debug.Log($"[RegisterScreenMultiStep] Initialized {TotalSteps} steps with CanvasGroups");
        }

        private void Start()
        {
            currentStep = 1;
            SetupListeners();
            InitializeStepIndicatorTexts();
            ShowStep(currentStep);
            
            // Log configuration status
            if (!CHECK_TO_BACKEND)
            {
                Debug.LogWarning("[RegisterScreenMultiStep] Username backend validation is DISABLED");
                Debug.LogWarning("[RegisterScreenMultiStep] 🧪 TEST MODE ENABLED:");
                Debug.LogWarning("  → Verification code shown in Toast popup (no real email)");
                Debug.LogWarning("  → Registration simulated (no database/API call)");
                Debug.LogWarning("  → Direct redirect to Login with pre-filled email");
            }
        }

        private void SetupListeners()
        {
            // Step 1
            usernameField.onValueChanged.AddListener(OnUsernameChanged);

            // Step 2
            emailField.onValueChanged.AddListener(OnEmailChanged);
            confirmEmailField.onValueChanged.AddListener(OnConfirmEmailChanged);

            // Step 3
            passwordField.onValueChanged.AddListener(OnPasswordChanged);
            confirmPasswordField.onValueChanged.AddListener(OnConfirmPasswordChanged);

            // Step 4 - Email Verification
            verificationCodeField.onValueChanged.AddListener(OnVerificationCodeChanged);
            resendCodeButton.onClick.AddListener(OnResendCodeClicked);

            // Buttons
            backButton.onClick.AddListener(OnBackButtonClick);
            nextButton.onClick.AddListener(OnNextButtonClick);

            // Password toggles
            if (passwordToggleButton != null)
                passwordToggleButton.onClick.AddListener(TogglePasswordVisibility);
            if (confirmPasswordToggleButton != null)
                confirmPasswordToggleButton.onClick.AddListener(ToggleConfirmPasswordVisibility);
        }

        /// <summary>
        /// Initialise automatiquement les textes des indicateurs de steps
        /// </summary>
        private void InitializeStepIndicatorTexts()
        {
            if (stepIndicatorTexts == null || stepIndicatorTexts.Length < TotalSteps)
            {
                Debug.LogWarning($"[RegisterScreenMultiStep] stepIndicatorTexts array size ({stepIndicatorTexts?.Length ?? 0}) is less than total steps ({TotalSteps})");
                return;
            }

            for (int i = 0; i < TotalSteps; i++)
            {
                if (i < stepIndicatorTexts.Length && stepIndicatorTexts[i] != null)
                {
                    stepIndicatorTexts[i].text = steps[i].indicatorText;
                }
            }

            Debug.Log($"[RegisterScreenMultiStep] Step indicators initialized for {TotalSteps} steps");
        }

        #region Step Navigation

        private async void ShowStep(int step)
        {
            int previousStep = currentStep;
            currentStep = step;

            // Transition animation between steps
            if (previousStep != currentStep && previousStep != 0)
            {
                // Fade out previous step
                CanvasGroup previousCanvasGroup = GetCanvasGroupForStep(previousStep);
                if (previousCanvasGroup)
                {
                    previousCanvasGroup.interactable = false;
                    await previousCanvasGroup.DOFade(0f, AnimationHelper.FAST)
                        .SetEase(AnimationHelper.OUT_SMOOTH)
                        .AsyncWaitForCompletion();
                }
            }

            // Activate/deactivate panels dynamically
            for (int i = 0; i < TotalSteps; i++)
            {
                if (steps[i].panel != null)
                {
                    steps[i].panel.SetActive(i == step - 1);
                }
            }

            // Fade in new step
            CanvasGroup currentCanvasGroup = GetCanvasGroupForStep(currentStep);
            if (currentCanvasGroup)
            {
                currentCanvasGroup.alpha = 0f;
                currentCanvasGroup.interactable = false;
                await currentCanvasGroup.DOFade(1f, AnimationHelper.NORMAL)
                    .SetEase(AnimationHelper.IN_SMOOTH)
                    .AsyncWaitForCompletion();
                currentCanvasGroup.interactable = true;
            }

            // Update UI
            UpdateProgressIndicator();
            UpdateSubtitle();
            UpdateBackButton();
            UpdateNextButton();
            UpdateNextButtonState();
        }

        private CanvasGroup GetCanvasGroupForStep(int step)
        {
            if (step < 1 || step > TotalSteps)
                return null;
            
            return steps[step - 1].canvasGroup;
        }

        private void UpdateProgressIndicator()
        {
            for (int i = 0; i < stepIndicators.Length; i++)
            {
                int stepNumber = i + 1;
                Color targetIndicatorColor;
                Color targetTextColor;

                if (stepNumber < currentStep)
                {
                    // Completed step
                    targetIndicatorColor = completedStepColor;
                    targetTextColor = Color.white;
                }
                else if (stepNumber == currentStep)
                {
                    // Active step
                    targetIndicatorColor = activeStepColor;
                    targetTextColor = Color.white;
                }
                else
                {
                    // Inactive step
                    targetIndicatorColor = inactiveStepColor;
                    targetTextColor = textInactiveColor;
                }

                // Animate color transitions
                AnimationHelper.FadeColor(stepIndicators[i], targetIndicatorColor, AnimationHelper.NORMAL);
                AnimationHelper.FadeColor(stepIndicatorTexts[i], targetTextColor, AnimationHelper.NORMAL);
            }
        }

        private void UpdateSubtitle()
        {
            if (CurrentStepData != null && subtitleText)
            {
                subtitleText.text = CurrentStepData.subtitle;
            }
        }
        
        private void UpdateBackButton()
        {
            backButton.interactable = true;
            switch (currentStep)
            {
                case 1:
                    backButtonText.text = "Login";
                    break;
                case 2:
                case 3:
                case 4:
                    backButtonText.text = "Retour";
                    break;
            }
        }

        private void UpdateNextButton()
        {
            nextButton.interactable = true;
            
            // Dernier step = "S'inscrire", autres = "Suivant"
            if (currentStep == TotalSteps)
            {
                nextButtonText.text = "S'inscrire ✓";
            }
            else
            {
                nextButtonText.text = "Suivant ▶";
            }
        }

        #endregion

        #region Async Validation Helper (Generic)

        /// <summary>
        /// Méthode générique pour vérifier la disponibilité avec debounce
        /// </summary>
        private IEnumerator CheckAvailabilityDebounced<T>(
            string value,
            Func<string, System.Threading.Tasks.Task<T>> apiCall,
            TextMeshProUGUI messageField,
            Func<T, bool> isAvailableFunc,
            Action<bool> setAvailableFlag,
            Action updateUICallback) where T : class
        {
            // Attendre avant de faire l'appel API (debounce)
            yield return new WaitForSeconds(DEBOUNCE_DELAY);
            
            // Show loading state
            if (messageField)
            {
                messageField.text = "Vérification...";
                messageField.color = textInactiveColor;
                messageField.gameObject.SetActive(true);
            }
            
            // Appel API
            var task = apiCall(value);
            yield return new WaitUntil(() => task.IsCompleted);
            
            // Traiter le résultat
            if (task.Exception != null)
            {
                Debug.LogWarning($"[Register] Erreur check: {task.Exception.Message}");
                setAvailableFlag(true); // Considéré comme disponible en cas d'erreur réseau
            }
            else
            {
                bool available = isAvailableFunc(task.Result);
                setAvailableFlag(available);
            }
            
            // Update UI
            updateUICallback();
        }

        #endregion

        #region Step 1 - Username Validation

        private void OnUsernameChanged(string value)
        {
            isUsernameValid = ValidateUsername(value);
            
            // Si validation locale OK ET check backend activé, vérifier disponibilité avec debounce
            if (isUsernameValid && CHECK_TO_BACKEND)
            {
                // Cancel previous check
                if (usernameCheckCoroutine != null)
                {
                    StopCoroutine(usernameCheckCoroutine);
                }
                
                // Start new debounced check avec la méthode générique
                usernameCheckCoroutine = StartCoroutine(
                    CheckAvailabilityDebounced(
                        value,
                        APIService.Instance.CheckUsernameAvailabilityAsync,
                        usernameValidationMessage,
                        (response) => response.available,
                        (available) => {
                            isUsernameAvailable = available;
                            isCheckingUsername = false;
                        },
                        () => {
                            bool finalValid = isUsernameValid && isUsernameAvailable;
                            string errorMsg = !isUsernameAvailable ? "Ce pseudo est déjà pris" : GetUsernameErrorMessage(value);
                            UpdateValidationUI(usernameValidationIcon, usernameValidationMessage, finalValid, errorMsg);
                            UpdateNextButtonState();
                        }
                    )
                );
                
                isCheckingUsername = true;
            }
            else
            {
                // Si validation locale échoue OU check backend désactivé
                isUsernameAvailable = true;
                UpdateValidationUI(usernameValidationIcon, usernameValidationMessage, isUsernameValid, GetUsernameErrorMessage(value));
            }
            
            UpdateNextButtonState();
        }

        private static bool ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (username.Length < 3 || username.Length > 20) return false;
            
            // Uniquement alphanumerique
            return Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
        }

        private static string GetUsernameErrorMessage(string username)
        {
            if (string.IsNullOrEmpty(username)) return "";
            
            if (username.Length < 3) 
                return "Le pseudo doit contenir au moins 3 caractères";
            
            if (username.Length > 20)
                return "Le pseudo ne peut pas dépasser 20 caractères";
            
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$")) 
                return "Uniquement des lettres et chiffres";
            
            return "";
        }

        #endregion

        #region Step 2 - Email Validation

        private void OnEmailChanged(string value)
        {
            isEmailValid = ValidateEmail(value);
            
            UpdateValidationUI(emailValidationIcon, emailValidationMessage, isEmailValid, GetEmailErrorMessage(value));

            // Re-valider la confirmation si déjà remplie
            if (!string.IsNullOrEmpty(confirmEmailField.text))
            {
                OnConfirmEmailChanged(confirmEmailField.text);
            }

            UpdateNextButtonState();
        }

        private void OnConfirmEmailChanged(string value)
        {
            isConfirmEmailValid = ValidateConfirmEmail(value);
            UpdateValidationUI(confirmEmailValidationIcon, confirmEmailValidationMessage, isConfirmEmailValid, GetConfirmEmailErrorMessage(value));
            UpdateNextButtonState();
        }

        private static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            
            // Regex simple pour email
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool ValidateConfirmEmail(string confirmEmail)
        {
            if (string.IsNullOrEmpty(confirmEmail)) return false;
            return confirmEmail == emailField.text && isEmailValid;
        }

        private static string GetEmailErrorMessage(string email)
        {
            if (string.IsNullOrEmpty(email)) return "";
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Format d'email invalide";
            return "";
        }

        private string GetConfirmEmailErrorMessage(string confirmEmail)
        {
            if (string.IsNullOrEmpty(confirmEmail)) return "";
            if (confirmEmail != emailField.text) return "Les emails ne correspondent pas";
            return "";
        }

        #endregion

        #region Step 3 - Password Validation

        private void OnPasswordChanged(string value)
        {
            isPasswordValid = ValidatePassword(value);
            UpdateValidationUI(passwordValidationIcon, passwordValidationMessage, isPasswordValid, GetPasswordErrorMessage(value));

            // Re-valider la confirmation si déjà remplie
            if (!string.IsNullOrEmpty(confirmPasswordField.text))
            {
                OnConfirmPasswordChanged(confirmPasswordField.text);
            }

            UpdateNextButtonState();
        }

        private void OnConfirmPasswordChanged(string value)
        {
            isConfirmPasswordValid = ValidateConfirmPassword(value);
            UpdateValidationUI(confirmPasswordValidationIcon, confirmPasswordValidationMessage, isConfirmPasswordValid, GetConfirmPasswordErrorMessage(value));
            UpdateNextButtonState();
        }

        private static bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) 
                return false;
            if (password.Length < 8) 
                return false;
            
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"[0-9]");

            return hasUpper && hasLower && hasDigit;
        }

        private bool ValidateConfirmPassword(string confirmPassword)
        {
            if (string.IsNullOrEmpty(confirmPassword))
                return false;
            return confirmPassword == passwordField.text && isPasswordValid;
        }

        private static string GetPasswordErrorMessage(string password)
        {
            if (string.IsNullOrEmpty(password)) 
                return "";
            
            if (password.Length < 8)
                return "Minimum 8 caractères requis";
            
            if (!Regex.IsMatch(password, @"[A-Z]")) 
                return "Doit contenir une majuscule";
            
            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Doit contenir une minuscule";
            
            if (!Regex.IsMatch(password, @"[0-9]")) 
                return "Doit contenir un chiffre";
            
            return "";
        }

        private string GetConfirmPasswordErrorMessage(string confirmPassword)
        {
            if (string.IsNullOrEmpty(confirmPassword))
                return "";
            
            if (confirmPassword != passwordField.text) 
                return "Les mots de passe ne correspondent pas";
            
            return "";
        }

        private void TogglePasswordVisibility()
        {
            isPasswordVisible = !isPasswordVisible;
            passwordField.contentType = isPasswordVisible ?
                TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
        }

        private void ToggleConfirmPasswordVisibility()
        {
            isConfirmPasswordVisible = !isConfirmPasswordVisible;
            confirmPasswordField.contentType = isConfirmPasswordVisible ?
                TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            confirmPasswordField.ForceLabelUpdate();
        }

        #endregion

        #region Validation UI

        private void UpdateValidationUI(Image icon, TextMeshProUGUI message, bool isValid, string errorMessage)
        {
            if (icon)
            {
                icon.gameObject.SetActive(true);
                
                // Fade color animation for icon
                Color targetColor = isValid ? validColor : invalidColor;
                AnimationHelper.FadeColor(icon, targetColor, AnimationHelper.FAST);
            }

            if (message)
            {
                if (isValid)
                {
                    // Fade out then hide
                    AnimationHelper.FadeColor(message, new Color(invalidColor.r, invalidColor.g, invalidColor.b, 0f), AnimationHelper.FAST)
                        .OnComplete(() => message.gameObject.SetActive(false));
                }
                else
                {
                    bool shouldShow = !string.IsNullOrEmpty(errorMessage);
                    
                    if (shouldShow)
                    {
                        message.text = errorMessage;
                        
                        // Fade in with color
                        if (!message.gameObject.activeSelf)
                        {
                            message.gameObject.SetActive(true);
                            message.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, 0f);
                        }
                        
                        AnimationHelper.FadeColor(message, invalidColor, AnimationHelper.FAST);
                    }
                    else
                    {
                        message.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void UpdateNextButtonState()
        {
            bool canProceed = false;

            switch (currentStep)
            {
                case 1:
                    // Username valide ET disponible ET pas en cours de vérification
                    canProceed = isUsernameValid && isUsernameAvailable && !isCheckingUsername;
                    break;
                case 2:
                    // Email valide (format) ET confirmation valide
                    // Pas de vérification backend ici, l'email sera vérifié via le code envoyé
                    canProceed = isEmailValid && isConfirmEmailValid;
                    break;
                case 3:
                    canProceed = isPasswordValid && isConfirmPasswordValid;
                    break;
                case 4:
                    canProceed = isVerificationCodeValid;
                    break;
            }

            nextButton.interactable = canProceed;
        }

        #endregion

        #region Button Handlers

        private void OnBackButtonClick()
        {
            backButton.interactable = false;
            if (currentStep == 1)
            {           
                // Retour au Login Screen
                UIManager.Instance?.ShowScreen(ScreenType.Login);
            }
            else
            {
                // Retour à l'étape précédente
                ShowStep(currentStep - 1);
            }
        }

        private void OnNextButtonClick()
        {
            nextButton.interactable = false;
            
            if (currentStep < TotalSteps - 1)
            {
                // Étapes normales : passer à la suivante
                ShowStep(currentStep + 1);
            }
            else if (currentStep == TotalSteps - 1)
            {
                // Avant-dernière étape : envoyer le code de vérification
                StartCoroutine(SendVerificationCodeAndContinue());
            }
            else if (currentStep == TotalSteps)
            {
                // Dernière étape : s'inscrire
                StartCoroutine(RegisterUser());
            }
        }

        #endregion

        #region Step 4 - Email Verification

        /// <summary>
        /// Active le loading panel avec un message
        /// </summary>
        private void ShowLoadingWithMessage(string message)
        {
            loadingPanel.SetActive(true);
            loadingText.text = message;
        }

        /// <summary>
        /// Désactive le loading panel
        /// </summary>
        private void HideLoading()
        {
            loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Génère un nouveau code de vérification et l'affiche dans les logs
        /// </summary>
        private void GenerateAndLogVerificationCode(bool isResend = false)
        {
            generatedVerificationCode = GenerateVerificationCode();
            
            string prefix = isResend ? "NOUVEAU CODE" : "CODE GÉNÉRÉ";
            Debug.Log($"🔑 {prefix} : {generatedVerificationCode}");
            Debug.Log($"📧 Email : {emailField.text}");
        }

        /// <summary>
        /// Affiche le code de vérification via Toast selon le mode (test ou production)
        /// </summary>
        private void ShowVerificationCodeToast(bool isResend = false)
        {
            if (!CHECK_TO_BACKEND)
            {
                string message = isResend 
                    ? $"🔑 Nouveau code : {generatedVerificationCode}\n(Mode Test)"
                    : $"🔑 Code de vérification : {generatedVerificationCode}\n(Mode Test)";
                ToastManager.Show(message, ToastType.Info);
            }
            else
            {
                string message = isResend
                    ? $"Nouveau code envoyé à {emailField.text}"
                    : $"Code envoyé à {emailField.text}";
                ToastManager.Show(message, ToastType.Success);
            }
        }

        /// <summary>
        /// Met à jour le texte d'instruction pour la vérification email
        /// </summary>
        private void UpdateVerificationInstructionText()
        {
            if (!verificationInstructionText) return;

            if (!CHECK_TO_BACKEND)
            {
                verificationInstructionText.text = $"Un code de vérification a été généré\n<b>Voir popup ci-dessus (Mode Test)</b>\nEmail: {emailField.text}";
            }
            else
            {
                verificationInstructionText.text = $"Un code de vérification a été envoyé à\n<b>{emailField.text}</b>\nVérifiez votre boîte mail";
            }
        }

        /// <summary>
        /// Processus complet de génération et envoi du code avec délais aléatoires
        /// </summary>
        private IEnumerator GenerateAndSendCodeProcess(bool isResend = false)
        {
            // ===== ÉTAPE 1 : Génération du code =====
            string generationMessage = isResend 
                ? "Génération d'un nouveau code..."
                : "Génération du code de vérification...";
            ShowLoadingWithMessage(generationMessage);
            
            float generationDelay = UnityEngine.Random.Range(MIN_GENERATION_DELAY, MAX_GENERATION_DELAY);
            yield return new WaitForSeconds(generationDelay);
            
            GenerateAndLogVerificationCode(isResend);
            
            // ===== ÉTAPE 2 : Envoi du code =====
            string sendingMessage = isResend 
                ? "Renvoi du code de vérification..."
                : "Envoi du code de vérification...";
            ShowLoadingWithMessage(sendingMessage);
            
            float sendingDelay = UnityEngine.Random.Range(MIN_SENDING_DELAY, MAX_SENDING_DELAY);
            yield return new WaitForSeconds(sendingDelay);
            
            HideLoading();
            ShowVerificationCodeToast(isResend);
        }

        #endregion

        #region Step 4 - Email Verification (Old Code - To Refactor)

        private IEnumerator SendVerificationCodeAndContinue()
        {
            nextButton.interactable = false;
            backButton.interactable = false;
            
            yield return StartCoroutine(GenerateAndSendCodeProcess(isResend: false));
            
            // Afficher l'instruction
            UpdateVerificationInstructionText();
            
            /*if (validationFeedback)
                validationFeedback.SetActive(true);*/
            
            ShowStep(4);
            StartResendCooldown(RESEND_COOLDOWN_SECONDS);
            
            nextButton.interactable = true;
            backButton.interactable = true;
        }

        /// <summary>
        /// Génère un code de vérification à 6 chiffres aléatoire
        /// </summary>
        private static string GenerateVerificationCode()
        {
            System.Random random = new System.Random();
            int code = random.Next(100000, 999999); // Entre 100000 et 999999
            return code.ToString();
        }

        private void OnResendCodeClicked()
        {
            if (resendCodeCooldown > 0)
                return; // Cooldown actif
            
            StartCoroutine(ResendVerificationCode());
        }

        private IEnumerator ResendVerificationCode()
        {
            resendCodeButton.interactable = false;
            yield return StartCoroutine(GenerateAndSendCodeProcess(isResend: true));
            
            // Reset le champ de code
            verificationCodeField.text = "";
            StartResendCooldown(RESEND_COOLDOWN_SECONDS);
        }

        private void StartResendCooldown(int seconds)
        {
            // Arrêter le cooldown précédent si existe
            if (resendCooldownCoroutine != null)
            {
                StopCoroutine(resendCooldownCoroutine);
            }
            
            resendCooldownCoroutine = StartCoroutine(ResendCooldownTimer(seconds));
        }

        private IEnumerator ResendCooldownTimer(int seconds)
        {
            Assert.IsNotNull(resendCodeButton, "[RegisterScreenMultiStep] resendCodeButton is null");
            Assert.IsNotNull(resendCodeButtonText, "[RegisterScreenMultiStep] resendCodeButtonText is null");
            
            resendCodeCooldown = seconds;
            resendCodeButton.interactable = false;
            
            Debug.Log($"[RegisterScreenMultiStep] Cooldown started: {seconds} seconds");
            
            while (resendCodeCooldown > 0)
            {
                resendCodeButtonText.text = $"Renvoyer le code ({resendCodeCooldown}s)";
                yield return new WaitForSeconds(1f);
                resendCodeCooldown--;
            }
            
            resendCodeButton.interactable = true;
            resendCodeButtonText.text = "Renvoyer le code";
            
            Debug.Log("[RegisterScreenMultiStep] Cooldown finished, resend button enabled");
        }

        private void OnVerificationCodeChanged(string value)
        {
            // Validation format : exactement 6 chiffres
            bool isValidFormat = ValidateVerificationCode(value);
            
            // Vérification du code : doit correspondre au code généré
            bool isCodeCorrect = value == generatedVerificationCode;
            
            // Le code est valide si le format est bon ET qu'il correspond au code généré
            isVerificationCodeValid = isValidFormat && isCodeCorrect;
            
            string errorMsg = GetVerificationCodeErrorMessage(value, isCodeCorrect);
            UpdateValidationUI(verificationCodeValidationIcon, verificationCodeValidationMessage, isVerificationCodeValid, errorMsg);
            
            UpdateNextButtonState();
        }

        private static bool ValidateVerificationCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            if (code.Length != 6) return false;
            
            return Regex.IsMatch(code, @"^\d{6}$"); // Uniquement des chiffres
        }

        private static string GetVerificationCodeErrorMessage(string code, bool isCodeCorrect)
        {
            if (string.IsNullOrEmpty(code)) return "";
            if (code.Length < 6) return "Le code doit contenir 6 chiffres";
            if (code.Length > 6) return "Le code ne peut pas dépasser 6 chiffres";
            if (!Regex.IsMatch(code, @"^\d{6}$")) return "Uniquement des chiffres";
            if (code.Length == 6 && !isCodeCorrect) return "Code incorrect";
            return "";
        }

        #endregion

        #region Registration

        private IEnumerator RegisterUser()
        {
            string username = usernameField.text;
            string email = emailField.text;
            
            // MODE TEST : Simuler l'inscription sans appel API
            if (!CHECK_TO_BACKEND)
            {
                Debug.Log($"🧪 [MODE TEST] Simulation inscription pour {username}");
                
                // Afficher le loading brièvement pour réalisme
                loadingPanel.SetActive(true);
                nextButton.interactable = false;
                backButton.interactable = false;
                
                // Simuler un délai réseau (0.5-1.5s)
                float simulatedDelay = UnityEngine.Random.Range(0.5f, 1.5f);
                yield return new WaitForSeconds(simulatedDelay);
                
                loadingPanel.SetActive(false);
                
                // Succès simulé
                Debug.Log($"✅ [MODE TEST] Inscription simulée réussie pour {username}!");
                ToastManager.Show($"Compte \"{username}\" créé avec succès !\n(Mode Test - Pas de BDD)", ToastType.Success);
                
                yield return new WaitForSeconds(1.5f);
                
                // Retour au login avec email pré-rempli
                UIManager.Instance?.ShowScreen(ScreenType.Login, email);
                
                yield break; // Sortir de la coroutine
            }
            
            // MODE PRODUCTION : Appel API réel
            loadingPanel.SetActive(true);
            nextButton.interactable = false;
            backButton.interactable = false;

            Task<bool> task = AuthManager.Instance.RegisterAsync(
                username, 
                email, 
                passwordField.text,
                verificationCodeField.text  // Passer le code de vérification
            );
            yield return new WaitUntil(() => task.IsCompleted);

             
            loadingPanel.SetActive(false);
            nextButton.interactable = true;
            backButton.interactable = true;

            // Vérifier le résultat
            if (task.Exception != null)
            {
                // Erreur lors de l'inscription
                Exception exception = task.Exception.InnerException ?? task.Exception;
                string errorMessage = exception.Message;
                
                Debug.LogError($"❌ Erreur d'inscription: {errorMessage}");
                ShowError(errorMessage);
            }
            else if (task.Result)
            {
                // Succès
                Debug.Log($"✅ Inscription réussie pour {username}!");
                ToastManager.Show($"Compte \"{username}\" créé avec succès !", ToastType.Success);
                
                yield return new WaitForSeconds(1.5f);
                
                // Retour au login avec email pré-rempli
                UIManager.Instance?.ShowScreen(ScreenType.Login, email);
            }
            else
            {
                // Cas improbable (task terminée sans exception mais result = false)
                ShowError("Une erreur est survenue lors de l'inscription. Veuillez réessayer.");
            }
        }

        private new void ShowError(string message)
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Réinitialise le formulaire d'inscription
        /// </summary>
        private void ResetForm()
        {
            // Réinitialiser les champs
            usernameField.text = "";
            emailField.text = "";
            confirmEmailField.text = "";
            passwordField.text = "";
            confirmPasswordField.text = "";
            verificationCodeField.text = "";
            
            // Réinitialiser le code généré
            generatedVerificationCode = "";

            // Réinitialiser les validations
            isUsernameValid = false;
            isEmailValid = false;
            isConfirmEmailValid = false;
            isPasswordValid = false;
            isConfirmPasswordValid = false;
            isVerificationCodeValid = false;

            // Cacher les icônes de validation
            if (usernameValidationIcon) usernameValidationIcon.gameObject.SetActive(false);
            if (usernameValidationMessage) usernameValidationMessage.gameObject.SetActive(false);
            if (emailValidationIcon) emailValidationIcon.gameObject.SetActive(false);
            if (emailValidationMessage) emailValidationMessage.gameObject.SetActive(false);
            if (confirmEmailValidationIcon) confirmEmailValidationIcon.gameObject.SetActive(false);
            if (confirmEmailValidationMessage) confirmEmailValidationMessage.gameObject.SetActive(false);
            if (passwordValidationIcon) passwordValidationIcon.gameObject.SetActive(false);
            if (passwordValidationMessage) passwordValidationMessage.gameObject.SetActive(false);
            if (confirmPasswordValidationIcon) confirmPasswordValidationIcon.gameObject.SetActive(false);
            if (confirmPasswordValidationMessage) confirmPasswordValidationMessage.gameObject.SetActive(false);
            //if (validationFeedback) validationFeedback.SetActive(false);
            if (verificationCodeValidationIcon) verificationCodeValidationIcon.gameObject.SetActive(false);
            if (verificationCodeValidationMessage) verificationCodeValidationMessage.gameObject.SetActive(false);

            // Cacher les panels
            loadingPanel.SetActive(false);
            errorPanel.SetActive(false);

            // Retour à l'étape 1
            ShowStep(1);
        }

        /// <summary>
        /// Appelé quand l'écran devient actif
        /// </summary>
        public void OnScreenShow()
        {
            ResetForm();
        }

        #endregion

        #region ScreenBase Implementation

        public override async Task ShowAsync(object data = null)
        {
            gameObject.SetActive(true);
            ResetForm();
            
            // Fade in animation
            if (!canvasGroup)
            {
                await Task.CompletedTask;
                return; 
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            await canvasGroup.DOFade(1f, AnimationHelper.NORMAL)
                .SetEase(AnimationHelper.IN_SMOOTH)
                .AsyncWaitForCompletion();
            
            canvasGroup.interactable = true;
        }

        public override async Task HideAsync()
        {
            // Fade out animation (instant for quick exit)
            if (canvasGroup)
            {
                canvasGroup.interactable = false;
                
                await canvasGroup.DOFade(0f, AnimationHelper.INSTANT)
                    .SetEase(AnimationHelper.OUT_FAST)
                    .AsyncWaitForCompletion();
            }
            
            gameObject.SetActive(false);
        }

        #endregion
    }
}
