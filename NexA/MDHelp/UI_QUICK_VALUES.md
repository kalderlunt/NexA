# 🎯 Guide Ultra-Rapide - Valeurs UI Unity

> **Copier-coller rapide des valeurs les plus utilisées**  
> **Version:** 1.0.0

---

## 📐 Canvas Principal

```
Canvas:
- Render Mode: Screen Space - Overlay
- Reference Resolution: 1920 x 1080
- Match: 0.5
```

---

## 🎨 Couleurs Principales

```csharp
Primary Blue:    #4A90E2
Success Green:   #2ECC71
Error Red:       #E74C3C
Background:      #1A1A1A
Panel:           #2C2C2C
Text Primary:    #FFFFFF
Text Secondary:  #BDC3C7
Online:          #00FF00
Offline:         #888888
```

---

## 📏 Tailles Standards

| Élément | Width | Height |
|---------|-------|--------|
| **Button (standard)** | Auto | 50-60 |
| **Input Field** | Auto | 50 |
| **Avatar (small)** | 50 | 50 |
| **Avatar (medium)** | 64 | 64 |
| **Toast** | 400 | 80 |
| **FriendListItem** | Full | 80 |
| **MatchListItem** | Full | 120 |
| **Badge** | 24 | 24 |
| **Status Dot** | 14 | 14 |

---

## 🔧 Layout Groups

### Vertical Layout Group (standard)

```
Padding: 30
Spacing: 20
Child Alignment: Upper Center
Child Force Expand: Width ON, Height OFF
Child Control Size: Both ON
```

### Horizontal Layout Group (standard)

```
Padding: 20
Spacing: 15
Child Alignment: Middle Left
Child Force Expand: Width ON, Height ON
```

### Grid Layout Group

```
Cell Size: 280 x 80
Spacing: 20 x 20
Start Corner: Upper Left
Constraint: Fixed Column Count = 2
```

---

## 📍 Anchors Courants

| Usage | Min X | Min Y | Max X | Max Y |
|-------|-------|-------|-------|-------|
| **Stretch Both** | 0 | 0 | 1 | 1 |
| **Top Center** | 0.5 | 1 | 0.5 | 1 |
| **Middle Center** | 0.5 | 0.5 | 0.5 | 0.5 |
| **Top Stretch** | 0 | 1 | 1 | 1 |

**Raccourci Unity :** Alt+Shift+Click sur anchor preset = tout ajuster

---

## 🎯 FriendListItem (Copier-Coller)

```
Root:
- Height: 80
- Horizontal Layout Group
  - Padding: 10
  - Spacing: 10

Avatar:
- Size: 50x50
- Mask: Knob (cercle)

StatusIndicator:
- Size: 14x14
- Anchors: Bottom Right

InfoPanel:
- Flexible Width: 1
- Vertical Layout Group

ActionButton:
- Size: 100x40
- Color: #E74C3C
```

---

## 🎮 MatchListItem (Copier-Coller)

```
Root:
- Height: 120
- Horizontal Layout Group
  - Padding: 15
  - Spacing: 15

ResultPanel: Width 100
ChampionPanel: Width 80
GameInfoPanel: Flexible Width 1
StatsPanel: Width 230
ItemsPanel: Width 168
```

---

## 🍞 ToastNotification (Copier-Coller)

```
Root:
- Size: 400x80
- Canvas Group (Alpha: 1)

Icon:
- Size: 40x40
- Pos X: 50

MessageText:
- Left: 80, Right: 20
- Font Size: 16
```

---

## ✨ Animations DOTween

```csharp
// Durées
FAST = 0.15f
NORMAL = 0.25f
MEDIUM = 0.4f

// Easings
Ease.OutCubic    // Fade in/out
Ease.OutBack     // Pop in
Ease.InBack      // Pop out
```

---

## 🚀 Quick Start

**Créer un écran :**
1. Create Empty → Anchors Stretch Both
2. Add CanvasGroup
3. Add Script (LoginScreen.cs)

**Créer un bouton :**
1. UI → Button TMP
2. Normal Color: #4A90E2
3. Min Height: 60

**Créer un input :**
1. UI → Input Field TMP
2. Min Height: 50
3. Content Type: Email / Password

---

**Voir détails complets :** [UI_RECTRANSFORM_VALUES.md](./UI_RECTRANSFORM_VALUES.md)

