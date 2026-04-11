# Roadmap & MVP

## Vue d'ensemble

Ce document décrit la roadmap complète du projet avec phases, priorités, et estimations.

---

## Diagramme 60: MVP Scope

**Type de diagramme souhaité**: Feature Matrix

**Description**:
Périmètre exact du MVP avec priorités.

### Features MVP (Phase 0 - Semaines 1-4)

| Feature | Priority | Complexity | Estimation | Status |
|---------|----------|------------|------------|--------|
| **Authentication** |
| Register | P0 | Medium | 3 jours | ⬜ |
| Login | P0 | Medium | 2 jours | ⬜ |
| JWT tokens | P0 | Medium | 2 jours | ⬜ |
| Logout | P0 | Low | 1 jour | ⬜ |
| **User Profile** |
| View own profile | P0 | Low | 2 jours | ⬜ |
| View other profile | P1 | Low | 1 jour | ⬜ |
| Basic stats | P1 | Medium | 2 jours | ⬜ |
| **Friends System** |
| List friends | P0 | Medium | 2 jours | ⬜ |
| Search users | P0 | Medium | 2 jours | ⬜ |
| Send friend request | P0 | Medium | 3 jours | ⬜ |
| Accept/Decline | P0 | Medium | 2 jours | ⬜ |
| Remove friend | P1 | Low | 1 jour | ⬜ |
| Friend status (online/offline) | P1 | Medium | 2 jours | ⬜ |
| **Match History** |
| List matches | P0 | Medium | 3 jours | ⬜ |
| Match details | P0 | Medium | 3 jours | ⬜ |
| Pagination | P0 | Medium | 2 jours | ⬜ |
| **UI/UX** |
| Screen navigation | P0 | Medium | 3 jours | ⬜ |
| DOTween animations | P1 | Medium | 4 jours | ⬜ |
| Toast notifications | P0 | Low | 2 jours | ⬜ |
| Loading states | P0 | Medium | 2 jours | ⬜ |
| Error handling | P0 | Medium | 2 jours | ⬜ |
| **Backend** |
| Database schema | P0 | Medium | 2 jours | ⬜ |
| API endpoints | P0 | High | 5 jours | ⬜ |
| Input validation | P0 | Medium | 2 jours | ⬜ |
| Rate limiting | P1 | Medium | 2 jours | ⬜ |
| **Observability** |
| Structured logs | P1 | Medium | 2 jours | ⬜ |
| Prometheus metrics | P1 | Medium | 2 jours | ⬜ |
| Basic dashboard | P2 | Low | 1 jour | ⬜ |
| **Deployment** |
| Debian setup | P0 | High | 3 jours | ⬜ |
| Nginx config | P0 | Medium | 2 jours | ⬜ |
| Systemd services | P0 | Medium | 2 jours | ⬜ |
| SSL certificate | P0 | Low | 1 jour | ⬜ |

**Total estimation**: ~60 jours-personne → **12 semaines** (1 dev)

**MVP Deadline**: Fin Semaine 12

---

## Diagramme 61: Roadmap 3 Phases

**Type de diagramme souhaité**: Gantt Chart / Timeline

**Description**:
Roadmap complète sur 6 mois.

### Phase 0: MVP (Semaines 1-12)

**Objectif**: Démo fonctionnelle avec features essentielles

**Milestones**:
- Week 4: Backend API complet + DB
- Week 8: Client Unity avec auth + friends
- Week 10: Match history + UI polish
- Week 12: Déploiement Debian + observability

**Livrables**:
- ✅ Auth flow complet
- ✅ Friends system basique
- ✅ Match history consultation
- ✅ API REST documentée
- ✅ Logs + métriques basiques

**Exclusions MVP**:
- ❌ Matchmaking
- ❌ Chat temps réel
- ❌ Notifications push
- ❌ Avatar upload
- ❌ Edit profile

---

### Phase 1: Beta (Semaines 13-20)

**Objectif**: Features sociales avancées + temps réel

**Features**:

