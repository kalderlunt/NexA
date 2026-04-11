# 🚀 NexA - Guide de Démarrage Rapide

> **Client PC type Riot/League of Legends pour apprentissage**
> Stack : Unity (client) + Node.js (backend) + PostgreSQL + Debian

---

## ✅ Ce qui a été créé

### 📁 Architecture Unity Complète

```
Assets/Hub/Script/
├── Core/                   ✅ UIManager, ScreenBase
├── Screens/                ✅ Login, Register, Home, Profile, Friends, MatchHistory, MatchDetails
├── Components/             ✅ FriendListItem, MatchListItem, LoadingSpinner, ToastNotification
├── Services/               ✅ APIService, AuthManager, CacheManager, FriendsManager, MatchesManager
├── Models/                 ✅ User, Friend, Match, APIResponse, APIError, Enums
└── Utils/                  ✅ AnimationHelper, SecureStorage, CoroutineRunner
```

**Fonctionnalités implémentées :**
- ✅ Système d'authentification (login/register avec JWT)
- ✅ Navigation entre écrans avec state machine
- ✅ Animations DOTween (transitions, hover, loading)
- ✅ Gestion des amis (liste, recherche, demandes)
- ✅ Historique de matchs (pagination cursor-based)
- ✅ Cache en mémoire (éviter appels API répétés)
- ✅ Gestion d'erreurs robuste (retry, timeout)
- ✅ Notifications toast (success/error/info)
- ✅ Correlation IDs (traçabilité logs)

---

## 📋 Prochaines Étapes

### 1. **Setup Unity (Interface)**

#### a) Créer la hiérarchie de scène

```
MainHub.unity
└── Canvas
    ├── UIManager (+ script)
    │   ├── ScreensContainer
    │   │   ├── LoginScreen
    │   │   ├── RegisterScreen
    │   │   ├── HomeScreen
    │   │   ├── ProfileScreen
    │   │   ├── FriendsScreen
    │   │   ├── MatchHistoryScreen
    │   │   └── MatchDetailsScreen
    │   ├── FadeOverlay (Image noir + CanvasGroup)
    │   └── ToastContainer (Vertical Layout Group)
    └── Managers
        ├── APIService
        ├── AuthManager
        ├── CacheManager
        ├── FriendsManager
        └── MatchesManager
```

#### b) Configurer les écrans

Chaque écran doit avoir :
- `CanvasGroup` (pour animations fade)
- `RectTransform` (pour animations slide)
- Références UI sérialisées (inputs, buttons, texts, etc.)

**Exemple LoginScreen :**
```
LoginScreen (Panel)
├── CanvasGroup
├── FormPanel (RectTransform pour animation)
│   ├── Logo
│   ├── EmailInput (TMP_InputField)
│   ├── PasswordInput (TMP_InputField)
│   ├── LoginButton
│   └── RegisterButton
├── LoadingPanel (LoadingSpinner)
└── ErrorPanel (TextMeshProUGUI)
```

#### c) Créer les prefabs de liste

- `FriendListItem.prefab` : Layout horizontal (avatar, username, status, bouton)
- `MatchListItem.prefab` : Layout horizontal (résultat coloré, mode, durée, date)

#### d) Installer DOTween

- Asset Store ou Package Manager
- Créer `DOTweenSettings.asset` (Safe Mode ON, Recycle Tweens ON)

---

### 2. **Setup Backend Node.js**

#### a) Initialiser le projet

```bash
mkdir nexa-backend && cd nexa-backend
npm init -y
npm install express prisma @prisma/client bcrypt jsonwebtoken winston prom-client cors helmet express-rate-limit
npm install -D typescript @types/node @types/express @types/bcrypt @types/jsonwebtoken ts-node nodemon
npx tsc --init
```

#### b) Configurer Prisma

```bash
npx prisma init
# Éditer prisma/schema.prisma (voir BACKEND_ARCHITECTURE.md)
npx prisma migrate dev --name init
npx prisma generate
```

#### c) Structure du code

Créer les fichiers selon la structure dans `BACKEND_ARCHITECTURE.md` :
- Controllers (routes handlers)
- Services (business logic)
- Repositories (database access)
- Middleware (auth, logging, errors)
- Routes (API endpoints)

#### d) Fichier .env

```env
NODE_ENV=development
PORT=3000
DATABASE_URL="postgresql://user:password@localhost:5432/nexa"
JWT_SECRET="generate_with_openssl_rand_base64_32"
JWT_REFRESH_SECRET="another_secret"
JWT_EXPIRES_IN="1h"
JWT_REFRESH_EXPIRES_IN="7d"
```

#### e) Démarrer en dev

```bash
npm run dev  # ts-node + nodemon
```

---

### 3. **Setup PostgreSQL**

