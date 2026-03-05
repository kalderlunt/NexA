using System;
using System.Collections.Generic;

namespace NexA.Hub.Models
{
    // ============================================
    // USER MODELS
    // ============================================

    [Serializable]
    public class User
    {
        public string id;
        public string username;
        public string email;
        public string avatar;
        public int level;
        public int elo;
        public UserStats stats;
        public string status; // "online", "offline", "in-game"
        public string createdAt;
        public string lastSeenAt;
        public bool isFriend;
    }

    [Serializable]
    public class UserStats
    {
        public int totalMatches;
        public int wins;
        public int losses;
        public float winRate;
    }

    [Serializable]
    public class SearchUsersResponse
    {
        public List<User> users;
    }
}