# 📄 NexA - Index Complet des Fichiers UI Unity

> **Liste exhaustive de tous les fichiers, prefabs et scripts nécessaires**  
> **Version:** 1.0.0  
> **Date:** 2026-01-09

---

## 📊 Vue d'Ensemble

### Statistiques
- **Total Screens :** 7
- **Total Prefabs :** 10
- **Total Scripts :** 25+
- **Total Models :** 8
- **Temps de création estimé :** 6-8 heures

---

## 🎨 Scènes Unity

### MainScene.unity

**Path:** `Assets/Hub/Scenes/MainScene.unity`

**Contenu :**
- UIManager (GameObject root)
- EventSystem (créé automatiquement par Unity)

**Hiérarchie complète :**
```
MainScene
├── UIManager (Canvas + UIManager.cs)
│   ├── ScreensContainer
│   │   ├── LoginScreen (LoginScreen.cs)
│   │   ├── RegisterScreen (RegisterScreen.cs)
│   │   ├── HomeScreen (HomeScreen.cs)
│   │   ├── ProfileScreen (ProfileScreen.cs)
│   │   ├── FriendsScreen (FriendsScreen.cs)
│   │   ├── MatchHistoryScreen (MatchHistoryScreen.cs)
│   │   └── MatchDetailsScreen (MatchDetailsScreen.cs)
│   ├── FadeOverlay (Image)
│   └── ToastContainer (RectTransform)
└── EventSystem
```

---

## 📱 Screens (7 fichiers)

### 1. LoginScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/LoginScreen`

**Structure UI :**
```
LoginScreen
├── Background (Image)
├── Logo (Image)
├── FormPanel (CanvasGroup)
│   ├── TitleText (TMP)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── LoginButton (Button)
│   └── RegisterButton (Button)
├── LoadingPanel (GameObject)
│   └── LoadingSpinner (Prefab)
└── ErrorPanel (GameObject)
    ├── ErrorIcon (Image)
    └── ErrorText (TMP)
```

**Script Associé :** `Assets/Hub/Scripts/Screens/LoginScreen.cs`

**Composants Requis :**
- CanvasGroup (root)
- RectTransform (stretch both)

---

### 2. RegisterScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/RegisterScreen`

**Structure UI :**
```
RegisterScreen
├── Background (Image)
├── Logo (Image)
├── FormPanel (CanvasGroup)
│   ├── TitleText (TMP) "Inscription"
│   ├── UsernameInputField (TMP_InputField)
│   ├── EmailInputField (TMP_InputField)
│   ├── PasswordInputField (TMP_InputField)
│   ├── ConfirmPasswordInputField (TMP_InputField)
│   ├── PasswordStrengthIndicator (Image - Filled)
│   ├── RegisterButton (Button)
│   └── BackToLoginButton (Button)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject)
```

**Script Associé :** `Assets/Hub/Scripts/Screens/RegisterScreen.cs`

---

### 3. HomeScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/HomeScreen`

**Structure UI :**
```
HomeScreen
├── Background (Image)
├── TopBar (Panel)
│   ├── UserInfoPanel
│   │   ├── Avatar (Image + Mask)
│   │   ├── UsernameText (TMP)
│   │   ├── LevelText (TMP)
│   │   └── EloText (TMP)
│   ├── CurrencyPanel
│   │   ├── CoinsIcon (Image)
│   │   └── CoinsText (TMP)
│   └── SettingsButton (Button)
├── MainPanel (CanvasGroup + Grid Layout)
│   ├── PlayButton (Button)
│   ├── ProfileButton (Button)
│   ├── FriendsButton (Button)
│   │   └── NotificationBadge (Image + TMP)
│   ├── MatchHistoryButton (Button)
│   └── StoreButton (Button)
├── NewsPanel (ScrollView)
│   └── Content
│       └── NewsItem (Prefab - multiple)
├── LoadingPanel (GameObject)
└── ErrorPanel (GameObject)
```

**Script Associé :** `Assets/Hub/Scripts/Screens/HomeScreen.cs`

---

### 4. ProfileScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/ProfileScreen`

