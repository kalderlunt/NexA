# 📊 FriendListItem - Diagrammes Visuels

> **Visualisation des dimensions et positions**

---

## 🎨 Vue d'Ensemble (80px de hauteur)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         FriendListItem (Height: 80px)                       │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │                        Background (#2C2C2C)                             │ │
│ │                         (Stretch Both)                                   │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                               │
│ ┌─── ContentPanel (Horizontal Layout, Padding: 10, Spacing: 10) ──────────┐ │
│ │                                                                           │ │
│ │  ┌────────┐  ┌─────────────────────────────┐  ┌──────────┐  ┌───┐      │ │
│ │  │ Avatar │  │       InfoPanel             │  │  Action  │  │ ⋮ │      │ │
│ │  │  50x50 │  │  (Flexible Width: 1)        │  │  Button  │  │30 │      │ │
│ │  │ ┌──┐   │  │                             │  │  100x40  │  │x30│      │ │
│ │  │ │14│   │  │  PlayerName (Font 16, Bold) │  │  (Red)   │  │   │      │ │
│ │  │ └──┘   │  │  En ligne (Font 12, Gray)   │  │          │  │   │      │ │
│ │  │        │  │                             │  │          │  │   │      │ │
│ │  └────────┘  └─────────────────────────────┘  └──────────┘  └───┘      │ │
│ │                                                                           │ │
│ └───────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
   ←─────────────────────── Full Width (Stretch) ──────────────────────────→
```

---

## 🔍 Zoom sur Avatar + StatusIndicator

```
┌──────────────────────────────────────────┐
│        AvatarImage (50x50)               │
│                                          │
│    ┌────────────────────────────┐       │
│    │                            │       │
│    │                            │       │
│    │      Avatar circulaire     │       │
│    │        (Mask: Knob)        │       │
│    │                            │       │
│    │                            │       │
│    │                            │       │
│    │                        ┌──┐│       │
│    │                        │14││       │ ← StatusIndicator
│    └────────────────────────└──┘┘       │    (Bottom Right)
│                                    ↑     │    14x14
│                                    │     │    Anchors: (1, 0, 1, 0)
│                              StatusIndicator │    Pivot: (1, 0)
│                                          │
└──────────────────────────────────────────┘
```

**Position StatusIndicator :**
- Anchors : Bottom Right (1, 0, 1, 0)
- Pivot : (1, 0) = Coin bas-droit
- Pos X : 0
- Pos Y : 0

---

## 📐 Layout ContentPanel (Horizontal)

```
┌─── ContentPanel (Padding: 10 sur tous les côtés) ─────────────────────────┐
│                                                                            │
│  ← 10px →  ┌──────┐ ← 10px → ┌─────────────────────┐ ← 10px → ┌────┐    │
│  Padding   │Avatar│  Spacing │     InfoPanel       │  Spacing  │Act.│    │
│            │ 50x50│           │  (Flexible: 1)      │           │100 │    │
│            │      │           │                     │           │x40 │    │
│            └──────┘           └─────────────────────┘           └────┘    │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
  ↑                              ↑                                ↑
  Padding 10px          Spacing 10px entre enfants       Padding 10px
