using System;
using System.Collections.Generic;

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
        public string status; // "online", "offline", "in-game"
        public string lastSeenAt;
        public string friendsSince;
    }

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