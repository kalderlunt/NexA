# Architecture Client Unity

## Vue d'ensemble

Ce document décrit l'architecture complète du client Unity avec structure de dossiers, composants, et patterns.

---

## Diagramme 36: Structure de Dossiers Unity

**Type de diagramme souhaité**: Folder Tree Diagram

**Description**:
Organisation complète des dossiers du projet Unity.

```
Assets/
├── Scenes/
│   ├── _Boot.unity                    # Scene de démarrage (DontDestroyOnLoad objects)
│   ├── MainMenu.unity                 # Scene principale (tous les écrans)
│   └── Game.unity                     # Scene de jeu (futur)
│
├── Scripts/
│   ├── Core/                          # Core systems
│   │   ├── GameManager.cs            # Singleton principal
│   │   ├── SceneLoader.cs            # Gestion transitions scenes
│   │   └── Constants.cs              # Constantes globales
│   │
│   ├── Network/                       # Communication réseau
│   │   ├── NetworkManager.cs         # Wrapper HTTP avec retry
│   │   ├── APIEndpoints.cs           # URLs endpoints
│   │   └── WebSocketManager.cs       # WebSocket (futur)
│   │
│   ├── Services/                      # Services métier
│   │   ├── APIService.cs             # Appels API REST
│   │   ├── AuthService.cs            # Authentification (tokens)
│   │   ├── CacheService.cs           # Cache mémoire
│   │   └── AnimationService.cs       # Helpers DOTween
│   │
│   ├── Models/                        # Data models
│   │   ├── User.cs
│   │   ├── Friend.cs
│   │   ├── Match.cs
│   │   ├── MatchParticipant.cs
│   │   ├── APIResponse.cs
│   │   └── APIError.cs
│   │
│   ├── UI/                            # UI Components
│   │   ├── Screens/                  # Écrans principaux
│   │   │   ├── BaseScreen.cs        # Classe de base abstraite
│   │   │   ├── LoginScreen.cs
│   │   │   ├── RegisterScreen.cs
│   │   │   ├── HomeScreen.cs
│   │   │   ├── ProfileScreen.cs
│   │   │   ├── FriendsScreen.cs
│   │   │   ├── MatchHistoryScreen.cs
│   │   │   └── MatchDetailsScreen.cs
│   │   │
│   │   ├── Components/               # UI composants réutilisables
│   │   │   ├── FriendListItem.cs
│   │   │   ├── MatchListItem.cs
│   │   │   ├── LoadingSpinner.cs
│   │   │   ├── ToastNotification.cs
│   │   │   └── ConfirmDialog.cs
│   │   │
│   │   ├── Animations/               # Scripts d'animation
│   │   │   ├── UITransitions.cs     # Fade/Slide helpers
│   │   │   ├── ButtonAnimations.cs  # Hover/Click effects
│   │   │   └── SkeletonUI.cs        # Skeleton loading
│   │   │
│   │   └── UIManager.cs              # Gestion navigation + toasts
│   │
│   ├── Utilities/                     # Helpers
│   │   ├── Logger.cs                 # Custom logger
│   │   ├── Extensions.cs             # Extension methods
│   │   └── TimeFormatter.cs          # Format timestamps
│   │
│   └── Editor/                        # Tools Unity Editor
│       └── BuildHelper.cs            # Build automation
│
├── Prefabs/
│   ├── UI/
│   │   ├── FriendListItem.prefab
│   │   ├── MatchListItem.prefab
│   │   ├── Toast.prefab
│   │   └── ConfirmDialog.prefab
│   │
│   └── Network/
│       └── NetworkManager.prefab
│
├── Resources/
│   ├── Sprites/
│   │   ├── UI/                       # Icons, buttons
│   │   └── Avatars/                  # Default avatars
│   │
│   └── Fonts/
│       └── Poppins/                  # Font family
│
├── Settings/
│   ├── InputSystem_Actions.inputactions
│   └── APIConfig.asset               # ScriptableObject config
│
├── Plugins/
│   └── DOTween/                      # Animation library
│
└── StreamingAssets/
    └── config.json                   # Config externe (API URL)
```

---

## Diagramme 37: Pattern Singleton pour Services

**Type de diagramme souhaité**: Class Diagram + Code Pattern

**Description**:
Pattern Singleton utilisé pour les services globaux.

**Base Singleton Generic**:
```csharp
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = $"(Singleton) {typeof(T)}";
                        
                        DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _applicationIsQuitting = true;
        }
    }
}
```

