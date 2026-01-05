# 📖 NexA - Index de Documentation

Guide complet pour développer un client PC type League of Legends avec Unity + NestJS + PostgreSQL

---

## 🎯 Démarrage Rapide

| Document                                 | Temps | Description |
|------------------------------------------|-------|-------------|
| **[README.md](./../README.md)**          | 5 min | Vue d'ensemble du projet, stack, roadmap |
| **[QUICK_START.md](./QUICK_START.md)**   | 15 min | Setup complet (DB + Backend + Unity) |
| **[DEPENDENCIES.md](./DEPENDENCIES.md)** | 10 min | Installation de toutes les dépendances |

---

## 📚 Documentation Détaillée

### Architecture & Design

| Document | Contenu | Lignes |
|----------|---------|--------|
| **[ARCHITECTURE.md](./ARCHITECTURE.md)** | Architecture complète, MVP, Roadmap 3 phases | ~250 |
| **[API_CONTRACT.md](./API_CONTRACT.md)** | 20+ endpoints REST documentés avec JSON | ~500 |
| **[DATABASE_SCHEMA.sql](./DATABASE_SCHEMA.sql)** | Schema PostgreSQL complet (7 tables) | ~400 |

### Implémentation

| Document | Contenu | Lignes |
|----------|---------|--------|
| **[UNITY_ARCHITECTURE.md](./UNITY_ARCHITECTURE.md)** | Architecture Unity, Design System DOTween | ~400 |
| **[BACKEND_NESTJS.md](./BACKEND_NESTJS.md)** | Backend NestJS avec exemples concrets | ~600 |
| **[OBSERVABILITY_OPS.md](./OBSERVABILITY_OPS.md)** | Logs, métriques, Grafana, déploiement Debian | ~700 |

### Résumé

| Document | Contenu | Lignes |
|----------|---------|--------|
| **[SUMMARY.md](./SUMMARY.md)** | Récapitulatif complet du livrable | ~350 |

**Total**: ~3200 lignes de documentation structurée

---

## 💻 Code Source

### Unity Scripts (C#)

```
Assets/Hub/Script/
├── Core/
│   ├── UIManager.cs              (~200 lignes) - Navigation entre écrans
│   └── ScreenBase.cs             (~50 lignes)  - Classe de base screens
├── Services/
│   ├── APIService.cs             (~350 lignes) - HTTP avec retry/timeout
│   ├── AuthManager.cs            (~200 lignes) - JWT auth + auto-refresh
│   └── CacheManager.cs           (~150 lignes) - Cache avec TTL
├── Screens/
│   ├── LoginScreen.cs            (~120 lignes) - Écran login complet
│   └── FriendsScreen.cs          (~400 lignes) - Liste/recherche/requests
├── Components/
│   └── ToastNotification.cs      (~150 lignes) - Notifications Material Design
├── Models/
│   └── DataModels.cs             (~200 lignes) - Tous les DTOs
└── Utils/
    ├── AnimationHelper.cs        (~300 lignes) - 30+ helpers DOTween
    └── SecureStorage.cs          (~80 lignes)  - Encryption tokens
```

**Total**: ~2200 lignes de code C# Unity

### Backend (NestJS - À implémenter)

Voir [BACKEND_NESTJS.md](./BACKEND_NESTJS.md) pour les snippets complets.

---

## 🗺️ Parcours de Lecture Recommandé

### 🚀 Je veux démarrer maintenant (30 min)

1. **[README.md](./README.md)** - Comprendre le projet (5 min)
2. **[QUICK_START.md](./QUICK_START.md)** - Setup complet (15 min)
3. Lancer Unity + Backend + Test login (10 min)

### 🏗️ Je veux comprendre l'architecture (1h)

1. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Architecture globale (15 min)
2. **[API_CONTRACT.md](./API_CONTRACT.md)** - Contrats API (20 min)
3. **[DATABASE_SCHEMA.sql](./DATABASE_SCHEMA.sql)** - Schema DB (15 min)
4. **[UNITY_ARCHITECTURE.md](./UNITY_ARCHITECTURE.md)** - Client Unity (10 min)

### 💻 Je veux implémenter le backend (2h)

1. **[DEPENDENCIES.md](./DEPENDENCIES.md)** - Installer NestJS + deps (15 min)
2. **[BACKEND_NESTJS.md](./BACKEND_NESTJS.md)** - Copier les modules (1h)
3. **[DATABASE_SCHEMA.sql](./DATABASE_SCHEMA.sql)** - Créer la DB (15 min)
4. Test avec cURL (30 min)

