# 🎨 NexA - Hiérarchie UI Unity - Référence Rapide

> **Guide visuel pour créer rapidement la structure UI dans Unity**  
> **Version:** 1.0.0  
> **Date:** 2026-01-09

---

## 🚀 Démarrage Rapide

### 1. Créer la Scène Principale

1. Créer une nouvelle scène : `Assets/Scenes/MainScene.unity`
2. Supprimer la MainCamera par défaut
3. Créer un Canvas (UI → Canvas)
4. Renommer en "UIManager"
5. Attacher le script `UIManager.cs`

### 2. Configuration du Canvas

**Canvas Component :**
- Render Mode: `Screen Space - Overlay`
- Canvas Scaler: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080`
  - Screen Match Mode: `Match Width Or Height`
  - Match: `0.5`

---

## 📱 Hiérarchie Complète UIManager

```
UIManager (Canvas + UIManager.cs)
│
├── 🖼️ ScreensContainer (Empty GameObject)
│   ├── LoginScreen
│   ├── RegisterScreen
│   ├── HomeScreen
│   ├── ProfileScreen
│   ├── FriendsScreen
│   ├── MatchHistoryScreen
│   └── MatchDetailsScreen
│
├── 🌫️ FadeOverlay (Image - Full Screen)
│
└── 🍞 ToastContainer (Empty GameObject)
```

**FadeOverlay Configuration :**
- Anchors: Stretch (0,0,1,1)
- Color: Black (#000000)
- Alpha: 0
- Raycast Target: Enabled

**ToastContainer Configuration :**
- Anchors: Top Center
- Pivot: (0.5, 1)
- Position: (0, -50, 0)

---

## 🔐 LoginScreen - Détails

### Hiérarchie Complète

```
LoginScreen (Full Canvas Size)
│
├── Background (Image)
│   └── Gradient ou Image de fond
│
├── Logo (Image)
│   └── Position: Top Center (-300 from top)
│
├── FormPanel (CanvasGroup)
│   ├── Title (TextMeshProUGUI) "Connexion"
│   │
│   ├── EmailInputField (TMP_InputField)
│   │   ├── Placeholder: "Email"
│   │   ├── Text Input
│   │   └── Icon (Email icon)
│   │
│   ├── PasswordInputField (TMP_InputField)
│   │   ├── Placeholder: "Mot de passe"
│   │   ├── Content Type: Password
│   │   └── Icon (Lock icon)
│   │
│   ├── Spacer (Empty - 20px height)
│   │
│   ├── LoginButton (Button)
│   │   └── Text: "Se connecter"
│   │
│   ├── Divider (Image - 1px height)
│   │
│   └── RegisterButton (Button - Outline)
│       └── Text: "Créer un compte"
│
├── LoadingPanel (GameObject - Hidden by default)
│   └── LoadingSpinner (Prefab)
│
└── ErrorPanel (GameObject - Hidden by default)
    ├── Background (Red tinted panel)
    ├── Icon (Warning icon)
    └── ErrorText (TextMeshProUGUI)
```

### Configuration des Components

**EmailInputField :**
- Character Limit: 100
- Content Type: Email Address
- Line Type: Single Line

**PasswordInputField :**
- Character Limit: 50
- Content Type: Password
- Line Type: Single Line

**LoginButton :**
- Size: 400 x 60
- Colors: Normal (#4A90E2), Highlighted (#5BA3F5), Pressed (#3A7AC8)
- Font Size: 18

---

## 📝 RegisterScreen - Détails

### Différences avec LoginScreen

```
RegisterScreen
├── FormPanel
│   ├── UsernameInputField (NEW)
│   ├── EmailInputField
│   ├── PasswordInputField
│   ├── ConfirmPasswordInputField (NEW)
│   ├── PasswordStrengthIndicator (NEW - Image Fill)
│   ├── RegisterButton
│   └── BackToLoginButton (Text Button)
```

**PasswordStrengthIndicator :**
- Type: Image (Filled)
- Fill Method: Horizontal
- Colors: Red (weak) → Yellow (medium) → Green (strong)

---

## 🏠 HomeScreen - Layout Principal

### Zones Principales

```
HomeScreen
│
├── 📊 TopBar (Height: 80px)
│   ├── UserInfoPanel (Left)
│   ├── CurrencyPanel (Center)
│   └── SettingsButton (Right)
│
├── 🎮 MainPanel (Center - 60% screen)
│   ├── PlayButton (Large - 500x150)
│   ├── ProfileButton
│   ├── FriendsButton (with Badge)
│   ├── MatchHistoryButton
│   └── StoreButton
│
└── 📰 NewsPanel (Right - 30% screen)
    └── ScrollView with NewsItems