**Utilisation dans Services**:
```csharp
public class APIService : Singleton<APIService>
{
    protected override void Awake()
    {
        base.Awake();
        // Initialization
    }

    public async Task<APIResponse<User>> GetCurrentUser()
    {
        // Implementation
    }
}

// Usage dans autres scripts:
var response = await APIService.Instance.GetCurrentUser();
```

**Services utilisant Singleton**:
- APIService
- AuthService
- CacheService
- UIManager
- NetworkManager

---

## Diagramme 38: State Machine - Navigation UI

**Type de diagramme souhaité**: State Machine Diagram

**Description**:
Gestion des états de navigation entre écrans.

**States**:
```
[Splash] (initial state)
    ↓
[CheckAuth]
    ├─→ Token valid → [Home]
    └─→ No token → [Login]

[Login]
    ├─→ Register button → [Register]
    ├─→ Login success → [Home]
    └─→ Back (impossible)

[Register]
    ├─→ Back → [Login]
    ├─→ Success → [Home]
    └─→ Error → stay [Register]

[Home] (hub)
    ├─→ Profile → [Profile]
    ├─→ Friends → [Friends]
    ├─→ History → [MatchHistory]
    ├─→ Play → [Matchmaking] (futur)
    └─→ Logout → [Login]

[Profile]
    └─→ Back → [Home]

[Friends]
    └─→ Back → [Home]

[MatchHistory]
    ├─→ Click match → [MatchDetails]
    └─→ Back → [Home]

[MatchDetails]
    └─→ Back → [MatchHistory]
```

**Implementation UIManager**:
```csharp
public enum ScreenType
{
    None,
    Splash,
    Login,
    Register,
    Home,
    Profile,
    Friends,
    MatchHistory,
    MatchDetails
}

public class UIManager : Singleton<UIManager>
{
    private Stack<ScreenType> _navigationStack = new Stack<ScreenType>();
    private Dictionary<ScreenType, BaseScreen> _screens;
    private BaseScreen _currentScreen;

    public async void Navigate(ScreenType screenType, object data = null)
    {
        // Validation
        if (_currentScreen != null && _currentScreen.ScreenType == screenType)
        {
            Debug.LogWarning($"Already on screen: {screenType}");
            return;
        }

        // Hide current
        if (_currentScreen != null)
        {
            await _currentScreen.Hide();
        }

        // Push to stack
        _navigationStack.Push(screenType);

        // Show new
        _currentScreen = _screens[screenType];
        await _currentScreen.Show(data);
    }

    public async void NavigateBack()
    {
        if (_navigationStack.Count <= 1)
        {
            Debug.LogWarning("Cannot go back further");
            return;
        }

        // Pop current
        _navigationStack.Pop();

        // Get previous
        ScreenType previousScreen = _navigationStack.Peek();
        
        // Hide current
        await _currentScreen.Hide();

        // Show previous
        _currentScreen = _screens[previousScreen];
        await _currentScreen.Show();
    }
}
```

---

## Diagramme 39: BaseScreen Architecture

**Type de diagramme souhaité**: Class Hierarchy

**Description**:
Architecture des écrans avec classe de base abstraite.

**Hiérarchie**:
```
MonoBehaviour
    ↓
BaseScreen (abstract)
    ├── LoginScreen
    ├── RegisterScreen
    ├── HomeScreen
    ├── ProfileScreen
    ├── FriendsScreen
    ├── MatchHistoryScreen
    └── MatchDetailsScreen
```

**BaseScreen.cs**:
```csharp
public abstract class BaseScreen : MonoBehaviour
{
    [Header("Screen Config")]
    [SerializeField] protected ScreenType screenType;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected float transitionDuration = 0.3f;

    public ScreenType ScreenType => screenType;
    public bool IsVisible { get; protected set; }

    // Lifecycle
    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        // Hidden par défaut
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        IsVisible = false;
    }

    // Show with animation
    public virtual async Task Show(object data = null)
    {
        gameObject.SetActive(true);
        
        await AnimationService.Instance.FadeIn(canvasGroup, transitionDuration);
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        IsVisible = true;

        await OnScreenShown(data);
    }

    // Hide with animation
    public virtual async Task Hide()
    {
        await OnScreenHiding();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        await AnimationService.Instance.FadeOut(canvasGroup, transitionDuration);
        
        IsVisible = false;
        gameObject.SetActive(false);
    }

    // Override in child classes
    protected virtual async Task OnScreenShown(object data) 
    {
        // Called after show animation
        await Task.CompletedTask;
    }

    protected virtual async Task OnScreenHiding() 
    {
        // Called before hide animation
        await Task.CompletedTask;
    }
}
```