**Structure UI :**
```
ProfileScreen
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   └── EditButton (Button)
├── ProfilePanel (ScrollView)
│   ├── Viewport
│   │   └── Content (Vertical Layout)
│   │       ├── AvatarSection
│   │       │   ├── AvatarImage (Image + Mask)
│   │       │   ├── AvatarFrame (Image)
│   │       │   └── EditAvatarButton (Button)
│   │       ├── InfoSection
│   │       │   ├── UsernameText (TMP)
│   │       │   ├── BioText (TMP)
│   │       │   ├── LevelProgressBar (Slider)
│   │       │   └── LevelText (TMP)
│   │       ├── StatsSection (Grid Layout)
│   │       │   ├── StatItem (Prefab) x6
│   │       │   └── ...
│   │       └── BadgesSection
│   │           └── BadgeItem (Prefab) xN
│   └── Scrollbar (Vertical)
├── EditPanel (CanvasGroup - Hidden)
│   ├── UsernameInputField (TMP_InputField)
│   ├── BioInputField (TMP_InputField)
│   ├── SaveButton (Button)
│   └── CancelButton (Button)
├── LoadingPanel
└── ErrorPanel
```

**Script Associé :** `Assets/Hub/Scripts/Screens/ProfileScreen.cs`

---

### 5. FriendsScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/FriendsScreen`

**Structure UI :**
```
FriendsScreen
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TMP)
│   └── SearchPanel
│       ├── SearchInputField (TMP_InputField)
│       └── SearchButton (Button)
├── TabsPanel (Horizontal Layout + Toggle Group)
│   ├── FriendsTab (Button + Toggle)
│   │   ├── Icon (Image)
│   │   ├── Text (TMP)
│   │   └── CountBadge (TMP)
│   ├── RequestsTab (Button + Toggle)
│   └── OnlineTab (Button + Toggle)
├── ContentPanel (ScrollView)
│   ├── Viewport
│   │   └── Content (Vertical Layout)
│   │       └── FriendListItem (Prefab) xN
│   └── Scrollbar (Vertical)
├── SearchResultsPanel (ScrollView - Hidden)
│   └── Content
│       └── UserSearchItem (Prefab) xN
├── EmptyStatePanel (Hidden)
│   ├── Icon (Image)
│   └── MessageText (TMP)
├── LoadingPanel
└── ErrorPanel
```

**Script Associé :** `Assets/Hub/Scripts/Screens/FriendsScreen.cs`

---

### 6. MatchHistoryScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/MatchHistoryScreen`

**Structure UI :**
```
MatchHistoryScreen
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TMP)
│   └── RefreshButton (Button)
├── FiltersPanel (Horizontal Layout + Toggle Group)
│   ├── AllButton (Button + Toggle)
│   ├── VictoriesButton (Button + Toggle)
│   └── DefeatsButton (Button + Toggle)
├── StatsPanel
│   ├── TotalMatchesText (TMP)
│   ├── WinrateText (TMP)
│   └── RecentFormIndicator (5x Image)
├── ContentPanel (ScrollView)
│   └── Content
│       └── MatchListItem (Prefab) xN
├── LoadMoreButton (Button)
├── EmptyStatePanel
├── LoadingPanel
└── ErrorPanel
```

**Script Associé :** `Assets/Hub/Scripts/Screens/MatchHistoryScreen.cs`

---

### 7. MatchDetailsScreen

**GameObject dans Scène :** `UIManager/ScreensContainer/MatchDetailsScreen`

**Structure UI :**
```
MatchDetailsScreen
├── Background (Image)
├── HeaderPanel
│   ├── BackButton (Button)
│   ├── TitleText (TMP)
│   └── ShareButton (Button)
├── MatchOverviewPanel
│   ├── ResultBanner (Image)
│   │   └── ResultText (TMP)
│   ├── GameInfoPanel
│   │   ├── GameModeText (TMP)
│   │   ├── DateText (TMP)
│   │   └── DurationText (TMP)
│   └── FinalScoreText (TMP)
├── PlayerStatsPanel
│   ├── SectionTitle (TMP)
│   ├── PlayerAvatar (Image)
│   ├── PlayerNameText (TMP)
│   └── StatsGrid
│       └── StatCard (Prefab) x6
├── TabsPanel (Toggle Group)
│   ├── ScoreboardTab (Toggle)
│   ├── TimelineTab (Toggle)
│   └── GraphTab (Toggle)
├── ContentPanel (ScrollView)
│   ├── ScoreboardContent
│   │   ├── Team1Panel
│   │   │   ├── TeamHeader
│   │   │   └── PlayerRow (Prefab) x5
│   │   └── Team2Panel
│   │       └── PlayerRow (Prefab) x5
│   ├── TimelineContent (Hidden)
│   │   └── TimelineEventItem (Prefab) xN
│   └── GraphContent (Hidden)
│       └── GoldGraphImage
├── LoadingPanel
└── ErrorPanel
```

