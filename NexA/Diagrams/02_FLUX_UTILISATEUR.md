# Flux Utilisateur et User Journeys

## Vue d'ensemble

Ce document décrit tous les parcours utilisateur dans le client NexA, avec séquences d'écrans et interactions.

---

## Diagramme 6: Flow Chart - Parcours Complet Utilisateur

**Type de diagramme souhaité**: Flowchart / User Flow

**Description**:
Parcours complet d'un utilisateur depuis le lancement jusqu'aux différentes fonctionnalités.

**Étapes**:

### 1. Démarrage Application
```
[Launch App] 
    ↓
[Check Token] ─── Token valide? ───→ OUI ─→ [Home Screen]
    ↓                                    
   NON
    ↓
[Login Screen]
```

### 2. Login/Register Flow
```
[Login Screen]
    ├─→ [Input Email/Password] → [Submit] → [API Call]
    │                                           ├─→ Success → [Home Screen]
    │                                           └─→ Error → [Toast Error] → [Login Screen]
    │
    └─→ [Click "Register"] → [Register Screen]
                                 ├─→ [Input Credentials] → [Submit] → [API Call]
                                 │                                        ├─→ Success → [Home Screen]
                                 │                                        └─→ Error → [Toast Error]
                                 └─→ [Click "Back"] → [Login Screen]
```

### 3. Home Screen (Hub)
```
[Home Screen]
    ├─→ [Button "Play"] → [Matchmaking] (futur)
    ├─→ [Button "Profile"] → [Profile Screen]
    ├─→ [Button "Friends"] → [Friends Screen]
    ├─→ [Button "History"] → [Match History Screen]
    └─→ [Button "Logout"] → [Confirm Dialog] → [Login Screen]
```

### 4. Profile Flow
```
[Profile Screen]
    ├─→ [Display User Info] (Avatar, Pseudo, Level, Stats)
    ├─→ [Edit Button] → [Edit Profile] (futur)
    └─→ [Back Button] → [Home Screen]
```

### 5. Friends Flow
```
[Friends Screen]
    ├─→ [Tab "Friends List"]
    │     ├─→ [Display Friends] (online/offline/in-game)
    │     ├─→ [Click Friend] → [Context Menu]
    │     │                       ├─→ View Profile
    │     │                       ├─→ Invite to Game (futur)
    │     │                       └─→ Remove Friend → [Confirm] → [Refresh List]
    │     └─→ [Refresh] → [API Call]
    │
    ├─→ [Tab "Pending Requests"]
    │     ├─→ [Display Requests]
    │     ├─→ [Accept] → [API Call] → [Move to Friends]
    │     └─→ [Decline] → [API Call] → [Remove from List]
    │
    ├─→ [Tab "Add Friend"]
    │     ├─→ [Search Input] → [API Call] → [Display Results]
    │     └─→ [Send Request] → [API Call] → [Toast Success]
    │
    └─→ [Back Button] → [Home Screen]
```

### 6. Match History Flow
```
[Match History Screen]
    ├─→ [Display Match List] (pagination)
    ├─→ [Scroll Down] → [Load More] → [API Call]
    ├─→ [Click Match] → [Match Details Screen]
    │                      ├─→ [Display Participants]
    │                      ├─→ [Display Stats]
    │                      └─→ [Back] → [Match History Screen]
    └─→ [Back Button] → [Home Screen]
```

---

## Diagramme 7: Séquence - Authentification (Login)

**Type de diagramme souhaité**: Sequence Diagram

**Description**:
Séquence détaillée du processus de login avec tous les acteurs.

**Acteurs**:
1. User (Joueur)
2. LoginScreen (UI)
3. AuthService (Unity)
4. APIService (Unity)
5. API Backend (NestJS)
6. Database (PostgreSQL)

**Séquence**:

