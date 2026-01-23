﻿using System;
using System.Collections.Generic;

namespace NexA.Hub.Models
{
    // ============================================
    // AUTH RESPONSES
    // ============================================

    [Serializable]
    public class AuthResponse
    {
        public User user;
        public TokenData tokens;
    }

    [Serializable]
    public class RefreshResponse
    {
        public string accessToken;
        public long expiresIn;
    }

    [Serializable]
    public class TokenData
    {
        public string accessToken;
        public string refreshToken;
        public long expiresIn;
    }
    
    
    // ============================================
    // API RESPONSE WRAPPER
    // ============================================

    [Serializable]
    public class APIResponse<T>
    {
        public bool success;
        public T data;
        public PaginationMeta meta;
    }

    [Serializable]
    public class PaginatedResponse<T>
    {
        public List<T> data;
        public PaginationMeta meta;
    }

    [Serializable]
    public class PaginationMeta
    {
        public string nextCursor;
        public bool hasMore;
        public int total;
    }

    // ============================================
    // EMPTY RESPONSE (pour DELETE, etc.)
    // ============================================

    [Serializable]
    public class EmptyResponse
    {
        // Pas de données
    }
}