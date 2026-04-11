using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Overlay de recherche dans la liste d'amis du SocialPanel.
    /// Se construit entièrement par code via BuildUI() — aucun prefab requis.
    ///
    /// Deux modes :
    ///   1. Filtre local instantané sur la liste d'amis déjà chargés (frappe = résultat immédiat)
    ///   2. Si aucun ami trouvé : bouton pour basculer vers AddFriendOverlayV2 et inviter
    ///
    /// Structure :
    ///  ┌──────────────────────────────────────────────┐
    ///  │ [←]  Rechercher un ami                       │  ← TopRow
    ///  │ [_________________________________] [X]       │  ← SearchRow (input + clear)
    ///  ├──────────────────────────────────────────────┤
    ///  │  ● Pseudo       Niv.5     En ligne           │  ← FriendSearchResultItem (scroll)
    ///  │  ● Pseudo2      Niv.3     Hors ligne         │
    ///  │  (aucun résultat — "Inviter un ami ?")       │  ← emptyText + invite button
    ///  └──────────────────────────────────────────────┘
    /// </summary>
    public class SearchOverlayV2 : MonoBehaviour
    {
        // ── Refs (auto-créées par BuildUI) ─────────────────────────────
        [Header("Optionnel — assigné auto par BuildUI()")]
        [SerializeField] private CanvasGroup     canvasGroup;
        [SerializeField] private TMP_InputField  searchInput;
        [SerializeField] private Button          clearButton;
        [SerializeField] private Button          closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform       resultsContainer;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private Button          inviteButton;
        [SerializeField] private GameObject      loadingIndicator;

        [Header("Spinner")]
        [Tooltip("Sprite de ton spinner — assigné dans l'Inspector")]
        [SerializeField] private Sprite spinnerSprite;
        
        

        // ── Callback optionnel pour ouvrir AddFriendOverlayV2 ──────────
        private Action _onInviteClicked;

        // ── Cache des amis ─────────────────────────────────────────────
        private List<Friend> _allFriends = new();
        private string       _lastQuery  = "";

        // ── Unity lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            closeButton?.onClick.AddListener(OnCloseClicked);
            clearButton?.onClick.AddListener(OnClearClicked);
            searchInput?.onValueChanged.AddListener(OnInputChanged);
            inviteButton?.onClick.AddListener(OnInviteClicked);
            gameObject.SetActive(false);
        }

        // ── BuildUI ────────────────────────────────────────────────────

        /// <summary>Construit toute l'UI par code. Appelé par SocialPanel.EnsureOverlay().</summary>
        public void BuildUI()
        {
            int layer = gameObject.layer;

            // ── TopRow ─────────────────────────────────────────────────
            GameObject topRow = new GameObject("TopRow");
            topRow.transform.SetParent(transform, false);
            topRow.layer = layer;
            RectTransform topRT = topRow.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1);
            topRT.anchorMax = new Vector2(1, 1);
            topRT.pivot = new Vector2(0.5f, 1f);
            topRT.sizeDelta = new Vector2(0, 40);
            topRT.anchoredPosition = Vector2.zero;
            HorizontalLayoutGroup topHLG = topRow.AddComponent<HorizontalLayoutGroup>();
            topHLG.padding = new RectOffset(8, 8, 0, 0);
            topHLG.spacing = 6f;
            topHLG.childAlignment = TextAnchor.MiddleLeft;
            topHLG.childForceExpandHeight = true;
            topHLG.childControlWidth  = true;
            topHLG.childControlHeight = true;

            closeButton = CreateButton(topRow.transform, "CloseButton", "<", 28f, layer,
                new Color(0.3f, 0.3f, 0.4f, 0.7f));
            closeButton.onClick.AddListener(OnCloseClicked);

            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(topRow.transform, false);
            titleGO.layer = layer;
            titleGO.AddComponent<RectTransform>();
            titleGO.AddComponent<CanvasRenderer>();
            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text      = "Rechercher un ami";
            titleText.fontSize  = 13f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color     = Color.white;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // ── SearchRow ──────────────────────────────────────────────
            GameObject searchRow = new GameObject("SearchRow");
            searchRow.transform.SetParent(transform, false);
            searchRow.layer = layer;
            RectTransform searchRT = searchRow.AddComponent<RectTransform>();
            searchRT.anchorMin = new Vector2(0, 1);
            searchRT.anchorMax = new Vector2(1, 1);
            searchRT.pivot = new Vector2(0.5f, 1f);
            searchRT.sizeDelta = new Vector2(0, 36);
            searchRT.anchoredPosition = new Vector2(0, -40);
            HorizontalLayoutGroup searchHLG = searchRow.AddComponent<HorizontalLayoutGroup>();
            searchHLG.padding = new RectOffset(8, 8, 4, 4);
            searchHLG.spacing = 4f;
            searchHLG.childAlignment = TextAnchor.MiddleLeft;
            searchHLG.childForceExpandHeight = true;
            searchHLG.childControlWidth  = true;
            searchHLG.childControlHeight = true;

            // InputField
            GameObject inputGO = new GameObject("SearchInput");
            inputGO.transform.SetParent(searchRow.transform, false);
            inputGO.layer = layer;
            Image inputBg = inputGO.AddComponent<Image>();
            inputBg.color = new Color(0.12f, 0.12f, 0.22f, 1f);
            searchInput = inputGO.AddComponent<TMP_InputField>();
            searchInput.onValueChanged.AddListener(OnInputChanged);
            LayoutElement inputLE = inputGO.AddComponent<LayoutElement>();
            inputLE.flexibleWidth = 1f;

            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(inputGO.transform, false);
            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
            phRT.offsetMin = new Vector2(8, 0); phRT.offsetMax = new Vector2(-8, 0);
            placeholder.AddComponent<CanvasRenderer>();
            TextMeshProUGUI phText = placeholder.AddComponent<TextMeshProUGUI>();
            phText.text = "Pseudo...";
            phText.fontSize = 11f;
            phText.color = new Color(0.5f, 0.5f, 0.5f);
            phText.alignment = TextAlignmentOptions.MidlineLeft;

            // Text
            GameObject textArea = new GameObject("Text");
            textArea.transform.SetParent(inputGO.transform, false);
            RectTransform taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(8, 0); taRT.offsetMax = new Vector2(-8, 0);
            textArea.AddComponent<CanvasRenderer>();
            TextMeshProUGUI inputText = textArea.AddComponent<TextMeshProUGUI>();
            inputText.fontSize  = 11f;
            inputText.color     = Color.white;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            searchInput.textViewport  = taRT;
            searchInput.textComponent = inputText;
            searchInput.placeholder   = phText;

            // Bouton Clear (X)
            clearButton = CreateButton(searchRow.transform, "ClearButton", "X", 28f, layer,
                new Color(0.4f, 0.15f, 0.15f, 0.8f));
            clearButton.onClick.AddListener(OnClearClicked);
            clearButton.gameObject.SetActive(false);

            // ── Séparateur ─────────────────────────────────────────────
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(transform, false);
            sep.layer = layer;
            RectTransform sepRT = sep.AddComponent<RectTransform>();
            sepRT.anchorMin = new Vector2(0, 1);
            sepRT.anchorMax = new Vector2(1, 1);
            sepRT.pivot     = new Vector2(0.5f, 1f);
            sepRT.sizeDelta = new Vector2(-16, 1);
            sepRT.anchoredPosition = new Vector2(0, -76);
            Image sepImg = sep.AddComponent<Image>();
            sepImg.color = new Color(1f, 1f, 1f, 0.08f);

            // ── ScrollView ─────────────────────────────────────────────
            GameObject scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(transform, false);
            scrollGO.layer = layer;
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(0, 0);
            scrollRT.offsetMax = new Vector2(0, -80);
            scrollGO.AddComponent<RectMask2D>();
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical   = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20f;

            // Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
            contentGO.layer = layer;
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = Vector2.zero;
            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.spacing = 2f;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            resultsContainer   = contentRT;
            scrollRect.content  = contentRT;
            scrollRect.viewport = scrollRT;

            // ── EmptyState (centré, caché par défaut) ──────────────────
            GameObject emptyGO = new GameObject("EmptyState");
            emptyGO.transform.SetParent(transform, false);
            emptyGO.layer = layer;
            RectTransform emptyRT = emptyGO.AddComponent<RectTransform>();
            emptyRT.anchorMin = new Vector2(0, 0.5f);
            emptyRT.anchorMax = new Vector2(1, 0.5f);
            emptyRT.sizeDelta = new Vector2(-16, 80);
            LayoutElement emptyLE = emptyGO.AddComponent<LayoutElement>();
            emptyLE.ignoreLayout = true;
            VerticalLayoutGroup emptyVLG = emptyGO.AddComponent<VerticalLayoutGroup>();
            emptyVLG.childAlignment = TextAnchor.MiddleCenter;
            emptyVLG.childForceExpandWidth = true;
            emptyVLG.childForceExpandHeight = false;
            emptyVLG.childControlWidth  = true;
            emptyVLG.childControlHeight = true;
            emptyVLG.spacing = 8f;

            // Texte "Aucun résultat"
            GameObject emptyTxtGO = new GameObject("EmptyText");
            emptyTxtGO.transform.SetParent(emptyGO.transform, false);
            emptyTxtGO.AddComponent<RectTransform>();
            emptyTxtGO.AddComponent<CanvasRenderer>();
            emptyText = emptyTxtGO.AddComponent<TextMeshProUGUI>();
            emptyText.text = "Aucun ami trouvé";
            emptyText.fontSize = 12f;
            emptyText.color = new Color(0.6f, 0.6f, 0.6f);
            emptyText.alignment = TextAlignmentOptions.Center;
            LayoutElement emptyTxtLE = emptyTxtGO.AddComponent<LayoutElement>();
            emptyTxtLE.minHeight = 24f;

            // Bouton "Inviter un ami"
            GameObject inviteBtnGO = new GameObject("InviteButton");
            inviteBtnGO.transform.SetParent(emptyGO.transform, false);
            inviteBtnGO.layer = layer;
            Image inviteImg = inviteBtnGO.AddComponent<Image>();
            inviteImg.color = new Color(0.25f, 0.47f, 0.85f);
            inviteButton = inviteBtnGO.AddComponent<Button>();
            ColorBlock inviteCB = inviteButton.colors;
            inviteCB.normalColor      = new Color(0.25f, 0.47f, 0.85f);
            inviteCB.highlightedColor = new Color(0.35f, 0.57f, 0.95f);
            inviteCB.pressedColor     = new Color(0.18f, 0.35f, 0.65f);
            inviteButton.colors = inviteCB;
            inviteButton.targetGraphic = inviteImg;
            inviteButton.onClick.AddListener(OnInviteClicked);
            LayoutElement inviteLE = inviteBtnGO.AddComponent<LayoutElement>();
            inviteLE.minHeight = 30f; inviteLE.preferredHeight = 30f;
            GameObject inviteLblGO = new GameObject("Label");
            inviteLblGO.transform.SetParent(inviteBtnGO.transform, false);
            RectTransform inviteLblRT = inviteLblGO.AddComponent<RectTransform>();
            inviteLblRT.anchorMin = Vector2.zero; inviteLblRT.anchorMax = Vector2.one;
            inviteLblRT.sizeDelta = Vector2.zero;
            inviteLblGO.AddComponent<CanvasRenderer>();
            TextMeshProUGUI inviteLbl = inviteLblGO.AddComponent<TextMeshProUGUI>();
            inviteLbl.text = "+ Ajouter un ami";
            inviteLbl.fontSize = 11f;
            inviteLbl.fontStyle = FontStyles.Bold;
            inviteLbl.color = Color.white;
            inviteLbl.alignment = TextAlignmentOptions.Center;

            emptyGO.SetActive(false);

            // ── LoadingIndicator (UILoadingSpinner) ────────────────────
            loadingIndicator = new GameObject("LoadingIndicator");
            loadingIndicator.transform.SetParent(transform, false);
            loadingIndicator.layer = layer;
            RectTransform loadRT = loadingIndicator.AddComponent<RectTransform>();
            loadRT.anchorMin = new Vector2(0.5f, 0.5f);
            loadRT.anchorMax = new Vector2(0.5f, 0.5f);
            loadRT.sizeDelta = new Vector2(32, 32);
            Image loadImg = loadingIndicator.AddComponent<Image>();
            loadImg.color = Color.white;
            if (spinnerSprite) loadImg.sprite = spinnerSprite;
            else               loadImg.color  = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            loadingIndicator.AddComponent<CanvasGroup>();
            loadingIndicator.AddComponent<UILoadingSpinner>();
            loadingIndicator.SetActive(false);

            Debug.Log("[SearchOverlayV2] BuildUI OK.");
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>
        /// Ouvre l'overlay. Passe optionnellement une liste d'amis déjà chargés
        /// et un callback pour le bouton "Inviter".
        /// </summary>
        public async void Show(List<Friend> friends = null, Action onInviteClicked = null)
        {
            _onInviteClicked = onInviteClicked;
            gameObject.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
            }

            // Réinitialiser l'input
            if (searchInput)
            {
                searchInput.text = "";
                searchInput.Select();
                searchInput.ActivateInputField();
            }

            clearButton?.gameObject.SetActive(false);
            _lastQuery = "";

            if (friends != null)
            {
                // Liste fournie directement → affichage immédiat
                _allFriends = friends;
                ShowAll();
            }
            else
            {
                // Charger depuis le backend
                if (loadingIndicator) loadingIndicator.SetActive(true);
                ClearResults();

                try
                {
                    _allFriends = await APIService.Instance.GetFriendsAsync();
                    if (loadingIndicator) loadingIndicator.SetActive(false);
                    ShowAll();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SearchOverlayV2] Erreur chargement amis : {ex.Message}");
                    if (loadingIndicator) loadingIndicator.SetActive(false);
                    ToastManager.Show("Erreur lors du chargement", ToastType.Error);
                }
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

        // ── Filtrage ───────────────────────────────────────────────────

        private void OnInputChanged(string value)
        {
            string query = value.Trim();
            if (query == _lastQuery) return;
            _lastQuery = query;

            clearButton?.gameObject.SetActive(query.Length > 0);

            if (string.IsNullOrEmpty(query))
                ShowAll();
            else
                ShowFiltered(query);
        }

        private void ShowAll()
        {
            ClearResults();

            if (_allFriends == null || _allFriends.Count == 0)
            {
                ShowEmpty("Aucun ami pour l'instant", showInvite: true);
                return;
            }

            // Trier : online/in-game en premier, puis alphabétique
            List<Friend> sorted = _allFriends
                .OrderBy(f => f.StatusNormalized == "offline" ? 1 : 0)
                .ThenBy(f => f.username)
                .ToList();

            float delay = 0f;
            foreach (Friend friend in sorted)
            {
                SpawnItem(friend, delay);
                delay += 0.03f;
            }

            SetEmptyState(false);
        }

        private void ShowFiltered(string query)
        {
            ClearResults();

            List<Friend> matches = _allFriends
                .Where(f => f.username != null &&
                            f.username.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(f => f.StatusNormalized == "offline" ? 1 : 0)
                .ThenBy(f => f.username)
                .ToList();

            if (matches.Count == 0)
            {
                ShowEmpty($"Aucun ami « {query} »", showInvite: true);
                return;
            }

            float delay = 0f;
            foreach (Friend friend in matches)
            {
                SpawnItem(friend, delay);
                delay += 0.03f;
            }

            SetEmptyState(false);
        }

        // ── Construction items ─────────────────────────────────────────

        private void SpawnItem(Friend friend, float animDelay = 0f)
        {
            if (!resultsContainer) return;

            FriendSearchResultItem item = FriendSearchResultItem.CreateItem(resultsContainer);
            item.gameObject.name = $"Friend - {friend.username}";
            item.Setup(friend);

            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.18f).SetDelay(animDelay).SetEase(Ease.OutCubic);
            }
        }

        private void ClearResults()
        {
            if (!resultsContainer) return;
            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);
        }

        // ── Empty state ────────────────────────────────────────────────

        private void ShowEmpty(string message, bool showInvite = false)
        {
            SetEmptyState(true, message, showInvite);
        }

        private void SetEmptyState(bool visible, string message = "", bool showInvite = false)
        {
            Transform emptyParent = emptyText?.transform.parent;
            if (emptyParent) emptyParent.gameObject.SetActive(visible);

            if (visible)
            {
                if (emptyText) emptyText.text = message;
                inviteButton?.gameObject.SetActive(showInvite);
            }
        }

        // ── Actions ────────────────────────────────────────────────────

        private void OnClearClicked()
        {
            if (searchInput) searchInput.text = "";
            clearButton?.gameObject.SetActive(false);
            ShowAll();
            searchInput?.Select();
            searchInput?.ActivateInputField();
        }

        private void OnInviteClicked()
        {
            Hide();
            _onInviteClicked?.Invoke();
        }

        private void OnCloseClicked()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            Hide();
        }

        // ── Helper bouton ──────────────────────────────────────────────

        private static Button CreateButton(Transform parent, string goName, string label,
            float width, int layer, Color color)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.layer = layer;

            Image img = go.AddComponent<Image>();
            img.color = color;

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = color;
            cb.highlightedColor = color * 1.3f;
            cb.pressedColor     = color * 0.7f;
            cb.disabledColor    = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            btn.colors       = cb;
            btn.targetGraphic = img;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth = width; le.preferredWidth = width;

            GameObject lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            RectTransform lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text      = label;
            lbl.fontSize  = 11f;
            lbl.fontStyle = FontStyles.Bold;
            lbl.color     = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.textWrappingMode = TextWrappingModes.NoWrap;

            return btn;
        }

        private void OnDestroy()
        {
            closeButton?.onClick.RemoveAllListeners();
            clearButton?.onClick.RemoveAllListeners();
            inviteButton?.onClick.RemoveAllListeners();
            searchInput?.onValueChanged.RemoveAllListeners();
        }
    }
}

