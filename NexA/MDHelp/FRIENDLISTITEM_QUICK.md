# 🎯 FriendListItem - Guide Ultra-Rapide

> **Copier-coller direct pour Unity**

---

## 📐 Étapes Résumées

### 1️⃣ Root (Empty)
```
Create Empty "FriendListItem"
Anchors: Top Stretch (0, 1, 1, 1)
Add Layout Element: Min Height 80
```

### 2️⃣ Background (Image)
```
Parent: FriendListItem
Anchors: Stretch Both (0, 0, 1, 1)
Color: #2C2C2C
Raycast: OFF
→ Déplacer en PREMIER dans hiérarchie
```

### 3️⃣ ContentPanel (Empty)
```
Parent: FriendListItem
Anchors: Stretch Both (0, 0, 1, 1)
Add Horizontal Layout:
  Padding: 10
  Spacing: 10
  Child Alignment: Middle Left
```

### 4️⃣ AvatarImage (Image + Mask)
```
Parent: ContentPanel
Size: 50x50
Source: UI/Skin/Knob
Add Mask: Show Mask Graphic OFF
Add Layout Element: Min/Preferred 50x50
```

### 5️⃣ StatusIndicator (Image)
```
Parent: AvatarImage ⭐ (enfant direct)
Anchors: Bottom Right (1, 0, 1, 0)
Pivot: 1, 0
Pos: 0, 0
Size: 14x14
Color: #00FF00 (vert)
```

### 6️⃣ InfoPanel (Empty)
```
Parent: ContentPanel
Add Vertical Layout: Spacing 5
Add Layout Element: Flexible Width 1
```

### 7️⃣ UsernameText (TMP)
```
Parent: InfoPanel
Text: "PlayerName"
Font Size: 16
Bold
Color: #FFFFFF
```

### 8️⃣ StatusText (TMP)
```
Parent: InfoPanel
Text: "En ligne"
Font Size: 12
Color: #BDC3C7
```

### 9️⃣ ActionButton (Button)
```
Parent: ContentPanel
Size: 100x40
Color: #E74C3C (rouge)
Text: "Supprimer"
Add Layout Element: Min/Preferred 100x40
```

### 🔟 MenuButton (Button)
```
Parent: ContentPanel
Size: 30x30
Color: Transparent
Text: "⋮"
Add Layout Element: Min/Preferred 30x30
```

---

## ✅ Hiérarchie Finale

```
FriendListItem
├── Background ← EN PREMIER !
└── ContentPanel
    ├── AvatarImage
    │   └── StatusIndicator ← ENFANT !
    ├── InfoPanel
    │   ├── UsernameText
    │   └── StatusText
    ├── ActionButton
    └── MenuButton
```

---

## 🎨 Couleurs à Copier

```
Background:     #2C2C2C
Avatar:         #FFFFFF
Status Online:  #00FF00
Username:       #FFFFFF
Status Text:    #BDC3C7
Action Button:  #E74C3C
Button Hover:   #FF6B5B
```

---

## 📏 Tailles à Copier

```
Root Height:    80
Avatar:         50x50
Status:         14x14
Action Button:  100x40
Menu Button:    30x30
```

---

**Temps estimé : 10 minutes**

**Voir détails complets :** [FRIENDLISTITEM_DIMENSIONS.md](./FRIENDLISTITEM_DIMENSIONS.md)