**Exemple concret - FriendsScreen.cs**:
```csharp
public class FriendsScreen : BaseScreen
{
    [Header("Friends UI")]
    [SerializeField] private Transform friendsListContainer;
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private GameObject skeletonUI;

    private List<Friend> _cachedFriends;

    protected override async Task OnScreenShown(object data)
    {
        await base.OnScreenShown(data);
        await LoadFriends();
    }

    private async Task LoadFriends()
    {
        // 1. Check cache
        _cachedFriends = CacheService.Instance.GetCachedFriends();
        
        if (_cachedFriends != null)
        {
            // Display instantly from cache
            DisplayFriends(_cachedFriends);
            return;
        }

        // 2. Show skeleton
        skeletonUI.SetActive(true);

        // 3. Fetch from API
        var response = await APIService.Instance.GetFriends();

        // 4. Hide skeleton
        skeletonUI.SetActive(false);

        if (response.Success)
        {
            _cachedFriends = response.Data;
            CacheService.Instance.CacheFriends(_cachedFriends);
            DisplayFriends(_cachedFriends);
        }
        else
        {
            UIManager.Instance.ShowToast($"Error: {response.Error.Message}", ToastType.Error);
        }
    }

    private void DisplayFriends(List<Friend> friends)
    {
        // Clear existing
        foreach (Transform child in friendsListContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate with stagger animation
        for (int i = 0; i < friends.Count; i++)
        {
            GameObject item = Instantiate(friendItemPrefab, friendsListContainer);
            var friendItem = item.GetComponent<FriendListItem>();
            friendItem.Setup(friends[i]);

            // Stagger animation
            float delay = i * 0.05f;
            AnimationService.Instance.SlideInWithDelay(item.GetComponent<RectTransform>(), delay);
        }
    }
}
```

---

## Diagramme 40: Network Layer - Retry Logic

**Type de diagramme souhaité**: Flowchart + Sequence

**Description**:
Gestion des appels réseau avec retry automatique.

**NetworkManager.cs**:
```csharp
public class NetworkManager : Singleton<NetworkManager>
{
    [Header("Config")]
    [SerializeField] private int maxRetries = 3;
    [SerializeField] private float[] retryDelays = { 1f, 2f, 4f };
    [SerializeField] private float requestTimeout = 10f;

    public async Task<APIResponse<T>> SendRequest<T>(
        string url, 
        string method, 
        object body = null, 
        Dictionary<string, string> headers = null,
        bool shouldRetry = true)
    {
        int attempt = 0;
        
        while (attempt <= maxRetries)
        {
            try
            {
                var response = await ExecuteRequest<T>(url, method, body, headers);
                
                // Success
                if (response.Success)
                {
                    return response;
                }

                // Handle specific error codes
                if (response.StatusCode == 401)
                {
                    // Try refresh token
                    bool refreshed = await AuthService.Instance.RefreshToken();
                    if (refreshed)
                    {
                        // Retry with new token
                        continue;
                    }
                    else
                    {
                        // Redirect to login
                        UIManager.Instance.Navigate(ScreenType.Login);
                        return response;
                    }
                }

                // Don't retry 4xx errors (except 401)
                if (response.StatusCode >= 400 && response.StatusCode < 500)
                {
                    return response;
                }

                // Retry 5xx or network errors
                if (shouldRetry && attempt < maxRetries)
                {
                    attempt++;
                    Debug.LogWarning($"Request failed. Retry {attempt}/{maxRetries}");
                    await Task.Delay((int)(retryDelays[attempt - 1] * 1000));
                    continue;
                }

                return response;
            }
            catch (TimeoutException)
            {
                if (shouldRetry && attempt < maxRetries)
                {
                    attempt++;
                    await Task.Delay((int)(retryDelays[attempt - 1] * 1000));
                    continue;
                }

                return new APIResponse<T>
                {
                    Success = false,
                    Error = new APIError
                    {
                        Code = "TIMEOUT",
                        Message = "Request timeout"
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Request exception: {ex.Message}");
                return new APIResponse<T>
                {
                    Success = false,
                    Error = new APIError
                    {
                        Code = "NETWORK_ERROR",
                        Message = ex.Message
                    }
                };
            }
        }

        // Max retries exceeded
        return new APIResponse<T>
        {
            Success = false,
            Error = new APIError
            {
                Code = "MAX_RETRIES_EXCEEDED",
                Message = "Request failed after maximum retries"
            }
        };
    }

    private async Task<APIResponse<T>> ExecuteRequest<T>(
        string url, 
        string method, 
        object body, 
        Dictionary<string, string> headers)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            // Setup request
            request.timeout = (int)requestTimeout;
            request.downloadHandler = new DownloadHandlerBuffer();

            // Add body
            if (body != null)
            {
                string json = JsonUtility.ToJson(body);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            // Add headers
            request.SetRequestHeader("Content-Type", "application/json");
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            // Send
            await request.SendWebRequest();

            // Parse response
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<APIResponse<T>>(responseText);
                response.StatusCode = (int)request.responseCode;
                return response;
            }
            else
            {
                // Error
                var errorResponse = new APIResponse<T>
                {
                    Success = false,
                    StatusCode = (int)request.responseCode,
                    Error = new APIError
                    {
                        Code = "HTTP_ERROR",
                        Message = request.error
                    }
                };

                // Try parse error response
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        errorResponse = JsonUtility.FromJson<APIResponse<T>>(request.downloadHandler.text);
                    }
                    catch { }
                }

                return errorResponse;
            }
        }
    }
}

// Extension method for async
public static class UnityWebRequestExtensions
{
    public static Task SendWebRequest(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<bool>();
        request.SendWebRequest().completed += (op) => tcs.SetResult(true);
        return tcs.Task;
    }
}
```