| Feature | Description | Estimation |
|---------|-------------|------------|
| **WebSocket** | Présence temps réel (online/in-game) | 5 jours |
| **Notifications** | System de notifications in-app | 4 jours |
| **Chat (simple)** | Chat 1-to-1 avec amis | 6 jours |
| **Edit Profile** | Modifier pseudo, avatar | 3 jours |
| **Avatar Upload** | Upload image + S3 storage | 4 jours |
| **Friend Suggestions** | Algorithme suggestions amis | 3 jours |
| **Match filters** | Filtrer historique (type, date) | 2 jours |
| **User settings** | Préférences (language, theme) | 3 jours |
| **Advanced metrics** | Plus de métriques business | 2 jours |
| **Alerting** | Prometheus alerts + email | 3 jours |

**Milestones**:
- Week 14: WebSocket + présence temps réel
- Week 16: Chat + notifications
- Week 18: Upload avatar + edit profile
- Week 20: Alerting + monitoring avancé

**Livrables**:
- ✅ Expérience sociale riche
- ✅ Feedback temps réel
- ✅ Personnalisation profil
- ✅ Monitoring production-ready

---

### Phase 2: Production-Ready (Semaines 21-26)

**Objectif**: Scale + sécurité + matchmaking MVP

**Features**:

| Feature | Description | Estimation |
|---------|-------------|------------|
| **Matchmaking** | Queue + team formation basique | 10 jours |
| **Game lobby** | Pre-game lobby avec chat | 5 jours |
| **Redis cache** | Cache distribué (friends, sessions) | 3 jours |
| **DB Read replicas** | Scale lecture PostgreSQL | 4 jours |
| **Rate limiting v2** | Rate limit par user + Redis | 2 jours |
| **Security audit** | Audit complet + fixes | 5 jours |
| **CI/CD** | Pipeline automatisé (GitHub Actions) | 4 jours |
| **Load testing** | Tests performance (k6) | 3 jours |
| **Backup automation** | Backups automatiques + restore test | 2 jours |
| **Documentation** | Docs complète (API + deployment) | 3 jours |

**Milestones**:
- Week 22: Matchmaking fonctionnel
- Week 24: Redis + scale DB
- Week 26: Security audit + CI/CD

**Livrables**:
- ✅ Matchmaking opérationnel
- ✅ Infrastructure scalable
- ✅ Sécurité renforcée
- ✅ Déploiement automatisé
- ✅ Documentation complète

---

## Diagramme 62: Dependencies Graph

**Type de diagramme souhaité**: Dependency Graph

**Description**:
Graphe des dépendances entre features.

```
[Database Schema]
    ↓
[Backend API: Auth]
    ↓
[Client Unity: Login/Register] ──────┐
    ↓                                 ↓
[Backend API: Users]         [Client Unity: Navigation]
    ↓                                 ↓
[Backend API: Friends] ──────→ [Client Unity: Friends Screen]
    ↓                                 ↓
[Backend API: Matches] ──────→ [Client Unity: Match History]
    ↓
[Observability: Logs]
    ↓
[Observability: Metrics]
    ↓
[Deployment: Debian] ←───────── [Nginx + SSL]
    ↓
[Monitoring: Grafana]

// Phase 1
[WebSocket] ──────→ [Presence Temps Réel]
    ↓
[Notifications]
    ↓
[Chat]

// Phase 2
[Redis] ──────→ [Cache Layer]
    ↓
[Matchmaking] ──────→ [Game Lobby]
```

**Ordre d'implémentation optimal**:
1. DB + Auth backend
2. Auth client Unity
3. Users + Friends backend
4. Friends client Unity
5. Matches backend
6. Match history client
7. Observability (parallèle)
8. Deployment
9. Phase 1 features
10. Phase 2 features

---

## Diagramme 63: Risk Matrix

**Type de diagramme souhaité**: Risk Assessment Matrix

**Description**:
Matrice des risques projet.

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| **Technique** |
| Unity-Backend incompatibilité | Low | High | Prototyper tôt (Week 1-2) |
| Performance DB (matches) | Medium | High | Index + pagination cursor dès MVP |
| WebSocket scale issues | Medium | Medium | Redis pub/sub (Phase 2) |
| Security breach (auth) | Medium | Critical | Audit externe (Phase 2) |
| **Planning** |
| Underestimation efforts | High | Medium | Buffer 20% sur estimations |
| Scope creep | High | High | Strict MVP scope, backlog pour v2 |
| Dependencies bloquantes | Medium | High | Paralléliser backend/client |
| **Ops** |
| Debian server crash | Low | High | Monitoring + alerting (Phase 1) |
| Data loss | Low | Critical | Backups daily + test restore |
| DDoS / Rate limit bypass | Medium | Medium | Nginx rate limit + Cloudflare (futur) |
| **Business** |
| Pas d'utilisateurs test | High | Medium | Friends/family pour beta |
| Feedback négatif UX | Medium | Medium | Itérations rapides UI/UX |

