using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Dropdown d'options du SocialPanel — version 2, construite entièrement par code.
    /// Remplace le GameObject dropdown existant (gardé pour compatibilité).
    ///
    /// Options :
    ///   [✓] Trier A → Z          (tri alphabétique)
    ///   [✓] Trier par état        (tri online/offline — défaut)
    ///   [✓] Grouper hors ligne    (toggle)
    ///
    /// Apparaît sous le bouton ☰ avec un slide-down + fade-in.
    /// Se ferme en cliquant dehors ou en re-cliquant sur une option.
    /// </summary>
    public class OptionsOverlayV2 : MonoBehaviour
    {
        // ── Callbacks (assignés par SocialPanel) ───────────────────────
        private Action _onSortAlpha;
        private Action _onSortStatus;
        private Action _onGroupOffline;

        // ── État actuel (miroir de SocialPanel) ───────────────────────
        private bool _sortByAlpha  = false;
        private bool _groupOffline = false;

        // ── Refs UI ────────────────────────────────────────────────────
        private CanvasGroup    _canvasGroup;
        private RectTransform  _panel;

        private OptionRow _rowSortAlpha;
        private OptionRow _rowSortStatus;
        private OptionRow _rowGroupOffline;

        // ── Backdrop (ferme au clic dehors) ───────────────────────────
        private GameObject _backdrop;

        // ── Struct interne ─────────────────────────────────────────────
        private class OptionRow
        {
            public GameObject   Root;
            public Image        Bg;
            public Image        CheckIcon;
            public TextMeshProUGUI Label;
            public Button       Btn;
        }

        // ── Unity lifecycle ────────────────────────────────────────────

        private void Start()
        {
            gameObject.SetActive(false);
        }

        // ── BuildUI ────────────────────────────────────────────────────

        /// <summary>
        /// Construit le dropdown par code.
        /// Appelé par SocialPanel.EnsureOverlay() juste après AddComponent.
        /// </summary>
        public void BuildUI(Action onSortAlpha, Action onSortStatus, Action onGroupOffline)
        {
            _onSortAlpha    = onSortAlpha;
            _onSortStatus   = onSortStatus;
            _onGroupOffline = onGroupOffline;

            int layer = gameObject.layer;

            // ── CanvasGroup (sur le root, pour fade) ───────────────────
            _canvasGroup = gameObject.GetComponent<CanvasGroup>()
                        ?? gameObject.AddComponent<CanvasGroup>();

            // ── Backdrop transparent (capture les clics dehors) ────────
            _backdrop = new GameObject("Backdrop");
            _backdrop.transform.SetParent(transform, false);
            _backdrop.layer = layer;
            RectTransform bdRT = _backdrop.AddComponent<RectTransform>();
            bdRT.anchorMin = new Vector2(-10, -10);
            bdRT.anchorMax = new Vector2(10, 10);
            bdRT.sizeDelta = Vector2.zero;
            Image bdImg = _backdrop.AddComponent<Image>();
            bdImg.color = new Color(0, 0, 0, 0.001f); // quasi-transparent mais raycastable
            Button bdBtn = _backdrop.AddComponent<Button>();
            bdBtn.targetGraphic = bdImg;
            bdBtn.onClick.AddListener(Hide);
            _backdrop.transform.SetAsFirstSibling(); // derrière le panel

            // ── Panel principal ────────────────────────────────────────
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(transform, false);
            panelGO.layer = layer;
            _panel = panelGO.AddComponent<RectTransform>();
            _panel.anchorMin        = new Vector2(1, 1);
            _panel.anchorMax        = new Vector2(1, 1);
            _panel.pivot            = new Vector2(1, 1);
            _panel.anchoredPosition = new Vector2(0, 0);
            _panel.sizeDelta        = new Vector2(160, 0); // hauteur auto via CSF

            // Fond du panel
            Image panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0.10f, 0.10f, 0.18f, 0.98f);

            // Layout
            VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 4, 4);
            vlg.spacing = 1f;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;

            ContentSizeFitter csf = panelGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Séparateur titre ───────────────────────────────────────
            AddSectionTitle(panelGO.transform, "Options", layer);

            // ── Lignes d'options ───────────────────────────────────────
            _rowSortStatus  = AddOptionRow(panelGO.transform, "Trier par état",     layer, OnSortStatusClicked);
            _rowSortAlpha   = AddOptionRow(panelGO.transform, "Trier A → Z",        layer, OnSortAlphaClicked);

            // Séparateur
            AddDivider(panelGO.transform, layer);

            _rowGroupOffline = AddOptionRow(panelGO.transform, "Grouper hors ligne", layer, OnGroupOfflineClicked);

            RefreshVisuals();
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Ouvre le dropdown avec slide-down + fade-in.</summary>
        public void Show(bool sortByAlpha, bool groupOffline)
        {
            _sortByAlpha  = sortByAlpha;
            _groupOffline = groupOffline;
            RefreshVisuals();

            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.15f).SetEase(Ease.OutCubic);
            }

            if (_panel)
            {
                Vector2 target = _panel.anchoredPosition;
                _panel.anchoredPosition = target + new Vector2(0, 8f);
                _panel.DOAnchorPos(target, 0.18f).SetEase(Ease.OutBack);
            }
        }

        /// <summary>Ferme le dropdown avec fade-out.</summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            EventSystem.current?.SetSelectedGameObject(null);

            if (_canvasGroup)
                _canvasGroup.DOFade(0f, 0.12f).SetEase(Ease.InCubic)
                    .OnComplete(() => gameObject.SetActive(false));
            else
                gameObject.SetActive(false);
        }

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Met à jour les visuels sans rouvrir.</summary>
        public void UpdateState(bool sortByAlpha, bool groupOffline)
        {
            _sortByAlpha  = sortByAlpha;
            _groupOffline = groupOffline;
            RefreshVisuals();
        }

        // ── Actions ────────────────────────────────────────────────────

        private void OnSortAlphaClicked()
        {
            _sortByAlpha = true;
            RefreshVisuals();
            _onSortAlpha?.Invoke();
            Hide();
        }

        private void OnSortStatusClicked()
        {
            _sortByAlpha = false;
            RefreshVisuals();
            _onSortStatus?.Invoke();
            Hide();
        }

        private void OnGroupOfflineClicked()
        {
            _groupOffline = !_groupOffline;
            RefreshVisuals();
            _onGroupOffline?.Invoke();
            // Ne ferme pas — c'est un toggle visible
        }

        // ── Visuels ────────────────────────────────────────────────────

        private void RefreshVisuals()
        {
            SetRowActive(_rowSortAlpha,    _sortByAlpha);
            SetRowActive(_rowSortStatus,   !_sortByAlpha);
            SetRowActive(_rowGroupOffline, _groupOffline);
        }

        private static void SetRowActive(OptionRow row, bool active)
        {
            if (row == null) return;

            Color bgColor     = active
                ? new Color(0.25f, 0.47f, 0.85f, 0.25f)
                : new Color(1f, 1f, 1f, 0f);
            Color textColor   = active ? Color.white : new Color(0.75f, 0.75f, 0.75f);
            Color checkColor  = active
                ? new Color(0.25f, 0.47f, 0.85f, 1f)
                : new Color(1f, 1f, 1f, 0f);

            if (row.Bg)        row.Bg.color        = bgColor;
            if (row.Label)     row.Label.color      = textColor;
            if (row.CheckIcon) row.CheckIcon.color  = checkColor;
        }

        // ── Helpers de construction ────────────────────────────────────

        private static void AddSectionTitle(Transform parent, string title, int layer)
        {
            GameObject go = new GameObject("SectionTitle");
            go.transform.SetParent(parent, false);
            go.layer = layer;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 24f; le.preferredHeight = 24f;
            go.AddComponent<CanvasRenderer>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = title.ToUpper();
            tmp.fontSize  = 8f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color     = new Color(0.5f, 0.5f, 0.6f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.margin    = new Vector4(10, 0, 0, 0);
        }

        private static void AddDivider(Transform parent, int layer)
        {
            GameObject go = new GameObject("Divider");
            go.transform.SetParent(parent, false);
            go.layer = layer;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 1f; le.preferredHeight = 1f;
            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.07f);
        }

        private static OptionRow AddOptionRow(Transform parent, string label, int layer, Action onClick)
        {
            OptionRow row = new OptionRow();

            // Root
            row.Root = new GameObject($"Row_{label}");
            row.Root.transform.SetParent(parent, false);
            row.Root.layer = layer;

            LayoutElement le = row.Root.AddComponent<LayoutElement>();
            le.minHeight = 32f; le.preferredHeight = 32f;

            // Fond de surbrillance
            row.Bg = row.Root.AddComponent<Image>();
            row.Bg.color = new Color(1f, 1f, 1f, 0f);

            // Bouton sur tout le root
            row.Btn = row.Root.AddComponent<Button>();
            ColorBlock cb = row.Btn.colors;
            cb.normalColor      = new Color(1, 1, 1, 0);
            cb.highlightedColor = new Color(1, 1, 1, 0.06f);
            cb.pressedColor     = new Color(1, 1, 1, 0.12f);
            cb.fadeDuration     = 0.08f;
            row.Btn.colors       = cb;
            row.Btn.targetGraphic = row.Bg;
            row.Btn.onClick.AddListener(() => onClick?.Invoke());

            // Layout horizontal
            HorizontalLayoutGroup hlg = row.Root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 0, 0);
            hlg.spacing = 8f;
            hlg.childAlignment      = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth   = true;
            hlg.childControlHeight  = true;

            // Icône check (rond coloré)
            GameObject checkGO = new GameObject("CheckIcon");
            checkGO.transform.SetParent(row.Root.transform, false);
            checkGO.layer = layer;
            LayoutElement checkLE = checkGO.AddComponent<LayoutElement>();
            checkLE.minWidth = 8f; checkLE.preferredWidth = 8f;
            checkLE.minHeight = 8f;
            row.CheckIcon = checkGO.AddComponent<Image>();
            row.CheckIcon.color = new Color(1, 1, 1, 0); // invisible par défaut

            // Label
            GameObject lblGO = new GameObject("Label");
            lblGO.transform.SetParent(row.Root.transform, false);
            lblGO.layer = layer;
            lblGO.AddComponent<CanvasRenderer>();
            row.Label = lblGO.AddComponent<TextMeshProUGUI>();
            row.Label.text      = label;
            row.Label.fontSize  = 11f;
            row.Label.color     = new Color(0.75f, 0.75f, 0.75f);
            row.Label.alignment = TextAlignmentOptions.MidlineLeft;
            row.Label.textWrappingMode = TextWrappingModes.NoWrap;

            return row;
        }

        private void OnDestroy()
        {
            _rowSortAlpha?.Btn?.onClick.RemoveAllListeners();
            _rowSortStatus?.Btn?.onClick.RemoveAllListeners();
            _rowGroupOffline?.Btn?.onClick.RemoveAllListeners();
        }
    }
}

