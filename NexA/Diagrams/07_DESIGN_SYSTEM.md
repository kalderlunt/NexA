# Design System & Animations UI

## Vue d'ensemble

Ce document décrit le design system complet pour l'UI du client NexA, avec animations, composants, et guidelines.

---

## Diagramme 43: Design System - Palette de Couleurs

**Type de diagramme souhaité**: Color Palette / Style Guide

**Description**:
Palette de couleurs complète inspirée de League of Legends.

### Couleurs Primaires

**Gold (Accent principal)**
```
Primary Gold:     #C8AA6E  (boutons primaires, highlights)
Gold Hover:       #F0E6D2  (hover state)
Gold Dark:        #785A28  (pressed state)
```

**Dark Blues (Background)**
```
Background Dark:  #010A13  (fond principal)
Background Mid:   #1E2328  (panels)
Background Light: #31313C  (cards, items)
```

**Grays (Text & Borders)**
```
Text Primary:     #F0E6D2  (titres)
Text Secondary:   #A09B8C  (body text)
Text Disabled:    #5B5A56  (disabled)
Border:           #3C3C41  (séparateurs)
```

### Couleurs Sémantiques

**Status Colors**
```
Success:          #00C853  (accepté, victoire)
Error:            #D32F2F  (refusé, défaite)
Warning:          #FFA726  (pending, attention)
Info:             #42A5F5  (information)
```

**Friend Status**
```
Online:           #00C853  (vert)
Offline:          #78909C  (gris)
In-Game:          #FFA726  (orange)
Away:             #FDD835  (jaune)
```

### Gradients

**Background Gradient** (pour panels importants)
```
linear-gradient(135deg, #1E2328 0%, #010A13 100%)
```

**Gold Gradient** (boutons CTA)
```
linear-gradient(90deg, #C8AA6E 0%, #F0E6D2 100%)
```

---

## Diagramme 44: Typography System

**Type de diagramme souhaité**: Typography Guide

**Description**:
Hiérarchie typographique et usages.

### Font Family
**Primary**: Poppins (Google Fonts)
**Fallback**: Arial, sans-serif

### Échelle Typographique

| Niveau | Size | Weight | Line Height | Usage |
|--------|------|--------|-------------|-------|
| H1 | 48px | Bold (700) | 1.2 | Titre principal (Login screen) |
| H2 | 36px | Bold (700) | 1.3 | Titres de sections |
| H3 | 28px | SemiBold (600) | 1.4 | Sous-titres |
| H4 | 24px | SemiBold (600) | 1.4 | Card titles |
| Body Large | 18px | Regular (400) | 1.6 | Corps principal |
| Body | 16px | Regular (400) | 1.6 | Texte standard |
| Body Small | 14px | Regular (400) | 1.5 | Descriptions |
| Caption | 12px | Regular (400) | 1.4 | Timestamps, labels |
| Button | 16px | SemiBold (600) | 1 | Texte boutons |

### Examples Unity (TextMeshPro)
```
[SerializeField] private TMP_Text titleText;        // H2, #F0E6D2
[SerializeField] private TMP_Text usernameText;     // Body Large, #F0E6D2
[SerializeField] private TMP_Text statusText;       // Body Small, #A09B8C
[SerializeField] private TMP_Text timestampText;    // Caption, #5B5A56
```

---

## Diagramme 45: Component Library

**Type de diagramme souhaité**: Component Showcase

**Description**:
Bibliothèque de composants UI réutilisables.

### Button Variants

#### Primary Button (Gold)
**Usage**: Actions principales (Login, Send Request)
**Specs**:
- Background: #C8AA6E
- Text: #010A13 (dark for contrast)
- Height: 48px
- Border Radius: 4px
- Font: Button (16px, SemiBold)
**States**:
- Normal: #C8AA6E
- Hover: #F0E6D2 + scale 1.02
- Pressed: #785A28 + scale 0.98
- Disabled: #5B5A56 + opacity 0.5

#### Secondary Button (Transparent)
**Usage**: Actions secondaires (Cancel, Back)
**Specs**:
- Background: transparent
- Border: 2px solid #C8AA6E
- Text: #C8AA6E
- Height: 48px
**States**:
- Hover: Background #C8AA6E20 (20% opacity)
- Pressed: Background #C8AA6E40

#### Icon Button
**Usage**: Menus, actions rapides
**Specs**:
- Size: 40x40px
- Background: #31313C
- Icon color: #A09B8C
**States**:
- Hover: Background #3C3C41 + Icon #F0E6D2

### Input Fields

**Text Input**
**Specs**:
- Height: 48px
- Background: #1E2328
- Border: 1px solid #3C3C41
- Text: #F0E6D2 (16px)
- Placeholder: #5B5A56
- Padding: 12px
- Border Radius: 4px
**States**:
- Focus: Border #C8AA6E (2px)
- Error: Border #D32F2F (2px)
- Success: Border #00C853 (1px)

