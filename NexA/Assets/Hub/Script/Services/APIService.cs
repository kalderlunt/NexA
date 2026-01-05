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
        [SerializeField] private string baseURL = "https://api.nexa.game/v1";
        [SerializeField] private int timeoutSeconds = 10;
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float retryDelaySeconds = 1f;

        private void Awake()
        {
            if (Instance != null)
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
            return await SendRequestAsync<AuthResponse>("/auth/register", "POST", body, requiresAuth: false);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var body = new { email, password };
            return await SendRequestAsync<AuthResponse>("/auth/login", "POST", body, requiresAuth: false);
        }

        public async Task<RefreshResponse> RefreshTokenAsync(string refreshToken)
        {
            var body = new { refreshToken };
            return await SendRequestAsync<RefreshResponse>("/auth/refresh", "POST", body, requiresAuth: false);
        }

        public async Task LogoutAsync()
        {
            await SendRequestAsync<EmptyResponse>("/auth/logout", "POST");
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

        public async Task<List<Friend>> GetFriendsAsync()
        {
            return await SendRequestAsync<List<Friend>>("/friends", "GET");
        }

        public async Task<List<FriendRequest>> GetFriendRequestsAsync()
        {
            return await SendRequestAsync<List<FriendRequest>>("/friends/requests", "GET");
        }

        public async Task<FriendRequest> SendFriendRequestAsync(string targetUserId)
        {
            var body = new { targetUserId };
            return await SendRequestAsync<FriendRequest>("/friends/request", "POST", body);
        }

        public async Task<FriendshipResponse> AcceptFriendRequestAsync(string requestId)
        {
            var body = new { requestId };
            return await SendRequestAsync<FriendshipResponse>("/friends/accept", "POST", body);
        }

        public async Task DeclineFriendRequestAsync(string requestId)
        {
            var body = new { requestId };
            await SendRequestAsync<EmptyResponse>("/friends/decline", "POST", body);
        }

        public async Task RemoveFriendAsync(string userId)
        {
            await SendRequestAsync<EmptyResponse>($"/friends/{userId}", "DELETE");
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
            string url = baseURL + endpoint;
            
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

            try
            {
                // Parser l'erreur du backend
                string responseText = request.downloadHandler.text;
                var errorResponse = JsonConvert.DeserializeObject<APIErrorResponse>(responseText);
                
                if (errorResponse?.error != null)
                {
                    errorMessage = errorResponse.error.message;
                    errorCode = errorResponse.error.code;
                }
            }
            catch
            {
                // Si parsing échoue, message générique
                errorMessage = $"HTTP {request.responseCode}: {request.error}";
            }

            Debug.LogError($"[API] Error {errorCode} | {errorMessage} | Correlation: {correlationId}");

            throw new APIException(errorCode, errorMessage, (int)request.responseCode);
        }

        private T ParseResponse<T>(string json)
        {
            if (typeof(T) == typeof(EmptyResponse))
                return default;

            try
            {
                var response = JsonConvert.DeserializeObject<APIResponse<T>>(json);
                
                if (response.success && response.data != null)
                    return response.data;

                throw new Exception("Invalid API response format");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[API] Failed to parse response: {ex.Message}\nJSON: {json}");
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