**Script Associé :** `Assets/Hub/Scripts/Screens/MatchDetailsScreen.cs`

---

## 🧩 Prefabs (10 fichiers)

### 1. ToastNotification.prefab

**Path:** `Assets/Resources/UI/ToastNotification.prefab`

**Structure :**
```
ToastNotification (RectTransform + CanvasGroup)
├── Background (Image)
├── Icon (Image - 40x40)
└── MessageText (TMP)
```

**Script :** `ToastNotification.cs`

**Usage :** `ToastManager.Show("Message", ToastType.Success);`

---

### 2. LoadingSpinner.prefab

**Path:** `Assets/Hub/Prefabs/UI/LoadingSpinner.prefab`

**Structure :**
```
LoadingSpinner
├── Background (Image - Semi-transparent)
└── SpinnerContainer
    ├── SpinnerIcon (Image - Rotating)
    └── LoadingText (TMP - Optional)
```

**Script :** `LoadingSpinner.cs`

---

### 3. FriendListItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/FriendListItem.prefab`

**Structure :**
```
FriendListItem (Panel + Horizontal Layout)
├── Background (Image)
├── AvatarImage (Image + Mask - 50x50)
├── StatusIndicator (Image - 12x12)
├── InfoPanel (Vertical Layout)
│   ├── UsernameText (TMP)
│   └── StatusText (TMP)
├── ActionButton (Button - 100px)
│   └── ActionText (TMP)
└── MenuButton (Button - 30x30)
```

**Script :** `FriendListItem.cs`

**Layout Element :** Min Height 80

---

### 4. UserSearchItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/UserSearchItem.prefab`

**Structure :**
```
UserSearchItem (Panel + Horizontal Layout)
├── Background (Image)
├── AvatarImage (Image + Mask)
├── InfoPanel (Vertical Layout)
│   ├── UsernameText (TMP)
│   └── LevelText (TMP)
└── AddButton (Button)
    └── Icon (Image - + icon)
```

**Script :** `UserSearchItem.cs`

---

### 5. MatchListItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/MatchListItem.prefab`

**Structure :**
```
MatchListItem (Panel + Button + Horizontal Layout)
├── Background (Image - Tinted)
├── ResultPanel (Vertical Layout - 100px)
│   ├── ResultIcon (Image)
│   ├── ResultText (TMP)
│   └── DateText (TMP)
├── ChampionPanel (80px)
│   ├── ChampionIcon (Image - 64x64)
│   └── ChampionNameText (TMP)
├── GameInfoPanel (Vertical Layout - Flexible)
│   ├── GameModeText (TMP)
│   ├── DurationText (TMP)
│   ├── ScoreText (TMP)
│   └── TeamScoreText (TMP)
├── StatsPanel (Grid Layout - 250px)
│   ├── KDABadge (Panel)
│   ├── CSBadge (Panel)
│   └── DamageBadge (Panel)
├── ItemsPanel (Horizontal Layout - 180px)
│   └── ItemIcon (Image - 24x24) x6
└── ArrowIcon (Image - 30px)
```

**Script :** `MatchListItem.cs`

**Layout Element :** Min Height 120

---

### 6. PlayerRow.prefab

**Path:** `Assets/Hub/Prefabs/UI/PlayerRow.prefab`

**Structure :**
```
PlayerRow (Panel + Horizontal Layout)
├── RankText (TMP - 40px)
├── ChampionIcon (Image - 40x40)
├── PlayerNameText (TMP - Flexible)
├── KDAText (TMP - 100px)
├── DamageText (TMP - 80px)
├── GoldText (TMP - 80px)
└── ItemsPanel (Horizontal Layout - 180px)
    └── ItemIcon (Image - 24x24) x6
```