```
User → LoginScreen: Entre email/password
User → LoginScreen: Click "Login"
LoginScreen → LoginScreen: Validation input (email format, password length)
    [Si invalide]
    LoginScreen → User: Toast "Invalid input"
    [Fin]

LoginScreen → AuthService: Login(email, password)
    LoginScreen → LoginScreen: Show loading spinner

AuthService → APIService: POST /auth/login {email, password}
APIService → API Backend: HTTPS Request
API Backend → API Backend: Validate DTO
API Backend → Database: SELECT user WHERE email = ?
Database → API Backend: User data
API Backend → API Backend: Verify password hash (bcrypt)
    [Si incorrect]
    API Backend → APIService: 401 Unauthorized
    APIService → AuthService: APIError
    AuthService → LoginScreen: Error
    LoginScreen → User: Toast "Invalid credentials"
    LoginScreen → LoginScreen: Hide loading spinner
    [Fin]

API Backend → API Backend: Generate JWT (access + refresh)
API Backend → Database: UPDATE user SET last_login = NOW()
API Backend → APIService: 200 OK {accessToken, refreshToken, user}
APIService → AuthService: Success response
AuthService → AuthService: Store tokens (PlayerPrefs encrypted)
AuthService → CacheService: Cache user data
AuthService → LoginScreen: Success
LoginScreen → UIManager: Navigate(HomeScreen)
UIManager → HomeScreen: Show with fade animation
HomeScreen → User: Display home
```

**Points critiques**:
- Validation côté client AVANT appel API
- Loading state pendant l'appel
- Gestion erreur réseau (timeout, no internet)
- Stockage sécurisé des tokens
- Cache user data pour éviter re-fetch

---

## Diagramme 8: Séquence - Chargement Liste d'Amis

**Type de diagramme souhaité**: Sequence Diagram

**Description**:
Flux de chargement de la liste d'amis avec cache.

**Acteurs**:
1. User
2. FriendsScreen
3. APIService
4. CacheService
5. API Backend
6. Database

**Séquence**:

```
User → HomeScreen: Click "Friends"
HomeScreen → UIManager: Navigate(FriendsScreen)
UIManager → FriendsScreen: Show screen
FriendsScreen → FriendsScreen: OnScreenShown()
FriendsScreen → CacheService: GetCachedFriends()
CacheService → CacheService: Check cache validity (< 5 min)
    [Si cache valide]
    CacheService → FriendsScreen: Cached friends list
    FriendsScreen → User: Display friends instantly
    [Fin]

    [Si cache invalide ou vide]
    FriendsScreen → User: Show skeleton UI
    FriendsScreen → APIService: GET /friends
    APIService → API Backend: HTTPS Request + JWT
    API Backend → API Backend: Validate JWT
    API Backend → Database: SELECT friends + status
    Database → API Backend: Friends data
    API Backend → APIService: 200 OK {friends: [...]}
    APIService → FriendsScreen: Friends list
    FriendsScreen → CacheService: UpdateCache(friends)
    FriendsScreen → FriendsScreen: Hide skeleton
    FriendsScreen → User: Display friends with animation (stagger)
```

**Optimisations**:
- Skeleton UI pendant loading (pas de spinner vide)
- Stagger animation (items apparaissent progressivement)
- Cache 5 minutes (réduire appels API)
- Pull-to-refresh pour forcer update

---

## Diagramme 9: Séquence - Envoi Demande d'Ami

**Type de diagramme souhaité**: Sequence Diagram

**Description**:
Processus complet d'envoi d'une demande d'ami.

**Acteurs**:
1. User
2. FriendsScreen (Tab "Add Friend")
3. APIService
4. API Backend
5. Database

**Séquence**:

