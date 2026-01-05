# 📋 NexA - TODO List

## 🎯 MVP (Priorité 1 - 2 semaines)

### Unity Client
- [x] **Créer la scène MainHub.unity**
  - [ ] Canvas avec UIManager
  - [ ] ScreensContainer avec tous les écrans
  - [ ] FadeOverlay (CanvasGroup + Image)
  - [ ] ToastContainer (Vertical Layout)
  - [ ] Managers (APIService, AuthManager, etc.)

- [ ] **Créer les prefabs des écrans**
  - [ ] LoginScreen.prefab (EmailInput, PasswordInput, Buttons, Loading, Error)
  - [ ] RegisterScreen.prefab (Username, Email, Password, ConfirmPassword)
  - [ ] HomeScreen.prefab (PlayButton, ProfileButton, FriendsButton, HistoryButton)
  - [ ] ProfileScreen.prefab (Avatar, Username, Level, Stats, Winrate bar)
  - [ ] FriendsScreen.prefab (SearchBar, FriendsList, RequestsList)
  - [ ] MatchHistoryScreen.prefab (MatchList, LoadMoreButton)
  - [ ] MatchDetailsScreen.prefab (Result, Participants list)

- [ ] **Créer les prefabs de composants**
  - [ ] FriendListItem.prefab (Avatar, Username, StatusIndicator, ActionButton)
  - [ ] MatchListItem.prefab (ResultBg, GameMode, Duration, Date, Score)
  - [ ] LoadingSpinner.prefab (Rotating image)

- [ ] **Assigner les références dans les inspecteurs**
  - [ ] UIManager : screensContainer, fadeOverlay, toastContainer
  - [ ] Chaque Screen : tous les UI elements (buttons, inputs, panels, texts)
  - [ ] APIService : baseURL, timeout, retries

- [ ] **Installer les dépendances**
  - [ ] DOTween (Asset Store)
  - [ ] Newtonsoft.Json (Package Manager - com.unity.nuget.newtonsoft-json)

- [ ] **Tester le flow Unity sans backend**
  - [ ] Transitions Login → Register → Home
  - [ ] Animations (fade, slide, pop, hover)
  - [ ] Toasts (info, success, error)
  - [ ] Loading states

---

### Backend Node.js

- [ ] **Initialiser le projet**
  - [ ] `mkdir nexa-backend && cd nexa-backend`
  - [ ] `npm init -y`
  - [ ] Installer dépendances (voir BACKEND_ARCHITECTURE.md)
  - [ ] Configurer TypeScript (`tsconfig.json`)
  - [ ] Créer `.env` avec DATABASE_URL, JWT_SECRET, etc.

- [ ] **Setup Prisma**
  - [ ] `npx prisma init`
  - [ ] Copier le schema.prisma depuis BACKEND_ARCHITECTURE.md
  - [ ] `npx prisma migrate dev --name init`
  - [ ] `npx prisma generate`

- [ ] **Implémenter Auth**
  - [ ] `src/controllers/auth.controller.ts` (register, login, refresh, logout)
  - [ ] `src/services/auth.service.ts` (bcrypt, jwt, validation)
  - [ ] `src/middleware/auth.middleware.ts` (verify JWT)
  - [ ] `src/routes/auth.routes.ts`

- [ ] **Implémenter Users**
  - [ ] `src/controllers/users.controller.ts` (me, search)
  - [ ] `src/services/users.service.ts`
  - [ ] `src/routes/users.routes.ts`

- [ ] **Implémenter Friends**
  - [ ] `src/controllers/friends.controller.ts` (list, requests, accept, decline, remove)
  - [ ] `src/services/friends.service.ts`
  - [ ] `src/routes/friends.routes.ts`

- [ ] **Implémenter Matches**
  - [ ] `src/controllers/matches.controller.ts` (history, details)
  - [ ] `src/services/matches.service.ts` (pagination cursor-based)
  - [ ] `src/routes/matches.routes.ts`

