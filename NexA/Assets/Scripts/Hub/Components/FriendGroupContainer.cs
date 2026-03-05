using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public class FriendGroupContainer : MonoBehaviour,
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
        [SerializeField] public Transform contentContainer;        // parent des FriendItemPrefab

        [Header("Animation")]
        [SerializeField] [Range(0f, 1f)] private float animationDuration = 0.18f;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor   = new(0.08f, 0.08f, 0.12f, 0f);
        [SerializeField] private Color highlightColor = new(0.2f, 0.5f, 1f, 0.15f);

        // ── État ──────────────────────────────────────────────────────
        private bool isExpanded = true;
        private string groupName = "Nouveau groupe";

        // Angles cibles de la flèche (Z eulerAngles)
        // closedAngle = angle de départ défini dans le prefab
        // openAngle   = closedAngle + 90
        private float closedAngle;
        private float openAngle;

        /// <summary>
        /// Groupe par défaut = non supprimable, non renommable.
        /// Réceptionne automatiquement les nouveaux amis.
        /// </summary>
        public bool IsDefault { get; private set; }

        // ── Propriétés publiques ──────────────────────────────────────
        public string GroupName   => groupName;
        public int    FriendCount => contentContainer != null ? contentContainer.childCount : 0;

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
            if (groupNameLabel != null)
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

            UpdateUI();
        }

        /// <summary>Initialise le groupe avec un nom.</summary>
        /// <param name="groupTitle">Nom affiché.</param>
        /// <param name="asDefault">Si true : groupe non supprimable, non renommable.</param>
        public void Init(string groupTitle = "Nouveau groupe", bool asDefault = false)
        {
            groupName = groupTitle;
            IsDefault = asDefault;
            UpdateUI();

            // Résoudre le CanvasGroup cible (sur le HeaderButton de préférence, sinon root)
            CanvasGroup cg = ResolveHeaderCG();
            cg.alpha = asDefault ? 1f : 0f;   // sera animé via AnimateHeaderIn si custom
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

        /// <summary>Ajoute un FriendSidePanelRow existant à ce groupe.</summary>
        public void AddRow(GameObject rowGo)
        {
            rowGo.transform.SetParent(contentContainer, false);
            RefreshDisplay();
            SocialPanel.Instance?.RefreshGroupHeaders();
        }

        /// <summary>Rafraîchit les labels (compteur + bouton ×) depuis le childCount actuel.</summary>
        public void RefreshDisplay()
        {
            if (groupNameLabel)
                groupNameLabel.text = groupName.ToUpper();
            
            if (countLabel) 
                countLabel.text = $"({FriendCount})";
            
            if (deleteButton)
                deleteButton.gameObject.SetActive(!IsDefault && FriendCount == 0);
        }

        /// <summary>Rafraîchit le compteur avec des valeurs explicites (ex: GÉNÉRAL "8/51").</summary>
        public void RefreshCount(int online, int total)
        {
            if (groupNameLabel)
                groupNameLabel.text = groupName.ToUpper();
            
            if (countLabel)
                countLabel.text = $"({online}/{total})";
            
            if (deleteButton)
                deleteButton.gameObject.SetActive(!IsDefault && FriendCount == 0);
        }

        // ── Drop ──────────────────────────────────────────────────────

        public void OnDrop(PointerEventData eventData)
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;

            var dragged = FriendRowDragHandler.CurrentlyDragged;
            if (dragged == null) return;

            // Ne pas re-ajouter dans le même container
            if (dragged.transform.parent == contentContainer) return;

            AddRow(dragged.gameObject);
            FriendRowDragHandler.CurrentlyDragged = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (FriendRowDragHandler.CurrentlyDragged != null && backgroundImage != null)
                backgroundImage.DOColor(highlightColor, 0.15f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (backgroundImage != null)
                backgroundImage.DOColor(normalColor, 0.15f);
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

            if (contentContainer)
                contentContainer.gameObject.SetActive(isExpanded);
        }

        private void UpdateUI() => RefreshDisplay();

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
            if (IsDefault || groupNameInput == null) return;
            groupNameLabel?.gameObject.SetActive(false);
            groupNameInput.gameObject.SetActive(true);
            groupNameInput.text = groupName;
            groupNameInput.Select();
            groupNameInput.ActivateInputField();
        }

        private void OnNameEditEnd(string newName)
        {
            groupName = string.IsNullOrWhiteSpace(newName) ? "Nouveau groupe" : newName.Trim();
            groupNameLabel?.gameObject.SetActive(true);
            groupNameInput?.gameObject.SetActive(false);
            UpdateUI();
        }

        // ── Suppression ───────────────────────────────────────────────

        private void TryDelete()
        {
            if (IsDefault)
            {
                ToastManager.Show("Le groupe GÉNÉRAL ne peut pas être supprimé", ToastType.Warning);
                return;
            }

            if (FriendCount > 0)
            {
                ToastManager.Show("Videz le groupe avant de le supprimer", ToastType.Warning);
                return;
            }

            var cg = ResolveHeaderCG();
            cg.DOFade(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            headerButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
            if (groupNameInput != null)
                groupNameInput.onEndEdit.RemoveAllListeners();
        }
    }
}





