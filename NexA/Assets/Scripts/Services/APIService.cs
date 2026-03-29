using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using NexA.Hub.Models;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Service centralisé pour toutes les requêtes API vers le backend NexA
    /// Gère: timeout, retry, auth tokens, correlation IDs, parsing
    /// </summary>
    public class APIService : MonoBehaviour
    {
        public static APIService Instance { get; private set; }

        [Header("Configuration")]
//#if UNITY_EDITOR
        [SerializeField] private string baseURL = "http://192.168.2.184";
        [SerializeField] private string port = "8080";
		[SerializeField] private string extAPI = "api/v1";
		[SerializeField] private string extAPIAuth = "auth";
/*#else
        [SerializeField] private string baseURL = "https://api.nexa.game/v1";
#endif*/
        [SerializeField] private int timeoutSeconds = 10;
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float retryDelaySeconds = 1f;
        
        [Header("Testing Mode")]
        [Tooltip("Mode de vérification: true = backend réel, false = mode test (simulation)")]
        [SerializeField] private bool checkToBackend = true;
        
        /// <summary>
        /// Mode de vérification : true = backend, false = mode test
        /// Accessible globalement depuis n'importe où
        /// </summary>
        public static bool CHECK_TO_BACKEND => Instance != null ? Instance.checkToBackend : true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        #region Auth Endpoints

        public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
        {
            var body = new { username, email, password };
            return await SendRequestAsync<AuthResponse>($"/{extAPI}/{extAPIAuth}/register", "POST", body, requiresAuth: false);
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            var body = new { username, password };
            string bodyJson = JsonConvert.SerializeObject(body);
            Debug.Log($"  Body JSON: {bodyJson}");
            
            return await SendRequestAsync<AuthResponse>($"/{extAPI}/{extAPIAuth}/login", "POST", body, requiresAuth: false);
        }

        public async Task<RefreshResponse> RefreshTokenAsync(string refreshToken)
        {
            var body = new { refreshToken };
            return await SendRequestAsync<RefreshResponse>($"/{extAPI}/{extAPIAuth}/refresh", "POST", body, requiresAuth: false);
        }

        public async Task LogoutAsync()
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/{extAPIAuth}/logout", "POST");
        }

        /// <summary>
        /// Vérifie si un username existe déjà
        /// </summary>
        public async Task<CheckAvailabilityResponse> CheckUsernameAvailabilityAsync(string username)
        {
            string url = $"/{extAPI}/{extAPIAuth}/check-username?username={Uri.EscapeDataString(username)}";
            return await SendRequestAsync<CheckAvailabilityResponse>(url, "GET", requiresAuth: false);
        }

        /// <summary>
        /// Vérifie si un email existe déjà
        /// </summary>
        public async Task<CheckAvailabilityResponse> CheckEmailAvailabilityAsync(string email)
        {
            string url = $"/{extAPI}/{extAPIAuth}/check-email?email={Uri.EscapeDataString(email)}";
            return await SendRequestAsync<CheckAvailabilityResponse>(url, "GET", requiresAuth: false);
        }

        /// <summary>
        /// Inscription avec code de vérification (code généré côté Unity)
        /// </summary>
        public async Task<AuthResponse> RegisterWithCodeAsync(string username, string email, string password, string verificationCode)
        {
            Debug.Log($"[APIService] 📤 Envoi inscription avec code de vérification:");
            Debug.Log($"  Username: {username}");
            Debug.Log($"  Email: {email}");
            Debug.Log($"  Verification Code: '{verificationCode}' (longueur: {verificationCode.Length})");
            
            var body = new { username, email, password, verificationCode };
            return await SendRequestAsync<AuthResponse>($"/{extAPI}/{extAPIAuth}/register", "POST", body, requiresAuth: false);
        }

        /// <summary>
        /// Envoie un code de vérification par email
        /// </summary>
        public async Task<EmptyResponse> SendCustomCodeAsync(string email, string code)
        {
            Debug.Log($"[APIService] 📧 Envoi du code de vérification par email:");
            Debug.Log($"  Email: {email}");
            Debug.Log($"  Code: '{code}' (longueur: {code.Length})");
            
            var body = new { email, code };
            return await SendRequestAsync<EmptyResponse>($"/{extAPI}/{extAPIAuth}/send-custom-code", "POST", body, requiresAuth: false);
        }

        #endregion

        #region User Endpoints

        public async Task<User> GetCurrentUserAsync()
        {
            return await SendRequestAsync<User>("/me", "GET");
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            string url = $"/users/search?q={Uri.EscapeDataString(query)}";
            return await SendRequestAsync<List<User>>(url, "GET");
        }

        #endregion

        #region Friends Endpoints

        // ── Folders ────────────────────────────────────────────────────

        /// <summary>Liste sommaire de tous les dossiers (GET /api/v1/friends/folders).</summary>
        public async Task<List<FolderSummary>> GetFolderListAsync()
        {
            return await SendRequestAsync<List<FolderSummary>>($"/{extAPI}/friends/folders", "GET");
        }

        /// <summary>Détail d'un dossier avec ses amis (GET /api/v1/friends/folders/{id}).</summary>
        public async Task<FolderDetail> GetFolderDetailAsync(string folderId)
        {
            return await SendRequestAsync<FolderDetail>($"/{extAPI}/friends/folders/{folderId}", "GET");
        }

        /// <summary>Crée un nouveau dossier (POST /api/v1/friends/folders).</summary>
        public async Task<FolderSummary> CreateFolderAsync(string name)
        {
            var body = new { name };
            return await SendRequestAsync<FolderSummary>($"/{extAPI}/friends/folders", "POST", body);
        }

        /// <summary>Renomme un dossier (PATCH /api/v1/friends/folders/{id}).</summary>
        public async Task<FolderSummary> RenameFolderAsync(string folderId, string newName)
        {
            var body = new { name = newName };
            return await SendRequestAsync<FolderSummary>($"/{extAPI}/friends/folders/{folderId}", "PATCH", body);
        }

        /// <summary>Supprime un dossier (DELETE /api/v1/friends/folders/{id}).</summary>
        public async Task DeleteFolderAsync(string folderId)
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/friends/folders/{folderId}", "DELETE");
        }

        /// <summary>Déplace un ami dans un dossier (PUT /api/v1/friends/folders/{folderId}/friends/{friendshipId}).</summary>
        public async Task AssignFriendToFolderAsync(string folderId, string friendshipId)
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/friends/folders/{folderId}/friends/{friendshipId}", "PUT");
        }

        /// <summary>
        /// Retourne la liste plate de tous les amis en agrégeant tous les dossiers.
        /// Utilisé par SocialPanel pour l'affichage simple.
        /// </summary>
        public async Task<List<Friend>> GetFriendsAsync()
        {
            List<FolderSummary> folders = await GetFolderListAsync();
            HashSet<string>     seen    = new();
            List<Friend>        deduped = new();

            foreach (FolderSummary folder in folders)
            {
                FolderDetail detail = await GetFolderDetailAsync(folder.id);
                if (detail?.friendsList == null) continue;

                foreach (FolderFriendEntry entry in detail.friendsList)
                    if (entry?.friend != null && seen.Add(entry.friend.id))
                        deduped.Add(entry.friend);
            }

            return deduped;
        }

        /// <summary>
        /// Retourne les dossiers avec leurs amis (pour affichage multi-dossiers dans SocialPanel).
        /// </summary>
        public async Task<List<FolderDetail>> GetAllFoldersWithFriendsAsync()
        {
            var folders = await GetFolderListAsync();
            var result  = new List<FolderDetail>();
            foreach (var folder in folders)
            {
                var detail = await GetFolderDetailAsync(folder.id);
                if (detail != null) result.Add(detail);
            }
            return result;
        }

        // ── Friend Requests ────────────────────────────────────────────

        /// <summary>
        /// Envoie une demande d'ami (POST /api/v1/friends/request).
        /// Le body attend { friendId: uuid }
        /// </summary>
        public async Task<FriendRequest> SendFriendRequestAsync(string targetUserId)
        {
            var body = new { friendId = targetUserId };
            return await SendRequestAsync<FriendRequest>($"/{extAPI}/friends/request", "POST", body);
        }

        /// <summary>
        /// Récupère les demandes d'ami reçues en attente (GET /api/v1/friends/pending).
        /// </summary>
        public async Task<List<PendingFriendRequest>> GetPendingRequestsAsync()
        {
            return await SendRequestAsync<List<PendingFriendRequest>>($"/{extAPI}/friends/pending", "GET");
        }

        /// <summary>
        /// Accepte une demande d'ami (POST /api/v1/friends/{friendshipId}/accept).
        /// </summary>
        public async Task AcceptFriendRequestAsync(string friendshipId)
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/friends/{friendshipId}/accept", "POST");
        }

        /// <summary>
        /// Refuse une demande d'ami (POST /api/v1/friends/{friendshipId}/reject).
        /// </summary>
        public async Task RejectFriendRequestAsync(string friendshipId)
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/friends/{friendshipId}/reject", "POST");
        }

        /// <summary>
        /// Alias de GetPendingRequestsAsync — compatibilité avec FriendsScreen.
        /// </summary>
        public async Task<List<PendingFriendRequest>> GetFriendRequestsAsync()
            => await GetPendingRequestsAsync();

        /// <summary>
        /// Alias de RejectFriendRequestAsync — compatibilité avec FriendsScreen.
        /// </summary>
        public async Task DeclineFriendRequestAsync(string friendshipId)
            => await RejectFriendRequestAsync(friendshipId);

        /// <summary>
        /// Supprime un ami (DELETE /api/v1/friends/{friendshipId}).
        /// </summary>
        public async Task RemoveFriendAsync(string friendshipId)
        {
            await SendRequestAsync<EmptyResponse>($"/{extAPI}/friends/{friendshipId}", "DELETE");
        }

        #endregion

        #region Matches Endpoints

        public async Task<PaginatedResponse<Match>> GetMatchHistoryAsync(int limit = 20, string cursor = null)
        {
            string url = $"/matches?limit={limit}";
            if (!string.IsNullOrEmpty(cursor))
                url += $"&cursor={Uri.EscapeDataString(cursor)}";
            
            return await SendRequestAsync<PaginatedResponse<Match>>(url, "GET");
        }

        public async Task<Match> GetMatchDetailsAsync(string matchId)
        {
            return await SendRequestAsync<Match>($"/matches/{matchId}", "GET");
        }

        #endregion

        #region Core HTTP Logic

        private async Task<T> SendRequestAsync<T>(
            string endpoint,
            string method,
            object body = null,
            bool requiresAuth = true,
            int retryCount = 0)
        {
            string url = baseURL + $":{port}" + endpoint;
            
            using (UnityWebRequest request = CreateRequest(url, method, body))
            {
                // Auth header
                if (requiresAuth)
                {
                    string token = await AuthManager.Instance.GetValidAccessTokenAsync();
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                // Correlation ID pour traçabilité logs backend
                string correlationId = Guid.NewGuid().ToString();
                request.SetRequestHeader("X-Correlation-ID", correlationId);
                request.SetRequestHeader("Content-Type", "application/json");

                // Timeout
                request.timeout = timeoutSeconds;

                // Envoyer
                Debug.Log($"[API] {method} {url} | Correlation: {correlationId}");
                await request.SendWebRequest();

                // Gérer les erreurs
                if (request.result != UnityWebRequest.Result.Success)
                {
                    // Retry sur erreur réseau/serveur
                    if (ShouldRetry(request) && retryCount < maxRetries)
                    {
                        float delay = retryDelaySeconds * (retryCount + 1);
                        Debug.LogWarning($"[API] Retry {retryCount + 1}/{maxRetries} after {delay}s...");
                        await Task.Delay((int)(delay * 1000));
                        return await SendRequestAsync<T>(endpoint, method, body, requiresAuth, retryCount + 1);
                    }

                    // Erreur définitive
                    HandleError(request, correlationId);
                }

                // Parser réponse
                string responseText = request.downloadHandler.text;
                Debug.Log($"[API] Response: {responseText}");

                return ParseResponse<T>(responseText);
            }
        }

        private UnityWebRequest CreateRequest(string url, string method, object body)
        {
            UnityWebRequest request = new UnityWebRequest(url, method);
            request.downloadHandler = new DownloadHandlerBuffer();

            if (body != null && (method == "POST" || method == "PUT" || method == "PATCH"))
            {
                string jsonBody = JsonConvert.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            return request;
        }

        private bool ShouldRetry(UnityWebRequest request)
        {
            // Retry sur timeout/connexion ou erreur serveur 5xx
            return request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError && request.responseCode >= 500;
        }

        private void HandleError(UnityWebRequest request, string correlationId)
        {
            string errorMessage = "Une erreur est survenue";
            string errorCode = "UNKNOWN_ERROR";
            string rawResponse = "";

            try
            {
                // Parser l'erreur du backend
                rawResponse = request.downloadHandler.text;
                Debug.Log($"[API] Raw error response: {rawResponse}");
                
                // Essayer de parser comme objet structuré
                var errorResponse = JsonConvert.DeserializeObject<APIErrorResponse>(rawResponse);
                
                if (errorResponse?.error != null)
                {
                    errorMessage = errorResponse.error.message;
                    errorCode = errorResponse.error.code;
                }
                else
                {
                    // Essayer de parser comme format simple: {"error": "message"} ou {"details": {...}, "error": "..."}
                    try
                    {
                        var simpleError = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawResponse);
                        if (simpleError != null && simpleError.ContainsKey("error"))
                        {
                            string fullErrorMessage = simpleError["error"].ToString();
                            
                            // Si il y a un champ "details", extraire le premier message d'erreur
                            if (simpleError.ContainsKey("details"))
                            {
                                try
                                {
                                    var details = JsonConvert.DeserializeObject<Dictionary<string, string>>(simpleError["details"].ToString());
                                    if (details != null && details.Count > 0)
                                    {
                                        // Prendre le premier message d'erreur des détails
                                        foreach (var detail in details)
                                        {
                                            fullErrorMessage = detail.Value;
                                            break;
                                        }
                                    }
                                }
                                catch { /* Ignorer si parsing details échoue */ }
                            }
                            
                            // Nettoyer le message pour l'utilisateur
                            errorMessage = CleanErrorMessage(fullErrorMessage);
                            errorCode = GetErrorCodeFromMessage(fullErrorMessage);
                            
                            Debug.Log($"[API] Parsed simple error format - Code: {errorCode}, Message: {errorMessage}");
                        }
                        else
                        {
                            errorMessage = !string.IsNullOrEmpty(rawResponse) 
                                ? $"Réponse backend: {rawResponse}" 
                                : $"HTTP {request.responseCode}: {request.error}";
                        }
                    }
                    catch
                    {
                        // Si le parsing échoue, utiliser la réponse brute
                        errorMessage = !string.IsNullOrEmpty(rawResponse) 
                            ? $"Réponse backend: {rawResponse}" 
                            : $"HTTP {request.responseCode}: {request.error}";
                    }
                }
            }
            catch (Exception ex)
            {
                // Si parsing échoue, message détaillé
                Debug.LogError($"[API] Failed to parse error response: {ex.Message}");
                errorMessage = !string.IsNullOrEmpty(rawResponse)
                    ? $"HTTP {request.responseCode} - Réponse: {rawResponse}"
                    : $"HTTP {request.responseCode}: {request.error}";
            }

            Debug.LogError($"[API] Error {errorCode} | {errorMessage} | Correlation: {correlationId}");

            throw new APIException(errorCode, errorMessage, (int)request.responseCode);
        }
        
        /// <summary>
        /// Nettoie un message d'erreur SQL/technique pour le rendre user-friendly
        /// </summary>
        private string CleanErrorMessage(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage)) return "Une erreur est survenue";
            
            // Détecter les erreurs de contrainte de BDD
            if (rawMessage.Contains("clé dupliquée") || rawMessage.Contains("duplicate key"))
            {
                if (rawMessage.Contains("email"))
                    return "Cet email est déjà utilisé";
                if (rawMessage.Contains("username"))
                    return "Ce nom d'utilisateur est déjà utilisé";
                return "Cette valeur existe déjà dans la base de données";
            }
            
            // Détecter les erreurs SQL génériques
            if (rawMessage.Contains("could not execute statement") || rawMessage.Contains("SQL [insert"))
            {
                // Extraire le message utile si possible
                if (rawMessage.Contains("Detail:"))
                {
                    int detailIndex = rawMessage.IndexOf("Detail:", StringComparison.Ordinal);
                    int endIndex = rawMessage.IndexOf("]", detailIndex, StringComparison.Ordinal);
                    if (detailIndex > 0 && endIndex > detailIndex)
                    {
                        string detail = rawMessage.Substring(detailIndex + 7, endIndex - detailIndex - 7).Trim();
                        
                        // Traduire le message
                        if (detail.Contains("existe déjà"))
                        {
                            if (rawMessage.Contains("email"))
                                return "Cet email est déjà utilisé";
                            if (rawMessage.Contains("username"))
                                return "Ce nom d'utilisateur est déjà utilisé";
                        }
                        
                        return detail;
                    }
                }
                
                return "Erreur lors de l'enregistrement des données";
            }
            
            // Si le message est déjà simple et clair, le retourner tel quel
            if (rawMessage.Length < 200 && !rawMessage.Contains("[") && !rawMessage.Contains("SQL"))
            {
                return rawMessage;
            }
            
            return "Une erreur est survenue lors de l'opération";
        }
        
        /// <summary>
        /// Génère un code d'erreur à partir du message d'erreur
        /// </summary>
        private string GetErrorCodeFromMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return "UNKNOWN_ERROR";
            
            // Erreurs de validation de champs
            if (message.Contains("identifiant") && message.Contains("obligatoire"))
                return "MISSING_IDENTIFIER";
            if (message.Contains("mot de passe") && message.Contains("obligatoire"))
                return "MISSING_PASSWORD";
            if (message.Contains("obligatoire"))
                return "VALIDATION_ERROR";
            
            // Erreurs de contrainte de base de données
            if (message.Contains("clé dupliquée") || message.Contains("duplicate key") || message.Contains("existe déjà"))
            {
                if (message.Contains("email"))
                    return "EMAIL_ALREADY_EXISTS";
                if (message.Contains("username"))
                    return "USERNAME_ALREADY_EXISTS";
                return "DUPLICATE_ENTRY";
            }
            
            // Erreurs de validation
            if (message.Contains("email") && message.Contains("déjà utilisé"))
                return "EMAIL_ALREADY_EXISTS";
            if (message.Contains("username") && message.Contains("déjà utilisé"))
                return "USERNAME_ALREADY_EXISTS";
            if (message.Contains("code") && (message.Contains("incorrect") || message.Contains("invalide")))
                return "INVALID_CODE";
            if (message.Contains("code") && message.Contains("expiré"))
                return "CODE_EXPIRED";
                
            // Erreurs générales
            if (message.Contains("introuvable") || message.Contains("not found"))
                return "NOT_FOUND";
            if (message.Contains("non autorisé") || message.Contains("unauthorized"))
                return "UNAUTHORIZED";
            if (message.Contains("could not execute statement") || message.Contains("SQL"))
                return "DATABASE_ERROR";
            if (message.Contains("invalide"))
                return "VALIDATION_ERROR";
                
            return "UNKNOWN_ERROR";
        }

        private T ParseResponse<T>(string json)
        {
            if (typeof(T) == typeof(EmptyResponse))
                return default;

            try
            {
                Debug.Log($"[API] 📥 Parsing response for type: {typeof(T).Name}");
                Debug.Log($"[API] 📄 Raw JSON: {json}");
                
                // Parse directement le JSON (pas de wrapper APIResponse)
                T result = JsonConvert.DeserializeObject<T>(json);
                
                if (result != null)
                {
                    Debug.Log($"[API] ✅ Successfully parsed {typeof(T).Name}");
                    
                    // Log spécial pour AuthResponse pour voir les tokens
                    if (result is AuthResponse authResponse)
                    {
                        Debug.Log($"[API] 🔑 Token Data:");
                        Debug.Log($"  User: {authResponse.user?.username ?? "null"}");
                        Debug.Log($"  Access Token: {authResponse.tokens?.accessToken?.Substring(0, Math.Min(20, authResponse.tokens.accessToken?.Length ?? 0)) ?? "null"}...");
                        Debug.Log($"  Refresh Token: {authResponse.tokens?.refreshToken?.Substring(0, Math.Min(20, authResponse.tokens.refreshToken?.Length ?? 0)) ?? "null"}...");
                        Debug.Log($"  Expires In: {authResponse.tokens?.expiresIn ?? 0}");
                    }
                    
                    return result;
                }

                throw new Exception("Failed to deserialize response");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[API] ❌ Failed to parse response: {ex.Message}");
                Debug.LogError($"[API] JSON: {json}");
                Debug.LogError($"[API] Expected type: {typeof(T).FullName}");
                Debug.LogException(ex);
                throw;
            }
        }

        #endregion
    }

    #region Exceptions

    public class APIException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public APIException(string code, string message, int statusCode) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }

    #endregion
}

