# 🎮 NexA Client - Architecture Complète

## 📋 Stack Technique

**Client**: Unity 2022+ (C#) + DOTween  
**Backend**: Node.js 20+ avec NestJS  
**Database**: PostgreSQL 15+  
**Observabilité**: Prometheus + Grafana + Loki  
**Infra**: Debian 12, Nginx, systemd  

---

## 🎯 MVP - Features Minimales (2-3 semaines)

### ✅ Phase 1: Core (Semaine 1)
- [ ] Auth: Register/Login avec JWT (access + refresh tokens)
- [ ] Backend: Routes auth + users + middleware
- [ ] DB: Schema users + migrations
- [ ] Unity: Écrans Login/Register + APIService de base
- [ ] Unity: Gestionnaire de tokens + retry logic
- [ ] Observabilité: Logs JSON structurés

### ✅ Phase 2: Social (Semaine 2)
- [ ] Backend: Système d'amis complet (request/accept/decline/list)
- [ ] DB: Tables friend_requests + friendships
- [ ] Unity: Écran amis + recherche utilisateurs
- [ ] Unity: Notifications toasts (demandes d'amis)
- [ ] Backend: Pagination cursor-based

### ✅ Phase 3: Match History (Semaine 3)
- [ ] Backend: Endpoints matches + match details
- [ ] DB: Tables matches + match_participants
- [ ] Unity: Écran historique paginé
- [ ] Unity: Écran détails d'un match
- [ ] DOTween: Animations complètes (transitions, hover, loading)
- [ ] Métriques Prometheus de base

---

## 🗺️ ROADMAP (3 Phases)

### 🚀 MVP (Semaines 1-3)
- Auth JWT (access + refresh)
- CRUD amis (requests + friendships)
- Historique matches (read-only, données mockées)
- UI statique animée (DOTween)
- Logs JSON + métriques basiques

**Critères de succès**:
- Je peux créer un compte, me login, voir mon profil
- Je peux ajouter/accepter/refuser des amis
- Je peux voir l'historique de mes matchs (données mockées OK)
- L'UI est fluide avec animations

### 🎨 Beta (Semaines 4-6)
- **WebSocket**: Présence temps réel (online/offline/in-game)
- **Profil avancé**: Upload avatar, stats détaillées, niveau/elo
- **Match recording**: API pour enregistrer les résultats de parties
- **Rate limiting**: IP-based + user-based (express-rate-limit)
- **Cache Redis**: Sessions, friends lists, user profiles
- **Dashboards Grafana**: 3-4 dashboards complets

**Critères de succès**:
- Je vois le statut de mes amis en temps réel
- Les parties réelles sont enregistrées depuis le jeu
- L'API résiste à 1000 req/s (basic load test)

### 🏆 Prod-Like (Semaines 7-10)
- **Matchmaking/Lobby**: Queue système, team formation
- **Anti-cheat**: Signature des payloads client, rate limits agressifs
- **Sécurité**: HTTPS strict, CORS configuré, helmet.js, audit npm
- **CI/CD**: GitHub Actions → build → deploy Debian
- **Backup DB**: Automated backups + restore testing
- **Monitoring avancé**: Alertes Grafana, tracing distribué (Jaeger/Tempo)

**Critères de succès**:
- Je peux faire un matchmaking et rejoindre une partie
- Le système détecte les comportements anormaux
- 99% uptime sur 1 mois
- Temps de déploiement < 5 min

---

## 🔑 Choix Technologiques

### Pourquoi NestJS (vs Express) ?
✅ **Architecture structurée** par défaut (modules/controllers/services)  
✅ **TypeScript natif** avec décorateurs (validation, guards, interceptors)  
✅ **Middleware écosystème** riche (Passport, TypeORM/Prisma, Bull)  
✅ **WebSocket** intégré (Socket.io) pour la future présence temps réel  
✅ **Testing** bien intégré (Jest)  
❌ Légèrement plus verbeux qu'Express, mais la structure vaut le coup

### Pourquoi cursor-based pagination ?
- Plus performant à grande échelle (pas de COUNT(*))
- Pas de problème de "page drift" si données ajoutées en temps réel
- Standard pour les apps sociales (Discord, Twitter, etc.)

### Pourquoi Loki (vs Elasticsearch) ?
- Plus léger, moins de ressources
- Intégration native Grafana
- Suffisant pour < 1M logs/jour
- Possibilité de migrer vers ELK plus tard


