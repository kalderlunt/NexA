# 🎯 NexA - Guide de Création UI Unity Étape par Étape

> **Guide pratique pour créer l'UI du client NexA dans Unity**  
> **Durée estimée :** 2-3 heures  
> **Niveau :** Intermédiaire

---

## 📋 Prérequis

### Logiciels
- ✅ Unity 2022.3 LTS ou plus récent
- ✅ Visual Studio ou Rider (pour le code)

### Packages Requis
1. **TextMeshPro** (via Package Manager)
   - Window → Package Manager
   - Unity Registry → TextMeshPro
   - Install
   - Import TMP Essentials

2. **DOTween** (via Asset Store)
   - Ouvrir Asset Store dans Unity
   - Chercher "DOTween"
   - Download & Import
   - Setup DOTween (choisir les modules)

3. **Newtonsoft.Json** (via Package Manager)
   - Package Manager → Add package by name
   - `com.unity.nuget.newtonsoft-json`

---

## 🚀 Partie 1 : Setup Initial (15 min)

### Étape 1.1 : Créer la Scène

1. File → New Scene
2. Sauvegarder : `Assets/Hub/Scenes/MainScene.unity`
3. Supprimer "Main Camera" (pas nécessaire pour UI pure)

### Étape 1.2 : Créer le Canvas Principal

1. **Clic droit dans Hierarchy** → UI → Canvas
2. Renommer en **"UIManager"**
3. Sélectionner UIManager, dans Inspector :

**Canvas Component :**
```
Render Mode: Screen Space - Overlay
```

**Canvas Scaler Component :**
```
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5 (slider au milieu)
```

**Note :** Unity créera automatiquement un EventSystem, c'est normal.

### Étape 1.3 : Créer la Structure de Base

Dans Hierarchy, sous UIManager, créer (Clic droit → Create Empty) :

1. **ScreensContainer** (Empty GameObject)
2. **FadeOverlay** (Clic droit → UI → Image)
3. **ToastContainer** (Empty GameObject)

**Configurer FadeOverlay :**
- Sélectionner FadeOverlay
- RectTransform :
  - Anchors : Click "Anchor Presets" → Stretch Both (Alt+Shift+Click)
  - Left, Top, Right, Bottom : 0