#### Linux/Mac
```bash
# Installer
sudo apt install postgresql postgresql-contrib  # Debian/Ubuntu
brew install postgresql                          # Mac

# Créer la DB
sudo -u postgres psql
CREATE DATABASE nexa;
CREATE USER nexauser WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE nexa TO nexauser;
\q
```

#### Windows
- Télécharger PostgreSQL Installer
- pgAdmin pour gérer la DB
- Créer la base `nexa` via l'interface

---

### 4. **Connecter Unity ↔ Backend**

#### Dans Unity (APIService Inspector)

```
Base URL: http://localhost:3000/api
Timeout Seconds: 10
Max Retries: 3
```

#### Tester le flow

1. **Start** → `LoginScreen` affiché
2. Clic "Register" → `RegisterScreen`
3. Créer un compte → Appel POST `/api/auth/register` → Token stocké → `HomeScreen`
4. Clic "Profile" → `ProfileScreen` (affiche stats)
5. Clic "Amis" → `FriendsScreen` (liste vide au début)
6. Clic "Historique" → `MatchHistoryScreen` (vide au début)
7. Logout → `LoginScreen`

---

## 🎨 Design System Animations

### Transitions d'écrans
- **Fade In** : alpha 0→1 + slide right→center (0.3s, OutQuad)
- **Fade Out** : alpha 1→0 + slide center→left (0.2s, InQuad)

### Composants
- **Pop In** : scale 0→1 (OutBack easing pour "bounce")
- **Hover** : scale 1→1.05 (0.2s)
- **Loading Spinner** : rotation 360° infinie
- **Toast** : slide Y -100→20, attente 3s, slide 20→-100

### Usage dans un Screen

```csharp
public override async Task ShowAsync(object data = null)
{
    gameObject.SetActive(true);
    await AnimationHelper.FadeInScreen(canvasGroup, rectTransform);
}

public override async Task HideAsync()
{
    await AnimationHelper.FadeOutScreen(canvasGroup, rectTransform);
    gameObject.SetActive(false);
}
```

---

## 🔐 Sécurité

### Tokens (Unity)
- **Access Token** : en mémoire uniquement (perdu au restart)
- **Refresh Token** : option de persister (désactivée par défaut pour sécurité)
- Auto-refresh géré par `AuthManager.GetValidAccessTokenAsync()`

### Backend
- Passwords : bcrypt (salt rounds 10+)
- JWT : secrets longs (32+ chars), expiration courte (1h)
- Rate limiting : 5 login attempts / 15 min par IP
- CORS : whitelist origins
- Helmet : headers de sécurité

---

## 📊 Observabilité

### Logs (Backend)
```json
{
  "timestamp": "2026-01-05T12:34:56.789Z",
  "level": "info",
  "type": "http_request",
  "correlationId": "uuid",
  "method": "POST",
  "path": "/api/auth/login",
  "statusCode": 200,
  "duration": 123,
  "userId": "uuid",
  "ip": "192.168.1.1"
}
```

### Métriques Prometheus
- `http_request_duration_seconds` (histogram)
- `db_query_duration_seconds` (histogram)
- `active_connections` (gauge)

### Dashboards Grafana
1. **API Health** : request rate, latency p95, errors 5xx
2. **Database** : query time, connections, slow queries
3. **Auth** : login success/fail, active sessions
4. **Business** : new users/day, matches/day

---

## 🐛 Debugging

### Unity
```csharp
// Logs structurés
Debug.Log($"[APIService] {method} {url} | Correlation: {correlationId}");

// Catch exceptions
try {
    await APIService.Instance.LoginAsync(email, password);
} catch (APIException ex) {
    Debug.LogError($"API Error {ex.Code}: {ex.Message}");
}
```

### Backend
```typescript
logger.info({
  type: 'api_call',
  correlationId: req.correlationId,
  userId: req.user?.id,
  action: 'login',
  success: true
});
```

### Tracer un problème
1. Unity affiche une erreur → copier le `correlationId`
2. Chercher dans les logs backend : `grep "correlationId" logs/combined.log`
3. Voir le stack trace complet côté serveur

---

## 🚀 MVP Roadmap

### Phase 1 : Core (1-2 semaines)
- [x] Architecture Unity
- [x] Architecture Backend
- [ ] UI Unity (prefabs, scènes)
- [ ] Backend endpoints Auth + Users
- [ ] Login/Register fonctionnel E2E

### Phase 2 : Features (2-3 semaines)
- [ ] Système d'amis complet
- [ ] Historique de matchs (fake data pour tester)
- [ ] Profil joueur avec stats
- [ ] Cache + optimisations

### Phase 3 : Polish (1 semaine)
- [ ] Animations finales
- [ ] Gestion d'erreurs améliorée
- [ ] Tests E2E
- [ ] Déploiement Debian