```
User → FriendsScreen: Click tab "Add Friend"
FriendsScreen → User: Show search input
User → FriendsScreen: Enter search query "John"
FriendsScreen → FriendsScreen: Debounce 300ms
FriendsScreen → APIService: GET /users/search?q=John
APIService → API Backend: HTTPS Request + JWT
API Backend → API Backend: Validate JWT, Rate limit check
API Backend → Database: SELECT users WHERE username LIKE '%John%' LIMIT 10
Database → API Backend: User results
API Backend → APIService: 200 OK {users: [...]}
APIService → FriendsScreen: User list
FriendsScreen → User: Display results

User → FriendsScreen: Click "Add Friend" on "JohnDoe"
FriendsScreen → APIService: POST /friends/request {targetUserId: 123}
    FriendsScreen → FriendsScreen: Disable button, show loading
APIService → API Backend: HTTPS Request + JWT
API Backend → API Backend: Validate JWT
API Backend → Database: Check if already friends/pending
    [Si déjà amis ou pending]
    Database → API Backend: Conflict detected
    API Backend → APIService: 409 Conflict
    APIService → FriendsScreen: Error
    FriendsScreen → User: Toast "Already sent or friends"
    FriendsScreen → FriendsScreen: Re-enable button
    [Fin]

API Backend → Database: INSERT INTO friendships (requester, receiver, status='pending')
API Backend → Database: Create notification (futur)
Database → API Backend: Success
API Backend → APIService: 201 Created
APIService → FriendsScreen: Success
FriendsScreen → FriendsScreen: Remove from search results
FriendsScreen → User: Toast "Friend request sent!"
FriendsScreen → FriendsScreen: Re-enable button
```

**Gestion d'erreurs**:
- Rate limit dépassé → Toast "Too many requests, wait 1 min"
- User not found → Toast "User not found"
- Already friends → Toast "Already friends"
- Network error → Toast "Network error, retry"

---

## Diagramme 10: État des Écrans (State Machine)

**Type de diagramme souhaité**: State Machine Diagram

**Description**:
Machine à états de la navigation UI.

**États**:

1. **Splash** (initial)
   - Loading assets
   - Check auth token
   → LoginScreen (si pas de token)
   → HomeScreen (si token valide)

2. **LoginScreen**
   - Idle
   - Loading (pendant API call)
   - Error (affiche toast)
   → RegisterScreen (click "Register")
   → HomeScreen (login success)

3. **RegisterScreen**
   - Idle
   - Loading
   - Error
   → LoginScreen (click "Back" ou success)

4. **HomeScreen** (hub central)
   - Idle
   → ProfileScreen
   → FriendsScreen
   → MatchHistoryScreen
   → Matchmaking (futur)
   → LoginScreen (logout)

5. **ProfileScreen**
   - Loading
   - Display
   - Edit (futur)
   → HomeScreen (back)

6. **FriendsScreen**
   - Tab: Friends List
   - Tab: Pending Requests
   - Tab: Add Friend
   → HomeScreen (back)

7. **MatchHistoryScreen**
   - Loading
   - Display list
   - Loading more (pagination)
   → MatchDetailsScreen (click match)
   → HomeScreen (back)

8. **MatchDetailsScreen**
   - Loading
   - Display
   → MatchHistoryScreen (back)

**Transitions globales**:
- Tout état → LoginScreen (si token expire)
- Tout état → Error overlay (si erreur critique)

**Règles**:
- UIManager gère l'état global
- Un seul écran actif à la fois
- Transitions animées (fade/slide)
- Back button retourne à l'écran précédent

---

## Diagramme 11: User Journey Map - Première Session

**Type de diagramme souhaité**: User Journey Map / Experience Map

**Description**:
Parcours émotionnel d'un nouvel utilisateur lors de sa première session.

**Timeline**:

### Phase 1: Découverte (0-30 sec)
**Étape**: Lancement app
**Action**: Voir écran login
**Émotion**: Curiosité 😊
**Pensée**: "C'est quoi ce jeu ?"
**Pain point**: Aucun
**UI State**: Login screen avec animation d'intro

### Phase 2: Onboarding (30 sec - 2 min)
**Étape**: Création de compte
**Action**: Register form
**Émotion**: Neutre 😐
**Pensée**: "Encore un formulaire..."
**Pain point**: 
- Email déjà utilisé ?
- Password trop court ?
**Amélioration**: 
- Validation en temps réel
- Indicateur force password
**UI State**: Register screen avec feedback instantané

### Phase 3: Premier succès (2-3 min)
**Étape**: Login réussi
**Action**: Arrivée sur Home
**Émotion**: Satisfait 😊
**Pensée**: "Ok, je suis dedans!"
**Pain point**: Aucun
**UI State**: Home screen avec animation d'accueil

