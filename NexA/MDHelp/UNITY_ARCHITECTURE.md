# 🎮 Unity Client - Architecture & Structure

## 📁 Structure des Dossiers (Assets/)

```
Assets/
├── Hub/                          # Écrans client (lobby)
│   ├── Scenes/
│   │   └── MainHub.unity
│   ├── Scripts/
│   │   ├── Core/                 # Architecture de base
│   │   │   ├── UIManager.cs
│   │   │   ├── ScreenBase.cs
│   │   │   └── ScreenTransition.cs
│   │   ├── Screens/              # Écrans UI
│   │   │   ├── LoginScreen.cs
│   │   │   ├── RegisterScreen.cs
│   │   │   ├── HomeScreen.cs
│   │   │   ├── ProfileScreen.cs
│   │   │   ├── FriendsScreen.cs
│   │   │   └── MatchHistoryScreen.cs
│   │   ├── Components/           # Composants réutilisables
│   │   │   ├── FriendListItem.cs
│   │   │   ├── MatchListItem.cs
│   │   │   ├── ToastNotification.cs
│   │   │   └── LoadingSpinner.cs
│   │   ├── Services/             # Logique métier
│   │   │   ├── APIService.cs
│   │   │   ├── AuthManager.cs
│   │   │   ├── FriendsManager.cs
│   │   │   ├── MatchesManager.cs
│   │   │   └── CacheManager.cs
│   │   ├── Models/               # Data models
│   │   │   ├── User.cs
│   │   │   ├── Friend.cs
│   │   │   ├── Match.cs
│   │   │   ├── APIResponse.cs
│   │   │   └── APIError.cs
│   │   └── Utils/                # Utilitaires
│   │       ├── AnimationHelper.cs
│   │       ├── SecureStorage.cs
│   │       └── CoroutineRunner.cs
│   └── Prefabs/
│       ├── UI/
│       │   ├── Screens/
│       │   ├── Components/
│       │   └── Notifications/
│       └── Particles/
├── Resources/
│   ├── Config/
│   │   └── APIConfig.asset
│   └── UI/
│       ├── Fonts/
│       ├── Icons/
│       └── Sprites/
└── Settings/
    └── DOTweenSettings.asset
```

---

## 🎨 Design System - DOTween Animations

### Durées Standards
```csharp
public static class AnimationTimings
{
    // Transitions rapides (UI feedback immédiat)
    public const float INSTANT = 0.1f;
    public const float FAST = 0.2f;
    
    // Transitions normales (défaut)
    public const float NORMAL = 0.3f;
    public const float MEDIUM = 0.4f;
    
    // Transitions lentes (emphasis)
    public const float SLOW = 0.5f;
    public const float VERY_SLOW = 0.8f;
}
```

### Easings Standards
```csharp
public static class AnimationEasings
{
    // Entrées (apparitions)
    public static Ease IN_BACK = Ease.OutBack;        // Pop effect
    public static Ease IN_SMOOTH = Ease.OutQuad;      // Smooth entry
    public static Ease IN_ELASTIC = Ease.OutElastic;  // Bouncy (attention: peut être too much)
    
    // Sorties (disparitions)
    public static Ease OUT_SMOOTH = Ease.InQuad;
    public static Ease OUT_FAST = Ease.InCubic;
    
    // Loops/Hover
    public static Ease LOOP = Ease.InOutSine;
    public static Ease HOVER = Ease.OutQuad;
}
```

### Patterns d'Animation

#### 1. **Screen Transition** (fade + slide)
```csharp
// Screen IN
screen.alpha = 0;
screen.transform.localPosition = new Vector3(50, 0, 0); // Légèrement à droite
screen.DOFade(1, AnimationTimings.NORMAL).SetEase(AnimationEasings.IN_SMOOTH);
screen.transform.DOLocalMoveX(0, AnimationTimings.NORMAL).SetEase(AnimationEasings.IN_SMOOTH);

// Screen OUT
screen.DOFade(0, AnimationTimings.FAST).SetEase(AnimationEasings.OUT_SMOOTH);
screen.transform.DOLocalMoveX(-50, AnimationTimings.FAST).SetEase(AnimationEasings.OUT_FAST);
```

