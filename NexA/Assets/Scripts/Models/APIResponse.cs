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

    [Serializable]
    public class CheckAvailabilityResponse
    {
        public bool available;
        public string message;
    }

    [Serializable]
    public class SendCodeResponse
    {
        public bool success;
        public string message;
        public int expiresIn; // Durée de validité du code en secondes (ex: 600 = 10 min)
    }

    [Serializable]
    public class VerifyCodeResponse
    {
        public bool success;
        public bool valid;
        public string message;
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