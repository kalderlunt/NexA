using NexA.Hub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Item d'un ami dans SearchOverlayV2.
    /// Créé entièrement par code via CreateItem() — aucun prefab requis.
    ///
    /// Structure :
    ///  ┌────────────────────────────────────────────────┐
    ///  │ ● [Username]      Niv.X          [En ligne]   │
    ///  └────────────────────────────────────────────────┘
    /// </summary>
    public class FriendSearchResultItem : MonoBehaviour
    {
        // ── Refs ───────────────────────────────────────────────────────
        private Image            statusDot;
        private TextMeshProUGUI  usernameText;
        private TextMeshProUGUI  levelText;
        private TextMeshProUGUI  statusLabel;

        // ── Factory ───────────────────────────────────────────────────

        public static FriendSearchResultItem CreateItem(Transform parent)
        {
            int layer = parent.gameObject.layer;

            // ── Racine ─────────────────────────────────────────────────
            GameObject root = new GameObject("FriendSearchResultItem");
            root.transform.SetParent(parent, false);
            root.layer = layer;

            root.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 38);
            root.AddComponent<CanvasGroup>(); // fade-in

            // Fond au survol
            Image rowBg = root.AddComponent<Image>();
            rowBg.color = new Color(1f, 1f, 1f, 0.03f);

            HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.spacing = 8f;
            hlg.childAlignment      = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth  = true;
            hlg.childControlHeight = true;

            ContentSizeFitter csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            FriendSearchResultItem item = root.AddComponent<FriendSearchResultItem>();

            // ── Point de statut ────────────────────────────────────────
            GameObject dotGO = new GameObject("StatusDot");
            dotGO.transform.SetParent(root.transform, false);
            dotGO.layer = layer;
            LayoutElement dotLE = dotGO.AddComponent<LayoutElement>();
            dotLE.minWidth = 8; dotLE.preferredWidth = 8;
            dotLE.minHeight = 8; dotLE.preferredHeight = 8;
            item.statusDot = dotGO.AddComponent<Image>();
            item.statusDot.color = Color.gray;

            // ── Info container ─────────────────────────────────────────
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

            // Username
            GameObject unameGO = new GameObject("UsernameText");
            unameGO.transform.SetParent(info.transform, false);
            unameGO.layer = layer;
            unameGO.AddComponent<LayoutElement>().minHeight = 16f;
            unameGO.AddComponent<CanvasRenderer>();
            item.usernameText = unameGO.AddComponent<TextMeshProUGUI>();
            item.usernameText.fontSize  = 12f;
            item.usernameText.fontStyle = FontStyles.Bold;
            item.usernameText.color     = Color.white;
            item.usernameText.textWrappingMode = TextWrappingModes.NoWrap;
            item.usernameText.overflowMode     = TextOverflowModes.Ellipsis;

            // Level
            GameObject lvlGO = new GameObject("LevelText");
            lvlGO.transform.SetParent(info.transform, false);
            lvlGO.layer = layer;
            lvlGO.AddComponent<LayoutElement>().minHeight = 12f;
            lvlGO.AddComponent<CanvasRenderer>();
            item.levelText = lvlGO.AddComponent<TextMeshProUGUI>();
            item.levelText.fontSize = 10f;
            item.levelText.color    = new Color(0.6f, 0.6f, 0.6f);
            item.levelText.textWrappingMode = TextWrappingModes.NoWrap;

            // ── Badge statut (droite) ──────────────────────────────────
            GameObject badgeGO = new GameObject("StatusBadge");
            badgeGO.transform.SetParent(root.transform, false);
            badgeGO.layer = layer;
            Image badgeBg = badgeGO.AddComponent<Image>();
            badgeBg.color = new Color(0.2f, 0.9f, 0.3f, 0.15f);
            LayoutElement badgeLE = badgeGO.AddComponent<LayoutElement>();
            badgeLE.minWidth = 52f; badgeLE.preferredWidth = 52f;

            GameObject statusLblGO = new GameObject("StatusLabel");
            statusLblGO.transform.SetParent(badgeGO.transform, false);
            RectTransform statusLblRT = statusLblGO.AddComponent<RectTransform>();
            statusLblRT.anchorMin = Vector2.zero; statusLblRT.anchorMax = Vector2.one;
            statusLblRT.sizeDelta = Vector2.zero;
            statusLblGO.AddComponent<CanvasRenderer>();
            item.statusLabel = statusLblGO.AddComponent<TextMeshProUGUI>();
            item.statusLabel.fontSize  = 9f;
            item.statusLabel.fontStyle = FontStyles.Bold;
            item.statusLabel.color     = new Color(0.2f, 0.9f, 0.3f);
            item.statusLabel.alignment = TextAlignmentOptions.Center;
            item.statusLabel.textWrappingMode = TextWrappingModes.NoWrap;

            // Garder une ref au fond du badge pour la couleur dynamique
            item._badgeBg = badgeBg;

            return item;
        }

        // Ref interne au fond du badge (pour colorisation)
        private Image _badgeBg;

        // ── Setup ──────────────────────────────────────────────────────

        public void Setup(Friend friend)
        {
            if (usernameText) usernameText.text = friend.username ?? "?";
            if (levelText)    levelText.text    = $"Niv. {friend.level}";

            ApplyStatus(friend.StatusNormalized);
        }

        // ── Statut ────────────────────────────────────────────────────

        private void ApplyStatus(string status)
        {
            Color dotColor, labelColor, bgColor;
            string labelText;

            switch (status)
            {
                case "online":
                    dotColor   = new Color(0.2f, 0.9f, 0.3f);
                    labelColor = new Color(0.2f, 0.9f, 0.3f);
                    bgColor    = new Color(0.2f, 0.9f, 0.3f, 0.12f);
                    labelText  = "En ligne";
                    break;
                case "in-game":
                    dotColor   = new Color(1f, 0.65f, 0.1f);
                    labelColor = new Color(1f, 0.65f, 0.1f);
                    bgColor    = new Color(1f, 0.65f, 0.1f, 0.12f);
                    labelText  = "En jeu";
                    break;
                default:
                    dotColor   = new Color(0.45f, 0.45f, 0.45f);
                    labelColor = new Color(0.45f, 0.45f, 0.45f);
                    bgColor    = new Color(0.45f, 0.45f, 0.45f, 0.1f);
                    labelText  = "Hors ligne";
                    break;
            }

            if (statusDot)   statusDot.color   = dotColor;
            if (statusLabel)
            {
                statusLabel.text  = labelText;
                statusLabel.color = labelColor;
            }
            if (_badgeBg) _badgeBg.color = bgColor;
        }
    }
}