### Phase 4: Exploration (3-10 min)
**Étape**: Découverte des features
**Action**: 
- Voir profil (vide)
- Voir amis (vide)
- Voir historique (vide)
**Émotion**: Confusion 😕
**Pensée**: "C'est vide... qu'est-ce que je fais ?"
**Pain point**: **CRITIQUE** - No guidance
**Amélioration**:
- Tutorial overlay
- Dummy data pour démo
- Call-to-action clair: "Add your first friend!"
**UI State**: Home/Profile/Friends vides

### Phase 5: Action (10-15 min)
**Étape**: Ajout premier ami
**Action**: 
- Search user
- Send friend request
**Émotion**: Satisfait 😊
**Pensée**: "Ça marche !"
**Pain point**: "Personne n'accepte..."
**Amélioration**: 
- Bot friend auto-accept (pour démo)
**UI State**: Friends screen avec premier ami

### Phase 6: Engagement (15+ min)
**Étape**: Jouer première partie
**Action**: Click "Play"
**Émotion**: Excité 🤩
**Pensée**: "Let's go!"
**Pain point**: Matchmaking long
**UI State**: Matchmaking (futur)

**KPIs à mesurer**:
- % users qui complètent register
- % users qui ajoutent au moins 1 ami
- % users qui reviennent le lendemain
- Temps moyen avant premier "Play"

---

## Diagramme 12: Flow d'erreur et retry logic

**Type de diagramme souhaité**: Flowchart

**Description**:
Gestion des erreurs réseau avec retry automatique.

**Scénario**: API Call échoue

```
[API Call Initiated]
    ↓
[Try 1] ───→ Success? ───→ OUI ─→ [Return Data]
    ↓
   NON
    ↓
[Check Error Type]
    ├─→ Timeout / Network Error
    │      ↓
    │   [Wait 1 sec]
    │      ↓
    │   [Try 2] ───→ Success? ───→ OUI ─→ [Return Data]
    │      ↓
    │     NON
    │      ↓
    │   [Wait 2 sec]
    │      ↓
    │   [Try 3] ───→ Success? ───→ OUI ─→ [Return Data]
    │      ↓
    │     NON
    │      ↓
    │   [Toast "Network error. Please check your connection."]
    │      ↓
    │   [Return Error]
    │
    ├─→ 401 Unauthorized
    │      ↓
    │   [Try Refresh Token]
    │      ├─→ Success ─→ [Retry Original Request]
    │      └─→ Fail ─→ [Redirect to Login] + [Toast "Session expired"]
    │
    ├─→ 403 Forbidden
    │      ↓
    │   [Toast "Access denied"]
    │      ↓
    │   [Return Error]
    │
    ├─→ 429 Too Many Requests
    │      ↓
    │   [Get Retry-After header]
    │      ↓
    │   [Toast "Too many requests. Wait X seconds."]
    │      ↓
    │   [Wait X sec]
    │      ↓
    │   [Retry Request] (1 fois max)
    │
    ├─→ 500 Server Error
    │      ↓
    │   [Toast "Server error. Try again later."]
    │      ↓
    │   [Log to analytics]
    │      ↓
    │   [Return Error]
    │
    └─→ Other 4xx
           ↓
        [Parse error message]
           ↓
        [Toast error.message]
           ↓
        [Return Error]
```

**Retry Config**:
- Max retries: 3 (seulement pour network errors)
- Backoff: 1s, 2s, 4s (exponential)
- Retry uniquement GET et idempotent POST
- Pas de retry pour 4xx (sauf 401)

---

## Métadonnées pour génération

### Palette d'émotions
- 🤩 Excité: #4CAF50 (vert)
- 😊 Satisfait: #8BC34A (vert clair)
- 😐 Neutre: #9E9E9E (gris)
- 😕 Confusion: #FF9800 (orange)
- 😤 Frustré: #F44336 (rouge)

### Icônes suggérées
- ✅ Success
- ❌ Error
- ⏳ Loading
- 🔄 Retry
- 🔒 Auth required

### Style flowchart
- Rectangles arrondis pour actions
- Diamants pour décisions
- Couleurs selon état (vert=success, rouge=error, bleu=process)


