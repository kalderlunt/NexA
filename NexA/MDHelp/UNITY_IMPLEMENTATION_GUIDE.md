# 🎮 NexA Client Unity - Guide d'Implémentation Complet

> **Version:** 1.0.0  
> **Date:** 2026-01-07  
> **Stack:** Unity 2022+ | DOTween | TextMeshPro | Newtonsoft.Json  
> **Architecture:** Services + Screens + Components Pattern

---

## 📋 Table des Matières

1. [Structures UI Unity - Référence Complète](#-structures-ui-unity---référence-complète)
2. [Vue d'Ensemble de l'Architecture](#vue-densemble-de-larchitecture)
3. [Structure du Projet](#structure-du-projet)
4. [Patterns et Conventions](#patterns-et-conventions)
5. [Guide d'Implémentation par Module](#guide-dimplémentation-par-module)
6. [Design System & Animations](#design-system--animations)
7. [Gestion des Erreurs](#gestion-des-erreurs)
8. [Bonnes Pratiques](#bonnes-pratiques)
9. [FAQ et Troubleshooting](#faq-et-troubleshooting)

---

## 🎨 Structures UI Unity - Référence Complète

Cette section présente toutes les structures UI à créer dans Unity pour chaque écran et composant.

### 🏗️ UIManager Structure

```
UIManager (GameObject + UIManager.cs) [DontDestroyOnLoad]
├── ScreensContainer (RectTransform)
│   ├── LoginScreen (GameObject + LoginScreen.cs)
│   ├── RegisterScreen (GameObject + RegisterScreen.cs)
│   ├── HomeScreen (GameObject + HomeScreen.cs)
│   ├── ProfileScreen (GameObject + ProfileScreen.cs)
│   ├── FriendsScreen (GameObject + FriendsScreen.cs)
│   ├── MatchHistoryScreen (GameObject + MatchHistoryScreen.cs)
│   └── MatchDetailsScreen (GameObject + MatchDetailsScreen.cs)
├── FadeOverlay (Image) [Black, full screen, alpha 0 by default]
└── ToastContainer (RectTransform) [Anchored top center]
```

### 🔐 LoginScreen Structure

```
LoginScreen (Canvas)
├── Background (Image)
├── Logo (Image + RectTransform)
├── FormPanel (CanvasGroup)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── LoginButton (Button)
│   └── RegisterButton (Button)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 📝 RegisterScreen Structure

```
RegisterScreen (Canvas)
├── Background (Image)
├── Logo (Image + RectTransform)
├── FormPanel (CanvasGroup)
│   ├── UsernameInputField (TMP_InputField)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── ConfirmPasswordInputField (TMP_InputField)
│   ├── PasswordStrengthIndicator (Image)
│   ├── RegisterButton (Button)
│   └── BackToLoginButton (Button)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 🏠 HomeScreen Structure

```
HomeScreen (Canvas)
├── Background (Image + Blur Effect)
├── TopBar (Panel)
│   ├── UserInfoPanel
│   │   ├── Avatar (Image + Circular Mask)
│   │   ├── UsernameText (TextMeshProUGUI)
│   │   ├── LevelText (TextMeshProUGUI)
│   │   └── EloText (TextMeshProUGUI)
│   ├── CurrencyPanel
│   │   ├── CoinsIcon (Image)
│   │   └── CoinsText (TextMeshProUGUI)
│   └── SettingsButton (Button)
├── MainPanel (CanvasGroup)
│   ├── PlayButton (Button + Hover Effect)
│   │   ├── Icon (Image)
│   │   └── Text (TextMeshProUGUI)
│   ├── ProfileButton (Button + Hover Effect)
│   ├── FriendsButton (Button + Hover Effect)
│   │   └── NotificationBadge (GameObject + TextMeshProUGUI)
│   ├── MatchHistoryButton (Button + Hover Effect)
│   └── StoreButton (Button + Hover Effect)
├── NewsPanel (ScrollView)
│   └── NewsItem (Prefab instantié)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 👤 ProfileScreen Structure

```
ProfileScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   └── EditButton (Button)
├── ProfilePanel (ScrollView)
│   ├── AvatarSection (Panel)
│   │   ├── AvatarImage (Image + Circular Mask)
│   │   ├── AvatarFrame (Image)
│   │   └── EditAvatarButton (Button)
│   ├── InfoSection (Panel)
│   │   ├── UsernameText (TextMeshProUGUI)
│   │   ├── BioText (TextMeshProUGUI)
│   │   ├── LevelProgressBar (Slider)
│   │   └── LevelText (TextMeshProUGUI)
│   ├── StatsSection (Panel + Grid Layout)
│   │   ├── StatItem (Prefab) - Matches Joués
│   │   ├── StatItem (Prefab) - Victoires
│   │   ├── StatItem (Prefab) - Défaites
│   │   ├── StatItem (Prefab) - Winrate
│   │   ├── StatItem (Prefab) - KDA
│   │   └── StatItem (Prefab) - Temps de jeu
│   └── BadgesSection (Panel)
│       └── BadgeItem (Prefab instantié)
├── EditPanel (GameObject + CanvasGroup) [Hidden by default]
│   ├── UsernameInputField (TMP_InputField)
│   ├── BioInputField (TMP_InputField - Multiline)
│   ├── SaveButton (Button)
│   └── CancelButton (Button)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 👥 FriendsScreen Structure

```
FriendsScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI)
│   └── SearchPanel
│       ├── SearchInputField (TMP_InputField)
│       └── SearchButton (Button)
├── TabsPanel (Horizontal Layout Group)
│   ├── FriendsTab (Button + Toggle)
│   │   ├── Icon (Image)
│   │   ├── Text (TextMeshProUGUI)
│   │   └── CountBadge (TextMeshProUGUI)
│   ├── RequestsTab (Button + Toggle)
│   │   ├── Icon (Image)
│   │   ├── Text (TextMeshProUGUI)
│   │   └── CountBadge (TextMeshProUGUI)
│   └── OnlineTab (Button + Toggle)
│       ├── Icon (Image)
│       ├── Text (TextMeshProUGUI)
│       └── CountBadge (TextMeshProUGUI)
├── ContentPanel (ScrollView + Scroll Rect)
│   └── Container (Vertical Layout Group)
│       └── FriendListItem (Prefab instantié)
├── SearchResultsPanel (GameObject + ScrollView) [Hidden by default]
│   ├── ResultsHeader (TextMeshProUGUI)
│   └── ResultsContainer (Vertical Layout Group)
│       └── UserSearchItem (Prefab instantié)
├── EmptyStatePanel (GameObject) [Shown when no friends]
│   ├── Icon (Image)
│   └── MessageText (TextMeshProUGUI)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 📜 MatchHistoryScreen Structure

```
MatchHistoryScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI)
│   └── RefreshButton (Button)
├── FiltersPanel (Horizontal Layout Group)
│   ├── AllButton (Button + Toggle)
│   ├── VictoriesButton (Button + Toggle)
│   └── DefeatButton (Button + Toggle)
├── StatsPanel (Horizontal Layout Group)
│   ├── TotalMatchesText (TextMeshProUGUI)
│   ├── WinrateText (TextMeshProUGUI)
│   └── RecentFormIndicator (5x Image icons)
├── ContentPanel (ScrollView + Scroll Rect)
│   └── Container (Vertical Layout Group)
│       └── MatchListItem (Prefab instantié)
├── LoadMoreButton (Button)
├── EmptyStatePanel (GameObject) [Shown when no matches]
│   ├── Icon (Image)
│   └── MessageText (TextMeshProUGUI)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 🔍 MatchDetailsScreen Structure

```
MatchDetailsScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI) [Match Details]
│   └── ShareButton (Button)
├── MatchOverviewPanel (Panel)
│   ├── ResultBanner (Image)
│   │   └── ResultText (TextMeshProUGUI) [VICTOIRE/DÉFAITE]
│   ├── GameInfoPanel (Horizontal Layout Group)
│   │   ├── GameModeText (TextMeshProUGUI)
│   │   ├── DateText (TextMeshProUGUI)
│   │   └── DurationText (TextMeshProUGUI)
│   └── FinalScoreText (TextMeshProUGUI) [Team 1: 25 - Team 2: 18]
├── PlayerStatsPanel (Panel)
│   ├── SectionTitle (TextMeshProUGUI) [Vos Statistiques]
│   ├── PlayerAvatar (Image + Circular Mask)
│   ├── PlayerNameText (TextMeshProUGUI)
│   └── StatsGrid (Grid Layout Group)
│       ├── StatCard (Prefab) - Kills
│       ├── StatCard (Prefab) - Deaths
│       ├── StatCard (Prefab) - Assists
│       ├── StatCard (Prefab) - KDA
│       ├── StatCard (Prefab) - Damage
│       └── StatCard (Prefab) - Gold
├── TabsPanel (Horizontal Layout Group)
│   ├── ScoreboardTab (Button + Toggle)
│   ├── TimelineTab (Button + Toggle)
│   └── GraphTab (Button + Toggle)
├── ContentPanel (ScrollView)
│   ├── ScoreboardContent (GameObject)
│   │   ├── Team1Panel (Vertical Layout Group)
│   │   │   ├── TeamHeader (Panel)
│   │   │   │   ├── TeamNameText (TextMeshProUGUI)
│   │   │   │   └── TeamScoreText (TextMeshProUGUI)
│   │   │   └── PlayerRow (Prefab instantié x5)
│   │   └── Team2Panel (Vertical Layout Group)
│   │       ├── TeamHeader (Panel)
│   │       └── PlayerRow (Prefab instantié x5)
│   ├── TimelineContent (GameObject) [Hidden by default]
│   │   └── TimelineEventItem (Prefab instantié)
│   └── GraphContent (GameObject) [Hidden by default]
│       └── GoldGraphImage (Image) [Chart/Graph]
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

### 🧩 Prefabs - Components Réutilisables

#### ToastNotification Prefab

```
ToastNotification (RectTransform + CanvasGroup + ToastNotification.cs)
├── Background (Image)
├── Icon (Image)
└── MessageText (TextMeshProUGUI)
```

**Path:** `Assets/Resources/UI/ToastNotification.prefab`

#### LoadingSpinner Prefab

```
LoadingSpinner (GameObject + LoadingSpinner.cs)
├── Background (Image) [Semi-transparent dark overlay]
├── SpinnerContainer (RectTransform)
│   ├── SpinnerIcon (Image) [Loading icon/circle]
│   └── LoadingText (TextMeshProUGUI) [Optional: "Chargement..."]
```

#### FriendListItem Prefab

```
FriendListItem (Panel + Layout Element + FriendListItem.cs)
├── Background (Image)
├── AvatarImage (Image + Circular Mask)
├── StatusIndicator (Image) [Green/Yellow/Gray dot]
├── InfoPanel (Vertical Layout Group)
│   ├── UsernameText (TextMeshProUGUI)
│   └── StatusText (TextMeshProUGUI) [Online/In Game/Offline]
├── ActionButton (Button)
│   └── ActionText (TextMeshProUGUI) [Remove/Accept/Decline]
└── MenuButton (Button) [3 dots icon]
```

#### UserSearchItem Prefab

```
UserSearchItem (Panel + Layout Element)
├── Background (Image)
├── AvatarImage (Image + Circular Mask)
├── InfoPanel (Vertical Layout Group)
│   ├── UsernameText (TextMeshProUGUI)
│   └── LevelText (TextMeshProUGUI)
└── AddButton (Button)
    └── Icon (Image) [+ icon]
```

#### MatchListItem Prefab

```
MatchListItem (Panel + Button + Layout Element + MatchListItem.cs)
├── Background (Image) [Green tint for victory, Red for defeat]
├── ResultPanel (Vertical Layout Group)
│   ├── ResultIcon (Image) [Victory/Defeat icon]
│   ├── ResultText (TextMeshProUGUI) [VICTOIRE/DÉFAITE]
│   └── DateText (TextMeshProUGUI) [Il y a 2h, Hier, 3 jours...]
├── ChampionPanel (Vertical Layout Group)
│   ├── ChampionIcon (Image + Circular Mask)
│   └── ChampionNameText (TextMeshProUGUI)
├── GameInfoPanel (Vertical Layout Group)
│   ├── GameModeText (TextMeshProUGUI) [Ranked Solo, Normal, ARAM]
│   ├── DurationText (TextMeshProUGUI) [32:45]
│   ├── ScoreText (TextMeshProUGUI) [15/3/8 - Kills/Deaths/Assists]
│   └── TeamScoreText (TextMeshProUGUI) [Team 1: 25 - Team 2: 18]
├── StatsPanel (Grid Layout Group)
│   ├── KDABadge (Panel)
│   │   ├── KDAValueText (TextMeshProUGUI) [8.67]
│   │   └── KDALabelText (TextMeshProUGUI) [KDA]
│   ├── CSBadge (Panel)
│   │   ├── CSValueText (TextMeshProUGUI) [184]
│   │   └── CSLabelText (TextMeshProUGUI) [CS]
│   └── DamageBadge (Panel)
│       ├── DamageValueText (TextMeshProUGUI) [32.5k]
│       └── DamageLabelText (TextMeshProUGUI) [DMG]
├── ItemsPanel (Horizontal Layout Group)
│   └── ItemIcon (Image x6) [Player's items]
└── ArrowIcon (Image) [> chevron icon]
```

#### PlayerRow Prefab (Scoreboard)

```
PlayerRow (Panel + Horizontal Layout Group)
├── RankText (TextMeshProUGUI) [#1]
├── ChampionIcon (Image + Circular Mask)
├── PlayerNameText (TextMeshProUGUI)
├── KDAText (TextMeshProUGUI) [12/3/8]
├── DamageText (TextMeshProUGUI) [25.4k]
├── GoldText (TextMeshProUGUI) [12.3k]
└── ItemsPanel (Horizontal Layout Group)
    └── ItemIcon (Image x6)
```

#### TimelineEventItem Prefab

```
TimelineEventItem (Panel + Horizontal Layout Group)
├── TimeText (TextMeshProUGUI) [15:32]
├── EventIcon (Image) [Kill/Tower/Dragon icon]
├── EventDescriptionText (TextMeshProUGUI)
└── Connector (Image) [Vertical line]
```

#### StatCard Prefab

```
StatCard (Panel)
├── Background (Image)
├── ValueText (TextMeshProUGUI) [25]
├── LabelText (TextMeshProUGUI) [Kills]
└── Icon (Image)
```

### 📋 Checklist de Création UI

#### ✅ Setup Initial
- [ ] Créer le GameObject UIManager avec tous les écrans enfants
- [ ] Ajouter TextMeshPro au projet (Package Manager)
- [ ] Importer DOTween (Asset Store ou Package Manager)
- [ ] Créer le dossier `Assets/Resources/UI/` pour les prefabs

#### ✅ Écrans Principaux
- [ ] LoginScreen avec tous les champs
- [ ] RegisterScreen avec validation password
- [ ] HomeScreen avec menu principal
- [ ] ProfileScreen avec stats
- [ ] FriendsScreen avec tabs
- [ ] MatchHistoryScreen avec scroll
- [ ] MatchDetailsScreen avec scoreboard

#### ✅ Prefabs Réutilisables
- [ ] ToastNotification dans Resources/UI/
- [ ] LoadingSpinner
- [ ] FriendListItem
- [ ] UserSearchItem
- [ ] MatchListItem
- [ ] PlayerRow
- [ ] TimelineEventItem
- [ ] StatCard

#### ✅ Configuration
- [ ] Assigner toutes les références dans l'Inspector
- [ ] Configurer les Layout Groups (spacing, padding)
- [ ] Configurer les Canvas Scaler (Scale With Screen Size)
- [ ] Ajouter les Event Triggers sur les boutons
- [ ] Configurer les Scroll Views (Vertical, Elastic)

---

### Principes Fondamentaux

Le client NexA utilise une **architecture en couches** avec séparation claire des responsabilités :

```
┌─────────────────────────────────────────────┐
│           UI Layer (Screens)                │  ← Présentation & Animations
├─────────────────────────────────────────────┤
│         Components Layer                    │  ← UI réutilisables
├─────────────────────────────────────────────┤
│         Services Layer                      │  ← Logique métier & API
├─────────────────────────────────────────────┤
│         Models Layer                        │  ← DTOs & Data structures
├─────────────────────────────────────────────┤
│         Utils Layer                         │  ← Helpers & Extensions
└─────────────────────────────────────────────┘
```

### Flow de Données

```
User Action (UI) 
    ↓
Screen (LoginScreen, etc.)
    ↓
Service (AuthManager, APIService)
    ↓
Backend API (Node.js)
    ↓
Models (User, Friend, Match)
    ↓
Cache (CacheManager)
    ↓
UI Update + Animations (DOTween)
```

---

## 📁 Structure du Projet

```
Assets/Hub/Script/
├── Core/                  # Systèmes centraux
│   ├── UIManager.cs       # Navigation & transitions
│   └── ScreenBase.cs      # Classe de base des écrans
│
├── Screens/               # Écrans du client
│   ├── LoginScreen.cs
│   ├── RegisterScreen.cs
│   ├── HomeScreen.cs
│   ├── ProfileScreen.cs
│   ├── FriendsScreen.cs
│   ├── MatchHistoryScreen.cs
│   └── MatchDetailsScreen.cs
│
├── Services/              # Logique métier
│   ├── APIService.cs      # HTTP client centralisé
│   ├── AuthManager.cs     # Auth & tokens
│   ├── FriendsManager.cs  # Gestion amis
│   ├── MatchesManager.cs  # Historique parties
│   └── CacheManager.cs    # Cache en mémoire
│
├── Components/            # UI réutilisables
│   ├── ToastNotification.cs
│   ├── LoadingSpinner.cs
│   ├── FriendListItem.cs
│   └── MatchListItem.cs
│
├── Models/                # Data models (DTOs)
│   ├── User.cs
│   ├── Friend.cs
│   ├── Match.cs
│   ├── APIResponse.cs
│   ├── APIError.cs
│   └── Enums.cs
│
└── Utils/                 # Utilitaires
    ├── AnimationHelper.cs # Constantes animations
    ├── SecureStorage.cs   # Stockage tokens
    └── CoroutineRunner.cs # Helper async
```

---

## 🎯 Patterns et Conventions

### 1. Singleton Services

Tous les services sont des **Singletons MonoBehaviour persistants** :

```csharp
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

**⚠️ Important :** 
- Un seul singleton par type
- `DontDestroyOnLoad` pour persistance entre scènes
- Initialisation dans `Awake()`

### 2. Screen Pattern

Chaque écran hérite de `ScreenBase` et implémente :

```csharp
public abstract class ScreenBase : MonoBehaviour
{
    public abstract ScreenType ScreenType { get; }
    public abstract Task ShowAsync(object data = null);
    public abstract Task HideAsync();
    
    protected async Task ExecuteWithLoadingAsync(Func<Task> action);
    protected virtual void ShowLoading(bool show);
    protected virtual void ShowError(string message);
    protected virtual void HideError();
}
```

**Exemple d'implémentation :**

```csharp
public class LoginScreen : ScreenBase
{
    public override ScreenType ScreenType => ScreenType.Login;
    
    public override async Task ShowAsync(object data = null)
    {
        gameObject.SetActive(true);
        // Animations d'entrée
        await AnimateIn();
    }
    
    public override async Task HideAsync()
    {
        // Animations de sortie
        await AnimateOut();
        gameObject.SetActive(false);
    }
}
```

### 3. Navigation avec UIManager

```csharp
// Transition simple
UIManager.Instance.ShowScreen(ScreenType.Home);

// Transition avec données
UIManager.Instance.ShowScreen(ScreenType.MatchDetails, matchId);

// Retour arrière
UIManager.Instance.GoBack();
```

**Transitions autorisées** (State Machine) :

```csharp
Login → Register
Login → Home (après auth)
Home → Profile | Friends | MatchHistory
MatchHistory → MatchDetails
```

### 4. API Calls Pattern

**Toujours utiliser `ExecuteWithLoadingAsync`** pour gérer automatiquement :
- Loading UI
- Gestion d'erreurs
- Retry automatique
- Logs

```csharp
private async void OnLoginClicked()
{
    await ExecuteWithLoadingAsync(async () =>
    {
        var user = await AuthManager.Instance.LoginAsync(email, password);
        ToastManager.Show($"Bienvenue, {user.username}!", ToastType.Success);
        UIManager.Instance.ShowScreen(ScreenType.Home);
    });
}
```

### 5. DTOs (Data Transfer Objects)

**Règle :** 1 DTO = 1 réponse API

```csharp
// Requête
public class LoginRequest
{
    public string email;
    public string password;
}

// Réponse
[Serializable]
public class AuthResponse
{
    public User user;
    public TokenData tokens;
}
```

**⚠️ Important :** Utiliser `[Serializable]` pour Unity et des champs publics (pas de propriétés) pour Newtonsoft.Json.

---

## 🔧 Guide d'Implémentation par Module

### Module 1️⃣ : Core (UIManager + ScreenBase)

#### UIManager.cs

**Rôle :** Gérer la navigation entre écrans avec transitions animées.

**Structure UI Unity :**

```
UIManager (GameObject + DontDestroyOnLoad)
├── ScreensContainer (RectTransform)
│   ├── LoginScreen (GameObject + LoginScreen.cs)
│   ├── RegisterScreen (GameObject + RegisterScreen.cs)
│   ├── HomeScreen (GameObject + HomeScreen.cs)
│   ├── ProfileScreen (GameObject + ProfileScreen.cs)
│   ├── FriendsScreen (GameObject + FriendsScreen.cs)
│   ├── MatchHistoryScreen (GameObject + MatchHistoryScreen.cs)
│   └── MatchDetailsScreen (GameObject + MatchDetailsScreen.cs)
├── FadeOverlay (Image) [Black, full screen, alpha 0 by default]
└── ToastContainer (RectTransform) [Anchored top center]
```

**Fonctionnalités clés :**
- State machine de navigation
- Transitions avec fade overlay
- Historique de navigation (back stack)
- Container pour les toasts

**Code clé :**

```csharp
public async void ShowScreen(ScreenType screenType, object data = null)
{
    if (_isTransitioning) return;
    
    if (!IsTransitionAllowed(_currentScreen, screenType))
    {
        Debug.LogWarning($"Transition {_currentScreen} → {screenType} non autorisée");
        return;
    }
    
    _isTransitioning = true;
    
    // Fade overlay
    await FadeOverlayAsync(true);
    
    // Cacher écran actuel
    if (_currentScreen != ScreenType.None)
    {
        await _screens[_currentScreen].HideAsync();
    }
    
    // Afficher nouvel écran
    await _screens[screenType].ShowAsync(data);
    
    await FadeOverlayAsync(false);
    
    _currentScreen = screenType;
    _isTransitioning = false;
}
```

#### ScreenBase.cs

**Méthodes protégées utiles :**

```csharp
protected async Task ExecuteWithLoadingAsync(Func<Task> action)
{
    try
    {
        ShowLoading(true);
        await action();
    }
    catch (AuthException ex)
    {
        ShowError(ex.Message);
        Debug.LogError($"Auth error: {ex.Message}");
    }
    catch (APIException ex)
    {
        ShowError(ex.Message);
        Debug.LogError($"API error: {ex.Code} - {ex.Message}");
    }
    catch (Exception ex)
    {
        ShowError("Une erreur est survenue.");
        Debug.LogError($"Unexpected error: {ex}");
    }
    finally
    {
        ShowLoading(false);
    }
}
```

---

### Module 2️⃣ : Services (APIService + AuthManager)

#### APIService.cs

**Rôle :** Client HTTP centralisé pour toutes les requêtes API.

**Features :**
- Gestion automatique des tokens (Authorization header)
- Retry avec exponential backoff
- Timeout configurable
- Correlation IDs
- Parsing JSON automatique

**Méthode générique :**

```csharp
private async Task<T> SendRequestAsync<T>(
    string endpoint, 
    string method, 
    object body = null, 
    bool requiresAuth = true)
{
    string url = baseURL + endpoint;
    string correlationId = Guid.NewGuid().ToString();
    
    using (UnityWebRequest request = CreateRequest(url, method, body))
    {
        // Headers
        request.SetRequestHeader("X-Correlation-ID", correlationId);
        
        if (requiresAuth)
        {
            string token = await AuthManager.Instance.GetValidAccessTokenAsync();
            request.SetRequestHeader("Authorization", $"Bearer {token}");
        }
        
        // Timeout
        request.timeout = timeoutSeconds;
        
        // Send
        await request.SendWebRequest();
        
        // Handle response
        if (request.result == UnityWebRequest.Result.Success)
        {
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }
        else
        {
            HandleError(request, correlationId);
            throw new APIException("REQUEST_FAILED", "La requête a échoué");
        }
    }
}
```

**Endpoints disponibles :**

| Méthode | Endpoint | Paramètres | Retour |
|---------|----------|------------|--------|
| `RegisterAsync()` | POST /auth/register | username, email, password | AuthResponse |
| `LoginAsync()` | POST /auth/login | email, password | AuthResponse |
| `RefreshTokenAsync()` | POST /auth/refresh | refreshToken | RefreshResponse |
| `GetCurrentUserAsync()` | GET /me | - | User |
| `SearchUsersAsync()` | GET /users/search | query | List\<User\> |
| `GetFriendsAsync()` | GET /friends | - | List\<Friend\> |
| `SendFriendRequestAsync()` | POST /friends/request | targetUserId | FriendRequest |
| `AcceptFriendRequestAsync()` | POST /friends/accept | requestId | FriendshipResponse |
| `GetMatchesAsync()` | GET /matches | limit, cursor | PaginatedResponse\<Match\> |

#### AuthManager.cs

**Rôle :** Gérer l'authentification et les tokens JWT.

**Features :**
- Login / Register
- Auto-refresh des access tokens (avant expiration)
- Stockage sécurisé des refresh tokens
- CurrentUser accessible globalement

**Flow d'authentification :**

```
Login/Register
    ↓
Recevoir tokens (access + refresh)
    ↓
Stocker en mémoire (_accessToken)
    ↓
Optionnel: Persister refresh token (SecureStorage)
    ↓
Auto-refresh avant expiration
```

**Code important :**

```csharp
public async Task<string> GetValidAccessTokenAsync()
{
    // Token valide ? (avec marge de 30s)
    if (!string.IsNullOrEmpty(_accessToken) && 
        DateTime.UtcNow < _tokenExpiresAt.AddSeconds(-30))
    {
        return _accessToken;
    }
    
    // Sinon, refresh
    if (!string.IsNullOrEmpty(_refreshToken))
    {
        await RefreshAccessTokenAsync();
        return _accessToken;
    }
    
    throw new AuthException("Session expirée. Reconnectez-vous.");
}

private async Task RefreshAccessTokenAsync()
{
    var response = await APIService.Instance.RefreshTokenAsync(_refreshToken);
    _accessToken = response.accessToken;
    _tokenExpiresAt = DateTime.UtcNow.AddSeconds(response.expiresIn);
}
```

#### FriendsManager.cs

**Rôle :** Cache et synchronisation de la liste d'amis.

**Features :**
- Cache en mémoire
- Méthodes pour add/remove/accept/decline
- Observable pattern (events pour UI updates)

```csharp
public event Action OnFriendsUpdated;

public async Task LoadFriendsAsync()
{
    _friends = await APIService.Instance.GetFriendsAsync();
    _friendRequests = await APIService.Instance.GetFriendRequestsAsync();
    
    OnFriendsUpdated?.Invoke();
}

public List<Friend> GetOnlineFriends()
{
    return _friends.Where(f => f.status == "online").ToList();
}
```

#### CacheManager.cs

**Rôle :** Cache générique en mémoire avec TTL.

```csharp
public void Set<T>(string key, T value, float ttlSeconds = 300f)
{
    _cache[key] = new CacheEntry
    {
        Data = value,
        ExpiresAt = Time.time + ttlSeconds
    };
}

public bool TryGet<T>(string key, out T value)
{
    if (_cache.TryGetValue(key, out var entry) && 
        Time.time < entry.ExpiresAt)
    {
        value = (T)entry.Data;
        return true;
    }
    
    value = default;
    return false;
}
```

---

### Module 3️⃣ : Screens (Implémentation UI)

#### LoginScreen.cs

**Responsabilités :**
- Formulaire de login (email + password)
- Validation des inputs
- Animations d'entrée/sortie
- Transition vers Register ou Home

**Structure UI Unity :**

```
LoginScreen (Canvas)
├── Background (Image)
├── Logo (Image + RectTransform)
├── FormPanel (CanvasGroup)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── LoginButton (Button)
│   └── RegisterButton (Button)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Code clé :**

```csharp
public override async Task ShowAsync(object data = null)
{
    gameObject.SetActive(true);
    HideError();
    
    // Animation logo bounce
    logoTransform.localScale = Vector3.zero;
    logoTransform.DOScale(1f, AnimationHelper.MEDIUM)
        .SetEase(AnimationHelper.IN_BACK);
    
    // Form fade in
    await formCanvasGroup.DOFade(1f, AnimationHelper.NORMAL)
        .SetEase(AnimationHelper.IN_SMOOTH)
        .From(0)
        .AsyncWaitForCompletion();
    
    formCanvasGroup.interactable = true;
}

private async void OnLoginClicked()
{
    await ExecuteWithLoadingAsync(async () =>
    {
        var user = await AuthManager.Instance.LoginAsync(
            emailInput.text.Trim(), 
            passwordInput.text
        );
        
        ToastManager.Show($"Bienvenue, {user.username}!", ToastType.Success);
        UIManager.Instance.ShowScreen(ScreenType.Home);
    });
}

private void ValidateInputs()
{
    bool isValid = 
        !string.IsNullOrWhiteSpace(emailInput.text) &&
        emailInput.text.Contains("@") &&
        passwordInput.text.Length >= 6;
    
    loginButton.interactable = isValid;
}
```

#### RegisterScreen.cs

**Responsabilités :**
- Formulaire d'inscription (username, email, password, confirm password)
- Validation des inputs
- Vérification de la force du mot de passe
- Transition vers Login ou Home

**Structure UI Unity :**

```
RegisterScreen (Canvas)
├── Background (Image)
├── Logo (Image + RectTransform)
├── FormPanel (CanvasGroup)
│   ├── UsernameInputField (TMP_InputField)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── ConfirmPasswordInputField (TMP_InputField)
│   ├── PasswordStrengthIndicator (Image)
│   ├── RegisterButton (Button)
│   └── BackToLoginButton (Button)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Code clé :**

```csharp
private void ValidateInputs()
{
    bool isValid = 
        !string.IsNullOrWhiteSpace(usernameInput.text) &&
        usernameInput.text.Length >= 3 &&
        !string.IsNullOrWhiteSpace(emailInput.text) &&
        emailInput.text.Contains("@") &&
        passwordInput.text.Length >= 6 &&
        passwordInput.text == confirmPasswordInput.text;
    
    registerButton.interactable = isValid;
    UpdatePasswordStrength();
}

private void UpdatePasswordStrength()
{
    int strength = CalculatePasswordStrength(passwordInput.text);
    passwordStrengthIndicator.fillAmount = strength / 4f;
    passwordStrengthIndicator.color = GetStrengthColor(strength);
}
```

---

#### HomeScreen.cs

**Responsabilités :**
- Menu principal du client
- Boutons d'accès aux différentes sections
- Affichage des informations utilisateur (niveau, elo)
- Statut en ligne / statut personnalisé

**Structure UI Unity :**

```
HomeScreen (Canvas)
├── Background (Image + Blur Effect)
├── TopBar (Panel)
│   ├── UserInfoPanel
│   │   ├── Avatar (Image + Circular Mask)
│   │   ├── UsernameText (TextMeshProUGUI)
│   │   ├── LevelText (TextMeshProUGUI)
│   │   └── EloText (TextMeshProUGUI)
│   ├── CurrencyPanel
│   │   ├── CoinsIcon (Image)
│   │   └── CoinsText (TextMeshProUGUI)
│   └── SettingsButton (Button)
├── MainPanel (CanvasGroup)
│   ├── PlayButton (Button + Hover Effect)
│   │   ├── Icon (Image)
│   │   └── Text (TextMeshProUGUI)
│   ├── ProfileButton (Button + Hover Effect)
│   ├── FriendsButton (Button + Hover Effect)
│   │   └── NotificationBadge (GameObject + TextMeshProUGUI)
│   ├── MatchHistoryButton (Button + Hover Effect)
│   └── StoreButton (Button + Hover Effect)
├── NewsPanel (ScrollView)
│   └── NewsItem (Prefab instantié)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Code clé :**

```csharp
public override async Task ShowAsync(object data = null)
{
    gameObject.SetActive(true);
    await LoadUserDataAsync();
    
    // Animation des boutons avec délai
    float delay = 0f;
    foreach (var button in menuButtons)
    {
        button.transform.localScale = Vector3.zero;
        button.transform.DOScale(1f, AnimationHelper.MEDIUM)
            .SetEase(AnimationHelper.IN_BACK)
            .SetDelay(delay);
        delay += 0.1f;
    }
    
    // Update notification badge
    UpdateFriendRequestsBadge();
}

private async Task LoadUserDataAsync()
{
    await ExecuteWithLoadingAsync(async () =>
    {
        var user = await AuthManager.Instance.GetCurrentUserAsync();
        usernameText.text = user.username;
        levelText.text = $"Niveau {user.level}";
        eloText.text = $"{user.elo} ELO";
    });
}
```

---

#### ProfileScreen.cs

**Responsabilités :**
- Affichage du profil utilisateur
- Statistiques du joueur
- Édition du profil (avatar, pseudo, bio)
- Historique de progression

**Structure UI Unity :**

```
ProfileScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   └── EditButton (Button)
├── ProfilePanel (ScrollView)
│   ├── AvatarSection (Panel)
│   │   ├── AvatarImage (Image + Circular Mask)
│   │   ├── AvatarFrame (Image)
│   │   └── EditAvatarButton (Button)
│   ├── InfoSection (Panel)
│   │   ├── UsernameText (TextMeshProUGUI)
│   │   ├── BioText (TextMeshProUGUI)
│   │   ├── LevelProgressBar (Slider)
│   │   └── LevelText (TextMeshProUGUI)
│   ├── StatsSection (Panel + Grid Layout)
│   │   ├── StatItem (Prefab) - Matches Joués
│   │   ├── StatItem (Prefab) - Victoires
│   │   ├── StatItem (Prefab) - Défaites
│   │   ├── StatItem (Prefab) - Winrate
│   │   ├── StatItem (Prefab) - KDA
│   │   └── StatItem (Prefab) - Temps de jeu
│   └── BadgesSection (Panel)
│       └── BadgeItem (Prefab instantié)
├── EditPanel (GameObject + CanvasGroup) [Hidden by default]
│   ├── UsernameInputField (TMP_InputField)
│   ├── BioInputField (TMP_InputField - Multiline)
│   ├── SaveButton (Button)
│   └── CancelButton (Button)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Code clé :**

```csharp
private async Task LoadProfileAsync()
{
    await ExecuteWithLoadingAsync(async () =>
    {
        var profile = await ProfileManager.Instance.GetProfileAsync(userId);
        PopulateProfileUI(profile);
        PopulateStatsUI(profile.stats);
    });
}

private void PopulateStatsUI(PlayerStats stats)
{
    statItems[0].SetValue("Matches", stats.matchesPlayed.ToString());
    statItems[1].SetValue("Victoires", stats.wins.ToString());
    statItems[2].SetValue("Défaites", stats.losses.ToString());
    
    float winrate = stats.matchesPlayed > 0 
        ? (float)stats.wins / stats.matchesPlayed * 100 
        : 0;
    statItems[3].SetValue("Winrate", $"{winrate:F1}%");
}
```

---

#### FriendsScreen.cs

**Responsabilités :**
- Liste d'amis avec statut (online/offline/in-game)
- Demandes d'amis pendantes
- Recherche d'utilisateurs
- Actions : add/accept/decline/remove

**Structure UI Unity :**

```
FriendsScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI)
│   └── SearchPanel
│       ├── SearchInputField (TMP_InputField)
│       └── SearchButton (Button)
├── TabsPanel (Horizontal Layout Group)
│   ├── FriendsTab (Button + Toggle)
│   │   ├── Icon (Image)
│   │   ├── Text (TextMeshProUGUI)
│   │   └── CountBadge (TextMeshProUGUI)
│   ├── RequestsTab (Button + Toggle)
│   │   ├── Icon (Image)
│   │   ├── Text (TextMeshProUGUI)
│   │   └── CountBadge (TextMeshProUGUI)
│   └── OnlineTab (Button + Toggle)
│       ├── Icon (Image)
│       ├── Text (TextMeshProUGUI)
│       └── CountBadge (TextMeshProUGUI)
├── ContentPanel (ScrollView + Scroll Rect)
│   └── Container (Vertical Layout Group)
│       └── FriendListItem (Prefab instantié)
├── SearchResultsPanel (GameObject + ScrollView) [Hidden by default]
│   ├── ResultsHeader (TextMeshProUGUI)
│   └── ResultsContainer (Vertical Layout Group)
│       └── UserSearchItem (Prefab instantié)
├── EmptyStatePanel (GameObject) [Shown when no friends]
│   ├── Icon (Image)
│   └── MessageText (TextMeshProUGUI)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Prefab: FriendListItem**

```
FriendListItem (Panel + Layout Element)
├── Background (Image)
├── AvatarImage (Image + Circular Mask)
├── StatusIndicator (Image) [Green/Yellow/Gray dot]
├── InfoPanel (Vertical Layout Group)
│   ├── UsernameText (TextMeshProUGUI)
│   └── StatusText (TextMeshProUGUI) [Online/In Game/Offline]
├── ActionButton (Button)
│   └── ActionText (TextMeshProUGUI) [Remove/Accept/Decline]
└── MenuButton (Button) [3 dots icon]
```

**Prefab: UserSearchItem**

```
UserSearchItem (Panel + Layout Element)
├── Background (Image)
├── AvatarImage (Image + Circular Mask)
├── InfoPanel (Vertical Layout Group)
│   ├── UsernameText (TextMeshProUGUI)
│   └── LevelText (TextMeshProUGUI)
└── AddButton (Button)
    └── Icon (Image) [+ icon]
```

**Pattern de liste réutilisable :**

```csharp
private void PopulateFriendsList()
{
    // Clear
    foreach (Transform child in friendsListContainer)
    {
        Destroy(child.gameObject);
    }
    
    // Populate
    var friends = FriendsManager.Instance.GetFriends();
    foreach (var friend in friends)
    {
        var item = Instantiate(friendListItemPrefab, friendsListContainer);
        item.GetComponent<FriendListItem>().Setup(friend, OnRemoveFriend);
    }
}

private async void OnRemoveFriend(string friendId)
{
    await ExecuteWithLoadingAsync(async () =>
    {
        await FriendsManager.Instance.RemoveFriendAsync(friendId);
        PopulateFriendsList();
        ToastManager.Show("Ami supprimé", ToastType.Info);
    });
}
```

#### MatchHistoryScreen.cs

**Responsabilités :**
- Liste paginée des matchs
- Infinite scroll (load more)
- Filtres (victoires/défaites)
- Clic sur un match → MatchDetailsScreen

**Structure UI Unity :**

```
MatchHistoryScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI)
│   └── RefreshButton (Button)
├── FiltersPanel (Horizontal Layout Group)
│   ├── AllButton (Button + Toggle)
│   ├── VictoriesButton (Button + Toggle)
│   └── DefeatButton (Button + Toggle)
├── StatsPanel (Horizontal Layout Group)
│   ├── TotalMatchesText (TextMeshProUGUI)
│   ├── WinrateText (TextMeshProUGUI)
│   └── RecentFormIndicator (5x Image icons)
├── ContentPanel (ScrollView + Scroll Rect)
│   └── Container (Vertical Layout Group)
│       └── MatchListItem (Prefab instantié)
├── LoadMoreButton (Button)
├── EmptyStatePanel (GameObject) [Shown when no matches]
│   ├── Icon (Image)
│   └── MessageText (TextMeshProUGUI)
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Prefab: MatchListItem**

```
MatchListItem (Panel + Layout Element + Button)
├── Background (Image) [Green tint for victory, Red for defeat]
├── ResultPanel (Vertical Layout Group)
│   ├── ResultText (TextMeshProUGUI) [VICTOIRE/DÉFAITE]
│   └── DateText (TextMeshProUGUI) [Il y a 2 heures]
├── GameInfoPanel (Vertical Layout Group)
│   ├── GameModeText (TextMeshProUGUI) [Ranked Solo]
│   ├── DurationText (TextMeshProUGUI) [32:45]
│   └── ScoreText (TextMeshProUGUI) [15/3/8]
├── ChampionIcon (Image + Circular Mask)
├── StatsPanel (Horizontal Layout Group)
│   ├── KDAText (TextMeshProUGUI) [8.67 KDA]
│   └── CSText (TextMeshProUGUI) [184 CS]
└── ArrowIcon (Image) [>]
```

**Pagination cursor-based :**

```csharp
private string _currentCursor = null;
private bool _hasMore = true;

private async Task LoadMatchesAsync(bool loadMore = false)
{
    if (!loadMore)
    {
        _currentCursor = null;
        ClearMatchList();
    }
    
    var response = await MatchesManager.Instance.GetMatchesAsync(
        limit: 20, 
        cursor: _currentCursor
    );
    
    PopulateMatchList(response.data);
    
    _currentCursor = response.meta?.nextCursor;
    _hasMore = !string.IsNullOrEmpty(_currentCursor);
    
    loadMoreButton.gameObject.SetActive(_hasMore);
}

private void OnScrollValueChanged(Vector2 scrollPos)
{
    // Infinite scroll: charger plus si proche du bas
    if (scrollPos.y < 0.1f && _hasMore && !_isLoading)
    {
        LoadMatchesAsync(loadMore: true);
    }
}
```

---

#### MatchDetailsScreen.cs

**Responsabilités :**
- Détails complets d'un match
- Liste des participants avec stats
- Timeline des événements
- Graph de progression

**Structure UI Unity :**

```
MatchDetailsScreen (Canvas)
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TextMeshProUGUI) [Match Details]
│   └── ShareButton (Button)
├── MatchOverviewPanel (Panel)
│   ├── ResultBanner (Image)
│   │   └── ResultText (TextMeshProUGUI) [VICTOIRE/DÉFAITE]
│   ├── GameInfoPanel (Horizontal Layout Group)
│   │   ├── GameModeText (TextMeshProUGUI)
│   │   ├── DateText (TextMeshProUGUI)
│   │   └── DurationText (TextMeshProUGUI)
│   └── FinalScoreText (TextMeshProUGUI) [Team 1: 25 - Team 2: 18]
├── PlayerStatsPanel (Panel)
│   ├── SectionTitle (TextMeshProUGUI) [Vos Statistiques]
│   ├── PlayerAvatar (Image + Circular Mask)
│   ├── PlayerNameText (TextMeshProUGUI)
│   └── StatsGrid (Grid Layout Group)
│       ├── StatCard (Prefab) - Kills
│       ├── StatCard (Prefab) - Deaths
│       ├── StatCard (Prefab) - Assists
│       ├── StatCard (Prefab) - KDA
│       ├── StatCard (Prefab) - Damage
│       └── StatCard (Prefab) - Gold
├── TabsPanel (Horizontal Layout Group)
│   ├── ScoreboardTab (Button + Toggle)
│   ├── TimelineTab (Button + Toggle)
│   └── GraphTab (Button + Toggle)
├── ContentPanel (ScrollView)
│   ├── ScoreboardContent (GameObject)
│   │   ├── Team1Panel (Vertical Layout Group)
│   │   │   ├── TeamHeader (Panel)
│   │   │   │   ├── TeamNameText (TextMeshProUGUI)
│   │   │   │   └── TeamScoreText (TextMeshProUGUI)
│   │   │   └── PlayerRow (Prefab instantié x5)
│   │   └── Team2Panel (Vertical Layout Group)
│   │       ├── TeamHeader (Panel)
│   │       └── PlayerRow (Prefab instantié x5)
│   ├── TimelineContent (GameObject) [Hidden by default]
│   │   └── TimelineEventItem (Prefab instantié)
│   └── GraphContent (GameObject) [Hidden by default]
│       └── GoldGraphImage (Image) [Chart/Graph]
├── LoadingPanel (GameObject + LoadingSpinner)
└── ErrorPanel (GameObject + TextMeshProUGUI)
```

**Prefab: PlayerRow (Scoreboard)**

```
PlayerRow (Panel + Horizontal Layout Group)
├── RankText (TextMeshProUGUI) [#1]
├── ChampionIcon (Image + Circular Mask)
├── PlayerNameText (TextMeshProUGUI)
├── KDAText (TextMeshProUGUI) [12/3/8]
├── DamageText (TextMeshProUGUI) [25.4k]
├── GoldText (TextMeshProUGUI) [12.3k]
└── ItemsPanel (Horizontal Layout Group)
    └── ItemIcon (Image x6)
```

**Prefab: TimelineEventItem**

```
TimelineEventItem (Panel + Horizontal Layout Group)
├── TimeText (TextMeshProUGUI) [15:32]
├── EventIcon (Image) [Kill/Tower/Dragon icon]
├── EventDescriptionText (TextMeshProUGUI)
└── Connector (Image) [Vertical line]
```

**Prefab: StatCard**

```
StatCard (Panel)
├── Background (Image)
├── ValueText (TextMeshProUGUI) [25]
├── LabelText (TextMeshProUGUI) [Kills]
└── Icon (Image)
```

**Code clé :**

```csharp
public override async Task ShowAsync(object data = null)
{
    if (data is string matchId)
    {
        await LoadMatchDetailsAsync(matchId);
    }
}

private async Task LoadMatchDetailsAsync(string matchId)
{
    await ExecuteWithLoadingAsync(async () =>
    {
        var match = await MatchesManager.Instance.GetMatchDetailsAsync(matchId);
        PopulateMatchOverview(match);
        PopulatePlayerStats(match);
        PopulateScoreboard(match.participants);
        PopulateTimeline(match.timeline);
    });
}

private void PopulateScoreboard(List<MatchParticipant> participants)
{
    var team1 = participants.Where(p => p.team == 1).OrderByDescending(p => p.kills);
    var team2 = participants.Where(p => p.team == 2).OrderByDescending(p => p.kills);
    
    foreach (var player in team1)
    {
        var row = Instantiate(playerRowPrefab, team1Container);
        row.GetComponent<PlayerRowUI>().Setup(player);
    }
    
    foreach (var player in team2)
    {
        var row = Instantiate(playerRowPrefab, team2Container);
        row.GetComponent<PlayerRowUI>().Setup(player);
    }
}
```

#### MatchListItem.cs (Component)

**Responsabilités :**
- Afficher un match dans la liste d'historique
- Indicateurs visuels (victoire/défaite)
- Stats du match (KDA, durée, score)
- Clickable pour ouvrir les détails

**Structure UI Unity Prefab :**

```
MatchListItem (Panel + Button + Layout Element)
├── Background (Image) [Green tint for victory, Red for defeat]
├── ResultPanel (Vertical Layout Group)
│   ├── ResultIcon (Image) [Victory/Defeat icon]
│   ├── ResultText (TextMeshProUGUI) [VICTOIRE/DÉFAITE]
│   └── DateText (TextMeshProUGUI) [Il y a 2h, Hier, 3 jours...]
├── ChampionPanel (Vertical Layout Group)
│   ├── ChampionIcon (Image + Circular Mask)
│   └── ChampionNameText (TextMeshProUGUI)
├── GameInfoPanel (Vertical Layout Group)
│   ├── GameModeText (TextMeshProUGUI) [Ranked Solo, Normal, ARAM]
│   ├── DurationText (TextMeshProUGUI) [32:45]
│   ├── ScoreText (TextMeshProUGUI) [15/3/8 - Kills/Deaths/Assists]
│   └── TeamScoreText (TextMeshProUGUI) [Team 1: 25 - Team 2: 18]
├── StatsPanel (Grid Layout Group)
│   ├── KDABadge (Panel)
│   │   ├── KDAValueText (TextMeshProUGUI) [8.67]
│   │   └── KDALabelText (TextMeshProUGUI) [KDA]
│   ├── CSBadge (Panel)
│   │   ├── CSValueText (TextMeshProUGUI) [184]
│   │   └── CSLabelText (TextMeshProUGUI) [CS]
│   └── DamageBadge (Panel)
│       ├── DamageValueText (TextMeshProUGUI) [32.5k]
│       └── DamageLabelText (TextMeshProUGUI) [DMG]
├── ItemsPanel (Horizontal Layout Group)
│   └── ItemIcon (Image x6) [Player's items]
└── ArrowIcon (Image) [> chevron icon]
```

**Code clé :**

```csharp
public class MatchListItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI kdaValueText;
    [SerializeField] private TextMeshProUGUI csValueText;
    [SerializeField] private TextMeshProUGUI damageValueText;
    [SerializeField] private Image[] itemIcons;
    
    [Header("Colors")]
    [SerializeField] private Color victoryColor = new Color(0.2f, 0.8f, 0.3f);
    [SerializeField] private Color defeatColor = new Color(0.8f, 0.2f, 0.2f);
    
    private string _matchId;
    
    public void Setup(Match match, Action<string> onClicked)
    {
        _matchId = match.id;
        
        // Result
        bool isVictory = match.playerResult == "victory";
        resultText.text = isVictory ? "VICTOIRE" : "DÉFAITE";
        resultText.color = isVictory ? victoryColor : defeatColor;
        
        // Date
        dateText.text = FormatRelativeTime(match.createdAt);
        
        // Game info
        gameModeText.text = match.gameMode;
        durationText.text = FormatDuration(match.duration);
        scoreText.text = $"{match.playerStats.kills}/{match.playerStats.deaths}/{match.playerStats.assists}";
        
        // Stats
        float kda = CalculateKDA(match.playerStats);
        kdaValueText.text = kda.ToString("F2");
        csValueText.text = match.playerStats.cs.ToString();
        
        // Click handler
        GetComponent<Button>().onClick.AddListener(() => onClicked?.Invoke(_matchId));
    }
    
    private float CalculateKDA(PlayerMatchStats stats)
    {
        if (stats.deaths == 0)
            return stats.kills + stats.assists;
        return (float)(stats.kills + stats.assists) / stats.deaths;
    }
    
    private string FormatDuration(int seconds)
    {
        int minutes = seconds / 60;
        int secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }
    
    private string FormatRelativeTime(DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date;
        
        if (timeSpan.TotalMinutes < 60)
            return $"Il y a {(int)timeSpan.TotalMinutes}min";
        if (timeSpan.TotalHours < 24)
            return $"Il y a {(int)timeSpan.TotalHours}h";
        if (timeSpan.TotalDays < 7)
            return $"Il y a {(int)timeSpan.TotalDays}j";
        
        return date.ToString("dd/MM/yyyy");
    }
}
```

---

### Module 4️⃣ : Components (UI Réutilisables)

#### ToastNotification.cs

**Usage :**

```csharp
ToastManager.Show("Message", ToastType.Success, duration: 3f);
ToastManager.Show("Erreur réseau", ToastType.Error);
```

**Types de toasts :**
- `ToastType.Info` (bleu)
- `ToastType.Success` (vert)
- `ToastType.Warning` (orange)
- `ToastType.Error` (rouge)

**Animation :**

```csharp
private void AnimateIn()
{
    // Position initiale: hors écran (bas)
    rectTransform.anchoredPosition = new Vector2(0, hiddenY);
    canvasGroup.alpha = 0;
    
    Sequence seq = DOTween.Sequence();
    seq.Append(rectTransform.DOAnchorPosY(visibleY, 0.3f).SetEase(Ease.OutBack));
    seq.Join(canvasGroup.DOFade(1f, 0.2f));
}

private void AnimateOut()
{
    Sequence seq = DOTween.Sequence();
    seq.Append(canvasGroup.DOFade(0f, 0.2f));
    seq.Join(rectTransform.DOAnchorPosY(hiddenY, 0.3f).SetEase(Ease.InBack));
    seq.OnComplete(() => Destroy(gameObject));
}
```

**Prefab structure :**

```
ToastNotification (RectTransform + CanvasGroup)
├── Background (Image)
├── Icon (Image)
└── MessageText (TextMeshProUGUI)
```

**⚠️ Important :** Le prefab doit être dans `Resources/UI/ToastNotification.prefab`.

#### LoadingSpinner.cs

**Structure UI Unity :**

```
LoadingSpinner (GameObject)
├── Background (Image) [Semi-transparent dark overlay]
├── SpinnerContainer (RectTransform)
│   ├── SpinnerIcon (Image) [Loading icon/circle]
│   └── LoadingText (TextMeshProUGUI) [Optional: "Chargement..."]
```

**Usage :**

```csharp
[SerializeField] private LoadingSpinner loadingSpinner;

protected override void ShowLoading(bool show)
{
    loadingSpinner.gameObject.SetActive(show);
    if (show) loadingSpinner.StartSpinning();
}
```

**Animation rotation infinie :**

```csharp
public void StartSpinning()
{
    _rotationTween?.Kill();
    _rotationTween = iconTransform
        .DORotate(new Vector3(0, 0, -360), rotationDuration, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear)
        .SetLoops(-1);
}
```

#### FriendListItem.cs

**Responsabilités :**
- Afficher un ami : avatar, username, statut
- Bouton d'action (Remove, Accept, Decline selon contexte)
- Indicateur de statut (online = vert, etc.)

**Code :**

```csharp
public void Setup(Friend friend, Action<string> onActionClicked)
{
    usernameText.text = friend.user.username;
    statusText.text = GetStatusLabel(friend.status);
    statusIndicator.color = GetStatusColor(friend.status);
    
    // Avatar (placeholder ou URL)
    if (!string.IsNullOrEmpty(friend.user.avatar))
    {
        StartCoroutine(LoadAvatarAsync(friend.user.avatar));
    }
    
    actionButton.onClick.AddListener(() => onActionClicked?.Invoke(friend.id));
}

private Color GetStatusColor(string status)
{
    return status switch
    {
        "online" => onlineColor,
        "in-game" => inGameColor,
        _ => offlineColor
    };
}
```

---

## 🎨 Design System & Animations

### Constantes d'Animation (AnimationHelper.cs)

```csharp
public static class AnimationHelper
{
    // Durées
    public const float FAST = 0.15f;
    public const float NORMAL = 0.25f;
    public const float MEDIUM = 0.4f;
    public const float SLOW = 0.6f;
    
    // Easings
    public static Ease IN_SMOOTH = Ease.OutCubic;
    public static Ease OUT_SMOOTH = Ease.InCubic;
    public static Ease IN_BACK = Ease.OutBack;
    public static Ease BOUNCE = Ease.OutBounce;
    public static Ease ELASTIC = Ease.OutElastic;
    
    // Scales
    public const float HOVER_SCALE = 1.05f;
    public const float PRESS_SCALE = 0.95f;
}
```

### Animations Réutilisables

#### 1. Fade In/Out

```csharp
public static async Task FadeInAsync(CanvasGroup group, float duration = AnimationHelper.NORMAL)
{
    await group.DOFade(1f, duration)
        .SetEase(AnimationHelper.IN_SMOOTH)
        .From(0)
        .AsyncWaitForCompletion();
}

public static async Task FadeOutAsync(CanvasGroup group, float duration = AnimationHelper.FAST)
{
    await group.DOFade(0f, duration)
        .SetEase(AnimationHelper.OUT_SMOOTH)
        .AsyncWaitForCompletion();
}
```

#### 2. Pop Animation (Panel Entry)

```csharp
public static void PopIn(RectTransform transform)
{
    transform.localScale = Vector3.zero;
    transform.DOScale(1f, AnimationHelper.MEDIUM)
        .SetEase(AnimationHelper.IN_BACK);
}
```

#### 3. Slide Animation

```csharp
public static async Task SlideInFromRight(RectTransform transform)
{
    float startX = Screen.width;
    transform.anchoredPosition = new Vector2(startX, 0);
    
    await transform.DOAnchorPosX(0, AnimationHelper.MEDIUM)
        .SetEase(AnimationHelper.OUT_SMOOTH)
        .AsyncWaitForCompletion();
}
```

#### 4. Hover Effect (Button)

```csharp
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform targetTransform;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetTransform.DOScale(AnimationHelper.HOVER_SCALE, 0.1f)
            .SetEase(Ease.OutCubic);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        targetTransform.DOScale(1f, 0.1f)
            .SetEase(Ease.OutCubic);
    }
}
```

### Skeleton UI (Loading State)

Pour un meilleur UX, afficher un "skeleton" pendant le chargement :

```csharp
public class SkeletonItem : MonoBehaviour
{
    [SerializeField] private Image[] skeletonImages;
    [SerializeField] private Color skeletonColor = new Color(0.8f, 0.8f, 0.8f);
    
    private void Start()
    {
        foreach (var img in skeletonImages)
        {
            img.color = skeletonColor;
            img.DOFade(0.3f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
```

---

## ⚠️ Gestion des Erreurs

### Hiérarchie des Exceptions

```csharp
// Exception de base API
public class APIException : Exception
{
    public string Code { get; }
    public APIException(string code, string message) : base(message)
    {
        Code = code;
    }
}

// Exception auth spécifique
public class AuthException : Exception
{
    public AuthException(string message, Exception inner = null) 
        : base(message, inner) { }
}
```

### Codes d'Erreur Backend

| Code | Signification | Action UI |
|------|---------------|-----------|
| `AUTH_INVALID_CREDENTIALS` | Email/password invalide | "Identifiants incorrects" |
| `AUTH_USER_EXISTS` | Email déjà utilisé | "Cet email est déjà utilisé" |
| `AUTH_TOKEN_EXPIRED` | Token expiré | Auto-refresh ou redirect login |
| `FRIEND_REQUEST_EXISTS` | Demande déjà envoyée | "Demande déjà envoyée" |
| `FRIEND_ALREADY_FRIENDS` | Déjà amis | "Vous êtes déjà amis" |
| `RATE_LIMIT_EXCEEDED` | Trop de requêtes | "Veuillez patienter..." |
| `INTERNAL_ERROR` | Erreur serveur | "Erreur serveur. Réessayez." |

### Messages d'Erreur Conviviaux

```csharp
private string GetFriendlyErrorMessage(string errorCode)
{
    return errorCode switch
    {
        "AUTH_INVALID_CREDENTIALS" => "Email ou mot de passe incorrect",
        "AUTH_USER_EXISTS" => "Cet email est déjà utilisé",
        "AUTH_TOKEN_EXPIRED" => "Votre session a expiré",
        "NETWORK_ERROR" => "Impossible de contacter le serveur",
        "FRIEND_REQUEST_EXISTS" => "Demande d'ami déjà envoyée",
        "FRIEND_ALREADY_FRIENDS" => "Vous êtes déjà amis",
        "RATE_LIMIT_EXCEEDED" => "Trop de requêtes. Veuillez patienter.",
        _ => "Une erreur est survenue. Veuillez réessayer."
    };
}
```

### Retry Logic

```csharp
private async Task<T> SendRequestWithRetryAsync<T>(Func<Task<T>> requestFunc)
{
    int attempts = 0;
    Exception lastException = null;
    
    while (attempts < maxRetries)
    {
        try
        {
            return await requestFunc();
        }
        catch (Exception ex) when (IsRetryableError(ex))
        {
            lastException = ex;
            attempts++;
            
            if (attempts < maxRetries)
            {
                float delay = retryDelaySeconds * Mathf.Pow(2, attempts - 1); // exponential backoff
                await Task.Delay((int)(delay * 1000));
            }
        }
    }
    
    throw lastException;
}

private bool IsRetryableError(Exception ex)
{
    // Retry sur erreurs réseau, pas sur erreurs client (4xx)
    if (ex is APIException apiEx)
    {
        return apiEx.Code == "NETWORK_ERROR" || apiEx.Code == "TIMEOUT";
    }
    return false;
}
```

---

## ✅ Bonnes Pratiques

### 1. Séparation Model / Service

**❌ Mauvais :**
```csharp
public class User : MonoBehaviour
{
    public async Task LoadProfile() { ... }
}
```

**✅ Bon :**
```csharp
// Model (data only)
[Serializable]
public class User
{
    public string id;
    public string username;
}

// Service (logic)
public class ProfileManager : MonoBehaviour
{
    public async Task<User> LoadProfileAsync(string userId) { ... }
}
```

### 2. Avoid Blocking Calls

**❌ Mauvais :**
```csharp
void Start()
{
    var user = AuthManager.Instance.LoginAsync(email, password).Result; // BLOQUE LE MAIN THREAD
}
```

**✅ Bon :**
```csharp
async void Start()
{
    var user = await AuthManager.Instance.LoginAsync(email, password);
}
```

### 3. Cleanup DOTween

```csharp
private Tween _currentTween;

private void OnDestroy()
{
    _currentTween?.Kill();
}

private void StartAnimation()
{
    _currentTween?.Kill();
    _currentTween = transform.DOScale(1.5f, 0.5f);
}
```

### 4. Cache Invalidation

```csharp
// Invalider le cache après une mutation
await FriendsManager.Instance.RemoveFriendAsync(friendId);
CacheManager.Instance.Remove("friends_list");
await FriendsManager.Instance.LoadFriendsAsync(); // Recharger depuis API
```

### 5. Null Checks

```csharp
public void UpdateUI()
{
    if (AuthManager.Instance?.CurrentUser == null)
    {
        Debug.LogWarning("CurrentUser is null, cannot update UI");
        return;
    }
    
    usernameText.text = AuthManager.Instance.CurrentUser.username;
}
```

### 6. Event Unsubscribe

```csharp
private void OnEnable()
{
    FriendsManager.Instance.OnFriendsUpdated += RefreshFriendsList;
}

private void OnDisable()
{
    FriendsManager.Instance.OnFriendsUpdated -= RefreshFriendsList;
}
```

---

## 🔍 FAQ et Troubleshooting

### Q: Quelle est la différence entre un Model et un DTO ?

**R:** Dans ce projet, ils sont identiques. Les **Models** (User, Friend, Match) sont des **DTOs** (Data Transfer Objects) qui représentent les réponses JSON de l'API. Ils ne contiennent que des données, pas de logique.

### Q: Pourquoi utiliser des `MonoBehaviour` pour les Services ?

**R:** Pour profiter de :
- `DontDestroyOnLoad` (persistence)
- Coroutines (bien que maintenant on utilise async/await)
- Lifecycle Unity (Awake, Start, OnDestroy)
- Singleton pattern simple

**Alternative :** Vous pouvez créer des services en "pure C#" (non-MonoBehaviour) mais il faudra gérer la lifetime manuellement.

### Q: Pourquoi séparer chaque Model dans un fichier ?

**✅ Recommandé :** Un fichier par classe pour :
- Meilleure lisibilité
- Meilleure organisation dans Unity Editor
- Faciliter la navigation (Ctrl+T dans Rider/VS)
- Éviter les conflits Git

**Acceptable pour de petits DTOs :**
```csharp
// Models/APIResponses.cs
namespace NexA.Hub.Models
{
    [Serializable] public class AuthResponse { ... }
    [Serializable] public class RefreshResponse { ... }
    [Serializable] public class FriendshipResponse { ... }
}
```

### Q: Erreur "NullReferenceException" sur `UIManager.Instance`

**Cause :** UIManager n'est pas initialisé.

**Solution :**
1. Créer un GameObject "UIManager" dans la scène
2. Attacher le script `UIManager.cs`
3. Assigner les références (screensContainer, fadeOverlay, toastContainer) dans l'Inspector
4. Optionnel : Créer un prefab "UIManager" dans Resources

### Q: Erreur "ToastManager prefab not found"

**Solution :**
1. Créer le prefab `ToastNotification.prefab`
2. Le placer dans `Assets/Resources/UI/ToastNotification.prefab`
3. Vérifier que le component `ToastNotification.cs` est attaché

### Q: Comment gérer la déconnexion réseau ?

```csharp
private async Task<T> SafeAPICallAsync<T>(Func<Task<T>> apiCall, T defaultValue = default)
{
    try
    {
        return await apiCall();
    }
    catch (APIException ex) when (ex.Code == "NETWORK_ERROR")
    {
        ToastManager.Show("Connexion perdue. Vérifiez votre réseau.", ToastType.Error);
        return defaultValue;
    }
}

// Usage
var friends = await SafeAPICallAsync(
    () => FriendsManager.Instance.LoadFriendsAsync(),
    new List<Friend>()
);
```

### Q: Comment tester sans backend ?

**Option 1 : Mock APIService**

```csharp
#if UNITY_EDITOR
public async Task<User> LoginAsync(string email, string password)
{
    // Mock delay
    await Task.Delay(1000);
    
    // Retourner des fausses données
    return new User
    {
        id = "test-123",
        username = "TestUser",
        email = email,
        level = 42,
        elo = 1500
    };
}
#endif
```

**Option 2 : JSON local**

```csharp
public async Task<List<Friend>> GetFriendsAsync()
{
    #if UNITY_EDITOR
    var jsonFile = Resources.Load<TextAsset>("MockData/friends");
    return JsonConvert.DeserializeObject<List<Friend>>(jsonFile.text);
    #else
    return await SendRequestAsync<List<Friend>>("/friends", "GET");
    #endif
}
```

---

## 📝 Checklist d'Implémentation

### ✅ Core Setup
- [ ] UIManager créé et configuré
- [ ] ScreenBase implémenté
- [ ] Tous les écrans héritent de ScreenBase
- [ ] Transitions testées

### ✅ Services
- [ ] APIService configuré (baseURL, timeout)
- [ ] AuthManager implémenté
- [ ] Tokens stockés et auto-refresh fonctionnel
- [ ] FriendsManager avec cache
- [ ] MatchesManager avec pagination

### ✅ Screens
- [ ] LoginScreen avec validation
- [ ] RegisterScreen
- [ ] HomeScreen (menu principal)
- [ ] ProfileScreen
- [ ] FriendsScreen (liste + recherche)
- [ ] MatchHistoryScreen (pagination)
- [ ] MatchDetailsScreen

### ✅ Components
- [ ] ToastNotification prefab créé
- [ ] LoadingSpinner
- [ ] FriendListItem
- [ ] MatchListItem

### ✅ Animations
- [ ] AnimationHelper constants définis
- [ ] Transitions écrans animées
- [ ] Hover effects sur boutons
- [ ] Toast animations

### ✅ Error Handling
- [ ] APIException / AuthException définis
- [ ] Messages d'erreur conviviaux
- [ ] Retry logic implémenté
- [ ] Network error handling

### ✅ Polish
- [ ] Skeleton UI pour loading states
- [ ] Smooth transitions partout
- [ ] Tooltips sur les boutons
- [ ] Confirmation dialogs (ex: supprimer ami)

---

## 🚀 Next Steps (Post-MVP)

### Phase 2 : Temps Réel
- [ ] WebSocket pour présence amis
- [ ] Notifications push (demandes d'amis)
- [ ] Chat temps réel

### Phase 3 : Optimisation
- [ ] Object pooling pour les listes
- [ ] Texture caching pour avatars
- [ ] Compression des requêtes API

### Phase 4 : Sécurité
- [ ] Certificat pinning (SSL)
- [ ] Obfuscation du code
- [ ] Anti-cheat basics

---

## 📚 Ressources

### Documentation
- [Unity Async/Await](https://docs.unity3d.com/Manual/overview-of-dot-net-in-unity.html)
- [DOTween Documentation](http://dotween.demigiant.com/documentation.php)
- [TextMeshPro Guide](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)

### Patterns
- [Unity Service Locator Pattern](https://unity.com/how-to/use-service-locator-pattern-inject-game-dependencies)
- [State Machine for UI](https://gameprogrammingpatterns.com/state.html)

### Best Practices
- [Unity Performance Tips](https://unity.com/how-to/optimize-garbage-collection-unity-games)
- [Clean Code in Unity](https://unity.com/how-to/unity-coding-best-practices)

---

**Version du Guide :** 1.0.0  
**Dernière Mise à Jour :** 2026-01-07  
**Auteur :** NexA Development Team  

---

> **Note :** Ce guide est un document vivant. N'hésitez pas à l'enrichir au fur et à mesure de l'évolution du projet.