#### 2. **Panel Pop** (apparition avec scale)
```csharp
panel.transform.localScale = Vector3.zero;
panel.transform.DOScale(1f, AnimationTimings.MEDIUM)
    .SetEase(AnimationEasings.IN_BACK) // OutBack = pop effect
    .OnComplete(() => panel.interactable = true);
```

#### 3. **Button Hover** (glow + scale)
```csharp
// Hover Enter
button.transform.DOScale(1.05f, AnimationTimings.FAST).SetEase(AnimationEasings.HOVER);
glow.DOFade(0.3f, AnimationTimings.FAST);

// Hover Exit
button.transform.DOScale(1f, AnimationTimings.FAST).SetEase(AnimationEasings.HOVER);
glow.DOFade(0f, AnimationTimings.FAST);
```

#### 4. **Loading Spinner** (rotate infini)
```csharp
spinner.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
    .SetEase(Ease.Linear)
    .SetLoops(-1, LoopType.Restart);
```

#### 5. **Toast Notification** (slide + auto-hide)
```csharp
toast.anchoredPosition = new Vector2(0, -100); // En bas, caché
toast.DOAnchorPosY(20, AnimationTimings.MEDIUM)
    .SetEase(AnimationEasings.IN_BACK)
    .OnComplete(() => {
        DOVirtual.DelayedCall(3f, () => {
            toast.DOAnchorPosY(-100, AnimationTimings.FAST)
                .SetEase(AnimationEasings.OUT_FAST)
                .OnComplete(() => Destroy(toast.gameObject));
        });
    });
```

#### 6. **Skeleton Loading** (shimmer effect)
```csharp
// Gradient qui se déplace de gauche à droite
float duration = 1.5f;
skeletonMask.rectTransform.DOAnchorPosX(width, duration)
    .SetEase(Ease.Linear)
    .SetLoops(-1, LoopType.Restart)
    .From(new Vector2(-width, 0));
```

---

## 🏗️ Architecture Pattern: UI Screens + Services + Data Models

### Flow Global
```
User Input → Screen (UI) → Manager (Logic) → APIService (HTTP) → Backend
                ↓                                  ↓
            Animation                          Cache/Storage
```

### State Machine des Écrans
```csharp
public enum ScreenType
{
    None,
    Login,
    Register,
    Home,
    Profile,
    Friends,
    MatchHistory,
    MatchDetails
}

// Transitions autorisées
private static Dictionary<ScreenType, List<ScreenType>> allowedTransitions = new()
{
    { ScreenType.Login, new() { ScreenType.Register, ScreenType.Home } },
    { ScreenType.Register, new() { ScreenType.Login } },
    { ScreenType.Home, new() { ScreenType.Profile, ScreenType.Friends, ScreenType.MatchHistory, ScreenType.Login } },
    { ScreenType.Profile, new() { ScreenType.Home } },
    { ScreenType.Friends, new() { ScreenType.Home, ScreenType.Profile } },
    { ScreenType.MatchHistory, new() { ScreenType.Home, ScreenType.MatchDetails } },
    { ScreenType.MatchDetails, new() { ScreenType.MatchHistory } }
};
```

---

## 🔐 Gestion de l'Auth & Tokens

### Stratégie de Stockage (Sécurisé)
```csharp
// Option 1: PlayerPrefs avec encryption simple (MVP)
public static class SecureStorage
{
    private static readonly string ENCRYPTION_KEY = SystemInfo.deviceUniqueIdentifier;
    
    public static void SaveToken(string key, string value)
    {
        string encrypted = Encrypt(value, ENCRYPTION_KEY);
        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.Save();
    }
    
    public static string GetToken(string key)
    {
        string encrypted = PlayerPrefs.GetString(key, null);
        return encrypted != null ? Decrypt(encrypted, ENCRYPTION_KEY) : null;
    }
}

// Option 2: In-Memory only (plus sécurisé mais perdu au restart)
public class TokenManager
{
    private static string _accessToken;
    private static string _refreshToken;
    
    // Jamais dans PlayerPrefs en prod
}
```