### 🎨 Je veux faire l'UI Unity (2h)

1. **[UNITY_ARCHITECTURE.md](./UNITY_ARCHITECTURE.md)** - Lire l'architecture (15 min)
2. Copier les scripts Unity depuis `Assets/Hub/Script/` (30 min)
3. Créer les prefabs UI (45 min)
4. Tester les animations DOTween (30 min)

### 📊 Je veux setup l'observabilité (1h)

1. **[OBSERVABILITY_OPS.md](./OBSERVABILITY_OPS.md)** - Lire la stratégie (15 min)
2. Installer Prometheus + Grafana (20 min)
3. Configurer les dashboards (25 min)

---

## 📋 Checklist MVP (3 Semaines)

### ✅ Semaine 1: Core (Auth + Users)

**Backend**
- [ ] Setup PostgreSQL (DATABASE_SCHEMA.sql)
- [ ] Créer projet NestJS (DEPENDENCIES.md)
- [ ] Module Auth (register/login/refresh) - voir BACKEND_NESTJS.md
- [ ] Module Users (me/search)
- [ ] Logs JSON structurés
- [ ] Métriques Prometheus basiques

**Unity**
- [ ] Setup projet + DOTween (QUICK_START.md)
- [ ] Scripts Core (UIManager, ScreenBase)
- [ ] Scripts Services (APIService, AuthManager, CacheManager)
- [ ] LoginScreen avec animations
- [ ] RegisterScreen
- [ ] Test flow Register → Login

**Validation**
- [ ] Créer un compte via Unity
- [ ] Se connecter
- [ ] Token refresh automatique fonctionne
- [ ] Animations fluides

---

### ✅ Semaine 2: Social (Friends)

**Backend**
- [ ] Module Friends (requests/accept/decline)
- [ ] Endpoints GET /friends
- [ ] Endpoints POST /friends/request
- [ ] Endpoints POST /friends/accept
- [ ] DELETE /friends/:id
- [ ] Pagination cursor-based

**Unity**
- [ ] FriendsScreen complet
- [ ] Liste amis avec statut (online/offline/in-game)
- [ ] Recherche utilisateurs
- [ ] Envoyer demande d'ami
- [ ] Accepter/refuser demandes
- [ ] Cache friends list (1 min TTL)
- [ ] ToastNotification pour feedback

**Validation**
- [ ] Créer 2 comptes
- [ ] Envoyer demande d'ami
- [ ] Accepter la demande
- [ ] Voir l'ami dans la liste
- [ ] Animations smooth

---

### ✅ Semaine 3: Matches (Historique)

**Backend**
- [ ] Module Matches (get/details)
- [ ] GET /matches avec pagination cursor
- [ ] GET /matches/:matchId avec participants
- [ ] POST /matches (pour créer un match - admin)
- [ ] Requêtes SQL optimisées (index)

**Unity**
- [ ] MatchHistoryScreen
- [ ] Liste paginée (cursor-based)
- [ ] MatchDetailsScreen
- [ ] Affichage participants (teams, scores)
- [ ] Skeleton loading UI
- [ ] Cache match history

**Validation**
- [ ] Créer des matchs de test en DB
- [ ] Voir l'historique dans Unity
- [ ] Pagination fonctionne (scroll infini)
- [ ] Cliquer sur un match → détails
- [ ] Toutes les animations OK

---

## 🎓 Concepts Clés par Document

### ARCHITECTURE.md
- Architecture client/serveur
- Pattern Services + Screens + Models
- Cursor-based pagination
- MVP → Beta → Prod roadmap

### API_CONTRACT.md
- Format de réponse standard
- Gestion d'erreurs uniforme
- Rate limiting
- JWT authentication flow

### DATABASE_SCHEMA.sql
- Relations many-to-many (friendships)
- Index optimisés
- Contraintes d'intégrité
- Vues SQL (user_friends, user_stats)
- Fonctions utilitaires

### UNITY_ARCHITECTURE.md
- Design system DOTween (timings/easings)
- State machine UI
- Cache manager avec TTL
- Gestion erreurs/loading
- Animations réutilisables

### BACKEND_NESTJS.md
- Modules NestJS (structure)
- Auth avec JWT (access + refresh)
- Validation DTOs (class-validator)
- Métriques Prometheus
- Logging avec correlation ID

