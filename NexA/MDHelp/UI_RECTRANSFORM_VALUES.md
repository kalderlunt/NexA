# 📐 NexA - Valeurs RectTransform Complètes

> **Référence complète de toutes les dimensions et valeurs UI**  
> **Version:** 1.0.0  
> **Date:** 2026-01-09

---

## 🎯 Canvas Principal - UIManager

### UIManager (Canvas Root)

```
Type: Canvas
Components: Canvas + CanvasScaler + GraphicRaycaster + UIManager.cs

Canvas:
- Render Mode: Screen Space - Overlay
- Pixel Perfect: OFF
- Sort Order: 0

Canvas Scaler:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5
```

---

## 🔐 LoginScreen

### LoginScreen (Root)

```
RectTransform:
- Anchors: Stretch Both (0,0,1,1)
- Left: 0, Top: 0, Right: 0, Bottom: 0
- Pivot: 0.5, 0.5

Components:
- CanvasGroup (Alpha: 1)
- LoginScreen.cs
```

### Background

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: #1A1A1A
- Source Image: None (ou votre image)
- Raycast Target: OFF
```

### Logo

```
RectTransform:
- Anchors: Top Center (0.5, 1, 0.5, 1)
- Pivot: 0.5, 1
- Pos X: 0
- Pos Y: -100
- Width: 300
- Height: 100

Image:
- Color: #FFFFFF
- Preserve Aspect: ON
```

### FormPanel

```
RectTransform:
- Anchors: Middle Center (0.5, 0.5, 0.5, 0.5)
- Pivot: 0.5, 0.5
- Pos X: 0, Pos Y: 0
- Width: 500
- Height: 400

Canvas Group:
- Alpha: 1
- Interactable: ON
- Block Raycasts: ON

Vertical Layout Group:
- Padding: 30 (tous côtés)
- Spacing: 20
- Child Alignment: Upper Center
- Child Force Expand: Width ON, Height OFF
- Child Control Size: Width ON, Height ON
```

### TitleText

```
TextMeshProUGUI:
- Text: "Connexion"
- Font Size: 32
- Color: #FFFFFF
- Alignment: Center (Horizontal + Vertical)
- Font Style: Bold

Layout Element:
- Min Height: 50
```

### EmailInputField

```
RectTransform:
- Auto (géré par Vertical Layout)

TMP_InputField:
- Text: ""
- Placeholder: "Email"
- Character Limit: 100
- Content Type: Email Address
- Line Type: Single Line

Layout Element:
- Min Height: 50
- Preferred Height: 50
```

### PasswordInputField

```
RectTransform:
- Auto (géré par Vertical Layout)

TMP_InputField:
- Text: ""
- Placeholder: "Mot de passe"
- Character Limit: 50
- Content Type: Password
- Input Type: Password
- Line Type: Single Line

Layout Element:
- Min Height: 50
- Preferred Height: 50
```

### LoginButton

```
RectTransform:
- Auto (géré par Vertical Layout)

Button:
- Transition: Color Tint
- Normal Color: #4A90E2
- Highlighted Color: #5BA3F5
- Pressed Color: #3A7AC8
- Disabled Color: #7F8C8D

Layout Element:
- Min Height: 60
- Preferred Height: 60

Text (enfant):
- Text: "Se connecter"
- Font Size: 18
- Color: #FFFFFF
- Alignment: Center
```

### RegisterButton

```
RectTransform:
- Auto (géré par Vertical Layout)

