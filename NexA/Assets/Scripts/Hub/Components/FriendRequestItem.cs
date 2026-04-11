using System;
using DG.Tweening;
using NexA.Hub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Item représentant une demande d'ami reçue dans FriendRequestsOverlay.
    /// Créé entièrement par code via CreateItem() — pas de prefab nécessaire.
    /// </summary>
    public class FriendRequestItem : MonoBehaviour
    {
        // ── Refs UI ────────────────────────────────────────────────────
        private TextMeshProUGUI usernameText;
        private TextMeshProUGUI levelText;
        private Button acceptButton;
        private Button declineButton;
        private GameObject loadingSpinner;

        // ── État ──────────────────────────────────────────────────────
        private string _friendshipId;
        private Action<string, FriendRequestItem, bool> _onAction;

        // ── Factory ───────────────────────────────────────────────────

        /// <summary>
        /// Crée un FriendRequestItem complet par code et l'attache au parent donné.
        /// Aucun prefab requis.
        /// </summary>
        public static FriendRequestItem CreateItem(Transform parent)
        {
            // Racine
            GameObject root = new GameObject("FriendRequestItem");
            root.transform.SetParent(parent, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(0, 44);

            CanvasGroup cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Layout horizontal
            HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 6, 6);
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            ContentSizeFitter csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            FriendRequestItem item = root.AddComponent<FriendRequestItem>();

            // ── InfoContainer (flexible) ───────────────────────────────
            GameObject info = new GameObject("Info");
            info.transform.SetParent(root.transform, false);
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;

            VerticalLayoutGroup vlg = info.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.spacing = 2f;

            // Username
            GameObject unameGO = new GameObject("UsernameText");
            unameGO.transform.SetParent(info.transform, false);
            item.usernameText = unameGO.AddComponent<TextMeshProUGUI>();
            item.usernameText.text = "Utilisateur";
            item.usernameText.fontSize = 12f;
            item.usernameText.fontStyle = FontStyles.Bold;
            item.usernameText.color = Color.white;
            item.usernameText.overflowMode = TextOverflowModes.Ellipsis;
            item.usernameText.enableWordWrapping = false;
            LayoutElement unameLE = unameGO.AddComponent<LayoutElement>();
            unameLE.minHeight = 16f;

            // Level
            GameObject lvlGO = new GameObject("LevelText");
            lvlGO.transform.SetParent(info.transform, false);
            item.levelText = lvlGO.AddComponent<TextMeshProUGUI>();
            item.levelText.text = "Niv. 1";
            item.levelText.fontSize = 10f;
            item.levelText.color = new Color(0.6f, 0.6f, 0.6f);
            item.levelText.enableWordWrapping = false;
            LayoutElement lvlLE = lvlGO.AddComponent<LayoutElement>();
            lvlLE.minHeight = 13f;

            // ── Bouton Accepter ────────────────────────────────────────
            item.acceptButton = CreateButton(root.transform, "Accepter", new Color(0.18f, 0.8f, 0.44f), 50f);

            // ── Bouton Refuser ─────────────────────────────────────────
            item.declineButton = CreateButton(root.transform, "Refuser", new Color(0.8f, 0.24f, 0.24f), 50f);

            return item;
        }

        private static Button CreateButton(Transform parent, string label, Color color, float width)
        {
            GameObject go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.color = color;

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = color;
            cb.highlightedColor = color * 1.2f;
            cb.pressedColor     = color * 0.8f;
            cb.disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = cb;
            btn.targetGraphic = img;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth      = width;
            le.preferredWidth = width;

            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 10f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;

            return btn;
        }

        // ── Setup ──────────────────────────────────────────────────────

        /// <summary>
        /// Configure l'item avec la demande reçue.
        /// onAction(friendshipId, item, accepted) : accepted=true → accepter, false → refuser.
        /// </summary>
        public void Setup(PendingFriendRequest request, Action<string, FriendRequestItem, bool> onAction)
        {
            _friendshipId = request.id;
            _onAction     = onAction;

            string uname = request.user?.username ?? "Inconnu";
            int    level = request.user?.level ?? 0;

            if (usernameText) usernameText.text = uname;
            if (levelText)    levelText.text    = $"Niv. {level}";

            SetInteractable(true);

            acceptButton?.onClick.RemoveAllListeners();
            declineButton?.onClick.RemoveAllListeners();
            acceptButton?.onClick.AddListener(OnAcceptClicked);
            declineButton?.onClick.AddListener(OnDeclineClicked);

            if (loadingSpinner) loadingSpinner.SetActive(false);

            // Fade-in
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg) cg.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
        }

        // ── Clics ─────────────────────────────────────────────────────

        private void OnAcceptClicked()  => _onAction?.Invoke(_friendshipId, this, true);
        private void OnDeclineClicked() => _onAction?.Invoke(_friendshipId, this, false);

        // ── Helpers ────────────────────────────────────────────────────

        public void SetLoading(bool loading)
        {
            SetInteractable(!loading);
            if (loadingSpinner) loadingSpinner.SetActive(loading);
        }

        private void SetInteractable(bool value)
        {
            if (acceptButton)  acceptButton.interactable  = value;
            if (declineButton) declineButton.interactable = value;
        }

        public void AnimateOut(Action onComplete = null)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            cg.DOFade(0f, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
        }

        private void OnDestroy()
        {
            acceptButton?.onClick.RemoveAllListeners();
            declineButton?.onClick.RemoveAllListeners();
        }
    }
}
