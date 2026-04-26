using System;
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
    /// Overlay "Ajouter un ami" — se construit entièrement par code via BuildUI().
    /// Aucun prefab ni assignation dans l'Inspector nécessaire.
    ///
    /// Structure :
    ///  ┌─────────────────────────────────────────┐
    ///  │ [←]  Ajouter un ami                     │  ← TopRow
    ///  │ [________________________] [Rechercher] │  ← SearchRow
    ///  ├─────────────────────────────────────────┤
    ///  │  ● Pseudo        Niv.5      [+ Ami]     │  ← ResultItem (scroll)
    ///  │  ● Pseudo2       Niv.3      [✓ Envoyé]  │
    ///  │  (aucun résultat)                        │  ← emptyText
    ///  └─────────────────────────────────────────┘
    ///
    /// Utilisation depuis SocialPanel :
    ///   addFriendOverlayV2.Show();
    /// </summary>
    public class AddFriendOverlayV2 : MonoBehaviour
    {
        // ── Refs (créées par BuildUI ou assignées dans l'Inspector) ────
        [Header("Optionnel — assigné auto par BuildUI() si vide")]
        [SerializeField] private CanvasGroup   canvasGroup;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button         searchButton;
        [SerializeField] private Transform      resultsContainer;
        [SerializeField] private TextMeshProUGUI noResultsText;
        [SerializeField] private GameObject     loadingIndicator;
        [SerializeField] private Button         closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Spinner")]
        [Tooltip("Sprite de ton spinner — assigné dans l'Inspector ou via SetSpinnerSprite()")]
        [SerializeField] private Sprite spinnerSprite;

        /// <summary>
        /// Permet de passer le sprite du spinner depuis SocialPanel après création par code.
        /// Appelle cette méthode AVANT BuildUI().
        /// </summary>
        public void SetSpinnerSprite(Sprite sprite) => spinnerSprite = sprite;

        // ── État ──────────────────────────────────────────────────────
        private bool _searching = false;

        // ── Unity lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            closeButton?.onClick.AddListener(OnCloseClicked);
            searchButton?.onClick.AddListener(OnSearchClicked);
            searchInput?.onSubmit.AddListener(_ => OnSearchClicked());
            gameObject.SetActive(false);
        }

        // ── BuildUI ────────────────────────────────────────────────────

        /// <summary>
        /// Construit toute l'UI par code.
        /// Appelé par SocialPanel.EnsureOverlay() si l'overlay est créé dynamiquement.
        /// </summary>
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
            topRT.pivot     = new Vector2(0.5f, 1f);
            topRT.sizeDelta = new Vector2(0, 40);
            topRT.anchoredPosition = Vector2.zero;
            HorizontalLayoutGroup topHLG = topRow.AddComponent<HorizontalLayoutGroup>();
            topHLG.padding = new RectOffset(8, 8, 0, 0);
            topHLG.spacing = 6f;
            topHLG.childAlignment = TextAnchor.MiddleLeft;
            topHLG.childForceExpandHeight = true;
            topHLG.childControlWidth  = true;
            topHLG.childControlHeight = true;

            // Bouton Fermer
            closeButton = CreateIconButton(topRow.transform, "CloseButton", "<", 28f, layer);
            closeButton.onClick.AddListener(OnCloseClicked);

            // Titre
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(topRow.transform, false);
            titleGO.layer = layer;
            titleGO.AddComponent<RectTransform>();
            titleGO.AddComponent<CanvasRenderer>();
            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text      = "Ajouter un ami";
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
            searchRT.pivot     = new Vector2(0.5f, 1f);
            searchRT.sizeDelta = new Vector2(0, 36);
            searchRT.anchoredPosition = new Vector2(0, -40);
            HorizontalLayoutGroup searchHLG = searchRow.AddComponent<HorizontalLayoutGroup>();
            searchHLG.padding = new RectOffset(8, 8, 4, 4);
            searchHLG.spacing = 6f;
            searchHLG.childAlignment      = TextAnchor.MiddleLeft;
            searchHLG.childForceExpandHeight = true;
            searchHLG.childControlWidth   = true;
            searchHLG.childControlHeight  = true;

            // InputField
            GameObject inputGO = new GameObject("SearchInput");
            inputGO.transform.SetParent(searchRow.transform, false);
            inputGO.layer = layer;
            Image inputBg = inputGO.AddComponent<Image>();
            inputBg.color = new Color(0.12f, 0.12f, 0.22f, 1f);
            searchInput = inputGO.AddComponent<TMP_InputField>();
            searchInput.onSubmit.AddListener(_ => OnSearchClicked());
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
            phText.text    = "Pseudo ou email...";
            phText.fontSize = 11f;
            phText.color   = new Color(0.5f, 0.5f, 0.5f);
            phText.alignment = TextAlignmentOptions.MidlineLeft;
            // Text area
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

            // Bouton Rechercher
            searchButton = CreateIconButton(searchRow.transform, "SearchButton", "Chercher", 64f, layer,
                new Color(0.25f, 0.47f, 0.85f));
            searchButton.onClick.AddListener(OnSearchClicked);
            LayoutElement searchBtnLE = searchButton.gameObject.GetComponent<LayoutElement>()
                ?? searchButton.gameObject.AddComponent<LayoutElement>();
            searchBtnLE.minWidth = 64f; searchBtnLE.preferredWidth = 64f;

            // ── ScrollView ─────────────────────────────────────────────
            GameObject scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(transform, false);
            scrollGO.layer = layer;
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(0, 0);
            scrollRT.offsetMax = new Vector2(0, -76);
            scrollGO.AddComponent<RectMask2D>();
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal    = false;
            scrollRect.vertical      = true;
            scrollRect.movementType  = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20f;

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
            vlg.spacing = 3f;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            resultsContainer   = contentRT;
            scrollRect.content = contentRT;
            scrollRect.viewport = scrollRT;

            // ── NoResultsText ──────────────────────────────────────────
            GameObject noResGO = new GameObject("NoResultsText");
            noResGO.transform.SetParent(transform, false);
            noResGO.layer = layer;
            RectTransform noResRT = noResGO.AddComponent<RectTransform>();
            noResRT.anchorMin = new Vector2(0, 0.5f);
            noResRT.anchorMax = new Vector2(1, 0.5f);
            noResRT.sizeDelta = new Vector2(-16, 40);
            LayoutElement noResLE = noResGO.AddComponent<LayoutElement>();
            noResLE.ignoreLayout = true;
            noResGO.AddComponent<CanvasRenderer>();
            noResultsText = noResGO.AddComponent<TextMeshProUGUI>();
            noResultsText.text      = "Aucun résultat";
            noResultsText.fontSize  = 12f;
            noResultsText.color     = new Color(0.6f, 0.6f, 0.6f);
            noResultsText.alignment = TextAlignmentOptions.Center;
            noResGO.SetActive(false);

            // ── LoadingIndicator (UILoadingSpinner) ────────────────────
            loadingIndicator = new GameObject("LoadingIndicator");
            loadingIndicator.transform.SetParent(transform, false);
            loadingIndicator.layer = layer;
            RectTransform loadRT = loadingIndicator.AddComponent<RectTransform>();
            loadRT.anchorMin = new Vector2(0.5f, 0.5f);
            loadRT.anchorMax = new Vector2(0.5f, 0.5f);
            loadRT.sizeDelta = new Vector2(32, 32);
            // Image avec le sprite du spinner (ou fallback gris)
            Image loadImg = loadingIndicator.AddComponent<Image>();
            loadImg.color = Color.white;
            if (spinnerSprite) loadImg.sprite = spinnerSprite;
            else               loadImg.color  = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            // Composants requis par UILoadingSpinner
            loadingIndicator.AddComponent<CanvasGroup>();
            loadingIndicator.AddComponent<UILoadingSpinner>();
            loadingIndicator.SetActive(false);

            Debug.Log("[AddFriendOverlayV2] BuildUI OK.");
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Ouvre l'overlay avec animation fade-in.</summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
            }

            if (searchInput)
            {
                searchInput.text = "";
                searchInput.Select();
                searchInput.ActivateInputField();
            }

            ClearResults();
            if (noResultsText)    noResultsText.gameObject.SetActive(false);
            if (loadingIndicator) loadingIndicator.SetActive(false);
        }

        /// <summary>Ferme l'overlay avec animation fade-out.</summary>
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

        // ── Recherche ──────────────────────────────────────────────────

        private async void OnSearchClicked()
        {
            if (_searching) return;
            string query = searchInput ? searchInput.text.Trim() : "";

            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                ToastManager.Show("Minimum 2 caractères", ToastType.Warning);
                return;
            }

            _searching = true;
            ClearResults();
            if (loadingIndicator) loadingIndicator.SetActive(true);
            if (noResultsText)    noResultsText.gameObject.SetActive(false);

            try
            {
                List<User> users = await APIService.Instance.SearchUsersAsync(query);
                if (loadingIndicator) loadingIndicator.SetActive(false);

                if (users == null || users.Count == 0)
                {
                    if (noResultsText)
                    {
                        noResultsText.text = $"Aucun résultat pour « {query} »";
                        noResultsText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    float delay = 0f;
                    foreach (User user in users)
                    {
                        SpawnResultItem(user, delay);
                        //delay += 0.04f;
                        delay += 0.04f;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddFriendOverlayV2] Erreur recherche : {ex.Message}");
                if (loadingIndicator) loadingIndicator.SetActive(false);
                ToastManager.Show("Erreur lors de la recherche", ToastType.Error);
            }
            finally
            {
                _searching = false;
            }
        }

        // ── Construction résultats ─────────────────────────────────────

        private void SpawnResultItem(User user, float animDelay = 0f)
        {
            if (!resultsContainer) return;

            AddFriendResultItemV2 item = AddFriendResultItemV2.CreateItem(resultsContainer);
            item.gameObject.name = $"Result - {user.username}";
            item.Setup(user, OnSendRequestClicked);

            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetDelay(animDelay).SetEase(Ease.OutCubic);
            }
        }

        private void ClearResults()
        {
            if (!resultsContainer) return;
            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);
        }

        // ── Envoi demande ──────────────────────────────────────────────

        private async void OnSendRequestClicked(string userId, AddFriendResultItemV2 item)
        {
            item?.SetLoading(true);

            try
            {
                await APIService.Instance.SendFriendRequestAsync(userId);
                item?.SetSent();
                ToastManager.Show("Demande d'ami envoyée !", ToastType.Success);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddFriendOverlayV2] Erreur envoi demande : {ex.Message}");
                item?.SetLoading(false);
                ToastManager.Show("Erreur lors de l'envoi", ToastType.Error);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void OnCloseClicked()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            Hide();
        }

        private static Button CreateIconButton(Transform parent, string goName, string label,
            float width, int layer, Color? color = null)
        {
            Color btnColor = color ?? new Color(0.3f, 0.3f, 0.4f, 0.7f);

            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.layer = layer;

            Image img = go.AddComponent<Image>();
            img.color = btnColor;

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = btnColor;
            cb.highlightedColor = btnColor * 1.3f;
            cb.pressedColor     = btnColor * 0.7f;
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
            searchButton?.onClick.RemoveAllListeners();
        }
    }
}