### Auto-Refresh des Tokens
```csharp
public async Task<string> GetValidAccessTokenAsync()
{
    // Si token encore valide (on garde l'expiry en mémoire)
    if (!IsTokenExpired(_accessToken))
        return _accessToken;
    
    // Sinon, refresh
    try
    {
        var response = await RefreshTokenAsync(_refreshToken);
        _accessToken = response.accessToken;
        SecureStorage.SaveToken("refresh_token", _refreshToken);
        return _accessToken;
    }
    catch (Exception)
    {
        // Token refresh a échoué → déconnecter l'utilisateur
        Logout();
        UIManager.Instance.ShowScreen(ScreenType.Login);
        throw new UnauthorizedException();
    }
}
```

---

## 📡 Gestion HTTP avec Retry & Timeout

### Configuration
```csharp
public class APIConfig : ScriptableObject
{
    public string BaseURL = "https://api.nexa.game/v1";
    public int TimeoutSeconds = 10;
    public int MaxRetries = 3;
    public float RetryDelaySeconds = 1f;
}
```

### APIService avec Retry Logic
```csharp
public async Task<T> SendRequestAsync<T>(
    string endpoint, 
    HttpMethod method, 
    object body = null,
    bool requiresAuth = true,
    int retryCount = 0
)
{
    string url = $"{_config.BaseURL}{endpoint}";
    
    using (UnityWebRequest request = CreateRequest(url, method, body))
    {
        // Ajouter le token si nécessaire
        if (requiresAuth)
        {
            string token = await AuthManager.Instance.GetValidAccessTokenAsync();
            request.SetRequestHeader("Authorization", $"Bearer {token}");
        }
        
        // Correlation ID pour traçabilité
        string correlationId = Guid.NewGuid().ToString();
        request.SetRequestHeader("X-Correlation-ID", correlationId);
        
        // Timeout
        request.timeout = _config.TimeoutSeconds;
        
        // Envoyer
        await request.SendWebRequest();
        
        // Gérer les erreurs
        if (request.result != UnityWebRequest.Result.Success)
        {
            // Retry sur erreur réseau/timeout
            if (ShouldRetry(request) && retryCount < _config.MaxRetries)
            {
                await Task.Delay((int)(_config.RetryDelaySeconds * 1000 * (retryCount + 1)));
                return await SendRequestAsync<T>(endpoint, method, body, requiresAuth, retryCount + 1);
            }
            
            // Gérer l'erreur
            HandleError(request, correlationId);
        }
        
        // Parser la réponse
        return ParseResponse<T>(request.downloadHandler.text);
    }
}

private bool ShouldRetry(UnityWebRequest request)
{
    return request.result == UnityWebRequest.Result.ConnectionError 
        || request.result == UnityWebRequest.Result.ProtocolError && request.responseCode >= 500;
}
```

---

## 💾 Cache Manager (Friends, Matches)

### Stratégie de Cache
```csharp
public class CacheManager : MonoBehaviour
{
    private Dictionary<string, CacheEntry> _cache = new();
    
    public class CacheEntry
    {
        public object Data;
        public DateTime ExpiresAt;
    }
    
    public void Set(string key, object data, int ttlSeconds = 300)
    {
        _cache[key] = new CacheEntry
        {
            Data = data,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds)
        };
    }
    
    public T Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpiresAt)
                return entry.Data as T;
            
            // Expiré, supprimer
            _cache.Remove(key);
        }
        return null;
    }
    
    public void Invalidate(string key)
    {
        _cache.Remove(key);
    }
    
    public void InvalidateAll()
    {
        _cache.Clear();
    }
}

// Usage
public async Task<List<Friend>> GetFriendsAsync(bool forceRefresh = false)
{
    const string CACHE_KEY = "friends_list";
    
    if (!forceRefresh)
    {
        var cached = CacheManager.Instance.Get<List<Friend>>(CACHE_KEY);
        if (cached != null)
            return cached;
    }
    
    var friends = await APIService.Instance.GetFriendsAsync();
    CacheManager.Instance.Set(CACHE_KEY, friends, ttlSeconds: 60); // 1 min
    return friends;
}
```