```

### MainPanel - Grid Layout

**Grid Layout Group :**
- Cell Size: 240 x 80
- Spacing: 20 x 20
- Start Corner: Upper Left
- Start Axis: Horizontal
- Child Alignment: Middle Center

**PlayButton (Exception) :**
- Layout Element: Min Width 500, Min Height 150
- Flexible Width: 1

---

## 👥 FriendsScreen - Structure Détaillée

### Tabs System

```
TabsPanel (Horizontal Layout Group)
│
├── FriendsTab (Toggle + Button)
│   ├── Icon
│   ├── Text "Amis"
│   └── Badge (Circle with count)
│
├── RequestsTab
│   └── Badge (Pending requests count)
│
└── OnlineTab
    └── Badge (Online friends count)
```

**Tab Configuration :**
- Width: 150px
- Height: 50px
- Toggle Group: Assign same group to all tabs
- Is On (default): FriendsTab = true

### Content avec Scroll

```
ContentPanel (ScrollView)
│
├── Viewport
│   └── Content (Vertical Layout Group)
│       ├── FriendListItem (Prefab) x N
│       ├── FriendListItem
│       └── ...
│
└── Scrollbar (Vertical)
```

**Scroll Rect :**
- Horizontal: False
- Vertical: True
- Movement Type: Elastic
- Elasticity: 0.1
- Inertia: True
- Scroll Sensitivity: 20

---

## 📜 MatchHistoryScreen - Infinite Scroll

### Structure

```
MatchHistoryScreen
│
├── HeaderPanel (80px height)
│
├── FiltersPanel (60px height)
│   └── Horizontal Layout (All/Victories/Defeats)
│
├── StatsPanel (50px height)
│   └── Quick stats + Form indicator
│
├── ContentPanel (ScrollView - Flexible)
│   └── Content (Vertical Layout)
│       └── MatchListItem (Prefab) x N
│
└── LoadMoreButton (50px height)
```

### Infinite Scroll Implementation

**Scroll Rect - Add Script :**
```csharp
// Dans MatchHistoryScreen.cs
scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

