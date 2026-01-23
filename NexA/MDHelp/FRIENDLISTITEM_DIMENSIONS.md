# 📐 FriendListItem - Dimensions Complètes

> **Référence rapide pour créer FriendListItem**  
> **Structure :** Background séparé + ContentPanel avec Layout

---

## 🎯 Structure Hiérarchique

```
FriendListItem (Empty GameObject)
├── Background (Image) ← Stretch Both, derrière tout
└── ContentPanel (Empty) ← Horizontal Layout Group
    ├── AvatarImage (Image + Mask) ← 50x50
    ├── StatusIndicator (Image) ← 14x14, sur l'avatar
    ├── InfoPanel (Empty - Vertical Layout) ← Flexible Width
    │   ├── UsernameText (TMP)
    │   └── StatusText (TMP)
    ├── ActionButton (Button) ← 100x40
    └── MenuButton (Button) ← 30x30
```

---

## 📏 Dimensions et Valeurs

### 1. FriendListItem (Root)

```
Type: Empty GameObject

RectTransform:
- Anchors: Top Stretch (0, 1, 1, 1)
- Pivot: 0.5, 1
- Left: 0
- Right: 0
- Pos Y: 0

Layout Element:
☑ Min Height: 80
☑ Preferred Height: 80
```

**⚠️ Ne PAS ajouter de Layout Group ici !**

---

### 2. Background

```
Type: UI → Image

RectTransform:
- Anchors: Stretch Both (0, 0, 1, 1)
- Left: 0
- Right: 0
- Top: 0
- Bottom: 0

Image:
- Source Image: None (UI-Default)
- Color: #2C2C2C
  RGB: 44, 44, 44
  Alpha: 255
- Material: None
- Raycast Target: ☐ OFF (optimisation)
```

**💡 Déplacer en PREMIER dans la hiérarchie pour être derrière**

---

### 3. ContentPanel

```
Type: Empty GameObject

RectTransform:
- Anchors: Stretch Both (0, 0, 1, 1)
- Left: 0
- Right: 0
- Top: 0
- Bottom: 0

Horizontal Layout Group:
- Padding:
  Left: 10
  Right: 10
  Top: 10
  Bottom: 10
- Spacing: 10
- Child Alignment: Middle Left
- Child Force Expand:
  Width: ☐ OFF
  Height: ☑ ON
- Child Control Size:
  Width: ☐ OFF
  Height: ☑ ON
```

---

### 4. AvatarImage

```
Type: UI → Image
Parent: ContentPanel

RectTransform:
- Width: 50
- Height: 50
(Anchors automatiques par Layout)

Image:
- Source Image: UI/Skin/Knob (cercle Unity)
- Color: #FFFFFF (blanc)
- Image Type: Simple
- Preserve Aspect: ☑ ON
- Raycast Target: ☑ ON

Mask:
- Show Mask Graphic: ☐ OFF

Layout Element:
☑ Min Width: 50
☑ Min Height: 50
☑ Preferred Width: 50
☑ Preferred Height: 50
```

---

### 5. StatusIndicator (Option A : Enfant de ContentPanel)

```
Type: UI → Image
Parent: ContentPanel

RectTransform:
- Anchors: Bottom Right (1, 0, 1, 0)
- Pivot: 1, 0
- Pos X: -40 (ajuster pour être sur l'avatar)
- Pos Y: 10
- Width: 14
- Height: 14

Image:
- Source Image: UI/Skin/Knob
- Color: #00FF00 (vert = online)
  Autres couleurs:
  - Online: #00FF00 (0, 255, 0)
  - In-Game: #FFAA00 (255, 170, 0)
  - Offline: #888888 (136, 136, 136)

Layout Element:
☑ Ignore Layout: TRUE
```

### 5. StatusIndicator (Option B : Enfant d'AvatarImage) ⭐ Recommandé

```
Type: UI → Image
Parent: AvatarImage (enfant direct)

RectTransform:
- Anchors: Bottom Right (1, 0, 1, 0)
- Pivot: 1, 0
- Pos X: 0
- Pos Y: 0
- Width: 14
- Height: 14

Image:
- Source Image: UI/Skin/Knob
- Color: #00FF00 (vert)

(Pas de Layout Element nécessaire)
```

**💡 Cette méthode est plus simple car StatusIndicator suit automatiquement l'avatar**

---

### 6. InfoPanel

