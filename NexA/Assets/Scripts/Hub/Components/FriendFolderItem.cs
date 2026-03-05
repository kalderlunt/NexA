using DG.Tweening;
using NexA.Hub.Models;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Dossier rétractable dans le panel social.
    /// Exemple : "▼ Hors ligne (12)"
    /// Clic sur le header → réduit/agrandit la liste.
    /// </summary>
    public class FriendFolderItem : MonoBehaviour
    {
        [Header("Header du dossier")]
        [SerializeField] private Button headerButton;
        [SerializeField] private TextMeshProUGUI folderLabel;
        [SerializeField] private RectTransform arrowIcon;   // tourne 0° ouvert, -90° fermé

        [Header("Contenu")]
        [SerializeField] public Transform contentContainer; // enfants = FriendSidePanelRow

        private bool isExpanded = true;

        private CanvasGroup headerCg;

        private void Awake()
        {
            // CanvasGroup sur le header pour le fade-in du titre de dossier
            if (headerButton != null)
            {
                headerCg = headerButton.GetComponent<CanvasGroup>()
                           ?? headerButton.gameObject.AddComponent<CanvasGroup>();
                Assert.IsNotNull(headerCg, "CanvasGroup ajouté au headerButton ne doit pas être null");
                headerCg.alpha = 0f;
            }
        }

        /// <summary>
        /// Initialise le dossier avec un titre et une liste d'amis.
        /// </summary>
        public void Setup(string title, List<Friend> friends, GameObject friendItemPrefab)
        {
            if (folderLabel != null) folderLabel.text = title;
            headerButton?.onClick.AddListener(ToggleFolder);

            // Spawner les items
            foreach (var friend in friends)
            {
                var row = Instantiate(friendItemPrefab, contentContainer);
                row.GetComponent<FriendSidePanelRow>()?.Setup(friend);
                // alpha 0 dès le départ — AnimateIn() les révèle
                var rowCg = row.GetComponent<CanvasGroup>() ?? row.AddComponent<CanvasGroup>();
                rowCg.alpha = 0f;
            }
        }

        /// <summary>
        /// Anime l'apparition du dossier :
        ///   1. Fade-in du header
        ///   2. Si ouvert, fade-in de chaque enfant en cascade
        /// Retourne le delay cumulé après la dernière animation.
        /// </summary>
        public float AnimateIn(float startDelay = 0f)
        {
            const float headerDuration  = 0.18f;
            const float childStagger    = 0.06f;  // écart entre chaque enfant
            const float childDuration   = 0.16f;

            float cursor = startDelay;

            // ── 1. Header ────────────────────────────────────────────
            if (headerCg != null)
            {
                DOTween.Kill(headerCg);
                headerCg.alpha = 0f;

                var rt = headerButton.GetComponent<RectTransform>();
                Vector2 originalPos = rt != null ? rt.anchoredPosition : Vector2.zero;
                if (rt) rt.anchoredPosition = originalPos + new Vector2(20f, 0f);

                Sequence hs = DOTween.Sequence();
                hs.SetDelay(cursor);
                hs.Append(headerCg.DOFade(1f, headerDuration).SetEase(Ease.OutCubic));
                if (rt) hs.Join(rt.DOAnchorPos(originalPos, headerDuration).SetEase(Ease.OutCubic));

                cursor += headerDuration;
            }

            // ── 2. Enfants (uniquement si dossier ouvert) ────────────
            if (isExpanded && contentContainer != null)
            {
                foreach (Transform child in contentContainer)
                {
                    var row = child.GetComponent<FriendSidePanelRow>();
                    if (row != null)
                    {
                        cursor = row.AnimateIn(cursor);
                        cursor += childStagger - childDuration; // superposition légère
                    }
                }
            }

            return cursor;
        }

        private void ToggleFolder()
        {
            isExpanded = !isExpanded;

            // Rotation de la flèche
            if (arrowIcon != null)
            {
                float targetZ = isExpanded ? 0f : -90f;
                arrowIcon.DOLocalRotate(new Vector3(0f, 0f, targetZ), 0.2f).SetEase(Ease.OutCubic);
            }

            // Afficher / cacher le contenu
            if (contentContainer != null)
                contentContainer.gameObject.SetActive(isExpanded);
        }

        private void OnDestroy()
        {
            headerButton?.onClick.RemoveAllListeners();
        }
    }
}