private void OnScrollValueChanged(Vector2 scrollPos)
{
    if (scrollPos.y < 0.1f && _hasMore && !_isLoading)
    {
        LoadMatchesAsync(loadMore: true);
    }
}
```

---

## 🧩 Prefabs - Guide de Création

### ToastNotification

**Path:** `Assets/Resources/UI/ToastNotification.prefab`

**Setup Steps :**
1. Créer Empty GameObject "ToastNotification"
2. Add RectTransform (Width: 400, Height: 80)
3. Add CanvasGroup
4. Add ToastNotification.cs script
5. Ajouter enfants:
   - Background (Image - Rounded corners)
   - Icon (Image - 40x40)
   - MessageText (TMP - Flexible)

**Animation Setup :**
- Initial Position: Y = -100 (below screen)
- Visible Position: Y = -50
- Duration: 0.3s In, 0.2s Out
- Ease: OutBack (In), InBack (Out)

### FriendListItem

**Height:** 80px (Layout Element)

**Setup Steps :**
1. Créer Panel "FriendListItem"
2. Add Horizontal Layout Group
   - Padding: 10
   - Spacing: 10
   - Child Alignment: Middle Left
3. Ajouter enfants:
   - AvatarImage (50x50, Circle Mask)
   - StatusIndicator (12x12, absolute right of avatar)
   - InfoPanel (Vertical Layout)
   - ActionButton (100 width)
   - MenuButton (30x30)

**StatusIndicator Colors :**
- Online: #00FF00 (Green)
- In-Game: #FFAA00 (Orange)
- Offline: #888888 (Gray)

### MatchListItem

**Height:** 120px (Layout Element)

**Complex Layout :**
```
MatchListItem (Horizontal Layout)
│
├── ResultPanel (Width: 100)
│   ├── Icon (40x40)
│   ├── ResultText (Bold)
│   └── DateText (Small)
│
├── ChampionPanel (Width: 80)
│   └── ChampionIcon (64x64)
│
├── GameInfoPanel (Flexible)
│   └── Vertical Layout (4 texts)
│
├── StatsPanel (Width: 250)
│   └── Grid Layout (3 badges)
│
├── ItemsPanel (Width: 180)
│   └── Horizontal Layout (6 items, 24x24 each)
│
└── ArrowIcon (Width: 30)
```

---

## 🎨 Styles & Colors - Palette NexA

### Colors Principales

```
Primary Blue:     #4A90E2
Primary Dark:     #2C3E50
Success Green:    #2ECC71
Warning Orange:   #F39C12
Error Red:        #E74C3C
Text Primary:     #FFFFFF
Text Secondary:   #BDC3C7
Background Dark:  #1A1A1A
Panel BG:         #2C2C2C (Alpha 0.95)
```

### Typography

**Fonts (TextMeshPro) :**
- Title: Size 32, Bold
- Subtitle: Size 24, SemiBold
- Body: Size 16, Regular
- Small: Size 12, Regular

**Text Colors :**
- Headers: #FFFFFF
- Body: #BDC3C7
- Disabled: #7F8C8D

### Buttons

**Primary Button :**
- Normal: #4A90E2
- Highlighted: #5BA3F5
- Pressed: #3A7AC8
- Disabled: #7F8C8D

**Secondary Button (Outline) :**
- Background: Transparent
- Border: 2px #4A90E2
- Text: #4A90E2

---

## ⚡ Layout Groups - Best Practices

### Vertical Layout Group

**Settings Recommandés :**
- Padding: Top 10, Bottom 10, Left 10, Right 10
- Spacing: 10
- Child Alignment: Upper Center
- Child Force Expand: Width ON, Height OFF
- Child Control Size: Width ON, Height ON

### Horizontal Layout Group

**Settings Recommandés :**
- Spacing: 15
- Child Alignment: Middle Left
- Child Force Expand: Width OFF, Height ON

### Grid Layout Group

**Settings Recommandés :**
- Cell Size: Depend on use case
- Spacing: 10 x 10
- Start Corner: Upper Left
- Start Axis: Horizontal
- Constraint: Fixed Column Count (usually 3)

---

## 🔧 Components Setup - Checklist

### Canvas Scaler
- [ ] UI Scale Mode: Scale With Screen Size
- [ ] Reference Resolution: 1920x1080
- [ ] Screen Match Mode: Match Width Or Height
- [ ] Match: 0.5

### Scroll View
- [ ] Content Size Fitter sur Content (Vertical Fit: Preferred Size)
- [ ] Vertical Layout Group sur Content
- [ ] Scroll Rect: Vertical only
- [ ] Movement Type: Elastic

### Input Fields
- [ ] TextMeshPro - Input Field
- [ ] Placeholder configuré
- [ ] Character Limit défini
- [ ] Content Type approprié (Email, Password, etc.)

### Buttons
- [ ] Transition: Color Tint
- [ ] Target Graphic: Background Image
- [ ] Colors configurées (Normal, Highlighted, Pressed, Disabled)
- [ ] OnClick() event assigné dans script

### Images
- [ ] Sprite assigné (ou couleur unie)
- [ ] Preserve Aspect: selon besoin
- [ ] Raycast Target: OFF si pas interactif (optimisation)

---

## 📦 Dossiers & Organisation

### Structure Assets

```
Assets/
├── Hub/
│   ├── Scenes/
│   │   └── MainScene.unity
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Screens/
│   │   ├── Services/
│   │   ├── Components/
│   │   ├── Models/
│   │   └── Utils/
│   ├── Prefabs/
│   │   └── UI/
│   │       ├── ToastNotification.prefab
│   │       ├── LoadingSpinner.prefab
│   │       ├── FriendListItem.prefab
│   │       ├── MatchListItem.prefab
│   │       └── ...
│   └── Resources/
│       └── UI/
│           └── ToastNotification.prefab
└── Resources/ (Unity global)
```

---

## ✅ Validation Checklist

### Avant de commencer
- [ ] Unity 2022.3 LTS installé
- [ ] TextMeshPro importé (Package Manager)
- [ ] DOTween importé (Asset Store)
- [ ] Newtonsoft.Json configuré

### Après création de chaque écran
- [ ] Toutes les références assignées dans Inspector
- [ ] CanvasGroup ajouté pour fade animations
- [ ] LoadingPanel et ErrorPanel configurés
- [ ] Boutons avec OnClick() events
- [ ] Layout Groups correctement configurés
- [ ] Anchors et Pivot corrects pour responsive

### Tests de base
- [ ] Écran se charge sans erreur
- [ ] Boutons cliquables
- [ ] Input fields fonctionnels
- [ ] Animations smooth (pas de lag)
- [ ] Responsive sur différentes résolutions

---

## 🎬 Animation Quick Reference

### DOTween Durées

```csharp
FAST = 0.15f    // Hover effects
NORMAL = 0.25f  // Standard transitions
MEDIUM = 0.4f   // Panel animations
SLOW = 0.6f     // Screen transitions
```

### Easings Communs

```csharp
IN_SMOOTH = Ease.OutCubic    // Fade in, slide in
OUT_SMOOTH = Ease.InCubic    // Fade out, slide out
IN_BACK = Ease.OutBack       // Pop in effect
BOUNCE = Ease.OutBounce      // Bouncy landing
ELASTIC = Ease.OutElastic    // Elastic effect
```

### Snippets Réutilisables

**Fade In :**
```csharp
canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic).From(0);
```

**Pop In :**
```csharp
transform.localScale = Vector3.zero;
transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
```

**Slide In from Right :**
```csharp
rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
rectTransform.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutCubic);
```

---

## 🚨 Erreurs Courantes & Solutions

### "NullReferenceException sur UIManager.Instance"
**Cause :** UIManager pas initialisé  
**Solution :** Créer GameObject UIManager dans la scène avec script attaché

### "Canvas pas visible"
**Cause :** Canvas Render Mode incorrect  
**Solution :** Vérifier Render Mode = Screen Space - Overlay

### "Boutons ne répondent pas"
**Cause :** Pas d'EventSystem dans la scène  
**Solution :** Unity crée automatiquement EventSystem avec Canvas, vérifier qu'il existe

### "Text illisible/pixelisé"
**Cause :** TextMeshPro pas configuré  
**Solution :** Importer TextMeshPro Essentials (Window → TextMeshPro → Import TMP Essentials)

### "Prefab ToastNotification not found"
**Cause :** Prefab pas dans Resources/UI/  
**Solution :** Créer le dossier Assets/Resources/UI/ et y placer le prefab

---

## 📚 Ressources Utiles

### Documentation Unity
- [UI Layout Groups](https://docs.unity3d.com/Manual/UIAutoLayout.html)
- [Canvas Scaler](https://docs.unity3d.com/Manual/script-CanvasScaler.html)
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)

### DOTween
- [Documentation](http://dotween.demigiant.com/documentation.php)
- [Examples](http://dotween.demigiant.com/examples.php)

### Design Inspiration
- [League of Legends Client](https://www.leagueoflegends.com/)
- [Valorant UI](https://playvalorant.com/)

---

**Version:** 1.0.0  
**Dernière Mise à Jour :** 2026-01-09  
**Auteur :** NexA Development Team

---

> 💡 **Tip :** Utilisez Ctrl+D (Duplicate) pour créer rapidement des copies de UI elements similaires, puis modifiez les textes/images.

