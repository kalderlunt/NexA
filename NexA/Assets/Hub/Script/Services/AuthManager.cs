using System;
using System.Threading.Tasks;
using UnityEngine;
using NexA.Hub.Models;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Gère l'authentification: login, register, tokens, logout
    /// Auto-refresh des access tokens
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        [Header("Token Settings")]
        [SerializeField] private bool persistTokens = false; // false = in-memory only (plus sécurisé)

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
        public User CurrentUser { get; private set; }

        // Tokens (en mémoire)
        private string _accessToken;
        private string _refreshToken;
        private DateTime _tokenExpiresAt;

        private const string REFRESH_TOKEN_KEY = "nexa_refresh_token";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Charger refresh token si persisté
            if (persistTokens)
            {
                _refreshToken = SecureStorage.GetToken(REFRESH_TOKEN_KEY);
            }
        }

        #region Public Methods

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var response = await APIService.Instance.RegisterAsync(username, email, password);
                StoreTokens(response.tokens);
                CurrentUser = response.user;
                return CurrentUser;
            }
            catch (APIException ex)
            {
                throw new AuthException(GetFriendlyErrorMessage(ex.Code), ex);
            }
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var response = await APIService.Instance.LoginAsync(email, password);
                StoreTokens(response.tokens);
                CurrentUser = response.user;
                return CurrentUser;
            }
            catch (APIException ex)
            {
                throw new AuthException(GetFriendlyErrorMessage(ex.Code), ex);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await APIService.Instance.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Logout failed: {ex.Message}");
            }
            finally
            {
                ClearTokens();
                CurrentUser = null;
            }
        }

        /// <summary>
        /// Retourne un access token valide (refresh automatique si expiré)
        /// </summary>
        public async Task<string> GetValidAccessTokenAsync()
        {
            // Si token encore valide (avec marge de 30s)
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt.AddSeconds(-30))
            {
                return _accessToken;
            }

            // Sinon, refresh
            if (string.IsNullOrEmpty(_refreshToken))
            {
                throw new AuthException("No refresh token available. Please login again.");
            }

            try
            {
                Debug.Log("[Auth] Access token expired, refreshing...");
                var response = await APIService.Instance.RefreshTokenAsync(_refreshToken);
                
                _accessToken = response.accessToken;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(response.expiresIn);
                
                return _accessToken;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Token refresh failed: {ex.Message}");
                ClearTokens();
                CurrentUser = null;
                throw new AuthException("Session expired. Please login again.", ex);
            }
        }

        /// <summary>
        /// Tenter de restaurer la session au démarrage (si refresh token persisté)
        /// </summary>
        public async Task<bool> TryRestoreSessionAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
                return false;

            try
            {
                Debug.Log("[Auth] Attempting to restore session...");
                
                // Refresh le token
                var response = await APIService.Instance.RefreshTokenAsync(_refreshToken);
                _accessToken = response.accessToken;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(response.expiresIn);
                
                // Récupérer l'utilisateur
                //CurrentUser = await APIService.Instance.GetMeAsync();
                CurrentUser = await APIService.Instance.GetCurrentUserAsync();
                
                Debug.Log($"[Auth] Session restored for user: {CurrentUser.username}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Failed to restore session: {ex.Message}");
                ClearTokens();
                return false;
            }
        }

        #endregion

        #region Private Methods

        private void StoreTokens(TokenData tokens)
        {
            _accessToken = tokens.accessToken;
            _refreshToken = tokens.refreshToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.expiresIn);

            // Persister le refresh token si activé
            if (persistTokens)
            {
                SecureStorage.SaveToken(REFRESH_TOKEN_KEY, _refreshToken);
            }

            Debug.Log($"[Auth] Tokens stored. Expires at: {_tokenExpiresAt}");
        }

        private void ClearTokens()
        {
            _accessToken = null;
            _refreshToken = null;
            _tokenExpiresAt = DateTime.MinValue;

            if (persistTokens)
            {
                SecureStorage.DeleteToken(REFRESH_TOKEN_KEY);
            }

            Debug.Log("[Auth] Tokens cleared");
        }

        private string GetFriendlyErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                "USERNAME_TAKEN" => "Ce nom d'utilisateur est déjà pris",
                "EMAIL_TAKEN" => "Cet email est déjà utilisé",
                "INVALID_PASSWORD" => "Mot de passe trop faible (min 8 caractères, 1 majuscule, 1 chiffre)",
                "INVALID_CREDENTIALS" => "Email ou mot de passe incorrect",
                "ACCOUNT_BANNED" => "Votre compte a été banni",
                "VALIDATION_ERROR" => "Données invalides",
                _ => "Une erreur est survenue lors de l'authentification"
            };
        }

        #endregion
    }

    #region Exceptions

    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
        public AuthException(string message, Exception inner) : base(message, inner) { }
    }

    #endregion
}