- [ ] **Setup observabilité**
  - [ ] `src/utils/logger.ts` (Winston avec JSON)
  - [ ] `src/utils/metrics.ts` (Prometheus client)
  - [ ] Middleware logging (correlation IDs)
  - [ ] Endpoint `/metrics`

- [ ] **Middleware & sécurité**
  - [ ] Rate limiting (express-rate-limit)
  - [ ] CORS (whitelist origins)
  - [ ] Helmet (security headers)
  - [ ] Error handler centralisé

- [ ] **Tester avec Postman**
  - [ ] POST /auth/register (créer un compte)
  - [ ] POST /auth/login (obtenir tokens)
  - [ ] GET /me (vérifier auth)
  - [ ] Tous les autres endpoints

---

### Database PostgreSQL

- [ ] **Installer PostgreSQL**
  - [ ] Debian: `sudo apt install postgresql`
  - [ ] Mac: `brew install postgresql`
  - [ ] Windows: PostgreSQL Installer

- [ ] **Créer la base**
  ```sql
  CREATE DATABASE nexa;
  CREATE USER nexauser WITH PASSWORD 'secure_password';
  GRANT ALL PRIVILEGES ON DATABASE nexa TO nexauser;
  ```

- [ ] **Appliquer les migrations Prisma**
  - [ ] `npx prisma migrate dev`
  - [ ] Vérifier les tables créées: `\dt` dans psql

---

## 🔗 Intégration E2E (Priorité 2 - 3 jours)

- [ ] **Connecter Unity au backend local**
  - [ ] Changer `baseURL` dans APIService: `http://localhost:3000/api`
  - [ ] Démarrer le backend: `npm run dev`

- [ ] **Tester le flow complet**
  - [ ] Unity Play → LoginScreen
  - [ ] Register → Créer compte → Appel API → Token stocké → HomeScreen
  - [ ] Login → Connexion → Token stocké → HomeScreen
  - [ ] Profile → Charger stats → Affichage
  - [ ] Friends → Rechercher user → Envoyer demande
  - [ ] Friends → Accepter demande → Voir dans liste
  - [ ] History → Charger matchs (fake data) → Pagination
  - [ ] Details → Voir participants d'un match
  - [ ] Logout → Retour Login

- [ ] **Créer des données de test**
  - [ ] Script Prisma pour insérer fake users
  - [ ] Script pour insérer fake matches
  - [ ] Script pour créer friendships

---

## 🚀 Déploiement Debian (Priorité 3 - 2 jours)

- [ ] **Setup serveur**
  - [ ] Louer un VPS (DigitalOcean, Hetzner, AWS EC2)
  - [ ] Installer Node.js 20+ (`nvm install 20`)
  - [ ] Installer PostgreSQL
  - [ ] Installer Nginx
  - [ ] Configurer firewall (ports 80, 443, 22)

- [ ] **Déployer le backend**
  - [ ] Git clone du repo
  - [ ] `npm install && npm run build`
  - [ ] Créer `.env` de production
  - [ ] `npx prisma migrate deploy`
  - [ ] Créer service systemd (voir BACKEND_ARCHITECTURE.md)
  - [ ] `sudo systemctl start nexa-api`

- [ ] **Configurer Nginx**
  - [ ] Reverse proxy vers localhost:3000
  - [ ] Rate limiting
  - [ ] SSL avec Let's Encrypt (`certbot`)

- [ ] **Setup observabilité**
  - [ ] Installer Prometheus
  - [ ] Installer Grafana
  - [ ] Configurer dashboards (HTTP requests, DB queries, errors)
  - [ ] Installer Loki pour logs (optionnel)

- [ ] **Tester en production**
  - [ ] Unity: changer baseURL vers `https://api.nexa.game`
  - [ ] Vérifier HTTPS fonctionne
  - [ ] Vérifier métriques dans Grafana

---

## 🎨 Polish & UX (Priorité 4 - 3 jours)

- [ ] **Animations finales**
  - [ ] Victory/Defeat animations (particles, shake)
  - [ ] Friend request received notification (popup)
  - [ ] Level up animation
  - [ ] Smooth transitions partout

