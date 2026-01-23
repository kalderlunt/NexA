using System;
using System.Collections.Generic;

namespace NexA.Hub.Models
{
    // ============================================
    // MATCH MODELS
    // ============================================

    [Serializable]
    public class Match
    {
        public string id;
        public string mode; // "casual", "ranked", "custom"
        public string result; // "victory", "defeat", "draw"
        public int duration; // en secondes
        public int playerCount;
        public int myScore;
        public int myKills;
        public int myDeaths;
        public string createdAt;
    }

    [Serializable]
    public class MatchesListResponse
    {
        public List<Match> matches;
    }

    [Serializable]
    public class MatchDetails
    {
        public string id;
        public string mode;
        public string result;
        public int duration;
        public string map;
        public string createdAt;
        public List<MatchParticipant> participants;
    }

    [Serializable]
    public class MatchParticipant
    {
        public string userId;
        public string username;
        public string avatar;
        public string team; // "blue", "red", etc.
        public int score;
        public int kills;
        public int deaths;
        public int assists;
        public bool isMe;
    }

    [Serializable]
    public class MatchDetailsResponse
    {
        public MatchDetails match;
    }
}