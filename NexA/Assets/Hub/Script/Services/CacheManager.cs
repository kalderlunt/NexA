using System;
using System.Collections.Generic;
using UnityEngine;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Cache en mémoire simple pour friends list, match history, etc.
    /// Évite les appels API inutiles
    /// </summary>
    public class CacheManager : MonoBehaviour
    {
        public static CacheManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int defaultTTLSeconds = 300; // 5 minutes

        private Dictionary<string, CacheEntry> _cache = new();

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

        private void Update()
        {
            // Cleanup expired entries périodiquement (toutes les 10s)
            if (Time.frameCount % 600 == 0) // ~10s à 60fps
            {
                CleanupExpired();
            }
        }

        /// <summary>
        /// Stocker une valeur dans le cache
        /// </summary>
        public void Set<T>(string key, T data, int? ttlSeconds = null)
        {
            int ttl = ttlSeconds ?? defaultTTLSeconds;

            _cache[key] = new CacheEntry
            {
                Data = data,
                ExpiresAt = DateTime.UtcNow.AddSeconds(ttl),
                Type = typeof(T)
            };

            Debug.Log($"[Cache] Set key '{key}' with TTL {ttl}s");
        }

        /// <summary>
        /// Récupérer une valeur du cache (null si absente ou expirée)
        /// </summary>
        public T Get<T>(string key) where T : class
        {
            if (!_cache.TryGetValue(key, out CacheEntry entry))
            {
                Debug.Log($"[Cache] Miss for key '{key}'");
                return null;
            }

            // Vérifier expiration
            if (DateTime.UtcNow >= entry.ExpiresAt)
            {
                Debug.Log($"[Cache] Expired key '{key}'");
                _cache.Remove(key);
                return null;
            }

            // Vérifier le type
            if (entry.Type != typeof(T))
            {
                Debug.LogWarning($"[Cache] Type mismatch for key '{key}': expected {typeof(T)}, got {entry.Type}");
                return null;
            }

            Debug.Log($"[Cache] Hit for key '{key}'");
            return entry.Data as T;
        }

        /// <summary>
        /// Vérifier si une clé existe et est valide
        /// </summary>
        public bool Has(string key)
        {
            if (!_cache.TryGetValue(key, out CacheEntry entry))
                return false;

            if (DateTime.UtcNow >= entry.ExpiresAt)
            {
                _cache.Remove(key);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invalider (supprimer) une clé
        /// </summary>
        public void Invalidate(string key)
        {
            if (_cache.Remove(key))
            {
                Debug.Log($"[Cache] Invalidated key '{key}'");
            }
        }

        /// <summary>
        /// Invalider toutes les clés contenant un pattern
        /// </summary>
        public void InvalidatePattern(string pattern)
        {
            List<string> keysToRemove = new();

            foreach (var key in _cache.Keys)
            {
                if (key.Contains(pattern))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            Debug.Log($"[Cache] Invalidated {keysToRemove.Count} keys matching '{pattern}'");
        }

        /// <summary>
        /// Vider tout le cache
        /// </summary>
        public void InvalidateAll()
        {
            int count = _cache.Count;
            _cache.Clear();
            Debug.Log($"[Cache] Cleared all {count} entries");
        }

        /// <summary>
        /// Nettoyer les entrées expirées
        /// </summary>
        private void CleanupExpired()
        {
            List<string> expiredKeys = new();
            DateTime now = DateTime.UtcNow;

            foreach (var kvp in _cache)
            {
                if (now >= kvp.Value.ExpiresAt)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            if (expiredKeys.Count > 0)
            {
                Debug.Log($"[Cache] Cleaned up {expiredKeys.Count} expired entries");
            }
        }

        /// <summary>
        /// Obtenir des statistiques du cache
        /// </summary>
        public CacheStats GetStats()
        {
            int validCount = 0;
            int expiredCount = 0;
            DateTime now = DateTime.UtcNow;

            foreach (var entry in _cache.Values)
            {
                if (now >= entry.ExpiresAt)
                    expiredCount++;
                else
                    validCount++;
            }

            return new CacheStats
            {
                TotalEntries = _cache.Count,
                ValidEntries = validCount,
                ExpiredEntries = expiredCount
            };
        }

        private class CacheEntry
        {
            public object Data;
            public DateTime ExpiresAt;
            public Type Type;
        }
    }

    [Serializable]
    public struct CacheStats
    {
        public int TotalEntries;
        public int ValidEntries;
        public int ExpiredEntries;
    }

    /// <summary>
    /// Clés de cache prédéfinies (évite les magic strings)
    /// </summary>
    public static class CacheKeys
    {
        public const string FRIENDS_LIST = "friends_list";
        public const string FRIEND_REQUESTS = "friend_requests";
        public const string MATCH_HISTORY = "match_history";
        public const string USER_PROFILE = "user_profile_{0}"; // Format avec userId
        public const string CURRENT_USER = "current_user";

        public static string UserProfile(string userId) => string.Format(USER_PROFILE, userId);
    }
}