---

## 🎯 Gestion des Erreurs & Loading States

### Loading State Pattern
```csharp
public abstract class ScreenBase : MonoBehaviour
{
    [SerializeField] protected GameObject loadingPanel;
    [SerializeField] protected GameObject errorPanel;
    [SerializeField] protected TextMeshProUGUI errorText;
    
    protected async Task ExecuteWithLoadingAsync(Func<Task> action)
    {
        try
        {
            ShowLoading(true);
            HideError();
            
            await action();
        }
        catch (APIException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            ShowError("Une erreur est survenue. Veuillez réessayer.");
            Debug.LogError($"[{GetType().Name}] Error: {ex}");
        }
        finally
        {
            ShowLoading(false);
        }
    }
    
    protected void ShowLoading(bool show)
    {
        loadingPanel.SetActive(show);
    }
    
    protected void ShowError(string message)
    {
        errorText.text = message;
        errorPanel.SetActive(true);
        errorPanel.transform.DOShakePosition(0.3f, strength: 10);
    }
}
```

### Toast Notifications
```csharp
public enum ToastType { Success, Error, Info, Warning }

public static class ToastManager
{
    public static void Show(string message, ToastType type = ToastType.Info, float duration = 3f)
    {
        var toastPrefab = Resources.Load<GameObject>("UI/ToastNotification");
        var toast = Instantiate(toastPrefab, UIManager.Instance.ToastContainer);
        
        var toastComponent = toast.GetComponent<ToastNotification>();
        toastComponent.Setup(message, type, duration);
    }
}

// Usage
ToastManager.Show("Demande d'ami envoyée !", ToastType.Success);
ToastManager.Show("Erreur réseau", ToastType.Error);
```

---

## 🧩 Exemples de Scripts Complets

### UIManager.cs (Singleton, gestion des écrans)
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [SerializeField] private Transform screensContainer;
    [SerializeField] private CanvasGroup fadeOverlay;
    
    private Dictionary<ScreenType, ScreenBase> _screens = new();
    private ScreenType _currentScreen = ScreenType.None;
    private bool _isTransitioning = false;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Charger tous les écrans
        LoadAllScreens();
    }
    
    private void LoadAllScreens()
    {
        foreach (Transform child in screensContainer)
        {
            var screen = child.GetComponent<ScreenBase>();
            if (screen != null)
            {
                _screens[screen.ScreenType] = screen;
                screen.gameObject.SetActive(false);
            }
        }
    }
    
    public async void ShowScreen(ScreenType screenType, object data = null)
    {
        if (_isTransitioning) return;
        if (_currentScreen == screenType) return;
        
        // Vérifier si transition autorisée
        if (!IsTransitionAllowed(_currentScreen, screenType))
        {
            Debug.LogWarning($"Transition {_currentScreen} → {screenType} not allowed");
            return;
        }
        
        _isTransitioning = true;
        
        // Fade out current
        if (_currentScreen != ScreenType.None && _screens.ContainsKey(_currentScreen))
        {
            await _screens[_currentScreen].HideAsync();
        }
        
        // Fade overlay
        await FadeOverlayAsync(true);
        
        // Switch screens
        _currentScreen = screenType;
        var newScreen = _screens[screenType];
        
        // Fade overlay out
        await FadeOverlayAsync(false);
        
        // Fade in new
        await newScreen.ShowAsync(data);
        
        _isTransitioning = false;
    }
    
    private async Task FadeOverlayAsync(bool fadeIn)
    {
        float target = fadeIn ? 1f : 0f;
        await fadeOverlay.DOFade(target, AnimationTimings.FAST).AsyncWaitForCompletion();
    }
}
```

Continuons avec les snippets...


