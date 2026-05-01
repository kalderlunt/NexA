using DG.Tweening;
using NexA.Hub.Core;
using NexA.Hub.Models;
using NexA.Hub.Services;
using NexA.Hub.Screens;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Panel social latéral droit — TOUJOURS VISIBLE dans le Hub.
    ///
    /// Structure :
    ///  ┌─────────────────────────────┐
    ///  │ SOCIAL  [+ami] [+dossier] [⋮]│  ← TopBar (boutons)
    ///  ├─────────────────────────────┤
    ///  │  ▼ GÉNÉRAL (9/51)             │
    ///  │  ● Ami 1  En ligne          │  ← FriendItem (corps scrollable)
    ///  │  ● Ami 2  En jeu            │
    ///  │  ▼ Hors ligne (dossier)     │
    ///  │    ● Ami 3  Hors ligne      │
    ///  ├─────────────────────────────┤
    ///  │ [✉] [🎯] [🎤]              │  ← BottomBar
    ///  │ v1.0.0          [🐛]       │
    ///  └─────────────────────────────┘
    /// </summary>
    public class SocialPanel : MonoBehaviour
    {
        public static SocialPanel Instance { get; private set; }

        // ── Top bar ────────────────────────────────────────────────────
        [Header("Top bar")]
        [SerializeField] private Button addFriendButton;   // 👤+  Ajouter un ami
        [SerializeField] private Button addGroupButton;    // ⋮    Ajouter un dossier
        [SerializeField] private Button optionsButton;     // ☰    Options (tri, groupement)
        [SerializeField] private Button searchButton;      // 🔍   Rechercher
        [SerializeField] private Button notificationsButton; // 🔔  Demandes d'ami reçues

        // Badge sur le bouton notifications (nombre de demandes en attente)
        [Header("Notifications badge")]
        [SerializeField] private GameObject  pendingBadge;       // GO contenant le cercle rouge
        [SerializeField] private TextMeshProUGUI pendingBadgeText; // chiffre dans le badge

        // Overlays
        [Header("Overlays")]
        [SerializeField] private AddFriendOverlay      addFriendOverlay;       // ancien (gardé)
        [SerializeField] private AddFriendOverlayV2    addFriendOverlayV2;     // nouveau (créé par code)
        [SerializeField] private Sprite loadingSpinnerSprite;    // optionnel : sprite pour l'indicateur de chargement dans les overlays
        [SerializeField] private SearchOverlayV2       searchOverlayV2;        // recherche amis (créé par code)
        [SerializeField] private FriendRequestsOverlay friendRequestsOverlay;

        // Menu Options (activé/désactivé en overlay)
        [Header("Menu Options (dropdown)")]
        [SerializeField] private OptionsOverlayV2 optionsOverlayV2;    // nouveau (créé par code)
        [SerializeField] private GameObject optionsDropdown;            // ancien (gardé)
        [SerializeField] private Button sortAlphaButton;       // Trier A→Z
        [SerializeField] private Button sortStatusButton;      // Trier par état (défaut)
        [SerializeField] private Button groupOfflineButton;    // Grouper hors ligne

        // Labels des boutons du dropdown (pour afficher le ✓ actif)
        [Header("Labels dropdown (optionnel — enfant Text TMP de chaque bouton)")]
        [SerializeField] private TextMeshProUGUI sortAlphaLabel;
        [SerializeField] private TextMeshProUGUI sortStatusLabel;
        [SerializeField] private TextMeshProUGUI groupOfflineLabel;

        // ── Corps (liste) ──────────────────────────────────────────────
        [Header("Corps — liste des amis")]
        [SerializeField] private Transform friendsContainer;
        [SerializeField] private GameObject folderFriendItemPrefab; // prefab pour dossiers (GÉNÉRAL, custom, hors ligne)
        [SerializeField] private GameObject friendItemPrefab;
        [SerializeField] private TextMeshProUGUI emptyText;

        [Header("Folders (assignés directement dans la scène)")]
        private FriendFolderContainer generalFolder;  // Default(General) - Folder
        private FriendFolderContainer offlineFolder;  // dossier Hors ligne (toujours en scène, caché par défaut)

        // ── Propriété publique ─────────────────────────────────────────
        public Transform FriendsContainer => friendsContainer;

        // ── Bottom bar ─────────────────────────────────────────────────
        [Header("Bottom bar")]
        [SerializeField] private Button messagingButton;
        [SerializeField] private Button objectivesButton;       // désactivé
        [SerializeField] private Button microButton;              // désactivé
        [SerializeField] private TextMeshProUGUI versionText;   // ex: "26.03"
        [SerializeField] private Button bugButton;

        // ── Animation ─────────────────────────────────────────────────
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float staggerDelay = 0.04f;

        // ── État interne ───────────────────────────────────────────────
        private List<FolderDetail>          cachedFolders = new();
        private bool sortByAlpha = false;
        private bool groupOffline = false;
        private bool optionsOpen = false;

        // Source de vérité pour tous les dossiers actifs (GÉNÉRAL + custom)
        private readonly HashSet<FriendFolderContainer> activeFolders = new();

        /// <summary>FriendshipIds supprimés localement — pour ignorer le rebond STOMP.</summary>
        private readonly HashSet<string> _selfInitiatedRemovals = new();

        private FriendFolderContainer DefaultFolder => generalFolder;

        /// <summary>Dossier GÉNÉRAL accessible par FriendFolderContainer pour la migration d'amis.</summary>
        public FriendFolderContainer DefaultFolderPublic => generalFolder;

        // ── Awake / Start ──────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            addFriendButton?.onClick.AddListener(OnAddFriendClicked);
            addGroupButton?.onClick.AddListener(OnAddFolderClicked);
            searchButton?.onClick.AddListener(OnSearchClicked);
            notificationsButton?.onClick.AddListener(OnNotificationsClicked);

            // Cacher le badge au démarrage
            if (pendingBadge)
                pendingBadge.SetActive(false);

            // Écouter les changements de statut et les acceptations d'ami en temps réel
            if (FriendsManager.Instance != null)
            {
                FriendsManager.Instance.OnFriendStatusChanged += OnFriendStatusChanged;
                FriendsManager.Instance.OnFriendAccepted      += OnFriendAccepted;
                FriendsManager.Instance.OnFriendRemoved       += OnFriendRemoved;
            }

            // Créer les overlays et boutons par code s'ils ne sont pas assignés dans la scène
            EnsureOverlay();

            // Brancher optionsButton APRÈS EnsureOverlay (il peut être créé dedans)
            optionsButton?.onClick.AddListener(ToggleOptionsDropdown);

            sortAlphaButton?.onClick.AddListener(OnSortAlphaClicked);
            sortStatusButton?.onClick.AddListener(OnSortStatusClicked);
            groupOfflineButton?.onClick.AddListener(OnGroupOfflineClicked);

            messagingButton?.onClick.AddListener(OnMessagingClicked);
            objectivesButton?.onClick.AddListener(OnDisabledFeatureClicked);
            microButton?.onClick.AddListener(OnDisabledFeatureClicked);
            bugButton?.onClick.AddListener(OnBugReportClicked);

            SetButtonDisabled(objectivesButton);
            SetButtonDisabled(microButton);

            if (versionText)
                versionText.text = "";

            if (optionsDropdown)
                optionsDropdown.SetActive(false);

            UpdateSortLabels();
            CreateDefaultFolders();
            // Show() est appelé par HomeScreen.ShowAsync — pas besoin de le rappeler ici
        }

        private void CreateDefaultFolders()
        {
            generalFolder = CreateFolder();
            Assert.IsNotNull(generalFolder, "[SocialPanel] 'generalFolder' est null après assignment.");
            generalFolder.Init("GENERAL", asDefault: true);

            offlineFolder = CreateFolder();
            Assert.IsNotNull(offlineFolder, "[SocialPanel] 'offlineFolder' est null après assignment.");
            offlineFolder.Init("OFFLINE", asDefault: true);
            offlineFolder.Hide();
        }

        /// <summary>
        /// Crée le FriendRequestsOverlay et le bouton notifications par code
        /// si non assignés dans l'Inspector.
        /// </summary>
        private void EnsureOverlay()
        {
            // ── OptionsOverlayV2 ───────────────────────────────────────
            if (!optionsOverlayV2)
            {
                GameObject optGO = new GameObject("OptionsOverlayV2");
                optGO.transform.SetParent(transform, false);
                optGO.layer = gameObject.layer;
                RectTransform optRT = optGO.AddComponent<RectTransform>();
                optRT.anchorMin        = new Vector2(1, 1);
                optRT.anchorMax        = new Vector2(1, 1);
                optRT.pivot            = new Vector2(1, 1);
                optRT.anchoredPosition = new Vector2(0, -40);
                optRT.sizeDelta        = new Vector2(160, 0);
                optGO.AddComponent<CanvasGroup>();
                optionsOverlayV2 = optGO.AddComponent<OptionsOverlayV2>();
                optionsOverlayV2.BuildUI(
                    onSortAlpha:    OnSortAlphaClicked,
                    onSortStatus:   OnSortStatusClicked,
                    onGroupOffline: OnGroupOfflineClicked
                );
            }

            // ── optionsButton (☰) ──────────────────────────────────────
            if (!optionsButton)
            {
                Transform topBar = transform.childCount > 0 ? transform.GetChild(0) : transform;

                GameObject btnGO = new GameObject("OptionsButton");
                btnGO.transform.SetParent(topBar, false);
                btnGO.layer = gameObject.layer;

                Image btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.55f, 0.55f, 0.65f, 1f);

                optionsButton = btnGO.AddComponent<Button>();
                optionsButton.targetGraphic = btnImg;
                optionsButton.onClick.AddListener(ToggleOptionsDropdown);

                LayoutElement le = btnGO.AddComponent<LayoutElement>();
                le.minWidth = 24; le.preferredWidth = 24;
                le.minHeight = 24; le.preferredHeight = 24;

                GameObject lblGO = new GameObject("Label");
                lblGO.transform.SetParent(btnGO.transform, false);
                RectTransform lblRT = lblGO.AddComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
                lblRT.sizeDelta = Vector2.zero;
                TextMeshProUGUI lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = "=";
                lbl.fontSize  = 11f;
                lbl.fontStyle = FontStyles.Bold;
                lbl.color     = Color.white;
                lbl.alignment = TextAlignmentOptions.Center;

                Debug.Log("[SocialPanel] OptionsButton créé par code.");
            }

            // ── SearchOverlayV2 ────────────────────────────────────────
            if (!searchOverlayV2)
            {
                GameObject searchGO = new GameObject("SearchOverlayV2");
                searchGO.transform.SetParent(transform, false);
                searchGO.layer = gameObject.layer;

                RectTransform searchRT = searchGO.AddComponent<RectTransform>();
                searchRT.anchorMin = Vector2.zero;
                searchRT.anchorMax = Vector2.one;
                searchRT.offsetMin = Vector2.zero;
                searchRT.offsetMax = Vector2.zero;

                UnityEngine.UI.Image searchBg = searchGO.AddComponent<UnityEngine.UI.Image>();
                searchBg.color = new Color(0.063f, 0.063f, 0.122f, 0.97f);
                searchBg.raycastTarget = true;

                searchGO.AddComponent<CanvasGroup>();
                searchOverlayV2 = searchGO.AddComponent<SearchOverlayV2>();
                searchOverlayV2.BuildUI();
            }

            // ── AddFriendOverlayV2 ─────────────────────────────────────
            if (!addFriendOverlayV2)            {
                GameObject addOverlayGO = new GameObject("AddFriendOverlayV2");
                addOverlayGO.transform.SetParent(transform, false);
                addOverlayGO.layer = gameObject.layer;

                RectTransform addRT = addOverlayGO.AddComponent<RectTransform>();
                addRT.anchorMin = Vector2.zero;
                addRT.anchorMax = Vector2.one;
                addRT.offsetMin = Vector2.zero;
                addRT.offsetMax = Vector2.zero;

                UnityEngine.UI.Image addBg = addOverlayGO.AddComponent<UnityEngine.UI.Image>();
                addBg.color = new Color(0.063f, 0.063f, 0.122f, 0.97f);
                addBg.raycastTarget = true;

                addOverlayGO.AddComponent<CanvasGroup>();
                addFriendOverlayV2 = addOverlayGO.AddComponent<AddFriendOverlayV2>();
                if (loadingSpinnerSprite)
                    addFriendOverlayV2.SetSpinnerSprite(loadingSpinnerSprite);
                addFriendOverlayV2.BuildUI();
            }

            // ── FriendRequestsOverlay ──────────────────────────────────
            if (!friendRequestsOverlay)
            {
                GameObject overlayGO = new GameObject("FriendRequestsOverlay");
                overlayGO.transform.SetParent(transform, false);
                overlayGO.layer = gameObject.layer;

                // Couvre tout le SocialPanel
                RectTransform rt = overlayGO.AddComponent<RectTransform>();
                rt.anchorMin        = Vector2.zero;
                rt.anchorMax        = Vector2.one;
                rt.offsetMin        = Vector2.zero;
                rt.offsetMax        = Vector2.zero;

                // Fond semi-opaque
                UnityEngine.UI.Image bg = overlayGO.AddComponent<UnityEngine.UI.Image>();
                bg.color = new Color(0.063f, 0.063f, 0.122f, 0.97f);
                bg.raycastTarget = true;

                overlayGO.AddComponent<CanvasGroup>();
                friendRequestsOverlay = overlayGO.AddComponent<FriendRequestsOverlay>();
                friendRequestsOverlay.BuildUI(); // construit TopRow + ScrollView + textes
            }

            // ── Bouton notifications ───────────────────────────────────
            if (!notificationsButton)
            {
                // Cherche le PanelTopBar (premier enfant)
                Transform topBar = transform.childCount > 0 ? transform.GetChild(0) : transform;

                GameObject btnGO = new GameObject("NotificationsButton");
                btnGO.transform.SetParent(topBar, false);
                btnGO.layer = gameObject.layer;

                RectTransform btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(24, 24);

                UnityEngine.UI.Image btnImg = btnGO.AddComponent<UnityEngine.UI.Image>();
                btnImg.color = new Color(0.69f, 0.69f, 0.69f, 1f);

                notificationsButton = btnGO.AddComponent<UnityEngine.UI.Button>();
                notificationsButton.targetGraphic = btnImg;
                notificationsButton.onClick.AddListener(OnNotificationsClicked);

                UnityEngine.UI.LayoutElement le = btnGO.AddComponent<UnityEngine.UI.LayoutElement>();
                le.minWidth = 24; le.minHeight = 24;
                le.preferredWidth = 24; le.preferredHeight = 24;

                // Label "🔔" en TMP
                GameObject lblGO = new GameObject("Label");
                lblGO.transform.SetParent(btnGO.transform, false);
                RectTransform lblRT = lblGO.AddComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
                lblRT.sizeDelta = Vector2.zero;
                TMPro.TextMeshProUGUI lbl = lblGO.AddComponent<TMPro.TextMeshProUGUI>();
                lbl.text = "D"; // "D" pour Demandes — remplacer par icône si disponible
                lbl.fontSize = 9f;
                lbl.color = Color.white;
                lbl.alignment = TMPro.TextAlignmentOptions.Center;

                // Badge rouge
                GameObject badgeGO = new GameObject("PendingBadge");
                badgeGO.transform.SetParent(btnGO.transform, false);
                RectTransform badgeRT = badgeGO.AddComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(1, 1);
                badgeRT.anchorMax = new Vector2(1, 1);
                badgeRT.pivot     = new Vector2(1, 1);
                badgeRT.anchoredPosition = new Vector2(-1, -1);
                badgeRT.sizeDelta = new Vector2(13, 13);

                UnityEngine.UI.Image badgeImg = badgeGO.AddComponent<UnityEngine.UI.Image>();
                badgeImg.color = new Color(0.92f, 0.24f, 0.24f, 1f);

                pendingBadge = badgeGO;
                pendingBadge.SetActive(false);

                GameObject badgeTxtGO = new GameObject("BadgeText");
                badgeTxtGO.transform.SetParent(badgeGO.transform, false);
                RectTransform badgeTxtRT = badgeTxtGO.AddComponent<RectTransform>();
                badgeTxtRT.anchorMin = Vector2.zero; badgeTxtRT.anchorMax = Vector2.one;
                badgeTxtRT.sizeDelta = Vector2.zero;
                pendingBadgeText = badgeTxtGO.AddComponent<TMPro.TextMeshProUGUI>();
                pendingBadgeText.text = "0";
                pendingBadgeText.fontSize = 7f;
                pendingBadgeText.fontStyle = TMPro.FontStyles.Bold;
                pendingBadgeText.color = Color.white;
                pendingBadgeText.alignment = TMPro.TextAlignmentOptions.Center;

                Debug.Log("[SocialPanel] NotificationsButton créé par code.");
            }

            // ── FriendContextMenu ──────────────────────────────────────
            if (!FriendContextMenu.Instance)
            {
                GameObject ctxGO = new GameObject("FriendContextMenu");
                ctxGO.transform.SetParent(transform, false);
                ctxGO.layer = gameObject.layer;

                // Le panel flottant du menu contextuel
                RectTransform panelRT = ctxGO.AddComponent<RectTransform>();
                panelRT.sizeDelta = new Vector2(160, 0);
                panelRT.pivot     = new Vector2(0f, 1f);

                UnityEngine.UI.Image bg = ctxGO.AddComponent<UnityEngine.UI.Image>();
                bg.color = new Color(0.08f, 0.08f, 0.14f, 0.97f);

                VerticalLayoutGroup vlg = ctxGO.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(0, 0, 4, 4);
                vlg.spacing = 0f;
                vlg.childForceExpandWidth  = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlWidth  = true;
                vlg.childControlHeight = true;

                ContentSizeFitter csf = ctxGO.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                ctxGO.AddComponent<CanvasGroup>();

                // Assigner le panel sur le composant (panel = lui-même)
                FriendContextMenu ctx = ctxGO.AddComponent<FriendContextMenu>();

                // Réflexion pour assigner le champ panel (privé) sans le rendre public
                var panelField = typeof(FriendContextMenu)
                    .GetField("panel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                panelField?.SetValue(ctx, panelRT);

                // itemsContainer = le GO lui-même (VLG dessus)
                var itemsField = typeof(FriendContextMenu)
                    .GetField("itemsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                itemsField?.SetValue(ctx, ctxGO.transform);

                ctxGO.SetActive(false);
                Debug.Log("[SocialPanel] FriendContextMenu créé par code.");
            }

            Debug.Log("[SocialPanel] EnsureOverlay OK.");
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Affiche le numéro de version depuis la BDD. Ex: SetVersion("26.03")</summary>
        public void SetVersion(string version)
        {
            if (versionText)
                versionText.text = version;
        }

        public async void Show()
        {
            gameObject.SetActive(true);
            Debug.Log("[SocialPanel] Affichage du panneau social");

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic);
            }

            await RefreshAsync();
        }

        public void Hide(bool instant = false)
        {
            if (optionsDropdown)
                optionsDropdown.SetActive(false);
            
            optionsOpen = false;

            if (instant)
            {
                gameObject.SetActive(false);
                return;
            }

            if (canvasGroup)
                canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() => gameObject.SetActive(false));
            else
                gameObject.SetActive(false);
        }

        public async Task RefreshAsync()
        {
            try
            {
                cachedFolders = await APIService.Instance.GetAllFoldersWithFriendsAsync();
                RebuildList();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SocialPanel] Erreur : {ex.Message}");
            }

            // Mettre à jour le badge des demandes en attente (en parallèle — ne bloque pas)
            _ = RefreshPendingBadgeAsync();
        }

        /// <summary>Interroge le backend et met à jour le badge de demandes en attente.</summary>
        public async Task RefreshPendingBadgeAsync()
        {
            try
            {
                List<PendingFriendRequest> pending = await APIService.Instance.GetPendingRequestsAsync();
                int count = pending?.Count ?? 0;
                RefreshPendingBadge(count);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SocialPanel] Impossible de récupérer les demandes en attente : {ex.Message}");
            }
        }

        /// <summary>Met à jour l'affichage du badge (appelé par FriendRequestsOverlay après accept/reject).</summary>
        public void RefreshPendingBadge(int count)
        {
            if (pendingBadge)
                pendingBadge.SetActive(count > 0);

            if (pendingBadgeText)
                pendingBadgeText.text = count > 9 ? "9+" : count.ToString();
        }

        // ── Construction de la liste ───────────────────────────────────

        private void RebuildList()
        {
            if (!generalFolder)
            {
                Debug.LogError("[SocialPanel] generalGroup non assigné !");
                return;
            }

            // ── 1. Supprimer les groupes CUSTOM ──────────────────────
            HashSet<GameObject> toDestroy = new();
            
            foreach (Transform child in friendsContainer)
            {
                if (child.gameObject == generalFolder.gameObject) 
                    continue;
                
                if (offlineFolder && child.gameObject == offlineFolder.gameObject)
                    continue;

                if (child.GetComponent<FriendFolderContainer>())
                    toDestroy.Add(child.gameObject);
            }

            foreach (GameObject go in toDestroy)
            {
                FriendFolderContainer fc = go.GetComponent<FriendFolderContainer>();
                if (fc) 
                    activeFolders.Remove(fc);
                Destroy(go);
            }

            // ── 2. Vider le contenu du groupe GÉNÉRAL ──────────────────
            HashSet<GameObject> generalChildren = new();
            
            foreach (Transform child in generalFolder.friendContainer)
                generalChildren.Add(child.gameObject);
            
            foreach (GameObject go in generalChildren)
                Destroy(go);

            // ── 3. Vider et masquer le dossier Hors ligne ──────────────
            if (offlineFolder)
            {
                HashSet<GameObject> offlineChildren = new();
                
                foreach (Transform child in offlineFolder.friendContainer)
                    offlineChildren.Add(child.gameObject);
                
                foreach (GameObject go in offlineChildren)
                    Destroy(go);
                
                offlineFolder.Hide();
            }

            int totalFriends = 0;
            int totalOnline  = 0;

            // Curseur GLOBAL — anime dossiers et rows séquentiellement dans l'ordre affiché :
            // [1] header GÉNÉRAL → [2] Test2Kal → [3] Test3Kal → [4] header Groupe1 → [5] header Groupe2 → ...
            float cursor = 0f;

            if (cachedFolders != null && cachedFolders.Count > 0)
            {
                // ── Affichage par dossiers backend ──────────────────────
                foreach (FolderDetail folder in cachedFolders)
                {
                    if (folder?.friendsList == null) 
                        continue;

                    // Déterminer le container cible
                    Transform target;
                    FriendFolderContainer folderContainer;

                    if (folder.isDefault)
                    {
                        folderContainer = DefaultFolder;
                        target    = DefaultFolder.friendContainer;
                        // Stocker l'ID backend du dossier par défaut
                        DefaultFolder.FolderId = folder.id;
                        // Header du groupe GÉNÉRAL
                        cursor = folderContainer.AnimateHeaderIn(cursor);
                        cursor += staggerDelay;
                    }
                    else
                    {
                        if (!folderFriendItemPrefab)
                        {
                            folderContainer = DefaultFolder;
                            target     = DefaultFolder.friendContainer;
                        }
                        else
                        {
                            folderContainer = CreateFolder();
                            folderContainer?.Init(folder.name, asDefault: false, folderId: folder.id);
                            target = folderContainer?.friendContainer ?? friendsContainer;
                        }

                        // Header du dossier custom
                        cursor = folderContainer.AnimateHeaderIn(cursor);
                        cursor += staggerDelay;
                    }

                    // Séparer en ligne / hors ligne dans ce dossier
                    List<FolderFriendEntry> onlineEntries  = folder.friendsList
                        .Where(e => e.friend != null &&
                               (e.friend.StatusNormalized == "online" || e.friend.StatusNormalized == "in-game"))
                        .ToList();

                    List<FolderFriendEntry> offlineEntries = folder.friendsList
                        .Where(e => e.friend != null && e.friend.StatusNormalized == "offline")
                        .ToList();

                    // Trier
                    List<Friend> onlineFriends  = onlineEntries.Select(e => e.friend).ToList();
                    List<Friend> offlineFriends = offlineEntries.Select(e => e.friend).ToList();
                    SortList(onlineFriends);
                    SortList(offlineFriends);

                    // Reconstruire les listes d'entrées triées (pour garder le friendshipId)
                    Dictionary<string, string> friendshipIdMap = new();
                    foreach (FolderFriendEntry e in folder.friendsList)
                        if (e.friend != null)
                            friendshipIdMap[e.friend.id] = e.friendshipId;

                    foreach (Friend friend in onlineFriends)
                    {
                        friendshipIdMap.TryGetValue(friend.id, out string fid);
                        FriendSidePanelRow row = CreateFriendRow(friend, target, fid);
                        cursor = row.AnimateIn(cursor);
                        cursor += staggerDelay;
                        totalOnline++;
                    }

                    if (groupOffline && offlineFriends.Count > 0)
                    {
                        cursor = ShowOfflineFolder(offlineFriends, friendshipIdMap, friendsContainer, cursor);
                        cursor += staggerDelay;
                    }
                    else
                    {
                        foreach (Friend friend in offlineFriends)
                        {
                            friendshipIdMap.TryGetValue(friend.id, out string fid);
                            FriendSidePanelRow row = CreateFriendRow(friend, target, fid);
                            cursor = row.AnimateIn(cursor);
                            cursor += staggerDelay;
                        }
                    }

                    totalFriends += folder.friendsList.Count;
                    folderContainer?.RefreshCount(onlineFriends.Count, folder.friendsList.Count);
                }
            }

            if (emptyText)
                emptyText.gameObject.SetActive(totalFriends == 0);

            HubTopBar.Instance?.SetFriendsOnlineBadge(totalOnline);
        }


        private void SortList(List<Friend> list)
        {
            if (sortByAlpha)
                list.Sort((a, b) => string.Compare(a.username, b.username, System.StringComparison.OrdinalIgnoreCase));
            else
                list.Sort((a, b) =>
                {
                    int pa = StatusPriority(a.status), pb = StatusPriority(b.status);
                    return pa != pb ? pa.CompareTo(pb)
                        : string.Compare(a.username, b.username, System.StringComparison.OrdinalIgnoreCase);
                });
        }

        private FriendFolderContainer CreateFolder()
        {
            
            Assert.IsNotNull(folderFriendItemPrefab, "[SocialPanel] friendsContainer doit être assigné dans l'Inspector.");
            FriendFolderContainer folder = Instantiate(folderFriendItemPrefab, friendsContainer).GetComponent<FriendFolderContainer>();
            Assert.IsNotNull(folder, "[SocialPanel] Le prefab 'folderFriendItemPrefab' doit avoir un composant FriendFolderContainer.");
            Debug.Log($"[SocialPanel] Création d'un nouveau dossier d'amis {folder.gameObject.name}");

            activeFolders.Add(folder);

            return folder;
        }
        
        private FriendSidePanelRow CreateFriendRow(Friend friend, Transform target = null, string friendshipId = null)
        {
            Transform parent = target ? target : friendsContainer;
            GameObject go = Instantiate(friendItemPrefab, parent);
            go.name = $"Friend - {friend.username}";
            FriendSidePanelRow row = go.GetComponent<FriendSidePanelRow>();
            row?.Setup(friend, friendshipId);
            return row;
        }

        private float ShowOfflineFolder(List<Friend> offlineFriends, Dictionary<string, string> friendshipIdMap, Transform target, float cursor)
        {
            if (!offlineFolder)
            {
                Debug.LogError("[SocialPanel] offlineFolder non assigné !");
                return cursor;
            }

            // S'assurer qu'il est bien enfant du bon container
            offlineFolder.transform.SetParent(target, worldPositionStays: false);
            offlineFolder.Show();

            // Mettre à jour le titre avec le bon count (sans toucher à l'alpha)
            offlineFolder.Rename($"Hors ligne ({offlineFriends.Count})");

            // Animer le header dans le curseur global
            cursor = offlineFolder.AnimateHeaderIn(cursor);
            cursor += staggerDelay;

            // Peupler et animer les rows
            foreach (Friend friend in offlineFriends)
            {
                string fid = null;
                friendshipIdMap?.TryGetValue(friend.id, out fid);
                FriendSidePanelRow row = CreateFriendRow(friend, offlineFolder.friendContainer, fid);
                cursor = row.AnimateIn(cursor);
                cursor += staggerDelay;
            }

            return cursor;
        }

        private static int StatusPriority(string status) => status switch
        {
            "online"  => 0,
            "in-game" => 1,
            _         => 2
        };

        // ── Options dropdown ───────────────────────────────────────────

        private void ToggleOptionsDropdown()
        {
            Deselect();

            if (optionsOverlayV2 != null)
            {
                if (optionsOverlayV2.IsOpen)
                    optionsOverlayV2.Hide();
                else
                    optionsOverlayV2.Show(sortByAlpha, groupOffline);
                return;
            }

            // Fallback ancien dropdown
            optionsOpen = !optionsOpen;
            if (optionsDropdown)
            {
                if (optionsDropdown.transform.parent != transform)
                    optionsDropdown.transform.SetParent(transform, worldPositionStays: true);
                optionsDropdown.SetActive(optionsOpen);
                if (optionsOpen)
                    optionsDropdown.transform.SetAsLastSibling();
            }
        }

        private void OnSortAlphaClicked()
        {
            Deselect();
            sortByAlpha = true;
            CloseDropdown();
            UpdateSortLabels();
            RebuildList();
            ToastManager.Show("Tri : alphabétique", ToastType.Info);
        }

        private void OnSortStatusClicked()
        {
            Deselect();
            sortByAlpha = false;
            CloseDropdown();
            UpdateSortLabels();
            RebuildList();
            ToastManager.Show("Tri : par état", ToastType.Info);
        }

        private void OnGroupOfflineClicked()
        {
            Deselect();
            groupOffline = !groupOffline;
            optionsOverlayV2?.UpdateState(sortByAlpha, groupOffline);
            UpdateSortLabels();
            RebuildList();
            ToastManager.Show(groupOffline ? "Amis hors ligne groupés" : "Amis hors ligne affichés", ToastType.Info);
        }

        private void CloseDropdown()
        {
            optionsOpen = false;
            optionsOverlayV2?.Hide();
            if (optionsDropdown) optionsDropdown.SetActive(false);
        }

        /// <summary>Met à jour les labels du dropdown pour refléter le tri actif (✓ = actif).</summary>
        private void UpdateSortLabels()
        {
            if (sortAlphaLabel)
                sortAlphaLabel.text   = sortByAlpha  ? "✓ Trier A → Z"      : "Trier A → Z";
            if (sortStatusLabel)
                sortStatusLabel.text  = !sortByAlpha ? "✓ Trier par état"    : "Trier par état";
            if (groupOfflineLabel)
                groupOfflineLabel.text = groupOffline ? "✓ Grouper hors ligne" : "Grouper hors ligne";
        }

        // ── Boutons ────────────────────────────────────────────────────

        private void OnAddFriendClicked()
        {
            Deselect();
            if (addFriendOverlayV2 != null)
                addFriendOverlayV2.Show();
            else if (addFriendOverlay != null)
                addFriendOverlay.Show();
            else
                UIManager.Instance.ShowScreen(ScreenType.Friends);
        }

        private void OnNotificationsClicked()
        {
            Deselect();
            if (friendRequestsOverlay != null)
                friendRequestsOverlay.Show();
            else
                ToastManager.Show("Overlay non configuré", ToastType.Warning);
        }

        private async void OnAddFolderClicked()
        {
            Deselect();
            if (!folderFriendItemPrefab)
            {
                ToastManager.Show("Prefab manquant", ToastType.Warning);
                return;
            }

            int customCount = activeFolders.Count(f => !f.IsDefault);
            string folderName = $"Groupe {customCount + 1}";

            // Créer côté backend d'abord pour obtenir l'ID
            string folderId = null;
            try
            {
                FolderSummary created = await APIService.Instance.CreateFolderAsync(folderName);
                folderId = created?.id;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SocialPanel] Erreur création dossier : {ex.Message}");
                ToastManager.Show("Erreur lors de la création du dossier", ToastType.Error);
                return;
            }

            FriendFolderContainer folder = CreateFolder();
            folder?.Init(folderName, asDefault: false, folderId: folderId);
            folder?.AnimateHeaderIn();
        }

        private void OnSearchClicked()
        {
            Deselect();
            if (searchOverlayV2 != null)
            {
                // Passe la liste en cache si dispo, sinon SearchOverlayV2 charge lui-même
                List<Friend> cached = GetCachedFriends();
                searchOverlayV2.Show(
                    friends: cached,
                    onInviteClicked: () => addFriendOverlayV2?.Show()
                );
            }
            else
            {
                UIManager.Instance.ShowScreen(ScreenType.Friends);
            }
        }

        // ── Temps réel (WebSocket/STOMP) ──────────────────────────────

        /// <summary>
        /// Appelé par FriendsManager quand une demande d'ami est acceptée côté backend.
        /// Crée directement la row dans le dossier GÉNÉRAL sans rechargement complet.
        /// </summary>
        private void OnFriendAccepted(FriendAcceptedNotification notification)
        {
            Debug.Log($"[SocialPanel] Nouvel ami via WS : {notification.username} — ajout de la row");

            // Éviter le doublon si la row existe déjà (cas où on est l'initiateur de la demande)
            if (FindRowByFriendId(notification.userId) != null)
            {
                Debug.Log($"[SocialPanel] Row déjà existante pour {notification.username} — ignoré");
                return;
            }

            // Construire un Friend minimal depuis la notification
            Friend newFriend = new Friend
            {
                id       = notification.userId,
                username = notification.username,
                status   = "ONLINE",   // un ami qui vient d'accepter est forcément en ligne
                level    = 0,
                elo      = 0
            };

            // Créer la row (sans parent pour l'instant — AddRow positionnera correctement)
            FriendFolderContainer target = generalFolder;
            FriendSidePanelRow row = CreateFriendRow(newFriend, friendsContainer, notification.friendshipId);
            target?.AddRow(row.gameObject);  // re-parent dans friendContainer au bon index
            row.AnimateIn(0f);

            // Mettre à jour le badge en ligne
            RefreshGroupHeaders();
            HubTopBar.Instance?.SetFriendsOnlineBadge(CountOnlineFriends());

            // Fermer le badge de demandes en attente (décrémenter)
            _ = RefreshPendingBadgeAsync();
        }

        /// <summary>
        /// Appelé par FriendsManager quand un ami nous supprime (ou qu'on est supprimé) via STOMP.
        /// Retire la row en temps réel sans rechargement complet.
        /// </summary>
        private void OnFriendRemoved(FriendRemovedNotification notification)
        {
            Debug.Log($"[SocialPanel] Ami supprimé via WS : {notification.username} (friendshipId={notification.friendshipId})");

            // Ignorer si c'est nous qui avons initié la suppression (l'UI est déjà à jour)
            if (_selfInitiatedRemovals.Remove(notification.friendshipId))
            {
                Debug.Log($"[SocialPanel] Suppression self-initiated ignorée pour {notification.friendshipId}");
                return;
            }

            // Chercher la row par friendshipId en priorité, sinon par userId
            FriendSidePanelRow row = FindRowByFriendshipId(notification.friendshipId)
                                     ?? FindRowByFriendId(notification.userId);

            if (row == null)
            {
                Debug.LogWarning($"[SocialPanel] Row introuvable pour l'ami supprimé {notification.username}");
                return;
            }

            // Retirer du dossier
            FriendFolderContainer folder = row.GetComponentInParent<FriendFolderContainer>();
            folder?.RemoveRow(row.gameObject);

            // Supprimer le GO avec un petit fade
            CanvasGroup cg = row.GetComponent<CanvasGroup>() ?? row.gameObject.AddComponent<CanvasGroup>();
            cg.DOFade(0f, 0.2f).OnComplete(() =>
            {
                if (row != null) Destroy(row.gameObject);
                RefreshGroupHeaders();
                HubTopBar.Instance?.SetFriendsOnlineBadge(CountOnlineFriends());
            });

            ToastManager.Show($"{notification.username} vous a retiré de ses amis", ToastType.Info);
        }

        /// <summary>Cherche une FriendSidePanelRow par friendshipId dans tous les containers.</summary>
        private FriendSidePanelRow FindRowByFriendshipId(string friendshipId)
        {
            if (string.IsNullOrEmpty(friendshipId)) return null;
            foreach (FriendFolderContainer folder in activeFolders)
            {
                if (folder == null || folder.friendContainer == null) continue;
                foreach (Transform child in folder.friendContainer)
                {
                    FriendSidePanelRow row = child.GetComponent<FriendSidePanelRow>();
                    if (row != null && row.FriendshipId == friendshipId)
                        return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Appelé par FriendsManager quand un ami change de statut via STOMP.
        /// Met à jour uniquement la row concernée + le cache local.
        /// </summary>
        private void OnFriendStatusChanged(FriendStatusUpdate update)
        {
            if (string.IsNullOrEmpty(update?.userId))
                return;

            // 1. Mettre à jour le cache local
            if (cachedFolders != null)
            {
                foreach (FolderDetail folder in cachedFolders)
                    if (folder?.friendsList != null)
                        foreach (FolderFriendEntry entry in folder.friendsList)
                            if (entry?.friend != null && entry.friend.id == update.userId)
                                entry.friend.status = update.status;
            }

            // 2. Trouver la row correspondante et mettre à jour son statut
            FriendSidePanelRow targetRow = FindRowByFriendId(update.userId);
            if (targetRow != null)
            {
                bool wasOnline = targetRow.IsOnline;
                targetRow.UpdateStatus(update.status);
                bool isNowOnline = targetRow.IsOnline;

                Debug.Log($"[SocialPanel] Statut mis à jour : {update.username} → {update.status}");

                // Si le statut online/offline a changé et que le groupement hors ligne est actif,
                // il faut reconstruire la liste pour déplacer la row dans le bon dossier
                if (wasOnline != isNowOnline && groupOffline)
                {
                    RebuildList();
                    return;
                }

                // Rafraîchir les compteurs des dossiers
                RefreshGroupHeaders();

                // Mettre à jour le badge en ligne dans la top bar
                int totalOnline = CountOnlineFriends();
                HubTopBar.Instance?.SetFriendsOnlineBadge(totalOnline);
            }
        }

        /// <summary>Cherche une FriendSidePanelRow par l'ID de l'ami dans tous les containers.</summary>
        private FriendSidePanelRow FindRowByFriendId(string friendId)
        {
            foreach (FriendFolderContainer folder in activeFolders)
            {
                if (folder == null || folder.friendContainer == null) continue;

                foreach (Transform child in folder.friendContainer)
                {
                    FriendSidePanelRow row = child.GetComponent<FriendSidePanelRow>();
                    if (row != null && row.FriendId == friendId)
                        return row;
                }
            }

            return null;
        }

        /// <summary>Compte le nombre total d'amis en ligne dans tous les dossiers.</summary>
        private int CountOnlineFriends()
        {
            int count = 0;
            foreach (FriendFolderContainer folder in activeFolders)
            {
                if (folder == null || folder.friendContainer == null) continue;

                foreach (Transform child in folder.friendContainer)
                {
                    FriendSidePanelRow row = child.GetComponent<FriendSidePanelRow>();
                    if (row != null && row.IsOnline)
                        count++;
                }
            }
            return count;
        }

        /// <summary>Extrait la liste plate des amis depuis les dossiers en cache.</summary>
        private List<Friend> GetCachedFriends()
        {
            if (cachedFolders == null || cachedFolders.Count == 0) return null;
            HashSet<string> seen = new();
            List<Friend> result  = new();
            foreach (FolderDetail folder in cachedFolders)
                if (folder?.friendsList != null)
                    foreach (FolderFriendEntry entry in folder.friendsList)
                        if (entry?.friend != null && seen.Add(entry.friend.id))
                            result.Add(entry.friend);
            return result.Count > 0 ? result : null;
        }
        private void OnMessagingClicked()       { Deselect(); ToastManager.Show("Messagerie — bientôt disponible", ToastType.Info); }
        private void OnDisabledFeatureClicked() { Deselect(); ToastManager.Show("Fonctionnalité en cours de développement", ToastType.Warning); }
        private void OnBugReportClicked()       { Deselect(); ToastManager.Show("Signalement de bug — en développement", ToastType.Warning); }

        /// <summary>Désélectionne le bouton actif pour éviter le highlight persistant après clic.</summary>
        private static void Deselect() => EventSystem.current?.SetSelectedGameObject(null);

        // ── API publique groupes ───────────────────────────────────────

        /// <summary>Retourne tous les dossiers actifs (GÉNÉRAL + custom).</summary>
        public HashSet<FriendFolderContainer> GetAllGroups() => activeFolders;

        /// <summary>
        /// Enregistre une suppression initiée localement pour que le rebond STOMP
        /// /topic/friend-removed soit ignoré (l'UI est déjà mise à jour).
        /// </summary>
        public void RegisterSelfRemoval(string friendshipId)
        {
            if (!string.IsNullOrEmpty(friendshipId))
                _selfInitiatedRemovals.Add(friendshipId);
        }

        /// <summary>Met à jour les labels de tous les dossiers après un drag & drop.</summary>
        public void RefreshGroupHeaders()
        {
            foreach (FriendFolderContainer container in activeFolders)
                container.RefreshDisplay();
        }

        // ── Utilitaires ────────────────────────────────────────────────

        private static void SetButtonDisabled(Button btn)
        {
            if (!btn)
                return;
            
            btn.interactable = false;
            ColorBlock colors = btn.colors;
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            btn.colors = colors;
        }

        private void OnDestroy()
        {
            // Se désabonner des événements temps réel
            if (FriendsManager.Instance != null)
            {
                FriendsManager.Instance.OnFriendStatusChanged -= OnFriendStatusChanged;
                FriendsManager.Instance.OnFriendAccepted      -= OnFriendAccepted;
                FriendsManager.Instance.OnFriendRemoved       -= OnFriendRemoved;
            }

            addFriendButton?.onClick.RemoveAllListeners();
            addGroupButton?.onClick.RemoveAllListeners();
            searchButton?.onClick.RemoveAllListeners();
            optionsButton?.onClick.RemoveAllListeners();
            notificationsButton?.onClick.RemoveAllListeners();
            sortAlphaButton?.onClick.RemoveAllListeners();
            sortStatusButton?.onClick.RemoveAllListeners();
            groupOfflineButton?.onClick.RemoveAllListeners();
            messagingButton?.onClick.RemoveAllListeners();
            objectivesButton?.onClick.RemoveAllListeners();
            microButton?.onClick.RemoveAllListeners();
            bugButton?.onClick.RemoveAllListeners();

            foreach (FriendFolderContainer folder in activeFolders)
                Destroy(folder.gameObject);
        }
    }
}