Button:
- Normal Color: Transparent (ou #2C2C2C)
- Highlighted Color: #3C3C3C
- Pressed Color: #1C1C1C

Layout Element:
- Min Height: 50
- Preferred Height: 50

Text (enfant):
- Text: "Créer un compte"
- Font Size: 16
- Color: #4A90E2
```

### LoadingPanel

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: #000000
- Alpha: 0.7

Canvas Group:
- Alpha: 0 (hidden par défaut)

GameObject:
- Active: FALSE (désactivé par défaut)
```

### ErrorPanel

```
RectTransform:
- Anchors: Bottom Center (0.5, 0, 0.5, 0)
- Pivot: 0.5, 0
- Pos X: 0
- Pos Y: 50
- Width: 500
- Height: 60

Image:
- Color: #E74C3C
- Alpha: 0.3

GameObject:
- Active: FALSE (désactivé par défaut)
```

### ErrorText (enfant de ErrorPanel)

```
RectTransform:
- Anchors: Stretch Both
- Left: 10, Top: 10, Right: 10, Bottom: 10

TextMeshProUGUI:
- Text: "Message d'erreur"
- Font Size: 14
- Color: #FFFFFF
- Alignment: Center
- Wrapping: Enabled
```

---

## 🏠 HomeScreen

### HomeScreen (Root)

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Components:
- CanvasGroup
- HomeScreen.cs
```

### TopBar

```
RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Pos Y: 0
- Height: 100

Horizontal Layout Group:
- Padding: Left 20, Right 20, Top 10, Bottom 10
- Spacing: 20
- Child Alignment: Middle Center
- Child Force Expand: Width ON, Height ON
```

### UserInfoPanel (dans TopBar)

```
RectTransform:
- Auto (géré par Horizontal Layout)

Horizontal Layout Group:
- Spacing: 15
- Child Alignment: Middle Left

Layout Element:
- Flexible Width: 1
```

### Avatar (dans UserInfoPanel)

```
RectTransform:
- Width: 64
- Height: 64

Image:
- Source Image: UI/Skin/Knob (cercle)

Mask:
- Show Mask Graphic: OFF

Layout Element:
- Min Width: 64
- Min Height: 64
- Preferred Width: 64
- Preferred Height: 64
```

### UsernameText, LevelText, EloText

```
TextMeshProUGUI:
- Font Size: 16 (Username), 14 (Level/Elo)
- Color: #FFFFFF
- Alignment: Left + Middle
```

### MainPanel

```
RectTransform:
- Anchors: Middle Center
- Pivot: 0.5, 0.5
- Width: 600
- Height: 400

Canvas Group:
- Alpha: 1

Grid Layout Group:
- Cell Size: 280 x 80
- Spacing: 20 x 20
- Start Corner: Upper Left
- Start Axis: Horizontal
- Constraint: Fixed Column Count = 2
```

### PlayButton (exception dans Grid)

```
RectTransform:
- Auto (géré par Grid, mais avec Layout Element override)

Button:
- Normal Color: #4A90E2
- Highlighted Color: #5BA3F5

Layout Element:
- Min Width: 580 (prend 2 colonnes)
- Min Height: 120
- Flexible Width: 1

Text:
- Text: "JOUER"
- Font Size: 24
- Bold
```

### ProfileButton, FriendsButton, etc.

```
RectTransform:
- Auto (géré par Grid)

Button:
- Normal Color: #2C2C2C
- Highlighted Color: #3C3C3C

Layout Element:
- (Géré automatiquement par Grid si pas d'override)

Text:
- Font Size: 16
```

### NotificationBadge (sur FriendsButton)

```
RectTransform:
- Anchors: Top Right (1, 1, 1, 1)
- Pivot: 1, 1
- Pos X: 0
- Pos Y: 0
- Width: 24
- Height: 24

Image:
- Source Image: UI/Skin/Knob
- Color: #E74C3C (rouge)

Text (enfant):
- Text: "3"
- Font Size: 12
- Bold
- Alignment: Center
```

---

## 👥 FriendsScreen

### FriendsScreen (Root)

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Components:
- CanvasGroup
- FriendsScreen.cs
```

### HeaderPanel

```
RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Height: 80

Horizontal Layout Group:
- Padding: 20
- Spacing: 15
- Child Alignment: Middle Left
```

### BackButton (dans HeaderPanel)

```
RectTransform:
- Width: 60
- Height: 60

Button:
- Normal Color: #2C2C2C

Layout Element:
- Min Width: 60
- Min Height: 60

Text:
- Text: "←"
- Font Size: 24
```

### TitleText (dans HeaderPanel)

```
TextMeshProUGUI:
- Text: "Amis"
- Font Size: 24
- Bold

Layout Element:
- Min Width: 100
```

### SearchInputField (dans HeaderPanel)

```
TMP_InputField:
- Placeholder: "Rechercher..."

Layout Element:
- Flexible Width: 1
- Min Height: 50
```

### TabsPanel

```
RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Pos Y: -80
- Height: 60

Horizontal Layout Group:
- Spacing: 10
- Child Force Expand: Width ON

Toggle Group:
- (Component ajouté)
```

### FriendsTab, RequestsTab, OnlineTab

```
RectTransform:
- Auto (géré par Horizontal Layout)

Button + Toggle:
- Normal Color: #2C2C2C
- Selected Color: #4A90E2 (Toggle On)

Layout Element:
- Flexible Width: 1
- Min Height: 60

Text:
- Font Size: 14
```

### ContentPanel (ScrollView)

```
RectTransform:
- Anchors: Stretch Both
- Left: 0
- Top: -140 (sous header + tabs)
- Right: 0
- Bottom: 0

Scroll Rect:
- Horizontal: OFF
- Vertical: ON
- Movement Type: Elastic
- Elasticity: 0.1
- Inertia: ON
- Scroll Sensitivity: 20
```

### Content (dans Viewport)

```
RectTransform:
- Anchors: Top Stretch
- Pivot: 0.5, 1
- Pos Y: 0

Vertical Layout Group:
- Padding: 10
- Spacing: 10
- Child Alignment: Upper Center
- Child Force Expand: Width ON, Height OFF
- Child Control Size: Width ON, Height OFF

Content Size Fitter:
- Horizontal Fit: Unconstrained
- Vertical Fit: Preferred Size
```

---

## 🧩 Prefab: ToastNotification

### Root

```
RectTransform:
- Anchors: Middle Center
- Pivot: 0.5, 0.5
- Width: 400
- Height: 80

Canvas Group:
- Alpha: 1 (ou 0 au début de l'animation)
- Interactable: OFF
- Block Raycasts: OFF
```

### Background

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: #2C2C2C
- Raycast Target: ON
```

### Icon

```
RectTransform:
- Anchors: Middle Left (0, 0.5, 0, 0.5)
- Pivot: 0.5, 0.5
- Pos X: 50
- Pos Y: 0
- Width: 40
- Height: 40

Image:
- Color: #FFFFFF
- Preserve Aspect: ON

Layout Element:
- Ignore Layout: TRUE
```

### MessageText

```
RectTransform:
- Anchors: Stretch (0, 0, 1, 1)
- Left: 80
- Top: 10
- Right: 20
- Bottom: 10

TextMeshProUGUI:
- Font Size: 16
- Color: #FFFFFF
- Alignment: Middle Left
- Wrapping: Enabled
- Overflow: Truncate
```

---

## 🧩 Prefab: FriendListItem

### Root

```
RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Height: 80
- Left: 0, Right: 0

Horizontal Layout Group:
- Padding: 10 (tous côtés)
- Spacing: 10
- Child Alignment: Middle Left
- Child Force Expand: Width OFF, Height ON
- Child Control Size: Width OFF, Height ON

Layout Element:
- Min Height: 80
- Preferred Height: 80
```

### Background

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: #2C2C2C
- Raycast Target: OFF
```

### AvatarImage

```
RectTransform:
- Width: 50
- Height: 50

Image:
- Source Image: UI/Skin/Knob
- Color: #FFFFFF

Mask:
- Show Mask Graphic: OFF

Layout Element:
- Min Width: 50, Min Height: 50
- Preferred Width: 50, Preferred Height: 50
```

### StatusIndicator

```
RectTransform:
- Anchors: Bottom Right (relative à Avatar)
- Pivot: 1, 0
- Pos X: 0, Pos Y: 0
- Width: 14
- Height: 14

Image:
- Source Image: UI/Skin/Knob
- Color: #00FF00 (online), #888888 (offline), #FFAA00 (in-game)

Layout Element:
- Ignore Layout: TRUE
```

### InfoPanel

```
RectTransform:
- Auto (géré par Horizontal Layout parent)

Vertical Layout Group:
- Spacing: 5
- Child Alignment: Middle Left

Layout Element:
- Flexible Width: 1
```

### UsernameText

```
TextMeshProUGUI:
- Text: "PlayerName"
- Font Size: 16
- Color: #FFFFFF
- Bold
- Alignment: Left + Middle
- Overflow: Ellipsis
```

### StatusText

```
TextMeshProUGUI:
- Text: "En ligne"
- Font Size: 12
- Color: #BDC3C7
- Alignment: Left + Middle
```

### ActionButton

```
RectTransform:
- Width: 100
- Height: 40

Button:
- Normal Color: #E74C3C (rouge)
- Highlighted Color: #FF6B5B
- Pressed Color: #C0392B

Layout Element:
- Min Width: 100, Min Height: 40

Text:
- Text: "Supprimer"
- Font Size: 14
```

### MenuButton

```
RectTransform:
- Width: 30
- Height: 30

Button:
- Normal Color: Transparent

Layout Element:
- Min Width: 30, Min Height: 30

Text:
- Text: "⋮"
- Font Size: 20
```

---

## 🧩 Prefab: MatchListItem

### Root

```
RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Height: 120
- Left: 0, Right: 0

Button:
- Normal Color: Transparent
- Highlighted Color: #FFFFFF (alpha 0.1)

Horizontal Layout Group:
- Padding: 15 (tous côtés)
- Spacing: 15
- Child Alignment: Middle Left
- Child Force Expand: Width OFF, Height ON

Layout Element:
- Min Height: 120
- Preferred Height: 120
```

### Background

```
RectTransform:
- Anchors: Stretch Both
- Left: 0, Top: 0, Right: 0, Bottom: 0

Image:
- Color: #2ECC71 (victoire) ou #E74C3C (défaite)
- Alpha: 0.1
- Raycast Target: OFF
```

### ResultPanel

```
RectTransform:
- Auto (géré par Horizontal Layout)

Vertical Layout Group:
- Spacing: 5
- Child Alignment: Upper Center

Layout Element:
- Min Width: 100
- Preferred Width: 100
```

#### ResultIcon

```
RectTransform:
- Width: 40, Height: 40

Image:
- Color: #FFFFFF

Layout Element:
- Min Width: 40, Min Height: 40
```

#### ResultText

```
TextMeshProUGUI:
- Text: "VICTOIRE" / "DÉFAITE"
- Font Size: 14
- Bold
- Alignment: Center
```

#### DateText

```
TextMeshProUGUI:
- Text: "Il y a 2h"
- Font Size: 10
- Color: #BDC3C7
- Alignment: Center
```

### ChampionPanel

```
Vertical Layout Group:
- Spacing: 5

Layout Element:
- Min Width: 80
- Preferred Width: 80
```

#### ChampionIcon

```
RectTransform:
- Width: 64, Height: 64

Image + Mask:
- Source Image: UI/Skin/Knob

Layout Element:
- Min 64x64, Preferred 64x64
```

#### ChampionNameText

```
TextMeshProUGUI:
- Font Size: 10
- Alignment: Center
```

### GameInfoPanel

```
Vertical Layout Group:
- Spacing: 5
- Child Alignment: Middle Left

Layout Element:
- Flexible Width: 1
```

#### GameModeText, DurationText, ScoreText, TeamScoreText

```
TextMeshProUGUI:
- Font Size: 12 (GameMode/Duration), 14 (Score), 10 (TeamScore)
- Color: #FFFFFF
- Alignment: Left
```

### StatsPanel

```
Grid Layout Group:
- Cell Size: 70 x 30
- Spacing: 5 x 5
- Start Corner: Upper Left
- Constraint: Fixed Column Count = 3

Layout Element:
- Min Width: 230
- Preferred Width: 230
```

#### KDABadge, CSBadge, DamageBadge

```
Panel avec:
- Background (Image): Color selon type
- ValueText (TMP): Font 14, Bold, Center
- LabelText (TMP): Font 8, Center

RectTransform géré par Grid (70x30 automatique)
```

### ItemsPanel

```
Horizontal Layout Group:
- Spacing: 4

Layout Element:
- Min Width: 168
- Preferred Width: 168
```

#### ItemIcon (x6)

```
RectTransform:
- Width: 24, Height: 24

Image:
- Preserve Aspect: ON

Layout Element:
- Min 24x24, Preferred 24x24
```

### ArrowIcon

```
RectTransform:
- Width: 20, Height: 20

Image:
- Source Image: UI/Skin/UISprite
- Color: #BDC3C7

Layout Element:
- Min 20x20
```

---

## 📏 Tableau Récapitulatif des Tailles

| Élément | Width | Height | Notes |
|---------|-------|--------|-------|
| **Canvas** | 1920 | 1080 | Reference Resolution |
| **LoginScreen Logo** | 300 | 100 | Top centered |
| **FormPanel** | 500 | 400 | Centered |
| **Input Fields** | Auto | 50 | Min height |
| **Buttons (standard)** | Auto | 50-60 | Min height |
| **TopBar** | Full width | 100 | Top anchored |
| **Avatar (small)** | 50 | 50 | FriendListItem |
| **Avatar (medium)** | 64 | 64 | HomeScreen |
| **ToastNotification** | 400 | 80 | Prefab |
| **FriendListItem** | Full width | 80 | Prefab |
| **MatchListItem** | Full width | 120 | Prefab |
| **NotificationBadge** | 24 | 24 | Badge |
| **StatusIndicator** | 14 | 14 | Online dot |
| **Tab Buttons** | Flexible | 60 | In TabsPanel |

---

## 🎨 Palette de Couleurs Complète

```
Primary Blue:       #4A90E2
Primary Dark:       #2C3E50
Success Green:      #2ECC71
Warning Orange:     #F39C12
Error Red:          #E74C3C
Background Dark:    #1A1A1A
Panel BG:           #2C2C2C
Panel Hover:        #3C3C3C
Text Primary:       #FFFFFF
Text Secondary:     #BDC3C7
Text Disabled:      #7F8C8D
Online:             #00FF00
In-Game:            #FFAA00
Offline:            #888888
```

---

## 📐 Anchors Presets Référence

| Preset | Min X | Min Y | Max X | Max Y | Usage |
|--------|-------|-------|-------|-------|-------|
| **Top Left** | 0 | 1 | 0 | 1 | Coin haut gauche |
| **Top Center** | 0.5 | 1 | 0.5 | 1 | Centré en haut |
| **Top Right** | 1 | 1 | 1 | 1 | Coin haut droite |
| **Middle Left** | 0 | 0.5 | 0 | 0.5 | Milieu gauche |
| **Middle Center** | 0.5 | 0.5 | 0.5 | 0.5 | Centre absolu |
| **Middle Right** | 1 | 0.5 | 1 | 0.5 | Milieu droite |
| **Bottom Left** | 0 | 0 | 0 | 0 | Coin bas gauche |
| **Bottom Center** | 0.5 | 0 | 0.5 | 0 | Centré en bas |
| **Bottom Right** | 1 | 0 | 1 | 0 | Coin bas droite |
| **Stretch Horizontal** | 0 | 0.5 | 1 | 0.5 | Étire largeur |
| **Stretch Vertical** | 0.5 | 0 | 0.5 | 1 | Étire hauteur |
| **Stretch Both** | 0 | 0 | 1 | 1 | Plein écran |

---

## 💡 Tips Rapides

### Créer Rapidement des Anchors

1. **Shift+Click** sur anchor preset = Change Pivot aussi
2. **Alt+Click** sur anchor preset = Change Position aussi
3. **Alt+Shift+Click** = Change Anchor + Pivot + Position

### Layout Groups - Ordre de Priorité

1. **Layout Element** (plus haute priorité)
2. **Layout Group** du parent
3. **RectTransform** natif (plus basse priorité)

### Content Size Fitter

- **Horizontal/Vertical Fit: Preferred Size** = Ajuste à la taille du contenu
- **Min Size** = Ajuste au minimum requis
- **Unconstrained** = Ne change rien

---

**Version:** 1.0.0  
**Dernière MAJ:** 2026-01-09  
**Auteur:** NexA Dev Team

---

> 💡 **Conseil :** Gardez ce document ouvert pendant la création UI pour copier-coller les valeurs rapidement !