```
Type: Empty GameObject
Parent: ContentPanel

Vertical Layout Group:
- Padding: 0 (tous)
- Spacing: 5
- Child Alignment: Middle Left
- Child Force Expand:
  Width: ☐ OFF
  Height: ☐ OFF
- Child Control Size:
  Width: ☑ ON
  Height: ☑ ON

Layout Element:
☑ Flexible Width: 1
```

**💡 Flexible Width = 1 fait que InfoPanel prend tout l'espace restant**

---

### 7. UsernameText

```
Type: UI → Text - TextMeshPro
Parent: InfoPanel

TextMeshProUGUI:
- Text: "PlayerName"
- Font: LiberationSans SDF (par défaut TMP)
- Font Style: Bold
- Font Size: 16
- Auto Size: ☐ OFF
- Color: #FFFFFF
  RGBA: 255, 255, 255, 255
- Alignment: 
  Horizontal: Left
  Vertical: Middle
- Wrapping: ☐ Disabled
- Overflow: Ellipsis
- Extra Settings:
  - Rich Text: ☑ ON
```

---

### 8. StatusText

```
Type: UI → Text - TextMeshPro
Parent: InfoPanel

TextMeshProUGUI:
- Text: "En ligne"
- Font: LiberationSans SDF
- Font Style: Regular
- Font Size: 12
- Color: #BDC3C7
  RGBA: 189, 195, 199, 255
- Alignment:
  Horizontal: Left
  Vertical: Middle
- Wrapping: ☐ Disabled
```

---

### 9. ActionButton

```
Type: UI → Button - TextMeshPro
Parent: ContentPanel

RectTransform:
- Width: 100
- Height: 40

Button:
- Interactable: ☑ ON
- Transition: Color Tint
- Target Graphic: Background (Image enfant)
- Colors:
  Normal: #E74C3C (231, 76, 60)
  Highlighted: #FF6B5B (255, 107, 91)
  Pressed: #C0392B (192, 57, 43)
  Selected: #E74C3C
  Disabled: #7F8C8D (127, 140, 141)
- Color Multiplier: 1
- Fade Duration: 0.1

Layout Element:
☑ Min Width: 100
☑ Min Height: 40
☑ Preferred Width: 100
☑ Preferred Height: 40

Text (enfant "Text (TMP)"):
- Text: "Supprimer"
- Font Size: 14
- Color: #FFFFFF
- Alignment: Center (Horizontal + Vertical)
- Auto Size: ☐ OFF
```

---

### 10. MenuButton

```
Type: UI → Button - TextMeshPro
Parent: ContentPanel

RectTransform:
- Width: 30
- Height: 30

Button:
- Transition: Color Tint
- Colors:
  Normal: Transparent (0, 0, 0, 0)
    OU #2C2C2C (44, 44, 44, 255)
  Highlighted: #3C3C3C (60, 60, 60)
  Pressed: #1C1C1C (28, 28, 28)
  Disabled: #555555

Layout Element:
☑ Min Width: 30
☑ Min Height: 30
☑ Preferred Width: 30
☑ Preferred Height: 30

Text (enfant):
- Text: "⋮" (caractère Unicode U+22EE)
  Copier-coller: ⋮
  Windows: Alt+8942 (avec pavé numérique)
- Font Size: 20
- Color: #FFFFFF
- Alignment: Center
```

---

## 🎨 Palette de Couleurs

| Élément | Hex | RGB | Usage |
|---------|-----|-----|-------|
| **Background** | #2C2C2C | 44, 44, 44 | Fond du panel |
| **Avatar** | #FFFFFF | 255, 255, 255 | Placeholder avatar |
| **Status Online** | #00FF00 | 0, 255, 0 | Point vert |
| **Status In-Game** | #FFAA00 | 255, 170, 0 | Point orange |
| **Status Offline** | #888888 | 136, 136, 136 | Point gris |
| **Username** | #FFFFFF | 255, 255, 255 | Texte principal |
| **Status Text** | #BDC3C7 | 189, 195, 199 | Texte secondaire |
| **Action Button** | #E74C3C | 231, 76, 60 | Bouton supprimer |
| **Button Hover** | #FF6B5B | 255, 107, 91 | Hover supprimer |

---

## 📊 Tableau Récapitulatif des Tailles