---

## Diagramme 41: Cache Strategy

**Type de diagramme souhaité**: Component Diagram + Cache Flow

**Description**:
Stratégie de cache côté client pour réduire appels API.

**CacheService.cs**:
```csharp
public class CacheService : Singleton<CacheService>
{
    [Header("Cache Config")]
    [SerializeField] private float friendsCacheDuration = 300f; // 5 minutes
    [SerializeField] private float matchesCacheDuration = 300f; // 5 minutes
    [SerializeField] private float profileCacheDuration = 600f; // 10 minutes

    private class CacheEntry<T>
    {
        public T Data;
        public float Timestamp;
        public bool IsValid(float duration) => Time.time - Timestamp < duration;
    }

    // Cache storage
    private CacheEntry<List<Friend>> _friendsCache;
    private CacheEntry<User> _profileCache;
    private Dictionary<string, CacheEntry<Match>> _matchDetailsCache = new Dictionary<string, CacheEntry<Match>>();
    private CacheEntry<List<Match>> _matchHistoryCache;

    // Friends
    public List<Friend> GetCachedFriends()
    {
        if (_friendsCache != null && _friendsCache.IsValid(friendsCacheDuration))
        {
            Debug.Log("[Cache] Friends HIT");
            return _friendsCache.Data;
        }
        Debug.Log("[Cache] Friends MISS");
        return null;
    }

    public void CacheFriends(List<Friend> friends)
    {
        _friendsCache = new CacheEntry<List<Friend>>
        {
            Data = friends,
            Timestamp = Time.time
        };
        Debug.Log($"[Cache] Friends cached ({friends.Count} items)");
    }

    public void InvalidateFriendsCache()
    {
        _friendsCache = null;
        Debug.Log("[Cache] Friends invalidated");
    }

    // Profile
    public User GetCachedProfile()
    {
        if (_profileCache != null && _profileCache.IsValid(profileCacheDuration))
        {
            Debug.Log("[Cache] Profile HIT");
            return _profileCache.Data;
        }
        Debug.Log("[Cache] Profile MISS");
        return null;
    }

    public void CacheProfile(User user)
    {
        _profileCache = new CacheEntry<User>
        {
            Data = user,
            Timestamp = Time.time
        };
        Debug.Log("[Cache] Profile cached");
    }

    // Match details
    public Match GetCachedMatchDetails(string matchId)
    {
        if (_matchDetailsCache.ContainsKey(matchId))
        {
            var entry = _matchDetailsCache[matchId];
            if (entry.IsValid(matchesCacheDuration))
            {
                Debug.Log($"[Cache] Match {matchId} HIT");
                return entry.Data;
            }
        }
        Debug.Log($"[Cache] Match {matchId} MISS");
        return null;
    }

    public void CacheMatchDetails(string matchId, Match match)
    {
        _matchDetailsCache[matchId] = new CacheEntry<Match>
        {
            Data = match,
            Timestamp = Time.time
        };
        Debug.Log($"[Cache] Match {matchId} cached");
    }

    // Clear all
    public void ClearAll()
    {
        _friendsCache = null;
        _profileCache = null;
        _matchDetailsCache.Clear();
        _matchHistoryCache = null;
        Debug.Log("[Cache] All cache cleared");
    }
}
```