**Script :** `PlayerRowUI.cs`

---

### 7. TimelineEventItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/TimelineEventItem.prefab`

**Structure :**
```
TimelineEventItem (Panel + Horizontal Layout)
├── TimeText (TMP - 60px)
├── EventIcon (Image - 40x40)
├── EventDescriptionText (TMP - Flexible)
└── Connector (Image - 2px width, vertical line)
```

**Script :** `TimelineEventItem.cs`

---

### 8. StatCard.prefab

**Path:** `Assets/Hub/Prefabs/UI/StatCard.prefab`

**Structure :**
```
StatCard (Panel + Vertical Layout)
├── Background (Image)
├── Icon (Image - 40x40)
├── ValueText (TMP - Large, Bold)
└── LabelText (TMP - Small)
```

**Script :** `StatCard.cs`

**Layout Element :** Preferred Width/Height auto

---

### 9. NewsItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/NewsItem.prefab`

**Structure :**
```
NewsItem (Panel + Vertical Layout)
├── ThumbnailImage (Image)
├── TitleText (TMP)
├── DescriptionText (TMP)
└── DateText (TMP - Small)
```

**Script :** `NewsItem.cs` (simple)

---

### 10. BadgeItem.prefab

**Path:** `Assets/Hub/Prefabs/UI/BadgeItem.prefab`

**Structure :**
```
BadgeItem (Panel)
├── BadgeIcon (Image - 64x64)
├── BadgeNameText (TMP)
└── BadgeDescriptionText (TMP)
```

**Script :** `BadgeItem.cs`

---

## 💻 Scripts C# (Catégorisés)

### Core Scripts (3)

| Fichier | Path | Description |
|---------|------|-------------|
| `UIManager.cs` | `Assets/Hub/Scripts/Core/` | Gestion navigation écrans |
| `ScreenBase.cs` | `Assets/Hub/Scripts/Core/` | Classe de base pour écrans |
| `ToastManager.cs` | `Assets/Hub/Scripts/Core/` | Singleton pour toasts |

---

### Screen Scripts (7)

| Fichier | Path | Description |
|---------|------|-------------|
| `LoginScreen.cs` | `Assets/Hub/Scripts/Screens/` | Écran de connexion |
| `RegisterScreen.cs` | `Assets/Hub/Scripts/Screens/` | Écran d'inscription |
| `HomeScreen.cs` | `Assets/Hub/Scripts/Screens/` | Menu principal |
| `ProfileScreen.cs` | `Assets/Hub/Scripts/Screens/` | Profil utilisateur |
| `FriendsScreen.cs` | `Assets/Hub/Scripts/Screens/` | Liste d'amis |
| `MatchHistoryScreen.cs` | `Assets/Hub/Scripts/Screens/` | Historique matchs |
| `MatchDetailsScreen.cs` | `Assets/Hub/Scripts/Screens/` | Détails match |

---

### Service Scripts (5)

| Fichier | Path | Description |
|---------|------|-------------|
| `APIService.cs` | `Assets/Hub/Scripts/Services/` | Client HTTP centralisé |
| `AuthManager.cs` | `Assets/Hub/Scripts/Services/` | Authentification + tokens |
| `FriendsManager.cs` | `Assets/Hub/Scripts/Services/` | Gestion amis + cache |
| `MatchesManager.cs` | `Assets/Hub/Scripts/Services/` | Gestion matchs + cache |
| `CacheManager.cs` | `Assets/Hub/Scripts/Services/` | Cache générique |

---

### Component Scripts (10)

| Fichier | Path | Description |
|---------|------|-------------|
| `ToastNotification.cs` | `Assets/Hub/Scripts/Components/` | Toast animé |
| `LoadingSpinner.cs` | `Assets/Hub/Scripts/Components/` | Spinner rotation |
| `FriendListItem.cs` | `Assets/Hub/Scripts/Components/` | Item liste amis |
| `UserSearchItem.cs` | `Assets/Hub/Scripts/Components/` | Item recherche user |
| `MatchListItem.cs` | `Assets/Hub/Scripts/Components/` | Item liste matchs |
| `PlayerRowUI.cs` | `Assets/Hub/Scripts/Components/` | Ligne scoreboard |
| `TimelineEventItem.cs` | `Assets/Hub/Scripts/Components/` | Event timeline |
| `StatCard.cs` | `Assets/Hub/Scripts/Components/` | Carte statistique |
| `NewsItem.cs` | `Assets/Hub/Scripts/Components/` | Item actualité |
| `BadgeItem.cs` | `Assets/Hub/Scripts/Components/` | Badge joueur |

