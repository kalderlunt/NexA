using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NexA.Hub.Models
{
    // ============================================
    // FRIENDS MODELS
    // ============================================

    [Serializable]
    public class Friend
    {
        public string id;
        public string username;
        public string avatar;
        public int level;
        public int elo;
        // Backend retourne "ONLINE", "OFFLINE", "IN_GAME" (majuscules)
        public string status;
        public string currentActivity;  // ex: "Sélection des champions"
        public string lastSeenAt;
        public string friendsSince;

        /// <summary>Normalise le statut backend (majuscules) en statut Unity (minuscules).</summary>
        public string StatusNormalized => status?.ToUpper() switch
        {
            "ONLINE"   => "online",
            "IN_GAME"  => "in-game",
            "OFFLINE"  => "offline",
            _          => "offline"
        };
    }

    // ── API Folders ────────────────────────────────────────────────

    /// <summary>Réponse de GET /api/v1/friends/folders (liste sommaire).</summary>
    [Serializable]
    public class FolderSummary
    {
        public string id;
        public string name;
        public bool   isDefault;
        public int    friendCount;
        public string createdAt;
    }

    /// <summary>Entrée ami dans un dossier (GET /api/v1/friends/folders/{id}).</summary>
    [Serializable]
    public class FolderFriendEntry
    {
        public string friendshipId;
        public Friend friend;
        public string friendsSince;
        public string folderId;
        public string folderName;
    }

    /// <summary>Réponse de GET /api/v1/friends/folders/{id}.</summary>
    [Serializable]
    public class FolderDetail
    {
        public string id;
        public string name;
        public bool   isDefault;
        public string createdAt;

        [JsonProperty("friends")]
        public List<FolderFriendEntry> friendsList;
    }

    // ── Legacy models (garde la compatibilité) ─────────────────────

    [Serializable]
    public class FriendsListResponse
    {
        public List<Friend> friends;
    }

    [Serializable]
    public class FriendRequest
    {
        public string id;
        public User from;
        public User to;
        public string createdAt;
    }

    [Serializable]
    public class FriendRequestsResponse
    {
        public List<FriendRequest> incoming;
        public List<FriendRequest> outgoing;
    }

    [Serializable]
    public class FriendRequestResponse
    {
        public string requestId;
        public User to;
        public string createdAt;
    }

    [Serializable]
    public class FriendshipResponse
    {
        public FriendshipData friendship;
    }

    [Serializable]
    public class FriendshipData
    {
        public string id;
        public Friend friend;
        public string createdAt;
    }
}