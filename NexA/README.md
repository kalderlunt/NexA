# 🎮 NexA - Client PC (Style League of Legends)

**Projet d'apprentissage** : Client PC avec système de compte, amis, profil joueur, historique de parties, UI animée (DOTween).

---

## 📚 Documentation

| Fichier                                          | Description |
|--------------------------------------------------|-------------|
| [ARCHITECTURE.md](./MDHelp/ARCHITECTURE.md)      | Vue d'ensemble architecture + MVP + Roadmap |
| [API_CONTRACT.md](./MDHelp/API_CONTRACT.md)      | Contrats API (endpoints, payloads, erreurs) |
| [DATABASE_SCHEMA.sql](./MDHelp/DATABASE_SCHEMA.sql)     | Schéma PostgreSQL complet avec index/contraintes |
| [UNITY_ARCHITECTURE.md](./MDHelp/UNITY_ARCHITECTURE.md) | Architecture Unity (screens/services/animations) |
| [BACKEND_NESTJS.md](./MDHelp/BACKEND_NESTJS.md)         | Implémentation backend NestJS avec exemples |
| [OBSERVABILITY_OPS.md](./MDHelp/OBSERVABILITY_OPS.md)   | Logs, métriques, Grafana, déploiement Debian |

---

## 🚀 Quick Start

### 1. Backend (NestJS + PostgreSQL)

```bash
# Créer la base de données
psql -U postgres
CREATE DATABASE nexa_db;
CREATE USER nexa_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE nexa_db TO nexa_user;
\q

# Exécuter le schema SQL
psql -U nexa_user -d nexa_db -f DATABASE_SCHEMA.sql

# Installer le backend
cd backend
npm install
cp .env.example .env  # Éditer avec vos valeurs

# Lancer en dev
npm run start:dev
```

### 2. Unity Client

```bash
# Ouvrir le projet dans Unity 2022+
# Installer DOTween (Window → Package Manager → Add from Git URL)
# URL: https://github.com/Demigiant/dotween.git

# Ou via Asset Store: DOTween (gratuit)

# Configurer l'API URL dans Assets/Resources/Config/APIConfig
# Puis Play!
```

---

## 🎯 MVP Features (3 semaines)

### ✅ Semaine 1: Core
- [x] Auth: Register/Login avec JWT
- [x] Backend: Routes auth + users
- [x] DB: Schema users + migrations
- [x] Unity: Écrans Login/Register
- [x] Unity: APIService + retry logic
- [x] Logs JSON structurés

### ✅ Semaine 2: Social
- [ ] Backend: Système d'amis (request/accept/decline)
- [ ] DB: friend_requests + friendships
- [ ] Unity: Écran amis + recherche
- [ ] Unity: Notifications toasts
- [ ] Pagination cursor-based

### ✅ Semaine 3: Matches
- [ ] Backend: Endpoints matches
- [ ] DB: matches + match_participants
- [ ] Unity: Écran historique paginé
- [ ] Unity: Détails d'un match
- [ ] DOTween: Animations complètes

---

## 🏗️ Architecture Globale

```
Unity Client (C#)
      ↓ HTTPS/REST (JWT)
   Nginx (reverse proxy)
      ↓
  NestJS Backend (Node.js)
      ↓
 PostgreSQL Database
      ↓
Prometheus + Loki → Grafana
```

---

## 📐 Design System (DOTween)

### Durées Standards
- **INSTANT**: 0.1s
- **FAST**: 0.2s
- **NORMAL**: 0.3s (défaut)
- **MEDIUM**: 0.4s
- **SLOW**: 0.5s

### Easings
- **IN_BACK**: `Ease.OutBack` (pop effect)
- **IN_SMOOTH**: `Ease.OutQuad` (entrées)
- **OUT_SMOOTH**: `Ease.InQuad` (sorties)
- **HOVER**: `Ease.OutQuad` (interactions)

### Patterns d'Animation
- **Screen Transition**: Fade + slide (50px offset)
- **Panel Pop**: Scale 0 → 1 avec OutBack
- **Button Hover**: Scale 1.05 + glow fade
- **Toast**: Slide from bottom + auto-hide (3s)
- **Loading**: Rotate 360° infinite linear

Voir [AnimationHelper.cs](./Assets/Hub/Script/Utils/AnimationHelper.cs) pour tous les helpers.

---

## 🔐 Sécurité (MVP)

