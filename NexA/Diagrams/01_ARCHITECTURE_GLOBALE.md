# Architecture Globale du Système NexA

## Vue d'ensemble

Ce document décrit l'architecture complète du client PC NexA, inspirée de Riot/League of Legends.

---

## Diagramme 1: Architecture 3-Tiers

**Type de diagramme souhaité**: Architecture / Infrastructure

**Description**: 
Architecture complète montrant les 3 couches principales et leurs interactions.

**Composants à représenter**:

### Tier 1: Client Unity (PC)
- **Client Unity (Windows/Linux/Mac)**
  - UI Layer (DOTween animations)
  - Services Layer (API, Auth, Cache)
  - Data Models
  - Network Manager

### Tier 2: Backend API (Node.js)
- **API Server (NestJS sur Debian)**
  - Controllers (Auth, Users, Friends, Matches)
  - Services (Business Logic)
  - Middleware (JWT, Rate Limiting, Logging)
  - Database Access Layer (TypeORM/Prisma)

### Tier 3: Data & Observability
- **PostgreSQL Database**
  - Tables: users, friendships, matches, match_participants
- **Observability Stack**
  - Prometheus (Métriques)
  - Loki (Logs)
  - Grafana (Dashboards)

### Infrastructure
- **Nginx** (Reverse Proxy)
- **Systemd** (Service Management)

**Connexions**:
1. Client Unity ↔ Nginx (HTTPS)
2. Nginx ↔ API Server (HTTP/WebSocket)
3. API Server ↔ PostgreSQL (TCP/5432)
4. API Server → Prometheus (Metrics Endpoint)
5. API Server → Loki (Log Shipping)
6. Grafana → Prometheus + Loki (Queries)

**Flux de données**:
- Requêtes REST: Client → Nginx → API → DB
- WebSocket (futur): Client ↔ Nginx ↔ API (présence temps réel)
- Logs: API → Loki → Grafana
- Métriques: API → Prometheus → Grafana

---

## Diagramme 2: Architecture Client Unity (Layers)

**Type de diagramme souhaité**: Diagramme de composants / Layers

**Description**:
Architecture interne du client Unity avec séparation des responsabilités.

**Couches (de haut en bas)**:

### Layer 1: UI/Presentation
- **UI Screens** (MonoBehaviour)
  - LoginScreen.cs
  - HomeScreen.cs
  - ProfileScreen.cs
  - FriendsScreen.cs
  - MatchHistoryScreen.cs
  - MatchDetailsScreen.cs
- **UI Manager** (UIManager.cs)
  - Navigation entre écrans
  - Animations DOTween
  - Toast notifications
- **UI Components**
  - FriendListItem prefab
  - MatchListItem prefab

### Layer 2: Services (Singletons)
- **APIService** (Communication HTTP)
- **AuthService** (Gestion tokens JWT)
- **CacheService** (Données en mémoire)
- **AnimationService** (DOTween helpers)

### Layer 3: Data Models
- **User.cs**
- **Friend.cs**
- **Match.cs**
- **APIResponse<T>.cs**
- **APIError.cs**

### Layer 4: Network
- **NetworkManager** (UnityWebRequest wrapper)
  - Retry logic
  - Timeout handling
  - Token refresh

**Dépendances**:
- UI Screens → Services (appels async)
- Services → Network Manager
- Services → Cache Service
- Services → Data Models
- UI Screens → Animation Service

**Règles**:
- UI ne parle JAMAIS directement au Network
- Services sont des singletons (DontDestroyOnLoad)
- Cache invalide après 5 minutes ou selon événements

---

## Diagramme 3: Architecture Backend (NestJS Modules)

**Type de diagramme souhaité**: Module Architecture / Component Diagram

**Description**:
Structure modulaire du backend NestJS avec dépendances.

**Modules**:

### AppModule (Root)
- Configuration globale
- Database connection
- Middleware setup

### AuthModule
- **Controllers**: AuthController
- **Services**: AuthService
- **Guards**: JwtAuthGuard
- **Strategies**: JwtStrategy
- Responsabilités: Register, Login, Refresh Token

### UsersModule
- **Controllers**: UsersController
- **Services**: UsersService
- **Repositories**: UsersRepository
- Responsabilités: Profil utilisateur, Search

### FriendsModule
- **Controllers**: FriendsController
- **Services**: FriendsService
- **Repositories**: FriendsRepository
- Responsabilités: Amis, Demandes, Statut