---

### Model Scripts (8)

| Fichier | Path | Description |
|---------|------|-------------|
| `User.cs` | `Assets/Hub/Scripts/Models/` | Modèle utilisateur |
| `Friend.cs` | `Assets/Hub/Scripts/Models/` | Modèle ami |
| `Match.cs` | `Assets/Hub/Scripts/Models/` | Modèle match |
| `MatchParticipant.cs` | `Assets/Hub/Scripts/Models/` | Participant match |
| `APIResponse.cs` | `Assets/Hub/Scripts/Models/` | Réponse API générique |
| `APIError.cs` | `Assets/Hub/Scripts/Models/` | Erreur API |
| `Enums.cs` | `Assets/Hub/Scripts/Models/` | Enums (ScreenType, etc.) |
| `DTOs.cs` | `Assets/Hub/Scripts/Models/` | DTOs additionnels |

---

### Utility Scripts (5+)

| Fichier | Path | Description |
|---------|------|-------------|
| `AnimationHelper.cs` | `Assets/Hub/Scripts/Utils/` | Constantes animations |
| `SecureStorage.cs` | `Assets/Hub/Scripts/Utils/` | Stockage tokens |
| `CoroutineRunner.cs` | `Assets/Hub/Scripts/Utils/` | Runner coroutines |
| `Extensions.cs` | `Assets/Hub/Scripts/Utils/` | Extension methods |
| `ButtonHoverEffect.cs` | `Assets/Hub/Scripts/Utils/` | Hover effect réutilisable |

---

## 📦 Ressources & Assets

### Images Requises

**Icons :**
- Login icon (email, password)
- Menu icons (play, profile, friends, history, store)
- Status indicators (online, offline, in-game dots)
- Result icons (victory, defeat)
- Timeline event icons (kill, tower, dragon, etc.)

**Backgrounds :**
- Login background
- Home background
- Gradient overlays

**UI Elements :**
- Button backgrounds
- Panel backgrounds
- Avatar frames
- Badge frames

**Path recommandé :** `Assets/Hub/Art/UI/`

---

### Fonts (TextMeshPro)

**Fonts Recommandées :**
1. **Primary Font :** Roboto, Open Sans, ou Montserrat
2. **Secondary Font :** Roboto Condensed (pour stats)

**Import :**
- Window → TextMeshPro → Font Asset Creator
- Importer .ttf depuis Google Fonts

**Path :** `Assets/Hub/Fonts/`

---

## 🎨 Matériaux & Shaders

### Matériaux UI

| Nom | Usage | Properties |
|-----|-------|-----------|
| `UI-Default` | Standard UI | - |
| `UI-Rounded` | Boutons arrondis | Corner Radius |
| `UI-Blur` | Backgrounds | Blur Amount |
| `UI-Gradient` | Overlays | Gradient Colors |

**Path :** `Assets/Hub/Materials/UI/`

---

## 🔧 Configuration Files

### Project Settings

**Path :** `ProjectSettings/`

**Fichiers importants :**
- `QualitySettings.asset`
- `TagManager.asset`
- `InputManager.asset` (ou nouveau Input System)

### Packages

**Path :** `Packages/manifest.json`

**Dependencies requises :**
```json
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.demigiant.dotween": "1.2.745" // Si via UPM
  }
}
```

---

## 📊 Tableau Récapitulatif

### Par Catégorie

| Catégorie | Nombre | Path Principal |
|-----------|--------|----------------|
| Scènes | 1 | `Assets/Hub/Scenes/` |
| Screens (GameObjects) | 7 | Dans scène (ScreensContainer) |
| Prefabs UI | 10 | `Assets/Hub/Prefabs/UI/` |
| Scripts Core | 3 | `Assets/Hub/Scripts/Core/` |
| Scripts Screens | 7 | `Assets/Hub/Scripts/Screens/` |
| Scripts Services | 5 | `Assets/Hub/Scripts/Services/` |
| Scripts Components | 10 | `Assets/Hub/Scripts/Components/` |
| Scripts Models | 8 | `Assets/Hub/Scripts/Models/` |
| Scripts Utils | 5+ | `Assets/Hub/Scripts/Utils/` |
| **TOTAL** | **56+** | - |