### Cards

**Friend List Item Card**
**Dimensions**:
- Width: Stretch (parent)
- Height: 80px
- Padding: 12px
- Border Radius: 8px
- Background: #1E2328
**Hover**: Background #31313C + scale 1.01

**Match List Item Card**
**Dimensions**:
- Width: Stretch
- Height: 120px
- Padding: 16px
- Background: #1E2328
**Hover**: Background #31313C

### Badges

**Status Badge**
**Specs**:
- Size: 12x12px circle
- Border: 2px solid #010A13 (for contrast on avatar)
- Position: Bottom-right of avatar
**Colors**:
- Online: #00C853
- Offline: #78909C
- In-Game: #FFA726
- Away: #FDD835

**Level Badge**
**Specs**:
- Background: #C8AA6E
- Text: #010A13 (Bold)
- Padding: 4px 8px
- Border Radius: 12px (pill shape)
- Font: Caption (12px, Bold)

---

## Diagramme 46: Animation Timing & Easing

**Type de diagramme souhaité**: Animation Timeline

**Description**:
Durées et easings standardisés pour toutes les animations.

### Durées Standard

| Type | Duration | Usage |
|------|----------|-------|
| Instant | 0ms | Feedback immédiat |
| Fast | 150ms | Micro-interactions (hover) |
| Normal | 300ms | Transitions UI standard |
| Moderate | 400ms | Slides, swipes |
| Slow | 600ms | Animations importantes |

### Easing Functions (DOTween)

| Ease | Usage | Exemple |
|------|-------|---------|
| Ease.Linear | Loops, progress bars | Loading spinner |
| Ease.OutQuad | Default (smooth deceleration) | Fade in/out |
| Ease.OutCubic | Slides | Screen transitions |
| Ease.OutBack | Pop effect (overshoot) | Notifications, modals |
| Ease.InOutQuad | Bidirectionnel | Hover in/out |
| Ease.OutElastic | Attention (bounce) | Achievement popup |

### Animation Rules

**Principe**: Animations doivent être **subtiles** et **purposeful**.

1. **Performance First**
   - Max 60 FPS (16ms per frame)
   - Limiter animations simultanées (max 5-10)
   - Utiliser CanvasGroup pour fade (éviter alpha par élément)

2. **Consistency**
   - Même easing pour même type d'action
   - Durées cohérentes dans toute l'app

3. **User Control**
   - Possibilité de skip animations longues
   - Option "Reduce Motion" (futur)

---

## Diagramme 47: Screen Transitions

**Type de diagramme souhaité**: Transition Flow

**Description**:
Animations de transition entre écrans.

### Fade Transition (Default)
**Usage**: Login → Home, Home → Profile
**Steps**:
1. Fade out current screen (300ms, OutQuad)
2. Wait 50ms (buffer)
3. Fade in new screen (300ms, OutQuad)
**Total**: ~650ms

**Code**:
```csharp
public async Task TransitionFade(BaseScreen from, BaseScreen to)
{
    // Fade out
    await AnimationService.Instance.FadeOut(from.CanvasGroup, 0.3f);
    
    // Small delay
    await Task.Delay(50);
    
    // Fade in
    await AnimationService.Instance.FadeIn(to.CanvasGroup, 0.3f);
}
```

### Slide Transition
**Usage**: Home → Friends (slide from right)
**Steps**:
1. New screen starts offscreen right (+1000px)
2. Slide in new screen (400ms, OutCubic)
3. Simultaneously fade out old screen (300ms, OutQuad)
**Total**: ~400ms

**Code**:
```csharp
public async Task TransitionSlide(BaseScreen from, BaseScreen to, Vector2 direction)
{
    // Start both animations
    var fadeTask = AnimationService.Instance.FadeOut(from.CanvasGroup, 0.3f);
    var slideTask = AnimationService.Instance.SlideIn(to.RectTransform, direction, 0.4f);
    
    // Wait for both
    await Task.WhenAll(fadeTask, slideTask);
}
```

### Modal Popup
**Usage**: Confirm dialog, Toast
**Steps**:
1. Show overlay fade in (200ms)
2. Modal scales from 0 → 1 with OutBack easing (300ms)
**Total**: ~300ms (simultaneous)

**Code**:
```csharp
public async Task ShowModal(GameObject modal, CanvasGroup overlay)
{
    modal.transform.localScale = Vector3.zero;
    
    // Fade overlay + scale modal (parallel)
    var fadeTask = AnimationService.Instance.FadeIn(overlay, 0.2f);
    var scaleTask = AnimationService.Instance.PopIn(modal.transform, 0.3f);
    
    await Task.WhenAll(fadeTask, scaleTask);
}
```

