using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NexA.Hub.Models;
using Utils;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Ligne d'ami dans le panel social latéral.
    /// Affiche : avatar + dot statut + pseudo + activité en cours.
    /// FriendRowDragHandler est ajouté automatiquement dans Awake si absent du prefab.
    /// </summary>
    public class FriendSidePanelRow : MonoBehaviour
    {
        [Header("Avatar")]
        [SerializeField] private Image avatarImage;

        [Header("Statut")]
        [SerializeField] private Image statusDot;           // cercle coloré (12×12)

        [Header("Textes")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI activityText;

        [Header("Animation")]
        [SerializeField] [Range(0f, 1f)] private float animationDuration = 0.18f;

        private static readonly Color OnlineColor  = new(0.18f, 0.85f, 0.40f);   // vert
        private static readonly Color InGameColor  = new(0.98f, 0.68f, 0.10f);   // orange
        private static readonly Color OfflineColor = new(0.40f, 0.40f, 0.45f);   // gris

        // Chaque row a son propre CanvasGroup pour le fade-in
        private CanvasGroup cg;

        /// <summary>Vrai si l'ami est en ligne ou en jeu (utilisé par FriendFolderContainer pour les compteurs).</summary>
        public bool IsOnline { get; private set; }

        /// <summary>ID de l'amitié (friendshipId) — nécessaire pour les appels API de déplacement de dossier.</summary>
        public string FriendshipId { get; private set; }

        /// <summary>ID de l'ami (userId) — nécessaire pour suppression.</summary>
        public string FriendId { get; private set; }

        private void Awake()
        {
            // CanvasGroup pour le fade-in
            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Drag & drop + clic droit — ajouté par code pour ne pas forcer une modification du prefab
            if (!GetComponent<FriendRowDragHandler>())
                gameObject.AddComponent<FriendRowDragHandler>();
        }

        public void Setup(Friend friend, string friendshipId = null)
        {
            FriendId     = friend.id;
            FriendshipId = friendshipId;

            if (usernameText)
                usernameText.text = friend.username;

            IsOnline = friend.StatusNormalized == "online" || friend.StatusNormalized == "in-game";

            switch (friend.StatusNormalized)
            {
                case "online":
                    SetStatus(OnlineColor, "En ligne");
                    break;

                case "in-game":
                    string activity = !string.IsNullOrEmpty(friend.currentActivity)
                        ? friend.currentActivity
                        : "En jeu";
                    SetStatus(InGameColor, activity);
                    break;

                default:
                    SetStatus(OfflineColor, "Hors ligne");
                    break;
            }
        }

        /// <summary>
        /// Met à jour le statut en temps réel (appelé par SocialPanel via STOMP).
        /// Anime le changement de texte avec un effet typewriter.
        /// </summary>
        public void UpdateStatus(string backendStatus)
        {
            // Normaliser comme le fait Friend.StatusNormalized
            string normalized = backendStatus?.ToUpper() switch
            {
                "ONLINE"  => "online",
                "IN_GAME" => "in-game",
                "OFFLINE" => "offline",
                _         => "offline"
            };

            IsOnline = normalized == "online" || normalized == "in-game";

            switch (normalized)
            {
                case "online":
                    SetStatusAnimated(OnlineColor, "En ligne");
                    break;
                case "in-game":
                    SetStatusAnimated(InGameColor, "En jeu");
                    break;
                default:
                    SetStatusAnimated(OfflineColor, "Hors ligne");
                    break;
            }
        }

        private void SetStatus(Color color, string label)
        {
            if (statusDot)
                statusDot.color  = color;

            if (!activityText)
                return;

            activityText.text  = label;
            activityText.color = color;
        }

        /// <summary>Même chose que SetStatus mais avec animation typewriter sur le texte et transition de couleur sur le dot.</summary>
        private void SetStatusAnimated(Color color, string label)
        {
            // Animer la couleur du dot
            if (statusDot)
                statusDot.DOColor(color, animationDuration).SetEase(Ease.OutCubic);

            if (!activityText)
                return;

            // Animer la couleur du texte
            activityText.DOColor(color, animationDuration).SetEase(Ease.OutCubic);

            // Animer le changement de texte avec l'effet typewriter
            AnimationHelper.AnimateTextChange(activityText, label, animationDuration * 2f);
        }

        /// <summary>
        /// Joue l'animation d'apparition : fade-in uniquement.
        /// On ne touche PAS à anchoredPosition — le VerticalLayoutGroup la gère.
        /// Retourne le timestamp de fin d'animation (delay + duration).
        /// </summary>
        public float AnimateIn(float delay = 0f)
        {
            cg.alpha = 0f;
            DOTween.Kill(cg);
            cg.DOFade(1f, animationDuration)
              .SetDelay(delay)
              .SetEase(Ease.OutCubic);
            return delay + animationDuration;
        }

        private void OnDestroy()
        {
            DOTween.Kill(cg);
        }
    }
}
