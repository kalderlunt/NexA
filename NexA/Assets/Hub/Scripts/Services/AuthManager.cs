﻿﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using NexA.Hub.Models;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Gestionnaire d'authentification centralisé
    /// - Gère les tokens JWT (access + refresh)
    /// - Auto-refresh des tokens expirés
    /// - Persistance optionnelle du refresh token
    /// - Gestion du user actuel
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        #region Singleton

        public static AuthManager Instance { get; private set; }

        #endregion

        #region Configuration

        [Header("Token Settings")]
        [SerializeField] private bool persistTokens; // Sauvegarder refresh token dans PlayerPrefs

        #endregion

        #region Properties

        /// <summary>
        /// L'utilisateur est-il authentifié ?
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

        /// <summary>
        /// Utilisateur actuellement connecté
        /// </summary>
        public User CurrentUser { get; private set; }

        #endregion

        #region Private Fields

        private string _accessToken;
        private string _refreshToken;
        private DateTime _tokenExpiresAt;

        private const string REFRESH_TOKEN_KEY = "nexa_refresh_token";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Charger le refresh token si persistance activée
            if (persistTokens)
            {
                _refreshToken = SecureStorage.GetToken(REFRESH_TOKEN_KEY);
                
                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    Debug.Log("[AuthManager] Refresh token chargé depuis le stockage");
                }
            }
        }

        #endregion

        #region Public Methods - Authentication

        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        /// <param name="username">Nom d'utilisateur (3-20 caractères, alphanumerique)</param>
        /// <param name="email">Adresse email valide</param>
        /// <param name="password">Mot de passe (min 8 caractères, 1 maj, 1 min, 1 chiffre)</param>
        /// <param name="verificationCode">Code de vérification à 6 chiffres</param>
        /// <returns>True si l'inscription réussit, sinon lance une AuthException</returns>
        /// <exception cref="AuthException">Si l'inscription échoue</exception>
        public async Task<bool> RegisterAsync(string username, string email, string password, string verificationCode)
        {
            try
            {
                Debug.Log($"[AuthManager] Tentative d'inscription pour {username} ({email}) avec code de vérification");

                AuthResponse response = await APIService.Instance.RegisterWithCodeAsync(username, email, password, verificationCode);
                
                // Vérifier que la réponse contient bien les données nécessaires
                if (response == null)
                {
                    Debug.LogError("[AuthManager] ❌ Réponse d'inscription null !");
                    throw new AuthException("Le serveur n'a pas retourné de réponse valide");
                }
                
                Debug.Log($"[AuthManager] 📦 Réponse reçue:");
                Debug.Log($"  - User: {(response.user != null ? response.user.username : "NULL")}");
                Debug.Log($"  - Tokens: {(response.tokens != null ? "PRÉSENT" : "NULL ⚠️")}");
                
                if (response.tokens == null)
                {
                    Debug.LogError("[AuthManager] ❌ ERREUR CRITIQUE: Le backend n'a pas retourné de tokens !");
                    Debug.LogError("[AuthManager] Le backend doit retourner: { user: {...}, tokens: { accessToken, refreshToken, expiresIn } }");
                    throw new AuthException("Le serveur n'a pas retourné les tokens d'authentification. Contactez l'administrateur.");
                }
                
                if (response.user == null)
                {
                    Debug.LogError("[AuthManager] ❌ ERREUR: Le backend n'a pas retourné les données utilisateur !");
                    throw new AuthException("Le serveur n'a pas retourné les données utilisateur");
                }
                
                StoreTokens(response.tokens);
                CurrentUser = response.user;

                Debug.Log($"[AuthManager] ✅ Inscription réussie pour {CurrentUser.username} (ID: {CurrentUser.id})");
                return true;
            }
            catch (APIException ex)
            {
                Debug.LogError($"[AuthManager] ❌ Échec de l'inscription: {ex.Code} - {ex.Message}");
                throw new AuthException(GetFriendlyErrorMessage(ex.Code), ex);
            }
            catch (AuthException)
            {
                // Re-throw AuthException sans wrapper
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthManager] ❌ Erreur inattendue lors de l'inscription: {ex.Message}");
                Debug.LogException(ex);
                throw new AuthException("Une erreur inattendue s'est produite. Veuillez réessayer.", ex);
            }
        }

        /// <summary>
        /// Connexion d'un utilisateur existant
        /// </summary>
        public async Task<User> LoginAsync(string username, string password)
        {
            try
            {
                Debug.Log($"[AuthManager] Tentative de connexion pour {username}");

                var response = await APIService.Instance.LoginAsync(username, password);
                
                StoreTokens(response.tokens);
                CurrentUser = response.user;

                Debug.Log($"[AuthManager] Connexion réussie pour {CurrentUser.username} (ID: {CurrentUser.id})");
                return CurrentUser;
            }
            catch (APIException ex)
            {
                Debug.LogError($"[AuthManager] Échec de la connexion: {ex.Code} - {ex.Message}");
                throw new AuthException(GetFriendlyErrorMessage(ex.Code), ex);
            }
        }

        /// <summary>
        /// Déconnexion de l'utilisateur
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                Debug.Log("[AuthManager] Déconnexion en cours...");
                await APIService.Instance.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AuthManager] Échec de la déconnexion côté serveur: {ex.Message}");
            }
            finally
            {
                ClearTokens();
                CurrentUser = null;
                Debug.Log("[AuthManager] Déconnexion locale réussie");
            }
        }

        #endregion

        #region Public Methods - Token Management

        /// <summary>
        /// Retourne un access token valide
        /// Refresh automatiquement si expiré
        /// </summary>
        public async Task<string> GetValidAccessTokenAsync()
        {
            // Si token encore valide (avec marge de 30s pour éviter les race conditions)
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt.AddSeconds(-30))
            {
                return _accessToken;
            }

            // Token expiré, on doit le refresh
            if (string.IsNullOrEmpty(_refreshToken))
            {
                Debug.LogError("[AuthManager] Pas de refresh token disponible");
                throw new AuthException("Session expirée. Veuillez vous reconnecter.");
            }

            try
            {
                Debug.Log("[AuthManager] Access token expiré, rafraîchissement en cours...");
                
                var response = await APIService.Instance.RefreshTokenAsync(_refreshToken);
                
                _accessToken = response.accessToken;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(response.expiresIn);
                
                Debug.Log($"[AuthManager] Token rafraîchi avec succès. Expire à: {_tokenExpiresAt}");
                return _accessToken;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthManager] Échec du rafraîchissement du token: {ex.Message}");
                ClearTokens();
                CurrentUser = null;
                throw new AuthException("Session expirée. Veuillez vous reconnecter.", ex);
            }
        }

        /// <summary>
        /// Tenter de restaurer la session au démarrage
        /// Utilisé pour l'auto-login
        /// </summary>
        public async Task<bool> TryRestoreSessionAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                Debug.Log("[AuthManager] Pas de refresh token, impossible de restaurer la session");
                return false;
            }

            try
            {
                Debug.Log("[AuthManager] Tentative de restauration de la session...");
                
                // Refresh le token d'accès
                var tokenResponse = await APIService.Instance.RefreshTokenAsync(_refreshToken);
                _accessToken = tokenResponse.accessToken;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expiresIn);
                
                // Récupérer les infos utilisateur
                CurrentUser = await APIService.Instance.GetCurrentUserAsync();
                
                Debug.Log($"[AuthManager] Session restaurée pour {CurrentUser.username}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AuthManager] Échec de la restauration de la session: {ex.Message}");
                ClearTokens();
                return false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Stocke les tokens en mémoire (et optionnellement en persistance)
        /// </summary>
        private void StoreTokens(TokenData tokens)
        {
            if (tokens == null)
            {
                Debug.LogError("[AuthManager] TokenData null reçu");
                return;
            }

            _accessToken = tokens.accessToken;
            _refreshToken = tokens.refreshToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.expiresIn);

            // Persistance du refresh token si activée
            if (persistTokens && !string.IsNullOrEmpty(_refreshToken))
            {
                SecureStorage.SaveToken(REFRESH_TOKEN_KEY, _refreshToken);
                Debug.Log("[AuthManager] Refresh token persisté");
            }

            Debug.Log($"[AuthManager] Tokens stockés. Expire à: {_tokenExpiresAt:yyyy-MM-dd HH:mm:ss}");
        }

        /// <summary>
        /// Efface tous les tokens (mémoire + persistance)
        /// </summary>
        private void ClearTokens()
        {
            _accessToken = null;
            _refreshToken = null;
            _tokenExpiresAt = DateTime.MinValue;

            if (persistTokens)
            {
                SecureStorage.DeleteToken(REFRESH_TOKEN_KEY);
                Debug.Log("[AuthManager] Refresh token supprimé du stockage");
            }

            Debug.Log("[AuthManager] Tokens effacés");
        }

        /// <summary>
        /// Convertit un code d'erreur backend en message user-friendly
        /// </summary>
        private string GetFriendlyErrorMessage(string errorCode)
        {
            return errorCode switch
            {
                "USERNAME_TAKEN" => "Ce nom d'utilisateur est déjà pris",
                "EMAIL_TAKEN" => "Cet email est déjà utilisé",
                "INVALID_PASSWORD" => "Mot de passe trop faible (min 8 caractères, 1 majuscule, 1 chiffre)",
                "INVALID_CREDENTIALS" => "Email ou mot de passe incorrect",
                "INVALID_EMAIL" => "Format d'email invalide",
                "ACCOUNT_BANNED" => "Votre compte a été suspendu",
                "ACCOUNT_DISABLED" => "Votre compte est désactivé",
                "VALIDATION_ERROR" => "Données invalides",
                "TOKEN_EXPIRED" => "Session expirée",
                "TOKEN_INVALID" => "Token invalide",
                _ => "Une erreur est survenue lors de l'authentification"
            };
        }

        #endregion
    }

    #region Custom Exceptions

    /// <summary>
    /// Exception levée lors d'erreurs d'authentification
    /// </summary>
    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
        public AuthException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion
}