```

**Child Alignment : Middle Left**
- Tous les enfants sont centrés verticalement (Middle)
- Alignés à gauche horizontalement (Left)

---

## 🎯 InfoPanel (Vertical Layout)

```
InfoPanel (Flexible Width: 1 - prend l'espace restant)
┌──────────────────────────────────────────────┐
│                                              │
│  ┌────────────────────────────────────────┐ │
│  │  PlayerName (Font 16, Bold, #FFFFFF)  │ │
│  └────────────────────────────────────────┘ │
│                  ↕                           │
│               Spacing 5px                    │
│                  ↕                           │
│  ┌────────────────────────────────────────┐ │
│  │  En ligne (Font 12, #BDC3C7)          │ │
│  └────────────────────────────────────────┘ │
│                                              │
└──────────────────────────────────────────────┘
```

**Vertical Layout Group :**
- Spacing : 5px
- Child Alignment : Middle Left
- Child Control Size : Width ON, Height ON

---

## 📏 Dimensions Exactes

### Vue Isométrique

```
                    FriendListItem
                    ┌─────────────────────────────────────┐
                    │                                     │
      Height: 80px  │                                     │
                    │                                     │
                    └─────────────────────────────────────┘
                    ←─────── Full Width (Stretch) ────────→


Background          ┌═════════════════════════════════════┐
(Stretch Both)      ║                                     ║
                    ║         Color: #2C2C2C              ║
                    ║                                     ║
                    └═════════════════════════════════════┘


ContentPanel        ┌─────────────────────────────────────┐
(Stretch Both)      │ Padding: 10px sur tous les côtés   │
avec Padding 10     │ ┌─────────────────────────────────┐ │
                    │ │                                 │ │
                    │ │   Zone des enfants (Layout)     │ │
                    │ │                                 │ │
                    │ └─────────────────────────────────┘ │
                    └─────────────────────────────────────┘
```

---

## 🎨 Z-Order (Ordre d'Affichage)

```
Vue de profil (Z-axis) :

   ┌─────────────────────────┐
   │  MenuButton (⋮)         │  ← Dernier enfant (dessus)
   └─────────────────────────┘
   ┌─────────────────────────┐
   │  ActionButton           │
   └─────────────────────────┘
   ┌─────────────────────────┐
   │  InfoPanel              │
   │  - StatusText           │
   │  - UsernameText         │
   └─────────────────────────┘
   ┌─────────────────────────┐
   │  StatusIndicator        │
   └─────────────────────────┘
   ┌─────────────────────────┐
   │  AvatarImage            │
   └─────────────────────────┘
   ┌─────────────────────────┐
   │  ContentPanel           │  ← Premier enfant
   └─────────────────────────┘
═════════════════════════════════
   ┌─────────────────────────┐
   │  Background             │  ← EN DESSOUS de tout
   └─────────────────────────┘
═════════════════════════════════
```

**Règle Unity :** Les éléments plus BAS dans la hiérarchie sont dessinés PAR-DESSUS.

**Important :** Background doit être LE PREMIER enfant pour être derrière.

---

## 📊 Répartition de l'Espace (Exemple : Width 600px)

```
Total Width: 600px
────────────────────────────────────────────────────────────────

Padding Left:   10px  │
Avatar:         50px  │
Spacing:        10px  │
InfoPanel:     450px  │ ← Flexible Width (prend le reste)
Spacing:        10px  │
ActionButton:  100px  │
Spacing:        10px  │
MenuButton:     30px  │
Padding Right:  10px  │
                      │
Total:         600px  ✓
```

**Calcul InfoPanel :**
```
Total Width          = 600px
- Padding (10×2)     = -20px
- Avatar             = -50px
- Spacing (3×10)     = -30px
- ActionButton       = -100px
- MenuButton         = -30px
─────────────────────────────
InfoPanel Width      = 370px (flexible)
```

---

## 🔄 Flow Layout (Horizontal)

```
Step 1: ContentPanel calcule l'espace disponible
┌────────────────────────────────────────────────┐
│ Total Width - (Padding Left + Padding Right)  │
│ = Available Width                              │
└────────────────────────────────────────────────┘

Step 2: Layout place les enfants de gauche à droite
┌──────┐ → ┌─────────────┐ → ┌────────┐ → ┌────┐
│Avatar│   │  InfoPanel  │   │ Action │   │Menu│
└──────┘   └─────────────┘   └────────┘   └────┘
   ↓              ↓               ↓           ↓
 Fixed     Flexible (1)       Fixed       Fixed
  50px        (auto)          100px        30px

Step 3: Espacement (Spacing: 10px entre chaque)
┌──────┐ [10] ┌─────────────┐ [10] ┌────────┐ [10] ┌────┐
│Avatar│      │  InfoPanel  │      │ Action │      │Menu│
└──────┘      └─────────────┘      └────────┘      └────┘

Step 4: Centrage vertical (Child Alignment: Middle Left)
    │         │             │          │         │
    ├─────────┼─────────────┼──────────┼─────────┤  ← Ligne centrale
    │         │             │          │         │
```

---

## 💡 Astuces de Positionnement

### StatusIndicator : 2 Méthodes

**Méthode A : Enfant de ContentPanel (avec Ignore Layout)**
```
ContentPanel
├── AvatarImage (50x50)
├── StatusIndicator ← Ignore Layout: TRUE
│   Anchors: Bottom Right (1, 0, 1, 0)
│   Pos X: -40 (pour être sur Avatar)
│   Pos Y: 10
```

**Méthode B : Enfant d'AvatarImage (Recommandé) ⭐**
```
ContentPanel
├── AvatarImage (50x50)
│   └── StatusIndicator ← Enfant direct
│       Anchors: Bottom Right (1, 0, 1, 0)
│       Pos X: 0
│       Pos Y: 0
```

**Méthode B est plus simple** car StatusIndicator suit automatiquement l'avatar si celui-ci est déplacé.

---

## 🎯 Points d'Ancrage (Anchors)

### FriendListItem Root

```
(0, 1) ──────────────────── (1, 1) ← Top Stretch
  │                            │
  │      FriendListItem        │
  │       Height: 80px         │
  │                            │
(0, 1) ──────────────────── (1, 1)
  │                            │
  Anchors Min: (0, 1)          │
  Anchors Max: (1, 1)          │
  Pivot: (0.5, 1) ─────────────┘
```

### Background & ContentPanel

```
(0, 0) ──────────────────── (1, 0) ← Stretch Both
  │                            │
  │    Background/Content      │
  │     (Full Coverage)        │
  │                            │
(0, 1) ──────────────────── (1, 1)
```

### StatusIndicator (Enfant d'Avatar)

```
Avatar 50x50
┌────────────────────────────┐ (1, 1) Top Right
│                            │
│                            │
│         Avatar             │
│                            │
│                      ┌──┐  │
│                      │14│  │ ← StatusIndicator
└──────────────────────└──┘──┘ (1, 0) Bottom Right
                         ↑
                    Anchor Point
                    Min: (1, 0)
                    Max: (1, 0)
                    Pivot: (1, 0)
```

---

**Version:** 1.0.0  
**Dernière MAJ:** 2026-01-09  
**Auteur:** NexA Dev Team

---

> 💡 **Utilisez ces diagrammes comme référence visuelle pendant la création !**

