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

        // Menu Options (activé/désactivé en overlay)
        [Header("Menu Options (dropdown)")]
        [SerializeField] private GameObject optionsDropdown;
        [SerializeField] private Button sortAlphaButton;       // Trier A→Z
        [SerializeField] private Button sortStatusButton;      // Trier par état (défaut)
        [SerializeField] private Button groupOfflineButton;    // Grouper hors ligne

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

        // ── État interne ───────────────────────────────────────────────
        private List<FolderDetail>          cachedFolders = new();
        private bool sortByAlpha = false;
        private bool groupOffline = false;
        private bool optionsOpen = false;

        // Source de vérité pour tous les dossiers actifs (GÉNÉRAL + custom)
        private readonly HashSet<FriendFolderContainer> activeFolders = new();

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
            optionsButton?.onClick.AddListener(ToggleOptionsDropdown);
            searchButton?.onClick.AddListener(OnSearchClicked);

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
                canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
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
                canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
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
                        cursor += 0.04f;
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
                        cursor += 0.04f;
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
                        cursor += 0.04f;
                        totalOnline++;
                    }

                    if (groupOffline && offlineFriends.Count > 0)
                    {
                        cursor = ShowOfflineFolder(offlineFriends, friendshipIdMap, friendsContainer, cursor);
                        cursor += 0.04f;
                    }
                    else
                    {
                        foreach (Friend friend in offlineFriends)
                        {
                            friendshipIdMap.TryGetValue(friend.id, out string fid);
                            FriendSidePanelRow row = CreateFriendRow(friend, target, fid);
                            cursor = row.AnimateIn(cursor);
                            cursor += 0.04f;
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
            cursor += 0.04f;

            // Peupler et animer les rows
            foreach (Friend friend in offlineFriends)
            {
                string fid = null;
                friendshipIdMap?.TryGetValue(friend.id, out fid);
                FriendSidePanelRow row = CreateFriendRow(friend, offlineFolder.friendContainer, fid);
                cursor = row.AnimateIn(cursor);
                cursor += 0.04f;
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
            optionsOpen = !optionsOpen;
            if (optionsDropdown)
                optionsDropdown.SetActive(optionsOpen);
        }

        private void OnSortAlphaClicked()
        {
            Deselect();
            sortByAlpha = true;
            CloseDropdown();
            RebuildList();
            ToastManager.Show("Tri : alphabétique", ToastType.Info);
        }

        private void OnSortStatusClicked()
        {
            Deselect();
            sortByAlpha = false;
            CloseDropdown();
            RebuildList();
            ToastManager.Show("Tri : par état", ToastType.Info);
        }

        private void OnGroupOfflineClicked()
        {
            Deselect();
            groupOffline = !groupOffline;
            CloseDropdown();
            RebuildList();
            ToastManager.Show(groupOffline ? "Amis hors ligne groupés" : "Amis hors ligne affichés", ToastType.Info);
        }

        private void CloseDropdown()
        {
            optionsOpen = false;
            if (optionsDropdown) 
                optionsDropdown.SetActive(false);
        }

        // ── Boutons ────────────────────────────────────────────────────

        private void OnAddFriendClicked()
        {
            Deselect();
            UIManager.Instance.ShowScreen(ScreenType.Friends);
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

        private void OnSearchClicked()          { Deselect(); UIManager.Instance.ShowScreen(ScreenType.Friends); }
        private void OnMessagingClicked()       { Deselect(); ToastManager.Show("Messagerie — bientôt disponible", ToastType.Info); }
        private void OnDisabledFeatureClicked() { Deselect(); ToastManager.Show("Fonctionnalité en cours de développement", ToastType.Warning); }
        private void OnBugReportClicked()       { Deselect(); ToastManager.Show("Signalement de bug — en développement", ToastType.Warning); }

        /// <summary>Désélectionne le bouton actif pour éviter le highlight persistant après clic.</summary>
        private static void Deselect() => EventSystem.current?.SetSelectedGameObject(null);

        // ── API publique groupes ───────────────────────────────────────

        /// <summary>Retourne tous les dossiers actifs (GÉNÉRAL + custom).</summary>
        public HashSet<FriendFolderContainer> GetAllGroups() => activeFolders;

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
            addFriendButton?.onClick.RemoveAllListeners();
            addGroupButton?.onClick.RemoveAllListeners();
            searchButton?.onClick.RemoveAllListeners();
            optionsButton?.onClick.RemoveAllListeners();
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