### Post-MVP
- [ ] WebSocket (présence temps réel)
- [ ] Matchmaking (création de parties)
- [ ] Lobby système
- [ ] Anti-triche basique

---

## 📚 Documentation

### Architecture
- **Unity** : `MDHelp/UNITY_ARCHITECTURE.md` (patterns, code samples)
- **Backend** : `MDHelp/BACKEND_ARCHITECTURE.md` (API contracts, DB schema)
- **Implémentation** : `MDHelp/UNITY_IMPLEMENTATION_GUIDE.md` (setup, tests)

### Code Samples
Tous les scripts principaux sont déjà créés dans `Assets/Hub/Script/` :
- Screens : Login, Register, Home, Profile, Friends, MatchHistory, MatchDetails
- Services : APIService, AuthManager, CacheManager, FriendsManager, MatchesManager
- Components : FriendListItem, MatchListItem, LoadingSpinner, ToastNotification
- Utils : AnimationHelper, SecureStorage, CoroutineRunner

---

## ❓ FAQ

### "Comment tester sans backend ?"
Créer un `MockAPIService` qui retourne des données fake :
```csharp
public async Task<User> LoginAsync(string email, string password)
{
    await Task.Delay(500); // Simuler latence
    return new User { id = "fake-id", username = "TestUser", level = 5 };
}
```

### "Comment gérer les avatars ?"
- **MVP** : avatar placeholder (icône par défaut)
- **V2** : upload d'image → S3/CloudFlare R2 → URL stockée en DB
- **Unity** : `UnityWebRequestTexture.GetTexture(url)` + cache

### "WebSocket pour la présence ?"
- **MVP** : REST API uniquement (polling toutes les 30s)
- **V2** : Socket.IO côté backend + Unity WebSocket client
- Événements : `user_online`, `user_offline`, `friend_request_received`

### "Comment tester en local ?"
- Backend : `http://localhost:3000/api`
- Unity : changer `baseURL` dans `APIService` Inspector
- Utiliser Postman pour tester les endpoints backend manuellement

---

## 🎯 Checklist Avant de Commencer

### Unity
- [ ] DOTween installé
- [ ] TextMeshPro importé
- [ ] Scène `MainHub.unity` créée
- [ ] Tous les scripts assignés dans les GameObjects
- [ ] Références UI connectées dans les inspecteurs

### Backend
- [ ] Node.js 20+ installé
- [ ] PostgreSQL installé et DB créée
- [ ] Prisma configuré et migrations appliquées
- [ ] `.env` créé avec les secrets
- [ ] `npm install` exécuté

### Tests
- [ ] Backend démarre sans erreur (`npm run dev`)
- [ ] Postman : POST `/api/auth/register` fonctionne
- [ ] Unity : Appuyer sur Play → LoginScreen s'affiche
- [ ] Unity : Cliquer "Register" → transition vers RegisterScreen

---

## 💡 Tips Pro

### Performance Unity
- Désactiver Raycast sur images décoratives
- Object pooling pour les listes (FriendListItem, MatchListItem)
- TextMeshPro plutôt que Text (legacy)
- Atlas pour les sprites UI

### Backend
- Indexer les colonnes fréquemment requêtées (email, username, userId)
- Utiliser des transactions pour les opérations multi-tables
- Rate limiter TOUS les endpoints publics
- Logger les slow queries (> 100ms)

### Sécurité
- **Jamais** de secrets en clair dans le code Unity
- **Toujours** valider côté serveur (ne jamais faire confiance au client)
- **Obligatoirement** HTTPS en production
- **Régulièrement** scanner les dépendances npm (`npm audit`)

---

## 🆘 Besoin d'Aide ?

### Erreurs Courantes

**Unity : "NullReferenceException dans UIManager"**
→ Vérifier que tous les écrans ont un `ScreenBase` et que `ScreenType` est unique.

**Unity : "Unauthorized" sur toutes les requêtes**
→ Vérifier que `AuthManager.SetTokens()` a été appelé après login/register.

**Backend : "Connection refused" (ECONNREFUSED)**
→ PostgreSQL n'est pas démarré : `sudo systemctl start postgresql`.

**Backend : "JWT malformed"**
→ Token invalide ou expiré, vérifier `JWT_SECRET` et l'expiration.

---

## 🎉 Prêt à Coder !

Tu as maintenant :
- ✅ Architecture complète Unity (18 fichiers créés)
- ✅ Contrats d'API backend documentés
- ✅ Schéma de base de données Prisma
- ✅ Logs & métriques configurables
- ✅ Guide de déploiement Debian
- ✅ Roadmap MVP → Production

**Next Steps :**
1. Créer les prefabs UI dans Unity
2. Implémenter le backend Node.js (auth + users)
3. Tester le flow Login/Register E2E
4. Ajouter les features amis & matchs
5. Déployer sur Debian et brancher Grafana

**Bon dev ! 🚀🎮**