| Élément | Width | Height | Type |
|---------|-------|--------|------|
| **FriendListItem** | Full (Stretch) | 80 | Layout Element |
| **Background** | Full (Stretch) | Full | Stretch Both |
| **ContentPanel** | Full (Stretch) | Full | Stretch Both |
| **AvatarImage** | 50 | 50 | Layout Element |
| **StatusIndicator** | 14 | 14 | Fixed |
| **InfoPanel** | Flexible (1) | Auto | Flexible |
| **UsernameText** | Auto | Auto | Géré par parent |
| **StatusText** | Auto | Auto | Géré par parent |
| **ActionButton** | 100 | 40 | Layout Element |
| **MenuButton** | 30 | 30 | Layout Element |

---

## ✅ Checklist de Création

### Phase 1 : Structure
- [ ] Créer FriendListItem (Empty)
- [ ] Add Layout Element (Min Height 80)
- [ ] Créer Background (Image, Stretch Both)
- [ ] Créer ContentPanel (Empty, Stretch Both)
- [ ] Add Horizontal Layout Group sur ContentPanel

### Phase 2 : Enfants de ContentPanel
- [ ] AvatarImage (50x50, Mask, Layout Element)
- [ ] StatusIndicator (14x14, Ignore Layout OU enfant d'Avatar)
- [ ] InfoPanel (Vertical Layout, Flexible Width 1)
  - [ ] UsernameText (Font 16, Bold)
  - [ ] StatusText (Font 12, Gray)
- [ ] ActionButton (100x40, Red, Layout Element)
- [ ] MenuButton (30x30, Layout Element)

### Phase 3 : Configuration
- [ ] Background en PREMIER dans hiérarchie (Z-order)
- [ ] Couleurs appliquées (#2C2C2C, #FFFFFF, #00FF00, etc.)
- [ ] Textes configurés (fonts, sizes, colors)
- [ ] Boutons configurés (Normal, Highlighted, Pressed)
- [ ] Raycast Target OFF sur Background

### Phase 4 : Test
- [ ] Vérifier hauteur = 80px
- [ ] Vérifier Background stretch sur toute la surface
- [ ] Vérifier Avatar circulaire
- [ ] Vérifier Status indicator positionné sur l'avatar
- [ ] Vérifier InfoPanel prend l'espace restant
- [ ] Vérifier boutons alignés à droite

### Phase 5 : Prefab
- [ ] Drag FriendListItem vers Assets/Hub/Prefabs/UI/
- [ ] Vérifier que le prefab est bien créé
- [ ] Optionnel : supprimer de la scène

---

## 🐛 Troubleshooting

### Problème : "Je ne peux pas modifier la hauteur de FriendListItem"

**Cause :** Anchors Top Stretch contrôle la hauteur automatiquement.

**Solution :** Utiliser **Layout Element** avec Min/Preferred Height.

---

### Problème : "Background ne s'affiche pas"

**Cause 1 :** Background derrière dans le Z-order.  
**Solution :** Déplacer Background en PREMIER dans la hiérarchie.

**Cause 2 :** Raycast Target bloque.  
**Solution :** Désactiver Raycast Target sur Background.

---

### Problème : "StatusIndicator pas sur l'avatar"

**Solution 1 :** Mettre StatusIndicator comme **enfant direct d'AvatarImage**.

**Solution 2 :** Ajuster Pos X du StatusIndicator (essayer -40 à -50).

---

### Problème : "ContentPanel ne respecte pas le padding"

**Cause :** Horizontal Layout Group mal configuré.

**Solution :** Vérifier :
- Child Force Expand: Width OFF, Height ON
- Child Control Size: Width OFF, Height ON

---

### Problème : "InfoPanel trop petit"

**Cause :** Pas de Flexible Width.

**Solution :** Layout Element sur InfoPanel → Flexible Width = 1.

---

## 💡 Astuces Avancées

### Variante : StatusIndicator Animé

Ajouter un script pour faire pulser le point :

```csharp
using UnityEngine;
using DG.Tweening;

public class StatusPulse : MonoBehaviour
{
    private void Start()
    {
        transform.DOScale(1.2f, 0.8f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
```

---

### Variante : Hover Effect sur le Panel

Ajouter sur FriendListItem :

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class FriendItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image background;
    private Color normalColor = new Color(0.17f, 0.17f, 0.17f); // #2C2C2C
    private Color hoverColor = new Color(0.24f, 0.24f, 0.24f); // #3C3C3C

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = normalColor;
    }
}
```

---

**Version:** 1.0.0  
**Dernière MAJ:** 2026-01-09  
**Auteur:** NexA Dev Team

---

> 💡 **Conseil :** Imprimez cette page et gardez-la à côté de vous pendant la création !

