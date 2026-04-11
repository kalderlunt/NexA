using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Ajouté par code dans FriendSidePanelRow.Awake — pas besoin sur le prefab.
    /// Drag and drop vers un FriendFolderContainer + clic droit menu contextuel.
    ///
    /// Stratégie drop :
    ///   OnEndDrag effectue un raycast manuel (EventSystem.RaycastAll) pour trouver
    ///   le FriendFolderContainer sous le pointeur, sans dépendre de IDropHandler
    ///   (bloqué par les enfants du ScrollRect/Mask).
    /// </summary>
    public class FriendRowDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static FriendRowDragHandler CurrentlyDragged { get; set; }

        /// <summary>FriendshipId délégué au FriendSidePanelRow du même GameObject.</summary>
        public string FriendshipId => GetComponent<FriendSidePanelRow>()?.FriendshipId;

        /// <summary>Le FriendFolderContainer d'origine (avant drag).</summary>
        public FriendFolderContainer OriginalFolder { get; private set; }

        private CanvasGroup cg;
        private Transform   originalParent;
        private int         originalSiblingIndex;
        private Canvas      rootCanvas;
        private GameObject  ghost;
        private RectTransform ghostRect;

        private void Awake()
        {
            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
            while (rootCanvas != null && !rootCanvas.isRootCanvas)
                rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            CurrentlyDragged     = this;
            originalParent       = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            OriginalFolder       = GetComponentInParent<FriendFolderContainer>();

            // Ghost semi-transparent qui suit la souris (enfant du root Canvas)
            ghost = Instantiate(gameObject, rootCanvas.transform, false);
            ghost.name = "__DragGhost__";

            // Réinitialiser le RectTransform du ghost : anchors + pivot au centre
            // pour que anchoredPosition == position locale dans le canvas
            ghostRect = ghost.GetComponent<RectTransform>();
            ghostRect.anchorMin = new Vector2(0.5f, 0.5f);
            ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
            ghostRect.pivot     = new Vector2(0.5f, 0.5f);

            var ghostCg = ghost.GetComponent<CanvasGroup>() ?? ghost.AddComponent<CanvasGroup>();
            ghostCg.alpha          = 0.6f;
            ghostCg.blocksRaycasts = false;

            // Supprimer le handler sur le ghost pour éviter des drags parasites
            var dragComp = ghost.GetComponent<FriendRowDragHandler>();
            if (dragComp != null) Destroy(dragComp);

            // L'original reste en place, semi-transparent — il indique la position source
            cg.alpha          = 0.3f;
            cg.blocksRaycasts = false;

            UpdateGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghost != null) UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Rétablir l'original avant toute chose
            cg.alpha          = 1f;
            cg.blocksRaycasts = true;

            if (ghost != null) { Destroy(ghost); ghost = null; }

            // ── Raycast manuel pour trouver le FriendFolderContainer sous le pointeur ──
            // On ne peut pas se fier à IDropHandler car le ScrollRect/Mask intercepte les events.
            FriendFolderContainer targetFolder = FindFolderUnderPointer(eventData);

            if (targetFolder != null && targetFolder != OriginalFolder)
            {
                Debug.Log($"[FriendRowDragHandler] Drop manuel sur '{targetFolder.GroupName}'");

                // Retirer de la source
                OriginalFolder?.RemoveRow(gameObject);

                // Ajouter dans la cible
                targetFolder.AddRow(gameObject);

                // Appel API
                targetFolder.NotifyDropReceived(this);
            }
            else
            {
                // Annulation : remettre à la place d'origine
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
            }

            CurrentlyDragged = null;
            SocialPanel.Instance?.RefreshGroupHeaders();
        }

        /// <summary>
        /// Parcourt tous les résultats du raycast UI et retourne le premier
        /// FriendFolderContainer trouvé dans la hiérarchie.
        /// </summary>
        private FriendFolderContainer FindFolderUnderPointer(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                // Chercher le FriendFolderContainer sur le GO touché ou ses parents
                FriendFolderContainer folder = result.gameObject.GetComponentInParent<FriendFolderContainer>();
                if (folder != null)
                    return folder;
            }
            return null;
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (ghost == null || rootCanvas == null) return;
            if (ghostRect == null) ghostRect = ghost.GetComponent<RectTransform>();

            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.GetComponent<RectTransform>(),
                eventData.position, cam,
                out Vector2 local);
            ghostRect.anchoredPosition = local;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            FriendContextMenu.Instance?.Show(this, eventData.position);
        }
    }
}
