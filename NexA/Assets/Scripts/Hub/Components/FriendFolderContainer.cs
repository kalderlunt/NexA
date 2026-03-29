using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NexA.Hub.Services;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Groupe/dossier personnalisé dans le SocialPanel.
    /// Créé dynamiquement via le bouton [+dossier].
    /// Supporte :
    ///   - Renommage double-clic sur le label
    ///   - Collapse/expand
    ///   - Réception de FriendItemPrefab par drag & drop
    ///   - Suppression si vide (bouton ×)
    /// </summary>
    public class FriendFolderContainer : MonoBehaviour,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Header")]
        [SerializeField] private CanvasGroup     headerCanvasGroup;
        [SerializeField] private Button          headerButton;
        [SerializeField] private TMP_InputField  groupNameInput;   // double-clic → éditable
        [SerializeField] private TextMeshProUGUI groupNameLabel;   // affiché normalement
        [SerializeField] private TextMeshProUGUI countLabel;       // "(3)"
        [SerializeField] private RectTransform   arrowIcon;
        [SerializeField] private Button          deleteButton;     // × visible si vide
        
        [Header("Contenu")]
        [SerializeField] public Transform friendContainer;        // parent des FriendItemPrefab

        [Header("Animation")]
        [SerializeField] [Range(0f, 1f)] private float animationDuration = 0.18f;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor   = new(0.08f, 0.08f, 0.12f, 0f);
        [SerializeField] private Color highlightColor = new(0.2f, 0.5f, 1f, 0.15f);

        // ── État ──────────────────────────────────────────────────────
        private bool isExpanded = true;
        private string groupName = "Nouveau groupe";

        // Compteurs — source de vérité via HashSet, évite childCount ET les int désynchronisés
        private readonly HashSet<FriendSidePanelRow> rows = new();

        // Angles cibles de la flèche (Z eulerAngles)
        // closedAngle = angle de départ défini dans le prefab
        // openAngle   = closedAngle + 90
        private float closedAngle;
        private float openAngle;

        /// <summary>
        /// Groupe par défaut = non supprimable, non renommable.
        /// Réceptionne automatiquement les nouveaux amis.
        /// </summary>
        public bool   IsDefault  { get; private set; }
        public string GroupName  => groupName;
        public string FolderId   { get; set; }   // ID backend du dossier — null pour les dossiers offline locaux
        public int    FriendCount => rows.Count;

        // ── Init ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (backgroundImage)
                backgroundImage.color = normalColor;

            if (groupNameInput)
            {
                groupNameInput.gameObject.SetActive(false);
                groupNameInput.onEndEdit.AddListener(OnNameEditEnd);
            }

            // Mémoriser l'angle initial de la flèche comme position "fermé"
            if (arrowIcon)
            {
                closedAngle = arrowIcon.localEulerAngles.z;
                openAngle   = closedAngle - 90f;
                arrowIcon.localEulerAngles = new Vector3(0f, 0f, openAngle);
            }
        }

        private void Start()
        {
            headerButton?.onClick.AddListener(ToggleExpand);
            deleteButton?.onClick.AddListener(TryDelete);

            // Double-clic sur le label → passer en mode édition
            if (groupNameLabel)
            {
                EventTrigger trigger = groupNameLabel.gameObject.GetComponent<EventTrigger>()
                                       ?? groupNameLabel.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((_) => OnLabelClicked());
                trigger.triggers.Add(entry);
            }

            // Clic droit sur le header → menu contextuel dossier
            if (headerButton)
            {
                EventTrigger headerTrigger = headerButton.gameObject.GetComponent<EventTrigger>()
                                             ?? headerButton.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry rightClick = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                rightClick.callback.AddListener((data) =>
                {
                    PointerEventData ped = (PointerEventData)data;
                    if (ped.button == PointerEventData.InputButton.Right)
                        FriendContextMenu.Instance?.ShowForFolder(this, ped.position);
                });
                headerTrigger.triggers.Add(rightClick);
            }

            UpdateUI();
        }

        /// <summary>Initialise le groupe avec un nom.</summary>
        /// <param name="groupTitle">Nom affiché.</param>
        /// <param name="asDefault">Si true : groupe non supprimable, non renommable.</param>
        /// <param name="folderId">ID backend du dossier (null pour dossiers locaux).</param>
        public void Init(string groupTitle = "Nouveau groupe", bool asDefault = false, string folderId = null)
        {
            groupName = groupTitle;
            FolderId  = folderId;
            gameObject.name = $"Folder - {groupName}";
            IsDefault = asDefault;

            rows.Clear();
            UpdateUI();

            CanvasGroup cg = ResolveHeaderCG();
            cg.alpha = asDefault ? 1f : 0f;
        }

        /// <summary>
        /// Anime l'apparition du header du dossier.
        /// Retourne le timestamp de fin du fade (delay + duration).
        /// </summary>
        public float AnimateHeaderIn(float delay = 0f)
        {
            CanvasGroup cg = ResolveHeaderCG();
            cg.alpha = 0f;
            DOTween.Kill(cg);
            cg.DOFade(1f, animationDuration).SetDelay(delay).SetEase(Ease.OutCubic);
            return delay + animationDuration;
        }

        /// <summary>Retourne le CanvasGroup à animer — cherche sur HeaderButton en priorité.</summary>
        private CanvasGroup ResolveHeaderCG()
        {
            // 1. Champ explicitement assigné dans l'Inspector
            if (headerCanvasGroup)
                return headerCanvasGroup;

            // 2. CanvasGroup sur le GO HeaderButton (c'est là qu'il est placé dans la scène)
            if (headerButton)
            {
                headerCanvasGroup = headerButton.GetComponent<CanvasGroup>();
                if (headerCanvasGroup)
                    return headerCanvasGroup;
                // Pas de CanvasGroup sur HeaderButton → on en crée un
                headerCanvasGroup = headerButton.gameObject.AddComponent<CanvasGroup>();
                return headerCanvasGroup;
            }

            // 3. Chercher dans les enfants (HeaderButton non assigné mais présent dans le prefab)
            headerCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
            if (headerCanvasGroup)
                return headerCanvasGroup;

            // 4. Dernier recours : ajouter sur le root (Unity-safe, pas de ??)
            headerCanvasGroup = GetComponent<CanvasGroup>();
            if (!headerCanvasGroup)
                headerCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            return headerCanvasGroup;
        }

        /// <summary>Renomme le dossier sans toucher à l'alpha ni à l'état.</summary>
        public void Rename(string newTitle)
        {
            groupName = newTitle;
            gameObject.name = $"Folder - {groupName}";
            UpdateUI();
        }

        /// <summary>Ajoute un FriendSidePanelRow à ce groupe et met à jour les compteurs.
        /// Les amis online sont toujours insérés avant les offline.</summary>
        public void AddRow(GameObject rowGo)
        {
            FriendSidePanelRow row = rowGo.GetComponent<FriendSidePanelRow>();

            if (row && row.IsOnline)
            {
                // Insérer après le dernier online
                int insertIndex = 0;
                for (int i = 0; i < friendContainer.childCount; i++)
                {
                    FriendSidePanelRow child = friendContainer.GetChild(i).GetComponent<FriendSidePanelRow>();
                    if (child != null && child.IsOnline)
                        insertIndex = i + 1;
                }
                rowGo.transform.SetParent(friendContainer, false);
                rowGo.transform.SetSiblingIndex(insertIndex);
            }
            else
            {
                rowGo.transform.SetParent(friendContainer, false);
            }

            if (row)
                rows.Add(row);

            RefreshDisplay();
        }

        /// <summary>Retire un FriendSidePanelRow de ce groupe et met à jour les compteurs.</summary>
        public void RemoveRow(GameObject rowGo)
        {
            FriendSidePanelRow row = rowGo.GetComponent<FriendSidePanelRow>();
            if (row)
                rows.Remove(row);
            RefreshDisplay();
        }

        /// <summary>
        /// Synchronise le HashSet depuis les enfants réels du friendContainer.
        /// Appelé par SocialPanel après avoir peuplé via CreateFriendRow (Instantiate direct).
        /// </summary>
        public void RefreshCount(int online, int total)
        {
            rows.Clear();
            // Reconstruire depuis les enfants réels UNIQUEMENT ici au rebuild initial
            foreach (Transform child in friendContainer)
            {
                FriendSidePanelRow r = child.GetComponent<FriendSidePanelRow>();
                if (r)
                    rows.Add(r);
            }
            RefreshDisplay();
        }

        /// <summary>Rafraîchit les labels depuis le HashSet.</summary>
        public void RefreshDisplay()
        {
            if (groupNameLabel)
                groupNameLabel.text = groupName.ToUpper();

            if (countLabel)
            {
                int online = 0;
                foreach (FriendSidePanelRow r in rows)
                    if (r && r.IsOnline) online++;
                countLabel.text = $"({online}/{rows.Count})";
            }

            if (deleteButton)
                deleteButton.gameObject.SetActive(!IsDefault);
        }

        // ── Drop ──────────────────────────────────────────────────────

        public void OnDrop(PointerEventData eventData)
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;

            FriendRowDragHandler dragged = FriendRowDragHandler.CurrentlyDragged;
            if (!dragged) 
                return;
            if (dragged.transform.parent == friendContainer)
                return;

            // Décrémenter le container source
            FriendFolderContainer source = dragged.transform.parent?.GetComponentInParent<FriendFolderContainer>();
            source?.RemoveRow(dragged.gameObject);

            // Ajouter dans ce container
            AddRow(dragged.gameObject);
            FriendRowDragHandler.CurrentlyDragged = null;

            // Appel API — déplacer l'ami dans ce dossier
            MoveFriendToThisFolderAsync(dragged.gameObject);
        }

        /// <summary>Appelle l'API pour déplacer l'ami dans ce dossier.</summary>
        private async void MoveFriendToThisFolderAsync(GameObject rowGo)
        {
            if (string.IsNullOrEmpty(FolderId))
            {
                Debug.LogWarning($"[FriendFolderContainer] Dossier '{groupName}' sans FolderId — appel API ignoré.");
                return;
            }

            FriendSidePanelRow row = rowGo.GetComponent<FriendSidePanelRow>();
            if (row == null || string.IsNullOrEmpty(row.FriendshipId))
            {
                Debug.LogWarning("[FriendFolderContainer] FriendshipId manquant — appel API ignoré.");
                return;
            }

            try
            {
                await APIService.Instance.AssignFriendToFolderAsync(FolderId, row.FriendshipId);
                Debug.Log($"[FriendFolderContainer] {row.name} déplacé dans '{groupName}' (API OK)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FriendFolderContainer] Erreur déplacement ami : {ex.Message}");
                ToastManager.Show("Erreur lors du déplacement", ToastType.Error);
            }
        }
        
        
        float currentFill;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!FriendRowDragHandler.CurrentlyDragged || !backgroundImage)
                return;
            
            currentFill = backgroundImage.fillAmount;
            backgroundImage.DOColor(highlightColor, 0.15f);
            backgroundImage.DOFillAmount(1f, 0.2f).SetEase(Ease.OutCubic);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!FriendRowDragHandler.CurrentlyDragged || !backgroundImage) 
                return;
            
            float targetFill = isExpanded ? 1f : currentFill;
            backgroundImage.DOColor(normalColor, 0.15f);
            backgroundImage.DOFillAmount(targetFill, 0.2f).SetEase(Ease.OutCubic);
        }

        // ── UI ────────────────────────────────────────────────────────

        private void ToggleExpand()
        {
            isExpanded = !isExpanded;

            if (arrowIcon)
            {
                float targetAngle = isExpanded ? openAngle : closedAngle;
                arrowIcon.DOLocalRotate(new Vector3(0f, 0f, targetAngle), 0.2f)
                         .SetEase(Ease.OutCubic);
            }

            if (backgroundImage)
            {
                float targetFill = isExpanded ? 1f : 0f;
                backgroundImage.DOFillAmount(targetFill, 0.2f).SetEase(Ease.OutCubic);
            }

            if (friendContainer)
                friendContainer.gameObject.SetActive(isExpanded);
        }

        private void UpdateUI() => RefreshDisplay();

        // ── API publique renommage / suppression (appelée par FriendContextMenu) ──────

        /// <summary>Déclenche le renommage depuis l'extérieur (ex: menu contextuel).</summary>
        public void BeginRenamePublic() => BeginRename();

        /// <summary>Déclenche la suppression depuis l'extérieur (ex: menu contextuel).</summary>
        public void TryDeletePublic() => TryDelete();

        // ── Renommage ─────────────────────────────────────────────────

        private float lastClickTime;

        private void OnLabelClicked()
        {
            float now = Time.unscaledTime;
            if (now - lastClickTime < 0.35f)
                BeginRename();
            lastClickTime = now;
        }

        private void BeginRename()
        {
            if (IsDefault || !groupNameInput)
                return;
            
            groupNameLabel?.gameObject.SetActive(false);
            groupNameInput.gameObject.SetActive(true);
            groupNameInput.text = groupName;
            groupNameInput.Select();
            groupNameInput.ActivateInputField();
        }

        private async void OnNameEditEnd(string newName)
        {
            string trimmed = string.IsNullOrWhiteSpace(newName) ? "Nouveau groupe" : newName.Trim();
            groupNameLabel?.gameObject.SetActive(true);
            groupNameInput?.gameObject.SetActive(false);

            if (trimmed == groupName)
                return; // pas de changement

            groupName = trimmed;
            gameObject.name = $"Folder - {groupName}";
            UpdateUI();

            // Appel API si on a un ID
            if (!string.IsNullOrEmpty(FolderId))
            {
                try
                {
                    await APIService.Instance.RenameFolderAsync(FolderId, groupName);
                    Debug.Log($"[FriendFolderContainer] Dossier renommé en '{groupName}' (API OK)");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[FriendFolderContainer] Erreur renommage : {ex.Message}");
                    ToastManager.Show("Erreur lors du renommage", ToastType.Error);
                }
            }
        }

        // ── Suppression ───────────────────────────────────────────────

        private async void TryDelete()
        {
            if (IsDefault)
            {
                ToastManager.Show("Le groupe GÉNÉRAL ne peut pas être supprimé", ToastType.Warning);
                return;
            }

            // Déplacer tous les amis vers le dossier par défaut avant suppression
            await MigrateFriendsToDefaultAsync();

            // Appel API suppression du dossier (le backend déplace les amis vers le dossier par défaut)
            if (!string.IsNullOrEmpty(FolderId))
            {
                try
                {
                    await APIService.Instance.DeleteFolderAsync(FolderId);
                    Debug.Log($"[FriendFolderContainer] Dossier '{groupName}' supprimé (API OK)");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[FriendFolderContainer] Erreur suppression dossier : {ex.Message}");
                    ToastManager.Show("Erreur lors de la suppression", ToastType.Error);
                    return;
                }
            }

            // Rafraîchir depuis le serveur pour avoir l'état à jour
            await SocialPanel.Instance.RefreshAsync();

            var cg = ResolveHeaderCG();
            cg.DOFade(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        }

        /// <summary>
        /// Déplace tous les rows de ce dossier vers le dossier GÉNÉRAL (visuellement + API si FolderId défini).
        /// </summary>
        private async System.Threading.Tasks.Task MigrateFriendsToDefaultAsync()
        {
            FriendFolderContainer defaultFolder = SocialPanel.Instance?.DefaultFolderPublic;
            if (!defaultFolder)
                return;

            // Copier la liste car on va modifier rows pendant l'itération
            HashSet<FriendSidePanelRow> snapshot = new(rows);

            foreach (FriendSidePanelRow row in snapshot)
            {
                if (!row) 
                    continue;

                // Appel API — on déplace vers le dossier default
                if (!string.IsNullOrEmpty(defaultFolder.FolderId) && !string.IsNullOrEmpty(row.FriendshipId))
                {
                    try
                    {
                        await APIService.Instance.AssignFriendToFolderAsync(defaultFolder.FolderId, row.FriendshipId);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[FriendFolderContainer] Impossible de migrer {row.name} : {ex.Message}");
                    }
                }

                // Déplacement visuel
                RemoveRow(row.gameObject);
                defaultFolder.AddRow(row.gameObject);
            }
        }

        private void OnDestroy()
        {
            headerButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
            if (groupNameInput != null)
                groupNameInput.onEndEdit.RemoveAllListeners();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}