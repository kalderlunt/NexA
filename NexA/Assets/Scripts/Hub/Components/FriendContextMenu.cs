using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace NexA.Hub.Components
{
    public class FriendContextMenu : MonoBehaviour
    {
        public static FriendContextMenu Instance { get; private set; }

        [SerializeField] private RectTransform panel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject menuItemPrefab;

        private CanvasGroup cg;
        private Canvas rootCanvas;
        private FriendRowDragHandler targetRow;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject); return;
            }
            
            Instance = this;
            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
            gameObject.SetActive(false);
        }

        public void Show(FriendRowDragHandler row, Vector2 screenPos)
        {
            targetRow = row;
            BuildMenu();
            PositionNearCursor(screenPos);
            gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1f, 0.12f).SetEase(Ease.OutCubic);
        }

        public void Hide() =>
            cg.DOFade(0f, 0.1f).OnComplete(() => gameObject.SetActive(false));

        private void BuildMenu()
        {
            foreach (Transform child in itemsContainer)
                Destroy(child.gameObject);

            var groups = SocialPanel.Instance?.GetAllGroups() ?? new List<FriendGroupContainer>();
            AddSectionLabel("Deplacer vers");

            if (groups.Count == 0)
            {
                AddEntry("Aucun groupe", null, isEnabled: false);
            }
            else
            {
                foreach (FriendGroupContainer g in groups)
                {
                    bool cur = targetRow != null && targetRow.transform.parent == g.contentContainer;
                    FriendGroupContainer cap = g;
                    AddEntry("  " + g.GroupName, () => MoveToGroup(cap), !cur);
                }
            }

            if (targetRow && targetRow.GetComponentInParent<FriendGroupContainer>())
            {
                AddSeparator();
                AddEntry("Retirer du groupe", RemoveFromGroup);
            }
        }

        private void AddSectionLabel(string text)
        {
            GameObject go = new GameObject("Lbl", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(itemsContainer, false);
            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = 10; t.fontStyle = FontStyles.Bold;
            t.color = new Color(0.55f, 0.55f, 0.65f, 1f);
            go.AddComponent<LayoutElement>().minHeight = 22;
        }

        private void AddSeparator()
        {
            GameObject go = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(itemsContainer, false);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 1; le.preferredHeight = 1;
        }

        private void AddEntry(string label, System.Action onClick, bool isEnabled = true)
        {
            GameObject go;
            if (menuItemPrefab)
            {
                go = Instantiate(menuItemPrefab, itemsContainer);
                TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp)
                {
                    tmp.text = label;
                    tmp.color = isEnabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
            else
            {
                go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(itemsContainer, false);
                go.GetComponent<Image>().color = Color.clear;
                go.AddComponent<LayoutElement>().minHeight = 28;

                GameObject tgo = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
                tgo.transform.SetParent(go.transform, false);
                
                RectTransform rt = tgo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(12f, 0f); rt.offsetMax = new Vector2(-4f, 0f);
                
                TextMeshProUGUI tmp = tgo.GetComponent<TextMeshProUGUI>();
                tmp.text = label; tmp.fontSize = 12;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.color = isEnabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            Button btn = go.GetComponent<Button>();
            if (!btn)
                return;

            btn.interactable = isEnabled;
            if (onClick != null)
                btn.onClick.AddListener(() => { onClick(); Hide(); });
            
            ColorBlock c = btn.colors;
            c.normalColor = Color.clear;
            c.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            c.pressedColor = new Color(1f, 1f, 1f, 0.15f);
            btn.colors = c;
        }

        private void MoveToGroup(FriendGroupContainer group)
        {
            if (!targetRow || !group)
                return;
            
            group.AddRow(targetRow.gameObject);
        }

        private void RemoveFromGroup()
        {
            if (!targetRow) 
                return;
            
            Transform container = SocialPanel.Instance?.FriendsContainer;
            if (container)
                targetRow.transform.SetParent(container, false);
            SocialPanel.Instance?.RefreshGroupHeaders();
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
                return;
            /////////////////////////////////////////////////
            // TODO : NE PAS UTILISER L'ANCIEN INPUT SYSTEM 
            /////////////////////////////////////////////////
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) 
                return;
            
            if (!panel)
                return;
            
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    panel, Input.mousePosition,
                    rootCanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas?.worldCamera))
                Hide();
        }

        private void PositionNearCursor(Vector2 screenPos)
        {
            if (!panel || !rootCanvas)
                return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.GetComponent<RectTransform>(), screenPos,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out Vector2 local);
            panel.anchoredPosition = local;
        }
    }
}



