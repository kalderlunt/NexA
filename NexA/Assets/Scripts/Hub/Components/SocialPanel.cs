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
        [SerializeField] private GameObject friendItemPrefab;
        [SerializeField] private GameObject friendGroupContainerPrefab; // prefab pour groupes CUSTOM uniquement
        [SerializeField] private GameObject offlineFolderPrefab;
        [SerializeField] private TextMeshProUGUI headerCountText;
        [SerializeField] private TextMeshProUGUI emptyText;

        [Header("Groupe GÉNÉRAL (assigné directement dans la scène)")]
        [SerializeField] private FriendGroupContainer generalGroup; // glisser le GO Default(General)-Folder ici

        // ── Propriété publique ─────────────────────────────────────────
        public Transform FriendsContainer => friendsContainer;

        // ── Bottom bar ─────────────────────────────────────────────────
        [Header("Bottom bar")]
        [SerializeField] private Button messagingButton;
        [SerializeField] private Button objectivesButton;       // désactivé
        [SerializeField] private Button micButton;              // désactivé
        [SerializeField] private TextMeshProUGUI versionText;   // ex: "26.03"
        [SerializeField] private Button bugButton;

        // ── Animation ─────────────────────────────────────────────────
        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;

        // ── État interne ───────────────────────────────────────────────
        private List<Friend> cachedFriends = new();
        private List<FolderDetail> cachedFolders = new();
        private bool sortByAlpha = false;
        private bool groupOffline = false;
        private bool optionsOpen = false;
        // defaultGroup = alias vers generalGroup (assigné dans l'Inspector, jamais instancié dynamiquement)
        private FriendGroupContainer defaultGroup => generalGroup;

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
            addGroupButton?.onClick.AddListener(OnAddGroupClicked);
            optionsButton?.onClick.AddListener(ToggleOptionsDropdown);
            searchButton?.onClick.AddListener(OnSearchClicked);

            sortAlphaButton?.onClick.AddListener(OnSortAlphaClicked);
            sortStatusButton?.onClick.AddListener(OnSortStatusClicked);
            groupOfflineButton?.onClick.AddListener(OnGroupOfflineClicked);

            messagingButton?.onClick.AddListener(OnMessagingClicked);
            objectivesButton?.onClick.AddListener(OnDisabledFeatureClicked);
            micButton?.onClick.AddListener(OnDisabledFeatureClicked);
            bugButton?.onClick.AddListener(OnBugReportClicked);

            SetButtonDisabled(objectivesButton);
            SetButtonDisabled(micButton);

            if (versionText != null)
                versionText.text = "";

            if (optionsDropdown != null)
                optionsDropdown.SetActive(false);

            // Valider que le groupe GÉNÉRAL est bien assigné dans l'Inspector
            if (generalGroup == null)
                Debug.LogError("[SocialPanel] 'General Group' non assigné dans l'Inspector ! Glisser le GO Default(General)-Folder.");
            else
                generalGroup.Init("GÉNÉRAL", asDefault: true);

            Show();
            //Hide(instant: true);
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Affiche le numéro de version depuis la BDD. Ex: SetVersion("26.03")</summary>
        public void SetVersion(string version)
        {
            if (versionText != null)
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

                // Cache plat pour compatibilité
                cachedFriends = new List<Friend>();
                HashSet<string> seen = new System.Collections.Generic.HashSet<string>();
                foreach (var folder in cachedFolders)
                    if (folder?.friends != null)
                        foreach (var entry in folder.friends)
                            if (entry?.friend != null && seen.Add(entry.friend.id))
                                cachedFriends.Add(entry.friend);

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
            if (generalGroup == null)
            {
                Debug.LogError("[SocialPanel] generalGroup non assigné !");
                return;
            }

            // ── 1. Supprimer les groupes CUSTOM et dossiers temporaires ──
            // On collecte d'abord pour ne pas modifier friendsContainer pendant l'itération.
            var toDestroy = new List<GameObject>();
            foreach (Transform child in friendsContainer)
            {
                // Ignorer le groupe GÉNÉRAL (il est fixe dans la scène)
                if (child.gameObject == generalGroup.gameObject) continue;

                var g = child.GetComponent<FriendGroupContainer>();
                if (g != null) { toDestroy.Add(child.gameObject); continue; }

                var f = child.GetComponent<FriendFolderItem>();
                if (f != null) { toDestroy.Add(child.gameObject); continue; }
            }
            foreach (var go in toDestroy)
                Destroy(go);

            // ── 2. Vider le contenu du groupe GÉNÉRAL ──────────────────
            var generalChildren = new List<GameObject>();
            foreach (Transform child in generalGroup.contentContainer)
                generalChildren.Add(child.gameObject);
            foreach (var go in generalChildren)
                Destroy(go);

            int totalFriends = 0;
            int totalOnline  = 0;

            // Curseur GLOBAL — anime dossiers et rows séquentiellement dans l'ordre affiché :
            // [1] header GÉNÉRAL → [2] Test2Kal → [3] Test3Kal → [4] header Groupe1 → [5] header Groupe2 → ...
            float cursor = 0f;

            if (cachedFolders != null && cachedFolders.Count > 0)
            {
                // ── Affichage par dossiers backend ──────────────────────
                foreach (var folder in cachedFolders)
                {
                    if (folder?.friends == null) continue;

                    // Déterminer le container cible
                    Transform target;
                    FriendGroupContainer groupComp;

                    if (folder.isDefault)
                    {
                        groupComp = defaultGroup;
                        target    = defaultGroup.contentContainer;
                        // Header du groupe GÉNÉRAL
                        cursor = groupComp.AnimateHeaderIn(cursor);
                        cursor += 0.04f;
                    }
                    else
                    {
                        if (!friendGroupContainerPrefab)
                        {
                            groupComp = defaultGroup;
                            target    = defaultGroup.contentContainer;
                        }
                        else
                        {
                            GameObject go = Instantiate(friendGroupContainerPrefab, friendsContainer);
                            groupComp = go.GetComponent<FriendGroupContainer>();
                            groupComp?.Init(folder.name, asDefault: false);
                            target = groupComp?.contentContainer ?? friendsContainer;
                        }
                        // Header du dossier custom
                        cursor = groupComp.AnimateHeaderIn(cursor);
                        cursor += 0.04f;
                    }

                    // Séparer en ligne / hors ligne dans ce dossier
                    List<Friend> online  = folder.friends
                        .Where(e => e?.friend != null &&
                               (e.friend.StatusNormalized == "online" || e.friend.StatusNormalized == "in-game"))
                        .Select(e => e.friend).ToList();
                    List<Friend> offline = folder.friends
                        .Where(e => e?.friend != null && e.friend.StatusNormalized == "offline")
                        .Select(e => e.friend).ToList();

                    SortList(online);
                    SortList(offline);

                    foreach (var friend in online)
                    {
                        FriendSidePanelRow row = SpawnFriendRow(friend, target);
                        cursor = row.AnimateIn(cursor);
                        cursor += 0.04f;
                        totalOnline++;
                    }

                    if (groupOffline && offline.Count > 0)
                    {
                        cursor = SpawnOfflineFolder(offline, friendsContainer, cursor);
                        cursor += 0.04f;
                    }
                    else
                    {
                        foreach (var friend in offline)
                        {
                            FriendSidePanelRow row = SpawnFriendRow(friend, target);
                            cursor = row.AnimateIn(cursor);
                            cursor += 0.04f;
                        }
                    }

                    totalFriends += folder.friends.Count;
                    groupComp?.RefreshCount(online.Count, folder.friends.Count);
                }
            }
            else
            {
                // ── Fallback : affichage plat si pas de dossiers ────────
                List<Friend> online  = cachedFriends.Where(f => f.StatusNormalized == "online" || f.StatusNormalized == "in-game").ToList();
                List<Friend> offline = cachedFriends.Where(f => f.StatusNormalized == "offline").ToList();

                SortList(online);
                SortList(offline);

                Transform target = defaultGroup?.contentContainer ?? friendsContainer;

                // Header GÉNÉRAL
                if (defaultGroup != null)
                {
                    cursor = defaultGroup.AnimateHeaderIn(cursor);
                    cursor += 0.04f;
                }

                foreach (var friend in online)
                {
                    FriendSidePanelRow row = SpawnFriendRow(friend, target);
                    cursor = row.AnimateIn(cursor);
                    cursor += 0.04f;
                    totalOnline++;
                }

                if (groupOffline && offline.Count > 0)
                {
                    cursor = SpawnOfflineFolder(offline, target, cursor);
                }
                else
                {
                    foreach (var friend in offline)
                    {
                        FriendSidePanelRow row = SpawnFriendRow(friend, target);
                        cursor = row.AnimateIn(cursor);
                        cursor += 0.04f;
                    }
                }

                totalFriends = cachedFriends.Count;
                defaultGroup?.RefreshCount(totalOnline, totalFriends);
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

        private FriendSidePanelRow SpawnFriendRow(Friend friend, Transform target = null)
        {
            Transform parent = target ? target : friendsContainer;
            GameObject go = Instantiate(friendItemPrefab, parent);
            go.name = friend.username;  // nom du GO = pseudo de l'ami
            go.GetComponent<FriendSidePanelRow>()?.Setup(friend);
            return go.GetComponent<FriendSidePanelRow>();
        }

        private float SpawnOfflineFolder(List<Friend> offlineFriends, Transform target, float cursor)
        {
            if (!offlineFolderPrefab)
            {
                foreach (var f in offlineFriends)
                {
                    FriendSidePanelRow row = SpawnFriendRow(f, target);
                    cursor = row.AnimateIn(cursor);
                    cursor += 0.04f;
                }
                return cursor;
            }

            GameObject folder = Instantiate(offlineFolderPrefab, target);
            if (folder.TryGetComponent<FriendFolderItem>(out var folderComp))
            {
                folderComp.Setup($"Hors ligne ({offlineFriends.Count})", offlineFriends, friendItemPrefab);
                cursor = folderComp.AnimateIn(cursor);
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
            optionsOpen = !optionsOpen;
            if (optionsDropdown != null)
                optionsDropdown.SetActive(optionsOpen);
        }

        private void OnSortAlphaClicked()
        {
            sortByAlpha = true;
            CloseDropdown();
            RebuildList();
            ToastManager.Show("Tri : alphabétique", ToastType.Info);
        }

        private void OnSortStatusClicked()
        {
            sortByAlpha = false;
            CloseDropdown();
            RebuildList();
            ToastManager.Show("Tri : par état", ToastType.Info);
        }

        private void OnGroupOfflineClicked()
        {
            groupOffline = !groupOffline;
            CloseDropdown();
            RebuildList();
            ToastManager.Show(groupOffline ? "Amis hors ligne groupés" : "Amis hors ligne affichés", ToastType.Info);
        }

        private void CloseDropdown()
        {
            optionsOpen = false;
            if (optionsDropdown != null) optionsDropdown.SetActive(false);
        }

        // ── Boutons ────────────────────────────────────────────────────

        private void OnAddFriendClicked()   => UIManager.Instance.ShowScreen(ScreenType.Friends);

        private void OnAddGroupClicked()
        {
            if (!friendGroupContainerPrefab)
            {
                ToastManager.Show("Prefab FriendGroupContainer manquant", ToastType.Warning);
                return;
            }

            // Compter uniquement les groupes custom (pas le GÉNÉRAL)
            int customCount = GetAllGroups().Count(g => !g.IsDefault);
            GameObject go = Instantiate(friendGroupContainerPrefab, friendsContainer);
            FriendGroupContainer group = go.GetComponent<FriendGroupContainer>();
            group?.Init($"Groupe {customCount + 1}", asDefault: false);
        }

        private void OnSearchClicked()      => UIManager.Instance.ShowScreen(ScreenType.Friends);
        private void OnMessagingClicked()   => ToastManager.Show("Messagerie — bientôt disponible", ToastType.Info);
        private void OnDisabledFeatureClicked() => ToastManager.Show("Fonctionnalité en cours de développement", ToastType.Warning);
        private void OnBugReportClicked()   => ToastManager.Show("Signalement de bug — en développement", ToastType.Warning);

        // ── API publique groupes ───────────────────────────────────────

        /// <summary>Retourne tous les FriendGroupContainer actifs dans friendsContainer.</summary>
        public List<FriendGroupContainer> GetAllGroups()
        {
            List<FriendGroupContainer> result = new List<FriendGroupContainer>();
            
            if (friendsContainer == null)
                return result;
            
            foreach (Transform child in friendsContainer)
            {
                FriendGroupContainer container = child.GetComponent<FriendGroupContainer>();
                if (container) 
                    result.Add(container);
            }
            return result;
        }

        /// <summary>Met à jour les compteurs de tous les groupes après un déplacement.</summary>
        public void RefreshGroupHeaders()
        {
            foreach (FriendGroupContainer container in GetAllGroups())
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
            micButton?.onClick.RemoveAllListeners();
            bugButton?.onClick.RemoveAllListeners();
        }
    }
}