---

## Diagramme 48: Loading States

**Type de diagramme souhaité**: UI State Diagram

**Description**:
Différents états de chargement et leurs représentations.

### Skeleton UI
**Usage**: Chargement initial de listes (friends, matches)
**Design**:
- Shapes grises (#31313C) mimant le contenu
- Animation shimmer (gradient animé left→right)
- Durée shimmer: 1.5s loop

**Implementation**:
```csharp
public class SkeletonUI : MonoBehaviour
{
    [SerializeField] private Image shimmerImage;
    private Material shimmerMaterial;

    void Start()
    {
        shimmerMaterial = shimmerImage.material;
        AnimateShimmer();
    }

    void AnimateShimmer()
    {
        // Animate material property
        shimmerMaterial.DOFloat(1f, "_Progress", 1.5f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }
}
```

### Spinner
**Usage**: Attente courte (< 2 secondes)
**Design**:
- Icône circulaire rotative
- Couleur: #C8AA6E
- Size: 48x48px
- Rotation: 360° en 1s (Linear)

**Code**:
```csharp
public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private RectTransform spinnerRect;

    void OnEnable()
    {
        spinnerRect.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void OnDisable()
    {
        spinnerRect.DOKill();
    }
}
```

### Progress Bar
**Usage**: Upload/Download (futur - avatars)
**Design**:
- Height: 4px
- Background: #31313C
- Fill: Gradient gold (#C8AA6E → #F0E6D2)
- Animation: Smooth fill (0.3s per update)

### Empty State
**Usage**: Listes vides (no friends, no matches)
**Design**:
- Icône centrale (128x128px, #5B5A56)
- Texte: "No friends yet" (Body, #A09B8C)
- CTA Button: "Add Friend" (Primary)

---

## Diagramme 49: Toast Notifications

**Type de diagramme souhaité**: Component Specification

**Description**:
Système de notifications toast.

### Types de Toast

#### Success Toast
**Color**: #00C853
**Icon**: ✓ (checkmark)
**Duration**: 3 seconds
**Example**: "Friend request sent!"

#### Error Toast
**Color**: #D32F2F
**Icon**: ✗ (cross)
**Duration**: 5 seconds (plus long pour lire erreur)
**Example**: "Network error. Please try again."

#### Info Toast
**Color**: #42A5F5
**Icon**: ℹ (info)
**Duration**: 3 seconds
**Example**: "New match available"

#### Warning Toast
**Color**: #FFA726
**Icon**: ⚠ (warning)
**Duration**: 4 seconds
**Example**: "Session will expire in 5 minutes"

### Toast Specs

**Dimensions**:
- Max width: 400px
- Height: auto (min 60px)
- Padding: 16px
- Border Radius: 8px
- Position: Top center (anchored)
- Offset from top: 80px

**Animation**:
1. Slide down from top + fade in (300ms, OutBack)
2. Wait duration (3-5s)
3. Fade out (300ms, OutQuad)

**Implementation**:
```csharp
public enum ToastType { Success, Error, Info, Warning }

public class ToastNotification : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;

    private static readonly Dictionary<ToastType, Color> ColorMap = new Dictionary<ToastType, Color>
    {
        { ToastType.Success, new Color(0, 0.78f, 0.33f) },
        { ToastType.Error, new Color(0.83f, 0.18f, 0.18f) },
        { ToastType.Info, new Color(0.26f, 0.65f, 0.96f) },
        { ToastType.Warning, new Color(1f, 0.65f, 0.15f) }
    };

    public async void Show(string message, ToastType type, float duration = 3f)
    {
        messageText.text = message;
        backgroundImage.color = ColorMap[type];
        // Set icon...

        // Animate in
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition + Vector2.up * 100f;
        Vector2 targetPos = rect.anchoredPosition;
        
        rect.anchoredPosition = startPos;
        canvasGroup.alpha = 0;

        await Task.WhenAll(
            rect.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion(),
            canvasGroup.DOFade(1f, 0.3f).AsyncWaitForCompletion()
        );

        // Wait
        await Task.Delay((int)(duration * 1000));

        // Animate out
        await canvasGroup.DOFade(0f, 0.3f).AsyncWaitForCompletion();

        Destroy(gameObject);
    }
}
```

**UIManager Toast Queue**:
```csharp
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Transform toastContainer;
    
    private Queue<ToastData> _toastQueue = new Queue<ToastData>();
    private bool _isShowingToast = false;

    public void ShowToast(string message, ToastType type = ToastType.Info, float duration = 3f)
    {
        _toastQueue.Enqueue(new ToastData { Message = message, Type = type, Duration = duration });
        
        if (!_isShowingToast)
        {
            ProcessToastQueue();
        }
    }

    private async void ProcessToastQueue()
    {
        _isShowingToast = true;

        while (_toastQueue.Count > 0)
        {
            var toastData = _toastQueue.Dequeue();
            
            GameObject toastObj = Instantiate(toastPrefab, toastContainer);
            var toast = toastObj.GetComponent<ToastNotification>();
            
            toast.Show(toastData.Message, toastData.Type, toastData.Duration);
            
            // Wait for toast to finish (duration + animation time)
            await Task.Delay((int)((toastData.Duration + 0.6f) * 1000));
        }

        _isShowingToast = false;
    }
}
```

---

## Diagramme 50: Hover & Focus States

**Type de diagramme souhaité**: Interaction States

**Description**:
États interactifs des composants UI.

### Button States

**Normal → Hover → Pressed → Normal**

```csharp
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Transform contentTransform;

    private Color normalColor = new Color(0.78f, 0.67f, 0.43f); // #C8AA6E
    private Color hoverColor = new Color(0.94f, 0.9f, 0.82f);   // #F0E6D2
    private Color pressedColor = new Color(0.47f, 0.35f, 0.16f); // #785A28

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Color transition
        backgroundImage.DOColor(hoverColor, 0.15f);
        
        // Scale up slightly
        contentTransform.DOScale(1.02f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.DOColor(normalColor, 0.15f);
        contentTransform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        backgroundImage.DOColor(pressedColor, 0.1f);
        contentTransform.DOScale(0.98f, 0.1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        backgroundImage.DOColor(hoverColor, 0.1f);
        contentTransform.DOScale(1.02f, 0.1f);
    }
}
```

### List Item Hover

**FriendListItem hover**:
- Background: #1E2328 → #31313C (150ms)
- Scale: 1.0 → 1.01 (subtle)
- Border left: 0px → 4px gold (highlight)

```csharp
public class FriendListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image leftBorder;

    private Color normalBg = new Color(0.12f, 0.14f, 0.16f);    // #1E2328
    private Color hoverBg = new Color(0.19f, 0.19f, 0.24f);     // #31313C

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.DOColor(hoverBg, 0.15f);
        transform.DOScale(1.01f, 0.15f);
        leftBorder.DOFade(1f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.DOColor(normalBg, 0.15f);
        transform.DOScale(1f, 0.15f);
        leftBorder.DOFade(0f, 0.15f);
    }
}
```

### Input Focus

**Focus ring** (gold border):
- Border width: 1px → 2px
- Border color: #3C3C41 → #C8AA6E
- Glow: subtle outer shadow (optional)

---

## Diagramme 51: Responsive Layout

**Type de diagramme souhaité**: Layout Grid

**Description**:
System de layout responsive.

### Breakpoints (Résolutions cibles)

| Breakpoint | Width | Usage |
|------------|-------|-------|
| Desktop HD | 1920x1080 | Default (design de base) |
| Desktop | 1366x768 | Laptops standard |
| Tablet | 1024x768 | Tablets (futur support) |

### Grid System

**Container Max Width**: 1600px
**Padding**: 40px (sides)
**Column Gap**: 24px

### Safe Areas

**UI Safe Area** (pour éviter les bords écran):
- Top: 60px (header)
- Bottom: 40px
- Left/Right: 40px

---

## Diagramme 52: Iconography

**Type de diagramme souhaité**: Icon Library

**Description**:
Bibliothèque d'icônes utilisées.

### Icon Set
**Style**: Line icons (stroke 2px)
**Color**: #A09B8C (default), #F0E6D2 (active)
**Size**: 24x24px (standard), 32x32px (large), 16x16px (small)

### Icons essentiels

| Icon | Usage | Unicode |
|------|-------|---------|
| 👤 | Profile | U+1F464 |
| 👥 | Friends | U+1F465 |
| 📜 | History | U+1F4DC |
| ⚔️ | Play | U+2694 |
| ⚙️ | Settings | U+2699 |
| 🚪 | Logout | - |
| ✓ | Success | U+2713 |
| ✗ | Error | U+2717 |
| ➕ | Add | U+2795 |
| 🔍 | Search | U+1F50D |
| 🔔 | Notification | U+1F514 |
| ⋮ | Menu | U+22EE |

**Source**: Font Awesome / Material Icons (libres)

---

## Métadonnées pour génération

### Design Tools
- Figma (mockups)
- Adobe Color (palettes)
- Coolors.co (harmonies)

### Animation References
- Riot Client UI (inspiration)
- Material Design (motion guidelines)
- Apple Human Interface (animations)

### Accessibility
- Contrast ratio min: 4.5:1 (WCAG AA)
- Touch targets min: 44x44px
- Focus indicators toujours visibles