**Risques critiques à surveiller**:
1. ⚠️ **Auth security** → Audit dès Phase 1
2. ⚠️ **DB performance** → Load testing Week 10
3. ⚠️ **Deployment complexity** → Script automation

---

## Diagramme 64: Team Structure (Si équipe)

**Type de diagramme souhaité**: Org Chart

**Description**:
Structure d'équipe idéale si scaling.

### Solo Developer (MVP)
```
YOU
├── Backend (40%)
├── Frontend Unity (35%)
├── DevOps (15%)
└── Design/UX (10%)
```

### Small Team (Phase 1-2)
```
Tech Lead (YOU)
├── Backend Developer
│   ├── API development
│   ├── Database optimization
│   └── Observability
│
├── Unity Developer
│   ├── UI implementation
│   ├── Animations
│   └── Client architecture
│
└── DevOps Engineer (part-time)
    ├── Infrastructure
    ├── CI/CD
    └── Monitoring
```

### Scaleup Team (Post-Phase 2)
```
Engineering Manager
├── Backend Team
│   ├── Senior Backend Dev (tech lead)
│   ├── Backend Dev x2
│   └── Database Specialist
│
├── Client Team
│   ├── Senior Unity Dev (tech lead)
│   ├── Unity Dev x2
│   └── UI/UX Designer
│
├── DevOps Team
│   ├── DevOps Engineer
│   └── Security Engineer
│
└── QA Team
    ├── QA Engineer
    └── Performance Tester
```

---

## Diagramme 65: Success Metrics

**Type de diagramme souhaité**: KPI Dashboard

**Description**:
Métriques de succès par phase.

### MVP Success Criteria

**Technical KPIs**:
- ✅ API uptime > 99% (7 jours)
- ✅ API p95 latency < 200ms
- ✅ Zero security vulnerabilities (OWASP top 10)
- ✅ 100% endpoint coverage (tests)

**Product KPIs**:
- ✅ 10 beta testers actifs
- ✅ 100+ matches enregistrés
- ✅ 50+ friend connections
- ✅ < 5 bug reports critiques

**Observability KPIs**:
- ✅ Logs structurés 100% endpoints
- ✅ 20+ metrics exposées
- ✅ 3+ dashboards Grafana
- ✅ Alerting fonctionnel

---

### Phase 1 Success Criteria

**Technical KPIs**:
- ✅ WebSocket stable (< 1% disconnect rate)
- ✅ Chat latency < 100ms
- ✅ Notification delivery rate > 99%

**Product KPIs**:
- ✅ 50 beta testers actifs
- ✅ 500+ messages chat envoyés
- ✅ 200+ notifications delivered
- ✅ Retention D7 > 50%

**Business KPIs**:
- ✅ NPS score > 7/10
- ✅ < 10 churn rate (monthly)

---

### Phase 2 Success Criteria

**Technical KPIs**:
- ✅ Matchmaking median time < 30s
- ✅ System support 1000 concurrent users
- ✅ DB query time p95 < 50ms
- ✅ Zero downtime deployments

**Product KPIs**:
- ✅ 200 beta testers actifs
- ✅ 5000+ matches jouées
- ✅ Retention D30 > 30%

**Business KPIs**:
- ✅ Coût infrastructure < $100/month (1000 users)
- ✅ Ready for public beta

---

## Diagramme 66: Decision Log

**Type de diagramme souhaité**: Decision Record Table

**Description**:
Log des décisions d'architecture importantes.

