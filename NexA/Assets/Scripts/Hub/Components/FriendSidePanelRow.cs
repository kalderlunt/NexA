using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NexA.Hub.Models;

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

        private void Awake()
        {
            // CanvasGroup pour le fade-in
            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Drag & drop + clic droit — ajouté par code pour ne pas forcer une modification du prefab
            if (!GetComponent<FriendRowDragHandler>())
                gameObject.AddComponent<FriendRowDragHandler>();
        }

        public void Setup(Friend friend)
        {
            // Pseudo
            if (usernameText)
                usernameText.text = friend.username;

            // Utiliser StatusNormalized pour gérer les majuscules du backend ("ONLINE", "OFFLINE", "IN_GAME")
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

        private void SetStatus(Color color, string label)
        {
            if (statusDot)  
                statusDot.color  = color;

            if (!activityText)
                return;
            
            activityText.text  = label;
            activityText.color = color;
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
