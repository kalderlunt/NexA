using UnityEngine.EventSystems;
using UnityEngine;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Ajouté par code dans FriendSidePanelRow.Awake — pas besoin sur le prefab.
    /// Drag and drop vers un FriendGroupContainer + clic droit menu contextuel.
    /// </summary>
    public class FriendRowDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static FriendRowDragHandler CurrentlyDragged { get; set; }

        /// <summary>FriendshipId délégué au FriendSidePanelRow du même GameObject.</summary>
        public string FriendshipId => GetComponent<FriendSidePanelRow>()?.FriendshipId;

        private CanvasGroup   cg;
        private Transform     originalParent;
        private int           originalSiblingIndex;
        private Canvas        rootCanvas;
        private GameObject    ghost;
        private Vector2       dragOffset; // offset entre le pointeur et l'anchoredPosition du ghost

        private void Awake()
        {
            // CanvasGroup déjà créé par FriendSidePanelRow.Awake, on le récupère ou crée
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

            // Ghost semi-transparent qui suit la souris
            ghost = Instantiate(gameObject, rootCanvas.transform, false);
            ghost.name = "__DragGhost__";
            var ghostCg = ghost.GetComponent<CanvasGroup>() ?? ghost.AddComponent<CanvasGroup>();
            ghostCg.alpha          = 0.6f;
            ghostCg.blocksRaycasts = false;

            var dragComp = ghost.GetComponent<FriendRowDragHandler>();
            if (dragComp != null) Destroy(dragComp);

            cg.alpha          = 0.3f;
            cg.blocksRaycasts = false;

            // Calculer l'offset : position locale souris - anchoredPosition de l'élément original
            var canvasRect = rootCanvas.GetComponent<RectTransform>();
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out Vector2 pointerLocal);
            Vector2 elemLocal = ((RectTransform)transform).anchoredPosition;

            // Convertir anchoredPosition de l'élément (qui est dans son parent) en espace Canvas
            Vector2 elemInCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(cam, transform.position),
                cam, out elemInCanvas);

            dragOffset = elemInCanvas - pointerLocal;
            UpdateGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ghost != null) UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            cg.alpha          = 1f;
            cg.blocksRaycasts = true;

            if (ghost != null) { Destroy(ghost); ghost = null; }

            if (CurrentlyDragged == this)
            {
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
                CurrentlyDragged = null;
            }

            SocialPanel.Instance?.RefreshGroupHeaders();
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (ghost == null || rootCanvas == null) return;
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.GetComponent<RectTransform>(),
                eventData.position,
                cam,
                out Vector2 local);
            ghost.GetComponent<RectTransform>().anchoredPosition = local + dragOffset;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            FriendContextMenu.Instance?.Show(this, eventData.position);
        }
    }
}
