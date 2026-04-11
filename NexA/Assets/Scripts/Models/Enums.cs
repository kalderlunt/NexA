namespace NexA.Hub.Screens
{
    /// <summary>
    /// Types d'écrans disponibles dans le client
    /// </summary>
    public enum ScreenType
    {
        None,
        Login,
        Register,
        RegisterMultiStep,
        Home,
        Profile,
        Friends,
        MatchHistory,
        MatchDetails
    }
}

namespace NexA.Hub.Components
{
    /// <summary>
    /// Types de notifications toast
    /// </summary>
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }
}

namespace NexA.Hub.Models
{
    /// <summary>
    /// Statut d'un utilisateur
    /// </summary>
    public enum UserStatus
    {
        Offline,
        Online,
        InGame,
        Away
    }

    /// <summary>
    /// Statut d'une demande d'ami
    /// </summary>
    public enum FriendStatus
    {
        Pending,
        Accepted,
        Declined,
        Blocked
    }

    /// <summary>
    /// Résultat d'un match
    /// </summary>
    public enum MatchResult
    {
        Victory,
        Defeat,
        Draw
    }
}