- Image Component :
  - Color : Noir (#000000)
  - Alpha : 0 (slider à gauche)
  - Source Image : None (blanc uni)

**Configurer ToastContainer :**
- RectTransform :
  - Anchors : Top Center
  - Pivot : X=0.5, Y=1
  - Pos X : 0
  - Pos Y : -50
  - Width : 500
  - Height : 100

### Étape 1.4 : Créer les Dossiers

Dans Project, créer cette structure :

```
Assets/Hub/
├── Scenes/
├── Scripts/
│   ├── Core/
│   ├── Screens/
│   ├── Services/
│   ├── Components/
│   ├── Models/
│   └── Utils/
├── Prefabs/
│   └── UI/
└── Resources/
    └── UI/
```

**Raccourci :** Clic droit → Create → Folder

---

## 🔐 Partie 2 : Créer LoginScreen (30 min)

### Étape 2.1 : Structure de Base

1. Sous **ScreensContainer**, créer Empty GameObject "LoginScreen"
2. Sélectionner LoginScreen :
   - RectTransform : Anchors → Stretch Both (Alt+Shift)
   - Left, Top, Right, Bottom : 0

### Étape 2.2 : Background

1. Sous LoginScreen : UI → Image → Renommer "Background"
2. RectTransform : Stretch Both
3. Image :
   - Color : Gradient ou couleur unie (#1A1A1A)
   - Source Image : None (ou votre image de fond)

### Étape 2.3 : Logo

1. Sous LoginScreen : UI → Image → "Logo"
2. RectTransform :
   - Anchors : Top Center
   - Pivot : 0.5, 1
   - Pos Y : -100
   - Width : 300
   - Height : 100
3. Image : Votre logo (temporairement couleur blanche)

### Étape 2.4 : FormPanel

1. Sous LoginScreen : Create Empty → "FormPanel"
2. Add Component : Canvas Group
3. RectTransform :
   - Anchors : Middle Center
   - Pivot : 0.5, 0.5
   - Width : 500
   - Height : 400
4. Add Component : Vertical Layout Group
   - Padding : 30 (tous les côtés)
   - Spacing : 20
   - Child Alignment : Upper Center
   - Child Force Expand : Width ON, Height OFF
   - Child Control Size : Both ON

### Étape 2.5 : Title

1. Sous FormPanel : UI → Text - TextMeshPro → "TitleText"
2. Si popup TMP Importer : "Import TMP Essentials"
3. TextMeshProUGUI Component :
   - Text : "Connexion"
   - Font Size : 32
   - Alignment : Center
   - Color : Blanc
4. Add Component : Layout Element
   - Min Height : 50

### Étape 2.6 : EmailInputField

1. Sous FormPanel : UI → Input Field - TextMeshPro → "EmailInputField"
2. TextMeshPro - Input Field :
   - Text : (vide)
   - Character Limit : 100
   - Content Type : Email Address
   - Placeholder : "Email"
3. Développer EmailInputField dans Hierarchy :
   - "Text Area" → "Placeholder" → Modifier text : "Email"
4. Layout Element :
   - Min Height : 50

### Étape 2.7 : PasswordInputField

1. Dupliquer EmailInputField (Ctrl+D)
2. Renommer "PasswordInputField"
3. Modifier :
   - Content Type : Password
   - Placeholder text : "Mot de passe"
   - Input Type : Password

### Étape 2.8 : LoginButton

1. Sous FormPanel : UI → Button - TextMeshPro → "LoginButton"
2. Développer LoginButton :
   - Sélectionner "Text (TMP)" : Modifier text → "Se connecter"
3. Button Component :
   - Transition : Color Tint
   - Normal Color : #4A90E2
   - Highlighted Color : #5BA3F5
   - Pressed Color : #3A7AC8
4. Layout Element :
   - Min Height : 60

### Étape 2.9 : RegisterButton

1. Dupliquer LoginButton
2. Renommer "RegisterButton"
3. Modifier text : "Créer un compte"
4. Button Normal Color : Transparent ou #2C2C2C

### Étape 2.10 : LoadingPanel

1. Sous LoginScreen : Create Empty → "LoadingPanel"
2. RectTransform : Stretch Both
3. Add Component : Canvas Group
4. Image Component : Semi-transparent noir (#000000, Alpha 0.7)
5. **Désactiver** le GameObject (checkbox en haut de Inspector)

**Note :** On créera le LoadingSpinner plus tard.

### Étape 2.11 : ErrorPanel

1. Sous LoginScreen : UI → Image → "ErrorPanel"
2. RectTransform :
   - Anchors : Bottom Center
   - Pivot : 0.5, 0
   - Pos Y : 50
   - Width : 500
   - Height : 60
3. Image : Couleur rouge semi-transparent (#E74C3C, Alpha 0.3)
4. Sous ErrorPanel : UI → Text - TextMeshPro → "ErrorText"
   - Anchors : Stretch Both
   - Alignment : Center
   - Color : Blanc
   - Font Size : 14
5. **Désactiver** ErrorPanel

**Résultat :** Vous devriez avoir un écran de login fonctionnel visuellement !

---

## 🏠 Partie 3 : Créer HomeScreen (45 min)

### Étape 3.1 : Structure de Base

1. Sous ScreensContainer : Create Empty → "HomeScreen"
2. RectTransform : Stretch Both
3. Canvas Group : Add Component
4. **Désactiver** HomeScreen (pour l'instant)

### Étape 3.2 : Background

Même processus que LoginScreen, mais avec image de fond différente.

### Étape 3.3 : TopBar

1. Sous HomeScreen : Create Empty → "TopBar"
2. RectTransform :
   - Anchors : Top Stretch
   - Pivot : 0.5, 1
   - Pos Y : 0
   - Height : 100
3. Add Component : Horizontal Layout Group
   - Padding : 20
   - Spacing : 20
   - Child Alignment : Middle Center
   - Child Force Expand : Width ON

### Étape 3.4 : UserInfoPanel

1. Sous TopBar : Create Empty → "UserInfoPanel"
2. Horizontal Layout Group
3. Ajouter enfants :
   - **Avatar** : UI → Image
     - Width/Height : 64x64
     - Add Component : Mask (with Image)
     - Mask : Show Mask Graphic OFF
   - **UsernameText** : Text TMP
   - **LevelText** : Text TMP
   - **EloText** : Text TMP

**Configuration Avatar Mask (Cercle) :**
- Image Component : Source Image → UI/Skin/Knob (cercle Unity par défaut)

### Étape 3.5 : MainPanel (Menu Principal)

1. Sous HomeScreen : Create Empty → "MainPanel"
2. RectTransform : Middle Center, Width 600, Height 400
3. Canvas Group : Add
4. Grid Layout Group :
   - Cell Size : 280 x 80
   - Spacing : 20 x 20
   - Start Corner : Upper Left
   - Constraint : Fixed Column Count = 2

### Étape 3.6 : Buttons du Menu

Créer 5 boutons sous MainPanel :

1. **PlayButton**
   - UI → Button TMP
   - Text : "JOUER"
   - Background Color : #4A90E2
   - Font Size : 24
   - **Exception Layout :** Add Layout Element
     - Min Width : 580 (2 colonnes)
     - Min Height : 120

2. **ProfileButton**
   - Text : "Profil"
   - Background : #2C2C2C

3. **FriendsButton**
   - Text : "Amis"
   - Background : #2C2C2C
   - **Badge :** Sous FriendsButton :
     - UI → Image → "NotificationBadge"
     - RectTransform : Top Right corner
     - Width/Height : 24x24
     - Circle Shape (UI/Skin/Knob)
     - Color : Rouge
     - Sous Badge : Text TMP → "3" (nombre de notifications)

4. **MatchHistoryButton**
   - Text : "Historique"

5. **StoreButton**
   - Text : "Boutique"

---

## 👥 Partie 4 : Créer FriendsScreen (45 min)

### Étape 4.1 : Structure de Base

1. Sous ScreensContainer : Create Empty → "FriendsScreen"
2. Stretch Both + Canvas Group
3. Désactiver

### Étape 4.2 : HeaderPanel

1. Sous FriendsScreen : Create Empty → "HeaderPanel"
2. Anchors : Top Stretch, Height : 80
3. Horizontal Layout Group

**Contenu :**
- BackButton (Button with "←" text)
- TitleText ("Amis")
- SearchInputField (TMP Input)
- SearchButton

### Étape 4.3 : TabsPanel

1. Create Empty → "TabsPanel"
2. Anchors : Top Stretch, Pos Y : -80, Height : 60
3. Horizontal Layout Group

**Créer 3 Tabs :**

**FriendsTab :**
1. Button TMP
2. Add Component : Toggle
3. Développer le bouton :
   - Background : Modifier color selon état
   - Text : "Amis (12)"
4. Toggle :
   - Is On : TRUE (default)
   - Toggle Group : (on va créer)

**RequestsTab & OnlineTab :**
- Dupliquer FriendsTab
- Modifier textes
- Is On : FALSE

**Toggle Group :**
1. Sélectionner TabsPanel
2. Add Component : Toggle Group
3. Pour chaque Tab :
   - Toggle Component → Group → Assigner TabsPanel

### Étape 4.4 : ContentPanel (ScrollView)

1. UI → Scroll View → "ContentPanel"
2. Anchors : Stretch Both (avec offset top pour les tabs)
3. Left : 0, Right : 0, Top : -140 (après tabs), Bottom : 0

**Configurer Scroll Rect :**
- Horizontal : FALSE
- Vertical : TRUE
- Movement Type : Elastic
- Inertia : TRUE

**Configurer Content :**
1. Développer ContentPanel → Viewport → Content
2. Sélectionner "Content"
3. Add Component : Vertical Layout Group
   - Spacing : 10
   - Child Control Size : Both ON
4. Add Component : Content Size Fitter
   - Vertical Fit : Preferred Size

### Étape 4.5 : EmptyStatePanel

1. Sous FriendsScreen : Create Empty → "EmptyStatePanel"
2. Middle Center, Width 400, Height 200
3. Vertical Layout :
   - Icon (Image) : Icône triste ou vide
   - MessageText : "Aucun ami pour le moment"
4. **Désactiver** (affiché seulement si liste vide)

---

## 📜 Partie 5 : Créer MatchHistoryScreen (30 min)

### Étape 5.1 : Structure Similaire à FriendsScreen

1. HeaderPanel (avec BackButton + Title + RefreshButton)
2. FiltersPanel (3 boutons : All, Victories, Defeats)
3. ContentPanel (ScrollView comme FriendsScreen)
4. LoadMoreButton (en bas)

**Astuce :** Dupliquer FriendsScreen et modifier !

### Étape 5.2 : Filters avec Toggles

1. Sous FiltersPanel : 3 Buttons avec Toggle
2. Add Toggle Group sur FiltersPanel
3. Chaque bouton :
   - Normal : Gris
   - Selected (Toggle On) : Bleu

### Étape 5.3 : LoadMoreButton

1. Bottom Stretch, Height 50
2. Text : "Charger plus de matchs"
3. Background : Semi-transparent

---

## 🧩 Partie 6 : Créer les Prefabs (60 min)

### Prefab 1 : ToastNotification

**Étape 6.1.1 : Créer dans la Scène**

1. Hierarchy (n'importe où, pas sous Canvas) : Create Empty → "ToastNotification"
2. Add Component : **RectTransform**
3. RectTransform :
   - Anchors : Middle Center
   - Pivot : 0.5, 0.5
   - Width : 400
   - Height : 80

**Étape 6.1.2 : Background**

1. Sous ToastNotification : UI → Image → "Background"
2. RectTransform :
   - Anchors : Stretch Both
   - Left, Top, Right, Bottom : 0
3. Image :
   - Color : #2C2C2C
   - Source Image : UI/Skin/Background (ou None)
   - Type : Sliced (si image 9-slice)
   - Raycast Target : ON

**Étape 6.1.3 : Icon**

1. Sous ToastNotification : UI → Image → "Icon"
2. RectTransform :
   - Anchors : Middle Left
   - Pivot : 0.5, 0.5
   - Pos X : 50
   - Pos Y : 0
   - Width : 40
   - Height : 40
3. Image :
   - Color : Blanc (#FFFFFF)
   - Preserve Aspect : ON
4. Add Component : **Layout Element**
   - Ignore Layout : TRUE (pour position manuelle)

**Étape 6.1.4 : MessageText**

1. Sous ToastNotification : UI → Text - TextMeshPro → "MessageText"
2. RectTransform :
   - Anchors : Stretch (horizontal)
   - Left : 80 (après icon + padding)
   - Right : 20
   - Top : 10
   - Bottom : 10
3. TextMeshProUGUI :
   - Text : "Message de notification"
   - Font Size : 16
   - Color : Blanc (#FFFFFF)
   - Alignment : Middle Left
   - Wrapping : Enabled
   - Overflow : Truncate

**Étape 6.1.5 : Composants Root**

1. Sélectionner ToastNotification (root)
2. Add Component : **Canvas Group**
   - Alpha : 1
   - Interactable : OFF
   - Block Raycasts : OFF
3. Add Component : Script → ToastNotification.cs (créer plus tard)

**Étape 6.1.6 : Créer le Prefab**

1. **Important :** Créer dossier `Assets/Resources/UI/` si inexistant
2. Glisser ToastNotification depuis Hierarchy vers `Resources/UI/`
3. Unity demande confirmation → "Original Prefab"
4. Supprimer ToastNotification de la scène

### Prefab 2 : FriendListItem

**Structure Finale :**
```
FriendListItem (Panel ROOT - SANS Image component)
├── Background (Image) ← Stretch Both, indépendant du layout
└── ContentPanel (Empty) ← Horizontal Layout Group
    ├── AvatarImage
    ├── StatusIndicator
    ├── InfoPanel
    ├── ActionButton
    └── MenuButton
```

---

**Étape 6.2.1 : Créer Root**

1. Sous ContentPanel → Viewport → Content : Clic droit → **Create Empty** → "FriendListItem"
2. RectTransform :
   - Anchors : Top Stretch (0, 1, 1, 1)
   - Pivot : 0.5, 1
   - Left : 0
   - Right : 0
   - Pos Y : 0
3. Add Component : **Layout Element**
   - ☑ Min Height : 80
   - ☑ Preferred Height : 80

**⚠️ Important :** Ne PAS ajouter de Horizontal Layout Group sur le root !

---

**Étape 6.2.2 : Background**

1. Sous FriendListItem : UI → Image → "Background"
2. RectTransform :
   - Anchors : Stretch Both (0, 0, 1, 1)
   - Left : 0
   - Right : 0
   - Top : 0
   - Bottom : 0
3. Image :
   - Color : #2C2C2C (RGB: 44, 44, 44)
   - Source Image : None (UI-Default)
   - Raycast Target : ☐ OFF
4. **Dans Hierarchy :** Déplacer Background en HAUT (premier enfant) pour qu'il soit derrière

**💡 Astuce :** Le Background sera automatiquement derrière les autres éléments car Unity dessine dans l'ordre de la hiérarchie.

---

**Étape 6.2.3 : ContentPanel**

1. Sous FriendListItem : Clic droit → **Create Empty** → "ContentPanel"
2. RectTransform :
   - Anchors : Stretch Both (0, 0, 1, 1)
   - Left : 0
   - Right : 0
   - Top : 0
   - Bottom : 0
3. Add Component : **Horizontal Layout Group**
   - Padding : Left 10, Right 10, Top 10, Bottom 10
   - Spacing : 10
   - Child Alignment : Middle Left
   - Child Force Expand : Width ☐ OFF, Height ☑ ON
   - Child Control Size : Width ☐ OFF, Height ☑ ON

**Hiérarchie actuelle :**
```
FriendListItem
├── Background (Image)
└── ContentPanel (Empty - Horizontal Layout)
```

---

**Étape 6.2.4 : AvatarImage**

1. Sous **ContentPanel** : UI → Image → "AvatarImage"
2. RectTransform :
   - Width : 50
   - Height : 50
3. Add Component : **Mask**
   - Show Mask Graphic : ☐ OFF
4. Add Component : **Layout Element**
   - ☑ Min Width : 50
   - ☑ Min Height : 50
   - ☑ Preferred Width : 50
   - ☑ Preferred Height : 50
5. Image :
   - Source Image : UI/Skin/Knob (cercle Unity)
   - Color : #FFFFFF (blanc)
   - Raycast Target : ☑ ON

---

**Étape 6.2.5 : StatusIndicator**

1. Sous **ContentPanel** : UI → Image → "StatusIndicator"
2. RectTransform :
   - Anchors : Bottom Right (1, 0, 1, 0) **IMPORTANT : relatif à ContentPanel**
   - Pivot : 1, 0
   - Pos X : -40 (pour être sur l'avatar, ajuster selon besoin)
   - Pos Y : 10
   - Width : 14
   - Height : 14
3. Image :
   - Source Image : UI/Skin/Knob (cercle)
   - Color : #00FF00 (vert = online)
4. Add Component : **Layout Element**
   - ☑ Ignore Layout : TRUE

**💡 Note :** StatusIndicator doit être **après** AvatarImage dans la hiérarchie pour être par-dessus visuellement.

---

**Étape 6.2.6 : InfoPanel**

1. Sous **ContentPanel** : Create Empty → "InfoPanel"
2. Add Component : **Vertical Layout Group**
   - Padding : 0
   - Spacing : 5
   - Child Alignment : Middle Left
   - Child Force Expand : Width ☐ OFF, Height ☐ OFF
   - Child Control Size : Width ☑ ON, Height ☑ ON
3. Add Component : **Layout Element**
   - ☑ Flexible Width : 1

---

**Étape 6.2.7 : UsernameText**

1. Sous **InfoPanel** : UI → Text - TextMeshPro → "UsernameText"
2. TextMeshProUGUI :
   - Text : "PlayerName"
   - Font Size : 16
   - Color : #FFFFFF (blanc)
   - Font Style : Bold
   - Alignment : Left + Middle
   - Overflow : Ellipsis
   - Wrapping : ☐ Disabled

---

**Étape 6.2.8 : StatusText**

1. Sous **InfoPanel** : UI → Text - TextMeshPro → "StatusText"
2. TextMeshProUGUI :
   - Text : "En ligne"
   - Font Size : 12
   - Color : #BDC3C7 (gris clair: 189, 195, 199)
   - Alignment : Left + Middle

**Étape 6.2.4 : AvatarImage**

1. Sous FriendListItem : UI → Image → "AvatarImage"
2. RectTransform :
   - Width : 50, Height : 50
3. Add Component : **Mask**
   - Show Mask Graphic : OFF
4. Add Component : **Layout Element**
   - Min Width : 50
   - Min Height : 50
   - Preferred Width : 50
   - Preferred Height : 50
5. Image :
   - Source Image : UI/Skin/Knob (cercle Unity par défaut)
   - Color : Blanc ou votre couleur d'avatar

**Étape 6.2.5 : StatusIndicator**

1. Sous FriendListItem : UI → Image → "StatusIndicator"
2. RectTransform :
   - Anchors : Bottom Right (de AvatarImage)
   - Pivot : 1, 0
   - Width : 14, Height : 14
   - Pos X : 0, Pos Y : 0
3. Image :
   - Source Image : UI/Skin/Knob (cercle)
   - Color : #00FF00 (vert = online)
4. Add Component : **Layout Element**
   - Ignore Layout : TRUE (pour ne pas être affecté par le Horizontal Layout)

**Étape 6.2.6 : InfoPanel**

1. Sous FriendListItem : Create Empty → "InfoPanel"
2. Add Component : **Vertical Layout Group**
   - Padding : 0
   - Spacing : 5
   - Child Alignment : Middle Left
   - Child Force Expand : Width OFF, Height OFF
   - Child Control Size : Width ON, Height ON
3. Add Component : **Layout Element**
   - Flexible Width : 1 (prend l'espace restant)

**Étape 6.2.7 : UsernameText**

1. Sous InfoPanel : UI → Text - TextMeshPro → "UsernameText"
2. TextMeshProUGUI :
   - Text : "PlayerName"
   - Font Size : 16
   - Color : Blanc (#FFFFFF)
   - Font Style : Bold
   - Alignment : Left + Middle
   - Overflow : Ellipsis (pour textes longs)

**Étape 6.2.8 : StatusText**

1. Sous **InfoPanel** : UI → Text - TextMeshPro → "StatusText"
2. TextMeshProUGUI :
   - Text : "En ligne"
   - Font Size : 12
   - Color : #BDC3C7 (gris clair: 189, 195, 199)
   - Alignment : Left + Middle

---

**Étape 6.2.9 : ActionButton**

1. Sous **ContentPanel** : UI → Button - TextMeshPro → "ActionButton"
2. RectTransform :
   - Width : 100
   - Height : 40
3. Add Component : **Layout Element**
   - ☑ Min Width : 100
   - ☑ Min Height : 40
   - ☑ Preferred Width : 100
   - ☑ Preferred Height : 40
4. Button :
   - Transition : Color Tint
   - Normal Color : #E74C3C (rouge: 231, 76, 60)
   - Highlighted Color : #FF6B5B
   - Pressed Color : #C0392B
   - Disabled Color : #7F8C8D
5. Text (enfant automatique) :
   - Text : "Supprimer"
   - Font Size : 14
   - Color : #FFFFFF
   - Alignment : Center

---

**Étape 6.2.10 : MenuButton**

1. Sous **ContentPanel** : UI → Button - TextMeshPro → "MenuButton"
2. RectTransform :
   - Width : 30
   - Height : 30
3. Add Component : **Layout Element**
   - ☑ Min Width : 30
   - ☑ Min Height : 30
   - ☑ Preferred Width : 30
   - ☑ Preferred Height : 30
4. Button :
   - Transition : Color Tint
   - Normal Color : Transparent (ou #2C2C2C)
   - Highlighted Color : #3C3C3C
5. Text (enfant) :
   - Text : "⋮" (Alt+8942 sur Windows, ou copier : ⋮)
   - Font Size : 20
   - Color : #FFFFFF
   - Alignment : Center

---

**Étape 6.2.11 : Vérification Hiérarchie Finale**

Votre hiérarchie doit ressembler à ceci :

```
FriendListItem (Empty - Layout Element: Min Height 80)
├── Background (Image - Stretch Both, Color: #2C2C2C)
└── ContentPanel (Empty - Horizontal Layout Group)
    ├── AvatarImage (Image + Mask - 50x50)
    ├── StatusIndicator (Image - 14x14, Ignore Layout)
    ├── InfoPanel (Empty - Vertical Layout, Flexible Width 1)
    │   ├── UsernameText (TMP - Font 16, Bold)
    │   └── StatusText (TMP - Font 12, Gray)
    ├── ActionButton (Button - 100x40, Red)
    └── MenuButton (Button - 30x30)
```

---

**Étape 6.2.12 : Ajuster StatusIndicator Position**

Le StatusIndicator doit être **sur l'avatar**, pas dans le layout. Ajustons sa position :

1. Sélectionner **StatusIndicator**
2. RectTransform :
   - Anchors : **Custom** (pas de preset)
   - Min : 0, 0
   - Max : 0, 0
   - Pivot : 1, 0
   - Pos X : 50 (largeur avatar)
   - Pos Y : 0
   - Pos Z : 0
   - Width : 14
   - Height : 14

**Ou bien (méthode plus simple) :**

Déplacer StatusIndicator **SOUS AvatarImage** dans la hiérarchie :

```
ContentPanel
├── AvatarImage
│   └── StatusIndicator ← Enfant d'Avatar !
├── InfoPanel
├── ActionButton
└── MenuButton
```

Puis :
1. StatusIndicator RectTransform :
   - Anchors : Bottom Right (1, 0, 1, 0)
   - Pivot : 1, 0
   - Pos X : 0
   - Pos Y : 0
   - Width : 14
   - Height : 14
2. Layout Element :
   - ☐ Ignore Layout : FALSE (pas besoin car enfant d'Avatar)

---

**Étape 6.2.13 : Test dans l'Éditeur**

1. Sélectionner **FriendListItem** dans Hierarchy
2. Dans Scene View, vérifier :
   - Background : gris foncé, plein écran
   - Avatar : cercle blanc 50x50 à gauche
   - Status : petit point vert en bas à droite de l'avatar
   - Textes : "PlayerName" (gros) et "En ligne" (petit) au centre
   - ActionButton : bouton rouge "Supprimer" (100px)
   - MenuButton : trois points (30px)
3. Hauteur totale : 80px

---

**Étape 6.2.14 : Créer le Prefab**

1. Drag **FriendListItem** depuis Hierarchy vers `Assets/Hub/Prefabs/UI/`
2. Unity crée le prefab
3. **Optionnel :** Supprimer de la scène (ou garder pour tests)

**Étape 6.2.9 : ActionButton**

1. Sous FriendListItem : UI → Button - TextMeshPro → "ActionButton"
2. RectTransform : Width 100, Height 40
3. Add Component : **Layout Element**
   - Min Width : 100
   - Min Height : 40
4. Button :
   - Normal Color : #E74C3C (rouge pour "Supprimer")
   - Highlighted Color : #FF6B5B
   - Pressed Color : #C0392B
5. Text (enfant) :
   - Text : "Supprimer"
   - Font Size : 14

**Étape 6.2.10 : MenuButton**

1. Sous FriendListItem : UI → Button - TextMeshPro → "MenuButton"
2. RectTransform : Width 30, Height 30
3. Add Component : **Layout Element**
   - Min Width : 30
   - Min Height : 30
4. Button :
   - Normal Color : Transparent
5. Text (enfant) :
   - Text : "⋮" (trois points verticaux)
   - Font Size : 20

**Étape 6.2.11 : Créer le Prefab**

1. Drag FriendListItem depuis Hierarchy vers `Assets/Hub/Prefabs/UI/`
2. Supprimer de la scène (ou garder pour tests)

### Prefab 3 : MatchListItem

**Étape 6.3.1 : Créer Root**

1. Créer UI → Panel → "MatchListItem"
2. RectTransform :
   - Anchors : Top Stretch
   - Height : 120
   - Left : 0, Right : 0
3. Add Component : **Button** (pour rendre cliquable)
   - Transition : Color Tint
   - Normal Color : Transparent
   - Highlighted Color : #FFFFFF (alpha 0.1)
4. Add Component : **Horizontal Layout Group**
   - Padding : 15 (tous côtés)
   - Spacing : 15
   - Child Alignment : Middle Left
   - Child Force Expand : Width OFF, Height ON
5. Add Component : **Layout Element**
   - Min Height : 120
   - Preferred Height : 120

**Étape 6.3.2 : Background**

1. Sous MatchListItem : UI → Image → "Background"
2. RectTransform : Stretch Both
3. Image :
   - Color : #2ECC71 (vert pour victoire, #E74C3C pour défaite)
   - Alpha : 0.1
   - Raycast Target : OFF

**Étape 6.3.3 : ResultPanel**

1. Sous MatchListItem : Create Empty → "ResultPanel"
2. Add Component : **Vertical Layout Group**
   - Spacing : 5
   - Child Alignment : Upper Center
3. Add Component : **Layout Element**
   - Min Width : 100
   - Preferred Width : 100

**Contenu ResultPanel :**
- **ResultIcon** (Image) : 40x40
  - Layout Element : Min 40x40
- **ResultText** (TMP) : "VICTOIRE"
  - Font Size : 14, Bold, Center
- **DateText** (TMP) : "Il y a 2h"
  - Font Size : 10, Color : #BDC3C7

**Étape 6.3.4 : ChampionPanel**

1. Sous MatchListItem : Create Empty → "ChampionPanel"
2. Add Component : **Vertical Layout Group**
3. Add Component : **Layout Element**
   - Min Width : 80
   - Preferred Width : 80

**Contenu :**
- **ChampionIcon** (Image + Mask) : 64x64
- **ChampionNameText** (TMP) : Font Size 10

**Étape 6.3.5 : GameInfoPanel**

1. Sous MatchListItem : Create Empty → "GameInfoPanel"
2. Add Component : **Vertical Layout Group**
   - Spacing : 5
3. Add Component : **Layout Element**
   - Flexible Width : 1 (prend l'espace restant)

**Contenu (4 TextMeshPro) :**
- **GameModeText** : "Ranked Solo" (Font 12)
- **DurationText** : "32:45" (Font 12)
- **ScoreText** : "15/3/8" (Font 14, Bold)
- **TeamScoreText** : "Team 1: 25 - Team 2: 18" (Font 10)

**Étape 6.3.6 : StatsPanel**

1. Sous MatchListItem : Create Empty → "StatsPanel"
2. Add Component : **Grid Layout Group**
   - Cell Size : 70 x 30
   - Spacing : 5 x 5
   - Start Corner : Upper Left
   - Start Axis : Horizontal
   - Constraint : Fixed Column Count = 3
3. Add Component : **Layout Element**
   - Min Width : 230
   - Preferred Width : 230

**Contenu (3 Badges) :**

Chaque badge a cette structure :
```
KDABadge (Panel)
├── Background (Image - couleur unie)
├── KDAValueText (TMP) : "8.67" - Font 14, Bold, Center
└── KDALabelText (TMP) : "KDA" - Font 8, Center
```

Créer 3 fois : KDABadge, CSBadge, DamageBadge

**Étape 6.3.7 : ItemsPanel**

1. Sous MatchListItem : Create Empty → "ItemsPanel"
2. Add Component : **Horizontal Layout Group**
   - Spacing : 4
3. Add Component : **Layout Element**
   - Min Width : 168 (6 items × 24 + 5 spacing × 5)
   - Preferred Width : 168

**Contenu :**
- 6× **ItemIcon** (Image) : 24x24 chacune
  - Layout Element : Min 24x24, Preferred 24x24

**Étape 6.3.8 : ArrowIcon**

1. Sous MatchListItem : UI → Image → "ArrowIcon"
2. RectTransform : 20x20
3. Add Component : **Layout Element**
   - Min Width : 20
   - Min Height : 20
4. Image :
   - Source Image : UI/Skin/UISprite (ou flèche custom)
   - Color : #BDC3C7

**Étape 6.3.9 : Créer Prefab**

1. Drag vers `Assets/Hub/Prefabs/UI/MatchListItem.prefab`
2. Supprimer de scène

---

## 🎨 Partie 7 : Styling & Polish (30 min)

### Étape 7.1 : Appliquer les Couleurs

**Créer des "Color Presets" :**
1. Créer un GameObject vide "ColorPalette" (juste pour référence)
2. Add Component : Script avec couleurs publiques

```csharp
public class ColorPalette : MonoBehaviour
{
    public Color primaryBlue = new Color(0.29f, 0.56f, 0.89f); // #4A90E2
    public Color successGreen = new Color(0.18f, 0.8f, 0.44f); // #2ECC71
    public Color errorRed = new Color(0.91f, 0.3f, 0.24f); // #E74C3C
    // ... etc
}
```

### Étape 7.2 : Animations de Base

**Hover Effect sur Boutons :**

1. Sélectionner un Button
2. Add Component : "ButtonHoverEffect" (script à créer)

**Script ButtonHoverEffect.cs :**
```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.05f, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.1f);
    }
}
```

### Étape 7.3 : Rounded Corners (Optionnel)

**Méthode 1 : Sprites avec coins arrondis**
- Créer dans Photoshop/Figma
- Importer comme Sprite
- Slicing 9-Slice

**Méthode 2 : Shader (Avancé)**
- Utiliser UI-Default-Rounded shader
- Ou télécharger depuis Asset Store

---

## 🔗 Partie 8 : Lier les Scripts (45 min)

### Étape 8.1 : Créer UIManager.cs

1. Assets/Hub/Scripts/Core/ : Create → C# Script → "UIManager"
2. Ouvrir dans IDE

**Code Basique :**
```csharp
using UnityEngine;
using System;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject friendsScreen;
    [SerializeField] private GameObject matchHistoryScreen;

    [Header("Overlays")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private Transform toastContainer;

    private GameObject _currentScreen;

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

    private void Start()
    {
        ShowScreen(loginScreen);
    }

    public void ShowScreen(GameObject screen)
    {
        if (_currentScreen != null)
            _currentScreen.SetActive(false);

        screen.SetActive(true);
        _currentScreen = screen;
    }
}
```

### Étape 8.2 : Attacher le Script

1. Sélectionner UIManager dans Hierarchy
2. Add Component → UIManager (le script)
3. **Assigner les références** dans Inspector :
   - Login Screen → Drag LoginScreen depuis Hierarchy
   - Home Screen → Drag HomeScreen
   - Etc.
   - Fade Overlay → Drag FadeOverlay
   - Toast Container → Drag ToastContainer

### Étape 8.3 : Créer LoginScreen.cs

```csharp
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        Debug.Log($"Login: {email}");

        // TODO: Call AuthManager
    }

    private void OnRegisterClicked()
    {
        Debug.Log("Go to Register");
        // TODO: Navigate to RegisterScreen
    }
}
```

### Étape 8.4 : Attacher LoginScreen.cs

1. Sélectionner LoginScreen dans Hierarchy
2. Add Component → LoginScreen
3. Assigner références :
   - Email Input → Drag EmailInputField
   - Password Input → Drag PasswordInputField
   - Login Button → Drag LoginButton
   - Register Button → Drag RegisterButton

### Étape 8.5 : Test de Base

1. Play Mode
2. Taper email/password
3. Cliquer Login → Check Console pour log

**Si ça marche : Bravo ! 🎉**

---

## ✅ Checklist Finale

### Setup
- [x] Canvas créé avec bon scaling
- [x] TextMeshPro importé
- [x] DOTween importé
- [x] Structure de dossiers créée

### Screens
- [x] LoginScreen complet et fonctionnel
- [x] HomeScreen structure de base
- [x] FriendsScreen avec tabs
- [x] MatchHistoryScreen avec scroll

### Prefabs
- [x] ToastNotification dans Resources/UI/
- [x] FriendListItem
- [x] MatchListItem (ou au moins structure)

### Scripts
- [x] UIManager.cs créé et attaché
- [x] LoginScreen.cs créé et attaché
- [x] Références assignées dans Inspector

### Tests
- [x] Play Mode sans erreurs
- [x] Navigation manuelle entre écrans fonctionne
- [x] Boutons cliquables
- [x] Input fields fonctionnels

---

## 🚀 Prochaines Étapes

### Phase 1 : Compléter les Screens
1. Finir RegisterScreen
2. Compléter HomeScreen avec animations
3. Ajouter ProfileScreen
4. Finir MatchDetailsScreen

### Phase 2 : Intégration Backend
1. Créer AuthManager.cs
2. Créer APIService.cs
3. Tester authentification réelle
4. Cache et gestion d'erreurs

### Phase 3 : Animations & Polish
1. Transitions entre écrans avec DOTween
2. Loading states
3. Error toasts
4. Hover effects sur tous les boutons

### Phase 4 : Optimisation
1. Object pooling pour listes
2. Lazy loading des prefabs
3. Profiling Unity
4. Optimisation mémoire

---

## 🆘 Besoin d'Aide ?

### Problèmes Courants

**"Missing references" dans Console :**
→ Vérifier que toutes les références sont assignées dans Inspector

**"Text illisible" :**
→ Import TMP Essentials + Vérifier font size

**"Canvas pas visible en Play Mode" :**
→ Vérifier Canvas Render Mode = Screen Space Overlay

**"Boutons ne fonctionnent pas" :**
→ Vérifier qu'EventSystem existe dans scène

### Ressources

- [Unity Learn - UI](https://learn.unity.com/tutorial/ui-components)
- [TextMeshPro Documentation](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
- [DOTween Documentation](http://dotween.demigiant.com/documentation.php)

---

**Bon courage ! 💪**

*Si vous suivez ce guide étape par étape, vous aurez une base solide pour votre client NexA en 2-3 heures.*