### MatchesModule
- **Controllers**: MatchesController
- **Services**: MatchesService
- **Repositories**: MatchesRepository
- Responsabilités: Historique, Détails match

### ObservabilityModule
- **Services**: LoggerService, MetricsService
- **Exporters**: PrometheusExporter
- Responsabilités: Logs structurés, Métriques

**Dépendances inter-modules**:
- AuthModule est importé par tous (guards)
- UsersModule est importé par FriendsModule
- UsersModule est importé par MatchesModule
- ObservabilityModule est global (middleware)

---

## Diagramme 4: Infrastructure Debian (Deployment)

**Type de diagramme souhaité**: Infrastructure / Deployment Diagram

**Description**:
Vue de déploiement sur serveur Debian avec tous les services.

**Serveur Debian**:

### Services Systemd
1. **nexa-api.service**
   - Node.js process (NestJS)
   - Port 3000
   - Environment: /etc/nexa/api.env
   - User: nexa-api
   - Logs: journald + file

2. **postgresql.service**
   - Port 5432
   - Data: /var/lib/postgresql/data

3. **prometheus.service**
   - Port 9090
   - Config: /etc/prometheus/prometheus.yml
   - Scrapes: nexa-api:3000/metrics

4. **promtail.service** (Loki agent)
   - Reads: /var/log/nexa-api/*.log
   - Ships to: Loki

5. **loki.service**
   - Port 3100
   - Storage: /var/lib/loki

6. **grafana.service**
   - Port 3001
   - Datasources: Prometheus + Loki

### Nginx (Reverse Proxy)
- Port 80/443 (public)
- SSL/TLS Termination
- Routes:
  - `/api/*` → localhost:3000
  - `/metrics` → localhost:9090
  - `/grafana/*` → localhost:3001

### Filesystem
```
/opt/nexa-api/           # Application
/etc/nexa/api.env        # Secrets
/var/log/nexa-api/       # Logs
/var/lib/postgresql/     # DB data
/var/lib/loki/           # Log storage
```

### Ports exposés
- 443 (HTTPS public) → Nginx
- Tous les autres en localhost uniquement

---

## Diagramme 5: Flux réseau et sécurité

**Type de diagramme souhaité**: Network Flow / Security Diagram

**Description**:
Flux réseau avec couches de sécurité.

**Zones**:

### Internet (Zone Publique)
- Client Unity PC

### DMZ (Debian Server - Zone Frontend)
- **Nginx** (Port 443)
  - SSL/TLS
  - Rate limiting (général)
  - WAF basique

### Application Layer (Zone Backend)
- **API NestJS** (Port 3000, localhost only)
  - JWT Validation
  - Rate limiting (par user)
  - Input validation
  - CORS

### Data Layer (Zone Sécurisée)
- **PostgreSQL** (Port 5432, localhost only)
  - Credentials rotation
  - Connection pooling
  - Prepared statements (SQL injection prevention)

**Flux de sécurité**:
1. Client → Nginx: HTTPS (TLS 1.3)
2. Nginx → API: HTTP (localhost, pas d'exposition)
3. API → DB: TCP (localhost, user dédié)

**Headers de sécurité**:
- Authorization: Bearer <JWT>
- X-Request-ID: <correlation-id>
- X-Forwarded-For: <IP client> (via Nginx)

**Rate Limits**:
- Nginx: 100 req/sec global
- API: 10 req/sec par IP (login/register)
- API: 50 req/sec par user (authenticated)

---

## Métadonnées pour génération de diagrammes

### Palette de couleurs suggérée
- Client Unity: #00C853 (vert)
- Backend API: #2196F3 (bleu)
- Database: #FF9800 (orange)
- Observability: #9C27B0 (violet)
- Infrastructure: #607D8B (gris bleu)

### Style recommandé
- Diagrammes C4 ou Architecture hexagonale
- Flèches directionnelles pour les flux
- Icônes: Unity, Node.js, PostgreSQL, Nginx, Grafana

### Outils suggérés
- PlantUML
- Mermaid
- Draw.io
- Lucidchart
- Excalidraw

---

## Notes pour l'IA génératrice

1. **Contexte**: Jeu multijoueur inspiré de League of Legends
2. **Échelle**: MVP d'abord, puis Beta, puis Production
3. **Focus**: Architecture clean, observable, scalable
4. **Public cible**: Document technique pour GDD (Game Design Document)


