using System.Collections.Generic;
using DG.Tweening;
using NexA.Hub.Models;
using NexA.Hub.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Overlay inline dans le SocialPanel listant les demandes d'ami reçues.
    ///
    /// Hiérarchie attendue (enfant direct de SocialPanel, même niveau qu'AddFriendOverlay) :
    ///  ┌───────────────────────────────────────┐
    ///  │ [←] Demandes reçues           (badge) │  ← TopRow
    ///  │ ─────────────────────────────────────  │
    ///  │  ● Pseudo   Niv.5   [✓]  [✗]         │  ← FriendRequestItem (ScrollView > Content)
    ///  │  ● Pseudo2  Niv.3   [✓]  [✗]         │
    ///  │  (aucune demande en attente)           │  ← emptyText
    ///  └───────────────────────────────────────┘
    ///
    /// Utilisation depuis SocialPanel :
    ///   friendRequestsOverlay.Show();
    /// </summary>
    public class FriendRequestsOverlay : MonoBehaviour
    {
        [Header("Structure")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Top row")]
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Liste")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject loadingIndicator;

        // ── Cycle de vie ───────────────────────────────────────────────

        private void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            closeButton?.onClick.AddListener(OnCloseClicked);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Construit toute l'UI de l'overlay par code.
        /// Appelé par SocialPanel.EnsureOverlay() quand l'overlay est créé dynamiquement.
        /// </summary>
        public void BuildUI()
        {
            int layer = gameObject.layer;

            // ── TopRow ─────────────────────────────────────────────────
            GameObject topRowGO = new GameObject("TopRow");
            topRowGO.transform.SetParent(transform, false);
            topRowGO.layer = layer;
            RectTransform topRT = topRowGO.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1);
            topRT.anchorMax = new Vector2(1, 1);
            topRT.pivot     = new Vector2(0.5f, 1f);
            topRT.sizeDelta = new Vector2(0, 40);
            topRT.anchoredPosition = Vector2.zero;
            HorizontalLayoutGroup topHLG = topRowGO.AddComponent<HorizontalLayoutGroup>();
            topHLG.padding = new RectOffset(8, 8, 0, 0);
            topHLG.spacing = 4f;
            topHLG.childAlignment = TextAnchor.MiddleLeft;
            topHLG.childForceExpandHeight = true;
            topHLG.childControlWidth = true;
            topHLG.childControlHeight = true;

            // Close button
            GameObject closeBtnGO = new GameObject("CloseButton");
            closeBtnGO.transform.SetParent(topRowGO.transform, false);
            closeBtnGO.layer = layer;
            Image closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);
            closeButton = closeBtnGO.AddComponent<Button>();
            closeButton.targetGraphic = closeBtnImg;
            closeButton.onClick.AddListener(OnCloseClicked);
            LayoutElement closeLE = closeBtnGO.AddComponent<LayoutElement>();
            closeLE.minWidth = 28; closeLE.preferredWidth = 28;
            // Label "←"
            GameObject closeLblGO = new GameObject("Label");
            closeLblGO.transform.SetParent(closeBtnGO.transform, false);
            RectTransform closeLblRT = closeLblGO.AddComponent<RectTransform>();
            closeLblRT.anchorMin = Vector2.zero; closeLblRT.anchorMax = Vector2.one;
            closeLblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeLbl = closeLblGO.AddComponent<TextMeshProUGUI>();
            closeLbl.text = "<";
            closeLbl.fontSize = 14f; closeLbl.fontStyle = FontStyles.Bold;
            closeLbl.color = Color.white;
            closeLbl.alignment = TextAlignmentOptions.Center;

            // Title text
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(topRowGO.transform, false);
            titleGO.layer = layer;
            titleGO.AddComponent<RectTransform>();
            titleGO.AddComponent<CanvasRenderer>();
            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Demandes reçues";
            titleText.fontSize = 13f; titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // ── ScrollView ─────────────────────────────────────────────
            GameObject scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(transform, false);
            scrollGO.layer = layer;
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(0, 0);
            scrollRT.offsetMax = new Vector2(0, -40);
            scrollGO.AddComponent<RectMask2D>();
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical   = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
            contentGO.layer = layer;
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = Vector2.zero;
            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.spacing = 4f;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            itemsContainer = contentRT;
            scrollRect.content  = contentRT;
            scrollRect.viewport = scrollRT;

            // ── EmptyText ──────────────────────────────────────────────
            GameObject emptyGO = new GameObject("EmptyText");
            emptyGO.transform.SetParent(transform, false);
            emptyGO.layer = layer;
            RectTransform emptyRT = emptyGO.AddComponent<RectTransform>();
            emptyRT.anchorMin = new Vector2(0, 0.5f);
            emptyRT.anchorMax = new Vector2(1, 0.5f);
            emptyRT.sizeDelta = new Vector2(-16, 32);
            LayoutElement emptyLE = emptyGO.AddComponent<LayoutElement>();
            emptyLE.ignoreLayout = true;
            emptyGO.AddComponent<CanvasRenderer>();
            emptyText = emptyGO.AddComponent<TextMeshProUGUI>();
            emptyText.text = "Aucune demande en attente";
            emptyText.fontSize = 12f;
            emptyText.color = new Color(0.6f, 0.6f, 0.6f);
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyGO.SetActive(false);

            // ── LoadingIndicator ───────────────────────────────────────
            loadingIndicator = new GameObject("LoadingIndicator");
            loadingIndicator.transform.SetParent(transform, false);
            loadingIndicator.layer = layer;
            RectTransform loadRT = loadingIndicator.AddComponent<RectTransform>();
            loadRT.anchorMin = new Vector2(0.5f, 0.5f);
            loadRT.anchorMax = new Vector2(0.5f, 0.5f);
            loadRT.sizeDelta = new Vector2(32, 32);
            Image loadImg = loadingIndicator.AddComponent<Image>();
            loadImg.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            loadingIndicator.SetActive(false);

            Debug.Log("[FriendRequestsOverlay] BuildUI OK.");
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Ouvre l'overlay et charge les demandes en attente.</summary>
        public async void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
            }

            ClearItems();
            if (emptyText)         emptyText.gameObject.SetActive(false);
            if (loadingIndicator)  loadingIndicator.SetActive(true);

            try
            {
                List<PendingFriendRequest> requests = await APIService.Instance.GetPendingRequestsAsync();
                if (loadingIndicator) loadingIndicator.SetActive(false);

                if (requests == null || requests.Count == 0)
                {
                    if (emptyText)
                    {
                        emptyText.text = "Aucune demande en attente";
                        emptyText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (titleText) titleText.text = $"Demandes reçues ({requests.Count})";

                    float delay = 0f;
                    foreach (PendingFriendRequest req in requests)
                    {
                        SpawnItem(req, delay);
                        delay += 0.05f;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FriendRequestsOverlay] Erreur chargement : {ex.Message}");
                if (loadingIndicator) loadingIndicator.SetActive(false);
                ToastManager.Show("Erreur lors du chargement des demandes", ToastType.Error);
            }
        }

        /// <summary>Ferme l'overlay avec animation.</summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;

            EventSystem.current?.SetSelectedGameObject(null);

            if (canvasGroup)
                canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InCubic)
                    .OnComplete(() => gameObject.SetActive(false));
            else
                gameObject.SetActive(false);
        }

        // ── Construction de la liste ───────────────────────────────────

        private void SpawnItem(PendingFriendRequest request, float animDelay = 0f)
        {
            if (!itemsContainer) return;

            FriendRequestItem item = FriendRequestItem.CreateItem(itemsContainer);
            item.gameObject.name = $"Request - {request.user?.username ?? request.id}";
            item.Setup(request, OnActionClicked);

            // Le fade-in est géré dans Setup() avec le delay
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg && animDelay > 0f)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetDelay(animDelay).SetEase(Ease.OutCubic);
            }
        }

        private void ClearItems()
        {
            if (!itemsContainer) return;
            foreach (Transform child in itemsContainer)
                Destroy(child.gameObject);
        }

        // ── Actions Accept / Decline ───────────────────────────────────

        private async void OnActionClicked(string friendshipId, FriendRequestItem item, bool accepted)
        {
            item?.SetLoading(true);

            try
            {
                if (accepted)
                {
                    await APIService.Instance.AcceptFriendRequestAsync(friendshipId);
                    ToastManager.Show("Demande acceptée !", ToastType.Success);
                    // Rafraîchir le SocialPanel pour afficher le nouvel ami
                    SocialPanel.Instance?.RefreshAsync();
                }
                else
                {
                    await APIService.Instance.RejectFriendRequestAsync(friendshipId);
                    ToastManager.Show("Demande refusée", ToastType.Info);
                }

                // Retirer l'item de la liste avec animation
                item?.AnimateOut(UpdateEmptyState);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FriendRequestsOverlay] Erreur action ({(accepted ? "accept" : "reject")}) : {ex.Message}");
                item?.SetLoading(false);
                ToastManager.Show("Erreur lors de l'opération", ToastType.Error);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void UpdateEmptyState()
        {
            if (!itemsContainer) return;

            int remaining = itemsContainer.childCount;

            if (titleText) titleText.text = remaining > 0
                ? $"Demandes reçues ({remaining})"
                : "Demandes reçues";

            if (emptyText)
            {
                emptyText.text = "Aucune demande en attente";
                emptyText.gameObject.SetActive(remaining == 0);
            }

            // Notifier le SocialPanel pour mettre à jour le badge
            SocialPanel.Instance?.RefreshPendingBadge(remaining);
        }

        private void OnCloseClicked()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            Hide();
        }

        private void OnDestroy()
        {
            closeButton?.onClick.RemoveAllListeners();
        }
    }
}




