using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
using NexA.Hub.Services;
using NexA.Hub.Models;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Menu contextuel clic-droit pour les amis ET les dossiers dans le SocialPanel.
    ///
    /// Usage ami      : FriendContextMenu.Instance.ShowForFriend(row, screenPos)
    /// Usage dossier  : FriendContextMenu.Instance.ShowForFolder(folder, screenPos)
    /// </summary>
    public class FriendContextMenu : MonoBehaviour
    {
        public static FriendContextMenu Instance { get; private set; }

        [SerializeField] private RectTransform panel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject menuItemPrefab;

        // ── Sous-menu Déplacer ─────────────────────────────────────────
        [Header("Sous-menu Déplacer (optionnel)")]
        [SerializeField] private RectTransform subPanel;         // panneau du sous-menu
        [SerializeField] private Transform     subItemsContainer;// container des entrées du sous-menu

        private CanvasGroup cg;
        private CanvasGroup subCg;
        private Canvas rootCanvas;

        // Cible courante
        private FriendRowDragHandler targetRow;
        private FriendFolderContainer targetFolder;
        private bool subMenuOpen = false;

        // ── Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
            gameObject.SetActive(false);

            if (subPanel)
            {
                subCg = subPanel.GetComponent<CanvasGroup>() ?? subPanel.gameObject.AddComponent<CanvasGroup>();
                subPanel.gameObject.SetActive(false);
            }
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Affiche le menu contextuel d'un ami.</summary>
        public void ShowForFriend(FriendRowDragHandler row, Vector2 screenPos)
        {
            targetRow    = row;
            targetFolder = null;
            BuildFriendMenu();
            Show(screenPos);
        }

        /// <summary>Affiche le menu contextuel d'un dossier.</summary>
        public void ShowForFolder(FriendFolderContainer folder, Vector2 screenPos)
        {
            targetFolder = folder;
            targetRow    = null;
            BuildFolderMenu();
            Show(screenPos);
        }

        /// <summary>Rétro-compat : Show(row, pos) → ShowForFriend.</summary>
        public void Show(FriendRowDragHandler row, Vector2 screenPos) => ShowForFriend(row, screenPos);

        public void Hide()
        {
            CloseSubMenu();
            cg.DOFade(0f, 0.1f).OnComplete(() => gameObject.SetActive(false));
        }

        // ── Construction menus ─────────────────────────────────────────

        private void BuildFriendMenu()
        {
            ClearItems(itemsContainer);

            // — Actions grisées —
            AddEntry("Inviter dans la partie", null, isEnabled: false);
            AddEntry("Envoyer un message",     null, isEnabled: false);

            AddSeparator(itemsContainer);

            // — Déplacer → (sous-menu) —
            HashSet<FriendFolderContainer> groups = SocialPanel.Instance?.GetAllGroups()
                                                    ?? new HashSet<FriendFolderContainer>();
            AddMoveEntry(groups);

            AddSeparator(itemsContainer);

            // — Retirer l'ami —
            AddEntry("Retirer l'ami", OnRemoveFriendClicked, isEnabled: true, isDestructive: true);
        }

        private void BuildFolderMenu()
        {
            ClearItems(itemsContainer);

            if (targetFolder == null) return;

            if (!targetFolder.IsDefault)
            {
                AddEntry("Renommer",  OnRenameFolderClicked);
                AddSeparator(itemsContainer);
                AddEntry("Supprimer", OnDeleteFolderClicked, isDestructive: true);
            }
            else
            {
                AddEntry("Dossier par défaut — non modifiable", null, isEnabled: false);
            }
        }

        // ── Entrée "Déplacer >" avec sous-menu ────────────────────────

        private void AddMoveEntry(HashSet<FriendFolderContainer> groups)
        {
            // Créer le bouton principal "Déplacer ▶"
            GameObject go = CreateEntryGO("Déplacer", isEnabled: groups.Count > 0);

            // Ajouter la flèche ▶ au label
            TextMeshProUGUI lbl = go.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) lbl.text = "Déplacer  ▶";

            Button btn = go.GetComponent<Button>();
            if (btn)
            {
                btn.interactable = groups.Count > 0;
                btn.onClick.AddListener(() => ToggleSubMenu(go, groups));
                ColorBlock c = btn.colors;
                c.normalColor      = Color.clear;
                c.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
                c.pressedColor     = new Color(1f, 1f, 1f, 0.15f);
                btn.colors = c;
            }
        }

        private void ToggleSubMenu(GameObject anchorGO, HashSet<FriendFolderContainer> groups)
        {
            if (!subPanel) { BuildInlineSubMenu(groups); return; }

            if (subMenuOpen) { CloseSubMenu(); return; }

            // Positionner le sous-menu à côté de l'entrée "Déplacer"
            RectTransform anchorRT = anchorGO.GetComponent<RectTransform>();
            subPanel.anchoredPosition = anchorRT.anchoredPosition
                                        + new Vector2(-subPanel.rect.width, 0f);

            BuildSubMenuEntries(groups);
            subPanel.gameObject.SetActive(true);

            if (subCg)
            {
                subCg.alpha = 0f;
                subCg.DOFade(1f, 0.1f).SetEase(Ease.OutCubic);
            }
            subMenuOpen = true;
        }

        /// <summary>Fallback : si pas de subPanel, on injecte les entrées directement sous "Déplacer".</summary>
        private void BuildInlineSubMenu(HashSet<FriendFolderContainer> groups)
        {
            // Supprimer d'éventuelles entrées précédentes inline (marquées "sub_")
            List<GameObject> toRemove = new();
            foreach (Transform child in itemsContainer)
                if (child.name.StartsWith("sub_")) toRemove.Add(child.gameObject);
            foreach (GameObject go in toRemove) Destroy(go);

            // Injecter les entrées de dossier
            foreach (FriendFolderContainer g in groups)
            {
                bool isCurrent = targetRow != null && targetRow.transform.parent == g.friendContainer;
                FriendFolderContainer cap = g;
                string label = (isCurrent ? "✓ " : "    ") + g.GroupName;
                GameObject entry = CreateEntryGO(label, isEnabled: !isCurrent, namePrefix: "sub_");
                Button btn = entry.GetComponent<Button>();
                if (btn)
                {
                    btn.interactable = !isCurrent;
                    btn.onClick.AddListener(() => { MoveToGroup(cap); Hide(); });
                    ApplyButtonColors(btn);
                }
            }
        }

        private void BuildSubMenuEntries(HashSet<FriendFolderContainer> groups)
        {
            ClearItems(subItemsContainer);

            foreach (FriendFolderContainer g in groups)
            {
                bool isCurrent = targetRow != null && targetRow.transform.parent == g.friendContainer;
                FriendFolderContainer cap = g;
                string label = (isCurrent ? "✓ " : "") + g.GroupName;

                GameObject go = CreateEntryGO(label, isEnabled: !isCurrent, parent: subItemsContainer);
                Button btn = go.GetComponent<Button>();
                if (btn)
                {
                    btn.interactable = !isCurrent;
                    btn.onClick.AddListener(() => { MoveToGroup(cap); Hide(); });
                    ApplyButtonColors(btn);
                }
            }
        }

        private void CloseSubMenu()
        {
            if (!subPanel || !subMenuOpen) return;
            subMenuOpen = false;
            if (subCg)
                subCg.DOFade(0f, 0.08f).OnComplete(() => subPanel.gameObject.SetActive(false));
            else
                subPanel.gameObject.SetActive(false);
        }

        // ── Helpers ────────────────────────────────────────────────────

        private void Show(Vector2 screenPos)
        {
            CloseSubMenu();

            // Déplacer au root Canvas pour éviter le clipping du SocialPanel (RectMask2D)
            Canvas root = FindRootCanvas();
            if (root != null && transform.parent != root.transform)
            {
                transform.SetParent(root.transform, false);
                rootCanvas = root;
            }

            // Toujours au premier plan
            transform.SetAsLastSibling();

            PositionNearCursor(screenPos);
            gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1f, 0.12f).SetEase(Ease.OutCubic);
        }

        private Canvas FindRootCanvas()
        {
            if (rootCanvas != null && rootCanvas.isRootCanvas) return rootCanvas;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
                if (c.isRootCanvas) return c;
            return rootCanvas;
        }

        private void ClearItems(Transform container)
        {
            if (!container) return;
            foreach (Transform child in container)
                Destroy(child.gameObject);
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

        private void AddSeparator(Transform container)
        {
            GameObject go = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container, false);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 1; le.preferredHeight = 1;
        }

        /// <summary>Crée une entrée de menu et l'ajoute à itemsContainer (ou au parent spécifié).</summary>
        private GameObject CreateEntryGO(string label, bool isEnabled = true,
                                         string namePrefix = "", Transform parent = null)
        {
            Transform target = parent ?? itemsContainer;
            GameObject go;

            if (menuItemPrefab)
            {
                go = Instantiate(menuItemPrefab, target);
                go.name = namePrefix + label;
                TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp)
                {
                    tmp.text  = label;
                    tmp.color = isEnabled ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
                }
            }
            else
            {
                go = new GameObject(namePrefix + label, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(target, false);
                go.GetComponent<Image>().color = Color.clear;
                go.AddComponent<LayoutElement>().minHeight = 28;

                GameObject tgo = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
                tgo.transform.SetParent(go.transform, false);
                RectTransform rt = tgo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(12f, 0f); rt.offsetMax = new Vector2(-4f, 0f);
                TextMeshProUGUI tmp = tgo.GetComponent<TextMeshProUGUI>();
                tmp.text      = label;
                tmp.fontSize  = 12;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.color     = isEnabled ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
            }

            return go;
        }

        private void AddEntry(string label, System.Action onClick,
                              bool isEnabled = true, bool isDestructive = false,
                              Transform parent = null)
        {
            GameObject go = CreateEntryGO(label, isEnabled, parent: parent ?? itemsContainer);

            // Couleur destructive (rouge doux)
            if (isDestructive)
            {
                TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp && isEnabled) tmp.color = new Color(0.95f, 0.35f, 0.35f, 1f);
            }

            Button btn = go.GetComponent<Button>();
            if (!btn) return;

            btn.interactable = isEnabled;
            if (onClick != null)
                btn.onClick.AddListener(() => { onClick(); Hide(); });

            ApplyButtonColors(btn);
        }

        private static void ApplyButtonColors(Button btn)
        {
            ColorBlock c = btn.colors;
            c.normalColor      = Color.clear;
            c.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            c.pressedColor     = new Color(1f, 1f, 1f, 0.15f);
            btn.colors = c;
        }

        // ── Actions amis ───────────────────────────────────────────────

        private async void MoveToGroup(FriendFolderContainer folder)
        {
            if (!targetRow || !folder) return;

            if (!string.IsNullOrEmpty(folder.FolderId) && !string.IsNullOrEmpty(targetRow.FriendshipId))
            {
                try
                {
                    await APIService.Instance.AssignFriendToFolderAsync(folder.FolderId, targetRow.FriendshipId);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[FriendContextMenu] Erreur déplacement ami : {ex.Message}");
                    ToastManager.Show("Erreur lors du déplacement", ToastType.Error);
                    return;
                }
            }

            FriendFolderContainer source = targetRow.transform.parent?.GetComponentInParent<FriendFolderContainer>();
            source?.RemoveRow(targetRow.gameObject);
            folder.AddRow(targetRow.gameObject);
            SocialPanel.Instance?.RefreshGroupHeaders();
        }

        private async void OnRemoveFriendClicked()
        {
            if (!targetRow) return;

            string friendshipId = targetRow.FriendshipId;
            if (string.IsNullOrEmpty(friendshipId))
            {
                Debug.LogError("[FriendContextMenu] FriendshipId manquant — impossible de supprimer l'ami.");
                ToastManager.Show("Erreur : ID d'amitié introuvable", ToastType.Error);
                return;
            }

            try
            {
                // Signaler au SocialPanel que c'est nous qui supprimons (évite le rebond STOMP)
                SocialPanel.Instance?.RegisterSelfRemoval(friendshipId);

                await APIService.Instance.RemoveFriendAsync(friendshipId);
                ToastManager.Show("Ami retiré", ToastType.Info);

                // Retirer visuellement
                FriendFolderContainer source = targetRow.transform.parent?.GetComponentInParent<FriendFolderContainer>();
                source?.RemoveRow(targetRow.gameObject);
                Destroy(targetRow.gameObject);
                SocialPanel.Instance?.RefreshGroupHeaders();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FriendContextMenu] Erreur suppression ami : {ex.Message}");
                ToastManager.Show("Erreur lors de la suppression", ToastType.Error);
            }
        }

        // ── Actions dossiers ───────────────────────────────────────────

        private void OnRenameFolderClicked()
        {
            if (!targetFolder || targetFolder.IsDefault) return;
            targetFolder.BeginRenamePublic();
        }

        private void OnDeleteFolderClicked()
        {
            if (!targetFolder || targetFolder.IsDefault) return;
            targetFolder.TryDeletePublic();
        }

        // ── Update (fermer si clic ailleurs) ──────────────────────────

        private InputAction _clickAction;
        private InputAction _rightClickAction;

        private void OnEnable()
        {
            // Créer les actions à la volée si pas encore initialisées
            if (_clickAction == null)
            {
                _clickAction = new InputAction("LeftClick", InputActionType.Button,
                    binding: "<Mouse>/leftButton");
                _clickAction.performed += OnAnyClick;
            }
            if (_rightClickAction == null)
            {
                _rightClickAction = new InputAction("RightClick", InputActionType.Button,
                    binding: "<Mouse>/rightButton");
                _rightClickAction.performed += OnAnyClick;
            }

            _clickAction.Enable();
            _rightClickAction.Enable();
        }

        private void OnDisable()
        {
            _clickAction?.Disable();
            _rightClickAction?.Disable();
        }

        private void OnDestroy()
        {
            _clickAction?.Dispose();
            _rightClickAction?.Dispose();
        }

        private void OnAnyClick(InputAction.CallbackContext ctx)
        {
            if (!gameObject.activeSelf || !panel) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            bool inMain = RectTransformUtility.RectangleContainsScreenPoint(
                panel, mousePos,
                rootCanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas?.worldCamera);

            bool inSub = subPanel && subPanel.gameObject.activeSelf &&
                         RectTransformUtility.RectangleContainsScreenPoint(
                             subPanel, mousePos,
                             rootCanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas?.worldCamera);

            if (!inMain && !inSub)
                Hide();
        }

        private void PositionNearCursor(Vector2 screenPos)
        {
            if (!panel) return;

            if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                // Mode Camera : conversion via RectTransformUtility
                Camera cam = rootCanvas.worldCamera ?? Camera.main;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rootCanvas.GetComponent<RectTransform>(), screenPos, cam, out Vector2 local);
                panel.anchoredPosition = local;
            }
            else
            {
                // ScreenSpaceOverlay : coords écran == coords monde du canvas
                // Forcer un rebuild pour obtenir la vraie hauteur du menu
                Canvas.ForceUpdateCanvases();

                float menuW = panel.rect.width;
                float menuH = panel.rect.height;

                // Ajuster pour ne pas sortir de l'écran
                float x = screenPos.x;
                float y = screenPos.y;

                if (x + menuW > Screen.width)  x = screenPos.x - menuW;
                if (y - menuH < 0)             y = screenPos.y + menuH;

                panel.position = new Vector3(x, y, 0f);
            }
        }
    }
}