---

## ✅ Checklist de Création

### Phase 1 : Setup (1h)
- [ ] Créer MainScene.unity
- [ ] Créer structure de dossiers complète
- [ ] Importer TextMeshPro + DOTween
- [ ] Configurer Canvas + UIManager GameObject

### Phase 2 : Screens (3h)
- [ ] LoginScreen (GameObject + Script)
- [ ] RegisterScreen
- [ ] HomeScreen
- [ ] ProfileScreen
- [ ] FriendsScreen
- [ ] MatchHistoryScreen
- [ ] MatchDetailsScreen

### Phase 3 : Prefabs (2h)
- [ ] ToastNotification.prefab (dans Resources/UI/)
- [ ] LoadingSpinner.prefab
- [ ] FriendListItem.prefab
- [ ] UserSearchItem.prefab
- [ ] MatchListItem.prefab
- [ ] PlayerRow.prefab
- [ ] TimelineEventItem.prefab
- [ ] StatCard.prefab
- [ ] NewsItem.prefab
- [ ] BadgeItem.prefab

### Phase 4 : Scripts Core (1h)
- [ ] UIManager.cs + Attacher + Assigner références
- [ ] ScreenBase.cs
- [ ] ToastManager.cs
- [ ] AnimationHelper.cs
- [ ] Enums.cs

### Phase 5 : Scripts Screens (2h)
- [ ] Tous les scripts de Screens créés
- [ ] Attachés aux GameObjects
- [ ] Références assignées dans Inspector
- [ ] Tests basiques (logs)

### Phase 6 : Services (3h)
- [ ] APIService.cs (avec UnityWebRequest)
- [ ] AuthManager.cs (avec JWT)
- [ ] FriendsManager.cs
- [ ] MatchesManager.cs
- [ ] CacheManager.cs

### Phase 7 : Models (1h)
- [ ] User.cs
- [ ] Friend.cs
- [ ] Match.cs + MatchParticipant.cs
- [ ] APIResponse.cs + APIError.cs
- [ ] DTOs additionnels

### Phase 8 : Components (2h)
- [ ] Scripts pour tous les prefabs
- [ ] Logique de Setup() dans chaque component
- [ ] Tests avec données mockées

### Phase 9 : Polish (2h)
- [ ] Animations DOTween sur tous les écrans
- [ ] Hover effects sur boutons
- [ ] Loading states
- [ ] Error handling UI

### Phase 10 : Integration (2h)
- [ ] Connexion backend réelle
- [ ] Tests end-to-end
- [ ] Bug fixes
- [ ] Optimisations

**Total estimé : 19h**

---

## 🔍 Recherche Rapide

### "Je dois créer quoi pour..."

**... l'écran de login ?**
→ GameObject LoginScreen + Script LoginScreen.cs + Prefab LoadingSpinner

**... afficher une liste d'amis ?**
→ FriendsScreen + Prefab FriendListItem + Script FriendsManager.cs

**... afficher une notification ?**
→ Prefab ToastNotification + Script ToastManager.cs

**... faire une requête API ?**
→ Script APIService.cs + AuthManager.cs

**... stocker des données ?**
→ Scripts Models (User.cs, Friend.cs, etc.) + CacheManager.cs

---

## 📚 Ressources Externes

### Assets Store (Optionnels)

**Recommandés :**
- Modern UI Pack (UI assets)
- Rounded Corners (shader UI)
- Loading Screen Studio (spinners)

### Fonts Gratuites

**Google Fonts :**
- Roboto
- Open Sans
- Montserrat
- Inter

**Download :** https://fonts.google.com/

---

**Version:** 1.0.0  
**Dernière MAJ:** 2026-01-09  
**Auteur:** NexA Dev Team

---

> 💡 **Tip:** Utilisez Ctrl+F pour rechercher rapidement un fichier spécifique dans ce document.