| ID | Date | Décision | Rationale | Alternatives | Impact |
|----|------|----------|-----------|--------------|--------|
| ADR-001 | 2026-01 | NestJS pour backend | TypeScript, modules, DI, scalable | Express (plus simple mais moins structuré) | Medium |
| ADR-002 | 2026-01 | PostgreSQL | Relationnel, robuste, JSON support | MongoDB (pas de relations complexes) | High |
| ADR-003 | 2026-01 | JWT stateless (MVP) | Simple, scalable, pas de DB lookup | Session DB (plus de contrôle) | Low |
| ADR-004 | 2026-01 | DOTween pour animations | Standard Unity, performant | Custom ou Unity Animator (plus complexe) | Low |
| ADR-005 | 2026-01 | Cursor-based pagination | Scale mieux que offset | Offset pagination (plus simple) | Medium |
| ADR-006 | 2026-01 | Prometheus + Loki | Standard industry, Grafana integration | ELK stack (plus lourd) | High |
| ADR-007 | 2026-01 | Single server Debian (MVP) | Coût minimal, simple | Kubernetes (overkill pour MVP) | High |
| ADR-008 | TBD | Redis cache (Phase 2) | Scale reads, distributed cache | In-memory (pas distribué) | Medium |
| ADR-009 | TBD | WebSocket pour temps réel | Bi-directionnel, standard | Long polling (moins performant) | Medium |

**Future ADRs**:
- Microservices vs Monolith (si scale)
- Multi-region deployment
- CDN pour assets
- Authentication provider (OAuth2)

---

## Diagramme 67: Testing Strategy

**Type de diagramme souhaité**: Test Pyramid

**Description**:
Stratégie de tests par phase.

### Test Pyramid

```
           ┌─────────────┐
          /   E2E Tests   \     (5% - lent, fragile)
         /─────────────────\
        /  Integration Tests \  (25% - medium speed)
       /─────────────────────\
      /     Unit Tests         \ (70% - rapide, stable)
     /─────────────────────────\
```

### MVP Testing

**Unit Tests** (70%):
- Backend: Services, Repositories (Jest)
- Unity: Services, Data models (NUnit)
- Coverage target: 80%

**Integration Tests** (25%):
- Backend: API endpoints (Supertest)
- Database queries
- JWT flow
- Coverage target: 60%

**E2E Tests** (5%):
- Critical paths: Login → Home → Friends
- Smoke tests: Health check, basic flows

**Manual Testing**:
- UI/UX flows
- Animations smooth
- Error states

---

### Phase 1+ Testing

**Additional tests**:
- WebSocket integration tests
- Load tests (k6): 100 concurrent users
- Security tests: OWASP ZAP
- Performance tests: Lighthouse (client)

**CI/CD**:
- Tests automatiques sur PR
- Coverage report
- No merge si tests échouent

---

## Diagramme 68: Learning Checklist

**Type de diagramme souhaité**: Skills Matrix

**Description**:
Compétences nécessaires et ressources d'apprentissage.

| Compétence | Niveau requis | Ressources | Temps estimé |
|------------|---------------|------------|--------------|
| **Backend** |
| NestJS | Intermediate | Docs officielles + tutorial | 1 semaine |
| TypeORM/Prisma | Basic | Docs + examples | 3 jours |
| JWT Auth | Basic | jwt.io + tutorials | 2 jours |
| PostgreSQL | Intermediate | PostgreSQL tutorial | 1 semaine |
| **Unity** |
| C# | Intermediate | (assumé acquis) | - |
| UnityWebRequest | Basic | Unity docs | 1 jour |
| DOTween | Basic | DOTween docs + examples | 2 jours |
| UI Toolkit/UGUI | Intermediate | Unity Learn | 1 semaine |
| **DevOps** |
| Debian Linux | Basic | Linux Journey | 3 jours |
| Nginx | Basic | Nginx docs | 2 jours |
| Systemd | Basic | systemd tutorial | 1 jour |
| Prometheus | Basic | Prometheus docs | 2 jours |
| Grafana | Basic | Grafana tutorial | 1 jour |
| **Design** |
| UI/UX principles | Basic | Refactoring UI book | 3 jours |
| DOTween animations | Intermediate | Practice + examples | 1 semaine |

**Total learning time**: ~5 semaines (à planifier avant/pendant dev)

---

## Métadonnées pour génération

### Visualisation recommandée
- Gantt chart (roadmap)
- Kanban board (tasks)
- Burndown chart (progress)
- Risk matrix (2x2 grid)

### Outils gestion projet
- Notion (wiki + tasks)
- GitHub Projects (kanban)
- Linear (issue tracking)
- Miro (brainstorming)

### Templates utiles
- Sprint planning
- Retrospective
- Daily standup (si équipe)
- Weekly review