- [ ] **Skeleton loading**
  - [ ] Shimmer effect pendant chargement listes
  - [ ] Placeholder avatars

- [ ] **Gestion erreurs améliorée**
  - [ ] Retry automatique sur network error
  - [ ] Message "Connexion perdue" si offline
  - [ ] Bouton "Réessayer" sur erreurs

- [ ] **Optimisations**
  - [ ] Object pooling pour FriendListItem et MatchListItem
  - [ ] Lazy loading des avatars
  - [ ] Compression textures UI

- [ ] **Audio**
  - [ ] SFX sur boutons (clic, hover)
  - [ ] SFX sur notifications
  - [ ] Musique de fond (optionnel)

---

## 🧪 Tests (Priorité 5 - 2 jours)

### Tests Unitaires (C#)
- [ ] AuthManager.SetTokens() / GetValidAccessToken()
- [ ] CacheManager.Set() / Get() / Invalidate()
- [ ] AnimationHelper (toutes les méthodes)
- [ ] MatchesManager.CalculateWinrate()

### Tests Intégration (Backend)
- [ ] Auth flow complet (register → login → refresh)
- [ ] Friends flow (request → accept → list)
- [ ] Matches pagination (cursor-based)

### Tests E2E
- [ ] Playwright ou Selenium (automatiser le flow Unity)
- [ ] Scénarios: register, login, friends, matches, logout

---

## 🔮 Post-MVP (Futures Features)

### Temps Réel (WebSocket)
- [ ] Socket.IO côté backend
- [ ] Unity WebSocket client
- [ ] Présence amis en temps réel (online/offline/in-game)
- [ ] Notifications push (friend request, match found)

### Matchmaking
- [ ] Queue système (Redis)
- [ ] Algorithme de matching (ELO-based)
- [ ] Lobby screen
- [ ] Countdown avant match

### Chat
- [ ] Chat global
- [ ] Chat amis (1-to-1)
- [ ] Chat lobby (pre-match)

### Gameplay
- [ ] Game client (le jeu lui-même)
- [ ] Synchronisation réseau (Netcode for GameObjects ou Mirror)
- [ ] Anti-triche basique (server authority)

### Analytics
- [ ] Firebase Analytics
- [ ] Mixpanel (events tracking)
- [ ] Retention metrics
- [ ] A/B testing

### Monetization (si applicable)
- [ ] Cosmetics shop
- [ ] Battle pass
- [ ] In-app purchases (Unity IAP)

---

## 📝 Notes

### Dépendances Unity à Installer
```
- DOTween (Asset Store)
- TextMeshPro (Package Manager - com.unity.textmeshpro)
- Newtonsoft.Json (Package Manager - com.unity.nuget.newtonsoft-json)
```

### Dépendances Backend à Installer
```bash
npm install express prisma @prisma/client bcrypt jsonwebtoken \
  winston prom-client cors helmet express-rate-limit joi

npm install -D typescript @types/node @types/express \
  @types/bcrypt @types/jsonwebtoken ts-node nodemon
```

### Variables d'Environnement (.env)
```
NODE_ENV=development
PORT=3000
DATABASE_URL=postgresql://user:password@localhost:5432/nexa
JWT_SECRET=your_secret_here
JWT_REFRESH_SECRET=your_refresh_secret_here
JWT_EXPIRES_IN=1h
JWT_REFRESH_EXPIRES_IN=7d
ALLOWED_ORIGINS=http://localhost
LOG_LEVEL=info
```

---

## ✅ Checklist Avant Production

- [ ] Tous les logs Debug.Log() remplacés par un logger
- [ ] Aucun secret en clair dans le code Unity
- [ ] Toutes les URLs backend en variable d'environnement
- [ ] Rate limiting sur tous les endpoints
- [ ] Validation stricte côté serveur
- [ ] HTTPS obligatoire
- [ ] Backup automatique de la DB
- [ ] Monitoring (Grafana dashboards)
- [ ] Crash reporting (Sentry)
- [ ] Analytics configuré
- [ ] Tests E2E passent
- [ ] Documentation à jour

---

**Bon courage ! 💪🚀**