### OBSERVABILITY_OPS.md
- Logs JSON structurés
- Métriques business/techniques
- Dashboards Grafana
- Déploiement Debian
- Systemd + Nginx

---

## 🔍 Recherche Rapide

### Je cherche comment faire...

| Besoin | Document | Section |
|--------|----------|---------|
| **Installer le projet** | QUICK_START.md | Setup Rapide |
| **Créer un endpoint API** | BACKEND_NESTJS.md | Auth Module |
| **Faire une animation DOTween** | UNITY_ARCHITECTURE.md | Design System |
| **Configurer PostgreSQL** | DATABASE_SCHEMA.sql | + QUICK_START.md |
| **Gérer les tokens JWT** | UNITY_ARCHITECTURE.md | Gestion Auth |
| **Créer un écran Unity** | UNITY_ARCHITECTURE.md | + Screens/ |
| **Setup Grafana** | OBSERVABILITY_OPS.md | Dashboards |
| **Pagination cursor** | API_CONTRACT.md | GET /matches |
| **Cache en mémoire** | UNITY_ARCHITECTURE.md | Cache Manager |
| **Déployer sur Debian** | OBSERVABILITY_OPS.md | Déploiement |

---

## 📊 Statistiques du Projet

### Documentation
- **9 fichiers Markdown** (~3500 lignes)
- **1 fichier SQL** (~400 lignes)
- **Diagrammes ASCII**
- **Exemples JSON** (50+)
- **Commandes shell** (100+)

### Code
- **11 fichiers C# Unity** (~2200 lignes)
- **Code backend** (snippets complets)
- **Config files** (Nginx, Systemd, Prometheus, Loki)

### Couverture
- ✅ **Architecture complète** (client + serveur + DB + ops)
- ✅ **Contrats API** (20+ endpoints)
- ✅ **Schema DB** (7 tables, index, contraintes)
- ✅ **UI System** (navigation, animations, cache)
- ✅ **Observabilité** (logs, métriques, dashboards)
- ✅ **Déploiement** (Debian, Nginx, systemd)

---

## 🛠️ Technologies Utilisées

### Frontend
- Unity 2022.3 LTS
- C# .NET Standard 2.1
- DOTween Pro (animations)
- Newtonsoft.Json

### Backend
- Node.js 20 LTS
- NestJS 10
- TypeScript 5
- TypeORM (PostgreSQL)
- Passport (JWT)
- Winston (logging)
- prom-client (metrics)

### Database
- PostgreSQL 15+
- pg_trgm (fulltext search)
- Cursor-based pagination

### Observabilité
- Prometheus (metrics)
- Loki (logs)
- Grafana (dashboards)

### Infrastructure
- Debian 12
- Nginx (reverse proxy)
- Systemd (process management)
- Let's Encrypt (SSL/TLS)

---

## 🎯 Objectifs d'Apprentissage

### Phase MVP ✅
- [x] Architecture client/serveur moderne
- [x] REST API (NestJS)
- [x] JWT authentication
- [x] PostgreSQL avec optimisations
- [x] UI animée (DOTween)
- [x] Logs structurés
- [x] Métriques Prometheus

### Phase Beta 🔄
- [ ] WebSocket (temps réel)
- [ ] Redis caching
- [ ] File upload (avatars)
- [ ] Rate limiting avancé
- [ ] Dashboards complets

### Phase Prod 🔜
- [ ] Matchmaking
- [ ] Anti-cheat
- [ ] CI/CD
- [ ] Load testing
- [ ] Alertes

---

## 📞 Support & Ressources

### Documentation Officielle
- [NestJS Docs](https://docs.nestjs.com/)
- [DOTween Docs](http://dotween.demigiant.com/documentation.php)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
- [Prometheus Docs](https://prometheus.io/docs/)
- [Grafana Docs](https://grafana.com/docs/)

### Communauté
- NestJS Discord: https://discord.gg/nestjs
- Unity Forums: https://forum.unity.com/
- PostgreSQL Mailing Lists: https://www.postgresql.org/list/

---

## 🚀 Let's Go!

**Tu as maintenant tout ce qu'il faut pour démarrer !**

1. ⭐ Commence par [README.md](./README.md)
2. ⚡ Suis [QUICK_START.md](./QUICK_START.md)
3. 💻 Code le MVP (3 semaines)
4. 🎉 Célèbre ton client type LoL !

**Bon développement ! 🎮**

---

_Dernière mise à jour: 2026-01-04_