**Cache Invalidation Rules**:
```csharp
// After login
CacheService.Instance.ClearAll();

// After friend request accepted
CacheService.Instance.InvalidateFriendsCache();

// After match completed
CacheService.Instance.InvalidateMatchHistoryCache();

// After profile update (futur)
CacheService.Instance.InvalidateProfileCache();
```

---

## Diagramme 42: Animation Service (DOTween Helpers)

**Type de diagramme souhaité**: Utility Class Diagram

**Description**:
Service d'animations réutilisables avec DOTween.

**AnimationService.cs**:
```csharp
using DG.Tweening;

public class AnimationService : Singleton<AnimationService>
{
    [Header("Transition Durations")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float scaleDuration = 0.2f;

    [Header("Easings")]
    [SerializeField] private Ease defaultEase = Ease.OutQuad;
    [SerializeField] private Ease popEase = Ease.OutBack;

    protected override void Awake()
    {
        base.Awake();
        DOTween.Init();
        DOTween.defaultEaseType = defaultEase;
    }

    // Fade animations
    public async Task FadeIn(CanvasGroup canvasGroup, float duration = -1)
    {
        if (duration < 0) duration = fadeDuration;
        
        canvasGroup.alpha = 0;
        await canvasGroup.DOFade(1f, duration).AsyncWaitForCompletion();
    }

    public async Task FadeOut(CanvasGroup canvasGroup, float duration = -1)
    {
        if (duration < 0) duration = fadeDuration;
        
        await canvasGroup.DOFade(0f, duration).AsyncWaitForCompletion();
    }

    // Slide animations
    public async Task SlideIn(RectTransform rectTransform, Vector2 direction, float duration = -1)
    {
        if (duration < 0) duration = slideDuration;

        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector2 offscreenPos = originalPos + direction * 1000f;

        rectTransform.anchoredPosition = offscreenPos;
        await rectTransform.DOAnchorPos(originalPos, duration).SetEase(defaultEase).AsyncWaitForCompletion();
    }

    public void SlideInWithDelay(RectTransform rectTransform, float delay)
    {
        Vector2 originalPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = originalPos + Vector2.left * 100f;
        rectTransform.DOAnchorPos(originalPos, slideDuration).SetDelay(delay).SetEase(defaultEase);
    }

    // Scale animations (pop effect)
    public async Task PopIn(Transform transform, float duration = -1)
    {
        if (duration < 0) duration = scaleDuration;

        transform.localScale = Vector3.zero;
        await transform.DOScale(Vector3.one, duration).SetEase(popEase).AsyncWaitForCompletion();
    }

    // Button hover effect
    public void ButtonHoverIn(Transform button)
    {
        button.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void ButtonHoverOut(Transform button)
    {
        button.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
    }

    // Button click effect
    public async Task ButtonClick(Transform button)
    {
        await button.DOScale(0.95f, 0.1f).AsyncWaitForCompletion();
        await button.DOScale(1f, 0.1f).AsyncWaitForCompletion();
    }

    // Shake effect (error)
    public void ShakeElement(Transform transform, float strength = 20f)
    {
        transform.DOShakePosition(0.5f, strength, 10, 90, false, true);
    }

    // Pulse effect (notification)
    public void PulseElement(Transform transform)
    {
        Sequence pulse = DOTween.Sequence();
        pulse.Append(transform.DOScale(1.1f, 0.3f));
        pulse.Append(transform.DOScale(1f, 0.3f));
        pulse.SetLoops(-1, LoopType.Restart);
    }
}
```

---

## Métadonnées pour génération

### Palette couleurs par couche
- Core: #F44336 (rouge)
- Network: #2196F3 (bleu)
- Services: #4CAF50 (vert)
- UI: #FF9800 (orange)
- Models: #9C27B0 (violet)

### Icônes Unity
- 📁 Folder
- 📄 Script
- 🎨 Prefab
- 🖼️ Sprite
- ⚙️ Settings

### Style diagrammes
- Folder tree: ASCII art ou Tree diagram
- Class hierarchy: UML Class Diagram
- Flowcharts: Standard flowchart symbols