- **Auth**: JWT access (1h) + refresh (7d)
- **Storage**: Tokens en mémoire (option encryption PlayerPrefs)
- **HTTPS**: Obligatoire en prod (Let's Encrypt)
- **Rate Limiting**: 
  - Auth: 10 req/min
  - API: 100 req/min
- **Validation**: class-validator (backend) + input sanitization
- **CORS**: Configuré (origin whitelist)

---

## 📊 Observabilité

### Logs (JSON)
- **Format**: timestamp, level, message, correlationId, userId, context, metadata
- **Niveaux**: debug, info, warn, error, fatal
- **Stockage**: Winston → Loki → Grafana
- **Retention**: 14 jours

### Métriques (Prometheus)
- **HTTP**: requests_total, request_duration_ms, errors_total
- **DB**: query_duration_ms, connections_active, errors_total
- **Business**: users_online, logins_total, friend_requests_sent_total

### Dashboards Grafana
1. **API Overview**: req/s, latency P95, error rate
2. **Database**: query duration, connections, slow queries
3. **Business**: users online, registrations, matches
4. **System**: CPU, memory, event loop lag

---

## 🐧 Déploiement (Debian)

### Services Systemd
```bash
# API
sudo systemctl start nexa-api
sudo systemctl enable nexa-api
sudo systemctl status nexa-api

# Logs
sudo journalctl -u nexa-api -f
```

### Nginx
```bash
sudo nginx -t
sudo systemctl reload nginx
```

### PostgreSQL Backup
```bash
# Backup quotidien (cron)
0 2 * * * /opt/nexa/scripts/backup_db.sh

# Restore
pg_restore -U nexa_user -d nexa_db backup.dump
```

---

## 🗺️ Roadmap

### Phase 1: MVP (Semaines 1-3) ✅
- Auth, amis, historique
- UI animée
- Logs + métriques basiques

### Phase 2: Beta (Semaines 4-6)
- WebSocket (présence temps réel)
- Upload avatar
- Match recording (API)
- Redis cache
- Dashboards Grafana complets

### Phase 3: Prod-Like (Semaines 7-10)
- Matchmaking/Lobby
- Anti-cheat basique
- CI/CD (GitHub Actions)
- Backup automatisé
- Alertes Grafana
- Load testing (K6)

---

## 🛠️ Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Client | Unity | 2022+ |
| Animations | DOTween | 1.2+ |
| Backend | Node.js + NestJS | 20 + 10 |
| Database | PostgreSQL | 15+ |
| Auth | JWT (Passport) | - |
| Logs | Winston + Loki | - |
| Metrics | Prometheus | 2.x |
| Dashboards | Grafana | 10.x |
| Reverse Proxy | Nginx | 1.24+ |
| OS | Debian | 12 |

---

## 📝 Conventions de Code

### C# (Unity)
- **Naming**: PascalCase classes, camelCase variables
- **Async**: suffixe `Async` pour méthodes async
- **Regions**: grouper par fonctionnalité
- **Comments**: XML docs pour publics

### TypeScript (NestJS)
- **Naming**: PascalCase classes, camelCase méthodes/variables
- **DTOs**: suffixe `.dto.ts`
- **Entities**: suffixe `.entity.ts`
- **Services**: business logic only
- **Controllers**: routing + validation

### SQL
- **Tables**: snake_case pluriel
- **Columns**: snake_case
- **Index**: préfixe `idx_`
- **Contraintes**: préfixe `fk_` ou `unique_`

---

## 🧪 Testing (TODO Phase 2)

### Backend
```bash
# Unit tests
npm run test

# E2E tests
npm run test:e2e

# Coverage
npm run test:cov
```

### Unity
- Play Mode tests (Unity Test Framework)
- Mock APIService pour tests offline

---

## 📚 Ressources

### DOTween
- [Docs officielles](http://dotween.demigiant.com/documentation.php)
- [Easings visualizer](https://easings.net/)

### NestJS
- [Docs officielles](https://docs.nestjs.com/)
- [JWT Auth Guide](https://docs.nestjs.com/security/authentication)

### Grafana
- [Prometheus queries](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Loki LogQL](https://grafana.com/docs/loki/latest/logql/)

---

## 🤝 Contribution

Projet personnel d'apprentissage. Pas de contributions externes pour le moment.

---

## 📄 License

MIT (projet d'apprentissage)

---

## 🎓 Ce que j'apprends

- ✅ Architecture client/serveur moderne
- ✅ JWT + refresh tokens
- ✅ Cursor-based pagination
- ✅ Logs structurés JSON
- ✅ Métriques Prometheus
- ✅ Animations UI (DOTween)
- ✅ TypeORM + PostgreSQL optimisé
- 🔄 WebSocket (temps réel) - Phase 2
- 🔄 Redis caching - Phase 2
- 🔄 CI/CD - Phase 3
- 🔄 Load testing - Phase 3

---

## 📞 Contact

Pour questions/feedback: [Créer une issue](https://github.com/kalderlunt/nexa/issues)

---

**Dernière mise à jour**: 2026-01-04

