using System;
using DG.Tweening;
using NexA.Hub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Item de résultat de recherche dans AddFriendOverlayV2.
    /// Créé entièrement par code via CreateItem() — aucun prefab requis.
    ///
    /// Structure :
    ///   ┌──────────────────────────────────────────────┐
    ///   │ ● [Username]     Niv.X              [+ Ami] │
    ///   └──────────────────────────────────────────────┘
    /// </summary>
    public class AddFriendResultItemV2 : MonoBehaviour
    {
        // ── Refs internes ──────────────────────────────────────────────
        private TextMeshProUGUI usernameText;
        private TextMeshProUGUI levelText;
        private Image           statusDot;
        private Button          actionButton;
        private TextMeshProUGUI buttonLabel;

        // ── État ──────────────────────────────────────────────────────
        private string _userId;
        private Action<string, AddFriendResultItemV2> _onSendRequest;
        private bool   _sent = false;

        // ── Factory ───────────────────────────────────────────────────

        /// <summary>
        /// Crée un item complet par code et l'attache au parent.
        /// </summary>
        public static AddFriendResultItemV2 CreateItem(Transform parent)
        {
            int layer = parent.gameObject.layer;

            // ── Racine ─────────────────────────────────────────────────
            GameObject root = new GameObject("AddFriendResultItem");
            root.transform.SetParent(parent, false);
            root.layer = layer;

            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(0, 40);

            root.AddComponent<CanvasGroup>(); // pour fade-in

            HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 6, 6);
            hlg.spacing = 6f;
            hlg.childAlignment      = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth  = true;
            hlg.childControlHeight = true;

            ContentSizeFitter csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Fond léger pour distinguer les items
            Image rowBg = root.AddComponent<Image>();
            rowBg.color = new Color(1f, 1f, 1f, 0.04f);

            AddFriendResultItemV2 item = root.AddComponent<AddFriendResultItemV2>();

            // ── Point de statut ────────────────────────────────────────
            GameObject dotGO = new GameObject("StatusDot");
            dotGO.transform.SetParent(root.transform, false);
            dotGO.layer = layer;
            LayoutElement dotLE = dotGO.AddComponent<LayoutElement>();
            dotLE.minWidth = 8; dotLE.preferredWidth = 8;
            dotLE.minHeight = 8;
            item.statusDot = dotGO.AddComponent<Image>();
            item.statusDot.color = new Color(0.5f, 0.5f, 0.5f);

            // ── InfoContainer ──────────────────────────────────────────
            GameObject info = new GameObject("Info");
            info.transform.SetParent(root.transform, false);
            info.layer = layer;
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;
            VerticalLayoutGroup vlg = info.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = true;
            vlg.spacing = 1f;

            // UsernameText
            GameObject unameGO = new GameObject("UsernameText");
            unameGO.transform.SetParent(info.transform, false);
            unameGO.layer = layer;
            LayoutElement unameLE = unameGO.AddComponent<LayoutElement>();
            unameLE.minHeight = 16f;
            unameGO.AddComponent<CanvasRenderer>();
            item.usernameText = unameGO.AddComponent<TextMeshProUGUI>();
            item.usernameText.fontSize  = 12f;
            item.usernameText.fontStyle = FontStyles.Bold;
            item.usernameText.color     = Color.white;
            item.usernameText.overflowMode = TextOverflowModes.Ellipsis;
            item.usernameText.textWrappingMode = TextWrappingModes.NoWrap;

            // LevelText
            GameObject lvlGO = new GameObject("LevelText");
            lvlGO.transform.SetParent(info.transform, false);
            lvlGO.layer = layer;
            LayoutElement lvlLE = lvlGO.AddComponent<LayoutElement>();
            lvlLE.minHeight = 12f;
            lvlGO.AddComponent<CanvasRenderer>();
            item.levelText = lvlGO.AddComponent<TextMeshProUGUI>();
            item.levelText.fontSize = 10f;
            item.levelText.color    = new Color(0.6f, 0.6f, 0.6f);
            item.levelText.textWrappingMode = TextWrappingModes.NoWrap;

            // ── ActionButton ───────────────────────────────────────────
            GameObject btnGO = new GameObject("ActionButton");
            btnGO.transform.SetParent(root.transform, false);
            btnGO.layer = layer;

            Image btnImg = btnGO.AddComponent<Image>();
            Color addColor = new Color(0.25f, 0.47f, 0.85f);
            btnImg.color = addColor;

            item.actionButton = btnGO.AddComponent<Button>();
            ColorBlock cb = item.actionButton.colors;
            cb.normalColor      = addColor;
            cb.highlightedColor = addColor * 1.3f;
            cb.pressedColor     = addColor * 0.7f;
            cb.disabledColor    = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            item.actionButton.colors       = cb;
            item.actionButton.targetGraphic = btnImg;

            LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
            btnLE.minWidth = 58f; btnLE.preferredWidth = 58f;

            GameObject lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            RectTransform lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.sizeDelta = Vector2.zero;
            lblGO.AddComponent<CanvasRenderer>();
            item.buttonLabel = lblGO.AddComponent<TextMeshProUGUI>();
            item.buttonLabel.fontSize  = 10f;
            item.buttonLabel.fontStyle = FontStyles.Bold;
            item.buttonLabel.color     = Color.white;
            item.buttonLabel.alignment = TextAlignmentOptions.Center;
            item.buttonLabel.textWrappingMode = TextWrappingModes.NoWrap;

            return item;
        }

        // ── Setup ──────────────────────────────────────────────────────

        public void Setup(User user, Action<string, AddFriendResultItemV2> onSendRequest)
        {
            _userId        = user.id;
            _onSendRequest = onSendRequest;
            _sent          = false;

            if (usernameText) usernameText.text = user.username;
            if (levelText)    levelText.text    = $"Niv. {user.level}";

            // Couleur du point de statut
            if (statusDot)
            {
                statusDot.color = user.status?.ToUpper() switch
                {
                    "ONLINE"  => new Color(0.2f, 0.9f, 0.3f),
                    "IN_GAME" => new Color(1f, 0.65f, 0.1f),
                    _         => new Color(0.45f, 0.45f, 0.45f)
                };
            }

            if (user.isFriend) SetAlreadyFriend();
            else               SetAddable();
        }

        // ── États du bouton ────────────────────────────────────────────

        private void SetAddable()
        {
            _sent = false;
            SetButtonState("+ Ami", new Color(0.25f, 0.47f, 0.85f), interactable: true);
            actionButton?.onClick.RemoveAllListeners();
            actionButton?.onClick.AddListener(OnButtonClicked);
        }

        private void SetAlreadyFriend()
        {
            SetButtonState("Ami", new Color(0.2f, 0.55f, 0.2f), interactable: false);
            actionButton?.onClick.RemoveAllListeners();
        }

        public void SetLoading(bool loading)
        {
            if (actionButton) actionButton.interactable = !loading;
            if (buttonLabel && loading) buttonLabel.text = "...";
        }

        public void SetSent()
        {
            _sent = true;
            SetButtonState("Envoye !", new Color(0.2f, 0.65f, 0.35f), interactable: false);
            actionButton?.onClick.RemoveAllListeners();

            // Petite animation de confirmation
            transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 5, 0.5f);
        }

        private void SetButtonState(string label, Color color, bool interactable)
        {
            if (buttonLabel) buttonLabel.text = label;
            if (actionButton)
            {
                actionButton.interactable = interactable;
                Image img = actionButton.GetComponent<Image>();
                if (img)
                {
                    img.color = interactable ? color : new Color(0.3f, 0.3f, 0.3f, 0.6f);
                    ColorBlock cb = actionButton.colors;
                    cb.normalColor      = color;
                    cb.highlightedColor = color * 1.3f;
                    cb.pressedColor     = color * 0.7f;
                    actionButton.colors = cb;
                }
            }
        }

        // ── Click ──────────────────────────────────────────────────────

        private void OnButtonClicked()
        {
            if (_sent) return;
            _onSendRequest?.Invoke(_userId, this);
        }

        private void OnDestroy()
        {
            actionButton?.onClick.RemoveAllListeners();
        }
    }
}

