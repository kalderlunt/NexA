# Architecture API Backend (NestJS)

## Vue d'ensemble

Ce document décrit l'architecture du backend NestJS avec tous les modules, contrôleurs, services, et middlewares.

---

## Diagramme 20: Structure Modulaire NestJS

**Type de diagramme souhaité**: Module Dependency Diagram

**Description**:
Architecture modulaire du backend avec dépendances entre modules.

**Hiérarchie**:

```
AppModule (Root)
├── ConfigModule (global)
├── DatabaseModule (global)
├── LoggerModule (global)
├── MetricsModule (global)
│
├── AuthModule
│   ├── Controllers: AuthController
│   ├── Services: AuthService, TokenService
│   ├── Guards: JwtAuthGuard, LocalAuthGuard
│   ├── Strategies: JwtStrategy, LocalStrategy
│   └── Imports: UsersModule
│
├── UsersModule
│   ├── Controllers: UsersController
│   ├── Services: UsersService
│   ├── Repositories: UsersRepository
│   └── Exports: UsersService (for other modules)
│
├── FriendsModule
│   ├── Controllers: FriendsController
│   ├── Services: FriendsService
│   ├── Repositories: FriendsRepository
│   ├── Imports: UsersModule
│   └── Guards: JwtAuthGuard
│
├── MatchesModule
│   ├── Controllers: MatchesController
│   ├── Services: MatchesService
│   ├── Repositories: MatchesRepository, ParticipantsRepository
│   ├── Imports: UsersModule
│   └── Guards: JwtAuthGuard
│
└── HealthModule
    ├── Controllers: HealthController
    └── Services: HealthService
```

**Dépendances inter-modules**:
- FriendsModule → UsersModule (besoin de UsersService)
- MatchesModule → UsersModule (validation user_id)
- AuthModule → UsersModule (vérification credentials)

---

## Diagramme 21: Flux de Requête HTTP (Middleware Pipeline)

**Type de diagramme souhaité**: Sequence Diagram / Pipeline

**Description**:
Flux complet d'une requête HTTP à travers tous les middlewares.

**Pipeline**:

```
[Client Request]
    ↓
1. [Nginx] (Reverse Proxy)
    ├─→ SSL Termination
    ├─→ Rate Limiting (100 req/sec global)
    ├─→ Add X-Forwarded-For header
    └─→ Add X-Request-ID (si absent)
    ↓
2. [NestJS - Global Middleware]
    ↓
2.1 [CorsMiddleware]
    ├─→ Check Origin
    ├─→ Set CORS headers
    └─→ OPTIONS → 204 (preflight)
    ↓
2.2 [LoggerMiddleware]
    ├─→ Log request start
    ├─→ Store request context (request_id, user_id, timestamp)
    └─→ Attach to AsyncLocalStorage
    ↓
2.3 [MetricsMiddleware]
    ├─→ Start timer
    └─→ Increment http_requests_total
    ↓
3. [Route Handler]
    ↓
3.1 [Guards] (ex: JwtAuthGuard)
    ├─→ Extract JWT from Authorization header
    ├─→ Verify signature
    ├─→ Check expiration
    ├─→ Attach user to request.user
    └─→ Si échec → 401 Unauthorized
    ↓
3.2 [Interceptors] (ex: TransformInterceptor)
    ├─→ Transform input DTO
    └─→ Validation (class-validator)
    ↓
3.3 [Controller Method]
    ├─→ Extract params/body/query
    └─→ Call Service
        ↓
    3.4 [Service Layer]
        ├─→ Business logic
        ├─→ Call Repository
        │   ↓
        │ 3.5 [Repository]
        │   ├─→ TypeORM query
        │   └─→ Database
        │       ↓
        │   [PostgreSQL]
        │       ↓
        │   [Return data]
        │   ↓
        ├─→ Transform data
        └─→ Return to Controller
        ↓
    [Response Data]
    ↓
4. [Interceptors] (response transformation)
    ├─→ Wrap in APIResponse<T>
    └─→ Add metadata (timestamp, request_id)
    ↓
5. [Exception Filters] (si erreur)
    ├─→ Catch exceptions
    ├─→ Format error response
    ├─→ Log error with context
    └─→ Return JSON error
    ↓
6. [MetricsMiddleware] (after response)
    ├─→ Stop timer
    ├─→ Record http_request_duration_seconds
    └─→ Increment http_responses_total{status_code}
    ↓
7. [LoggerMiddleware] (after response)
    ├─→ Log response (status, duration, size)
    └─→ Log to file + stdout
    ↓
[Client Response]
```

**Métriques collectées**:
- http_requests_total (counter)
- http_request_duration_seconds (histogram)
- http_responses_total (counter) avec labels: status_code, method, route

---

## Diagramme 22: Architecture en Couches (Layered Architecture)

**Type de diagramme souhaité**: Layered Architecture Diagram

**Description**:
Séparation des responsabilités en couches distinctes.

**Couches (de haut en bas)**:

### Layer 1: Presentation (API Layer)
**Responsabilité**: Gestion des requêtes/réponses HTTP
**Composants**:
- Controllers
- DTOs (Data Transfer Objects)
- Validation pipes
- Guards

**Exemple**:
```typescript
@Controller('friends')
@UseGuards(JwtAuthGuard)
export class FriendsController {
  @Get()
  async getFriends(@CurrentUser() user) { ... }
}
```

**Règles**:
- ❌ PAS de logique métier
- ✅ Validation des inputs
- ✅ Transformation des outputs
- ✅ Gestion auth/authorization

---

### Layer 2: Business Logic (Service Layer)
**Responsabilité**: Logique métier
**Composants**:
- Services
- Business rules
- Use cases

**Exemple**:
```typescript
@Injectable()
export class FriendsService {
  async sendFriendRequest(requesterId: string, receiverId: string) {
    // 1. Vérifier que users existent
    // 2. Vérifier pas déjà amis
    // 3. Vérifier pas de demande pending
    // 4. Créer friendship avec status='pending'
    // 5. Créer notification (futur)
  }
}
```

**Règles**:
- ❌ PAS de dépendance HTTP
- ✅ Réutilisable (testable)
- ✅ Transactions gérées ici
- ✅ Appelle Repositories

---

### Layer 3: Data Access (Repository Layer)
**Responsabilité**: Accès base de données
**Composants**:
- Repositories
- Entities (TypeORM)
- Query builders

**Exemple**:
```typescript
@Injectable()
export class FriendsRepository {
  async findFriendsByUserId(userId: string): Promise<Friendship[]> {
    return this.friendshipRepository.find({
      where: [
        { requesterId: userId, status: 'accepted' },
        { receiverId: userId, status: 'accepted' }
      ]
    });
  }
}
```

**Règles**:
- ❌ PAS de logique métier
- ✅ Uniquement requêtes SQL
- ✅ Gestion transactions
- ✅ Optimisations (index, joins)

---

### Layer 4: Infrastructure
**Responsabilité**: Services externes
**Composants**:
- Database connection
- Redis cache (futur)
- S3 storage (avatars)
- Email service (futur)

---

## Diagramme 23: Gestion des Erreurs (Exception Handling)

**Type de diagramme souhaité**: Flowchart + Class Hierarchy

**Description**:
Stratégie de gestion des erreurs avec filtres globaux.

**Hiérarchie d'exceptions**:

```
BaseException (abstract)
├── BadRequestException (400)
│   ├── ValidationException (champs invalides)
│   └── InvalidInputException
│
├── UnauthorizedException (401)
│   ├── InvalidCredentialsException
│   └── TokenExpiredException
│
├── ForbiddenException (403)
│   └── InsufficientPermissionsException
│
├── NotFoundException (404)
│   ├── UserNotFoundException
│   ├── MatchNotFoundException
│   └── FriendshipNotFoundException
│
├── ConflictException (409)
│   ├── UserAlreadyExistsException
│   ├── AlreadyFriendsException
│   └── DuplicateRequestException
│
├── TooManyRequestsException (429)
│   └── RateLimitExceededException
│
└── InternalServerErrorException (500)
    ├── DatabaseException
    └── UnexpectedException
```

**Format de réponse d'erreur standardisé**:
```json
{
  "success": false,
  "error": {
    "code": "USER_NOT_FOUND",
    "message": "User with ID xxx not found",
    "details": {
      "userId": "xxx"
    },
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/users/xxx",
    "requestId": "req-uuid-123"
  }
}
```

**Exception Filter**:
```typescript
@Catch()
export class GlobalExceptionFilter implements ExceptionFilter {
  catch(exception: unknown, host: ArgumentsHost) {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse();
    const request = ctx.getRequest();

    // 1. Déterminer status code
    const status = exception instanceof HttpException 
      ? exception.getStatus() 
      : 500;

    // 2. Logger l'erreur
    this.logger.error({
      message: exception.message,
      stack: exception.stack,
      requestId: request.id,
      userId: request.user?.id,
      path: request.url,
    });

    // 3. Formater réponse
    const errorResponse = {
      success: false,
      error: {
        code: this.getErrorCode(exception),
        message: exception.message,
        timestamp: new Date().toISOString(),
        path: request.url,
        requestId: request.id,
      }
    };

    // 4. Masquer détails si 500
    if (status === 500) {
      errorResponse.error.message = 'Internal server error';
    }

    response.status(status).json(errorResponse);
  }
}
```

---

## Diagramme 24: Rate Limiting Strategy

**Type de diagramme souhaité**: Component Diagram + Configuration

**Description**:
Stratégie de rate limiting à plusieurs niveaux.

**Niveaux de rate limiting**:

### Level 1: Nginx (Global)
**Règle**: 100 req/sec global (tous clients confondus)
**Action si dépassé**: 429 Too Many Requests
**Config**:
```nginx
limit_req_zone $binary_remote_addr zone=global:10m rate=100r/s;
limit_req zone=global burst=20 nodelay;
```

### Level 2: NestJS - Par IP (Public routes)
**Routes**: /auth/login, /auth/register
**Règle**: 10 req/min par IP
**Action**: 429 + header Retry-After
**Implementation**: @nestjs/throttler
```typescript
@Throttle(10, 60) // 10 requests per 60 seconds
@Post('login')
async login() { ... }
```

### Level 3: NestJS - Par User (Authenticated)
**Routes**: Toutes routes authentifiées
**Règle**: 50 req/sec par user_id
**Action**: 429
**Implementation**: Custom decorator
```typescript
@RateLimit({ points: 50, duration: 1 })
@UseGuards(JwtAuthGuard)
@Get('friends')
async getFriends() { ... }
```

### Level 4: Business Logic (Custom)
**Exemple**: Friend requests
**Règle**: Max 20 demandes d'ami par jour par user
**Action**: 409 Conflict + message explicite
**Implementation**: Service layer
```typescript
async sendFriendRequest(userId: string, targetId: string) {
  const todayCount = await this.countRequestsToday(userId);
  if (todayCount >= 20) {
    throw new ConflictException('Daily friend request limit exceeded');
  }
  // ...
}
```

**Storage backend**:
- MVP: In-memory (Map)
- Beta: Redis (distributed, persiste redémarrage)

---

## Diagramme 25: Authentification Flow (JWT)

**Type de diagramme souhaité**: Sequence Diagram

**Description**:
Flux complet d'authentification avec JWT.

**Séquence Login**:

```
[Client] → POST /auth/login {email, password}
    ↓
[AuthController]
    ↓
[LocalAuthGuard] (Passport strategy)
    ↓
[LocalStrategy.validate(email, password)]
    ├─→ [UsersService.findByEmail(email)]
    ├─→ [bcrypt.compare(password, hash)]
    └─→ Return user (si valide) ou throw UnauthorizedException
    ↓
[AuthService.login(user)]
    ├─→ Generate access token (JWT, expire 15min)
    │   Payload: {sub: user.id, email: user.email}
    │   Secret: process.env.JWT_SECRET
    │   
    ├─→ Generate refresh token (JWT, expire 7 days)
    │   Payload: {sub: user.id, type: 'refresh'}
    │   
    └─→ [UsersRepository.update(last_login)]
    ↓
[Response 200]
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "eyJ...",
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "username": "JohnDoe"
    }
  }
}
```

**Séquence Requête Authentifiée**:

```
[Client] → GET /friends
    Headers: Authorization: Bearer <accessToken>
    ↓
[JwtAuthGuard]
    ↓
[JwtStrategy.validate(payload)]
    ├─→ Extract token from header
    ├─→ Verify signature (JWT_SECRET)
    ├─→ Check expiration
    │   └─→ Si expiré → throw UnauthorizedException
    ├─→ Extract user.id from payload
    └─→ [UsersService.findById(user.id)]
        └─→ Attach user to request.user
    ↓
[FriendsController.getFriends(@CurrentUser() user)]
    ├─→ user.id disponible
    └─→ [FriendsService.findFriends(user.id)]
```

**Séquence Refresh Token**:

```
[Client] → POST /auth/refresh
    Body: {refreshToken: "eyJ..."}
    ↓
[AuthController.refresh(refreshToken)]
    ↓
[AuthService.refreshAccessToken(refreshToken)]
    ├─→ Verify refresh token signature
    ├─→ Check expiration
    ├─→ Check type === 'refresh'
    ├─→ Extract user.id
    ├─→ [UsersService.findById(user.id)]
    │   └─→ Si user inexistant → throw UnauthorizedException
    ├─→ Generate new access token (15min)
    └─→ (Optionnel) Rotate refresh token
    ↓
[Response 200]
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "eyJ..." // nouveau (rotation)
  }
}
```

**Sécurité**:
- Access token court (15min) → limite fenêtre d'attaque
- Refresh token long (7 days) → UX (pas re-login)
- HTTPS obligatoire (pas de token en clair)
- Pas de stockage token côté serveur (stateless) en MVP
- Beta: Whitelist refresh tokens en DB pour révocation

---

## Diagramme 26: Structure de Logs JSON

**Type de diagramme souhaité**: Schema Diagram + Exemple

**Description**:
Format standardisé des logs pour observabilité.

**Schema Log**:

```json
{
  "timestamp": "2026-01-11T22:30:15.123Z",
  "level": "info",
  "message": "User login successful",
  "context": {
    "service": "nexa-api",
    "environment": "production",
    "host": "api-server-01",
    "version": "1.2.3"
  },
  "request": {
    "id": "req-uuid-abc123",
    "method": "POST",
    "path": "/api/auth/login",
    "ip": "192.168.1.100",
    "userAgent": "Unity/2021.3 (Windows)"
  },
  "user": {
    "id": "user-uuid-xyz",
    "email": "user@example.com"
  },
  "duration": 123,
  "statusCode": 200,
  "error": null
}
```

**Niveaux de log**:
- **debug**: Dev uniquement (SQL queries, cache hits)
- **info**: Flow normal (login, API calls)
- **warn**: Comportements suspects (rate limit proche, retry)
- **error**: Erreurs gérées (validation, not found)
- **fatal**: Erreurs critiques (DB down, crash)

**Exemples par contexte**:

### Log: Login Success
```json
{
  "timestamp": "2026-01-11T22:30:15.123Z",
  "level": "info",
  "message": "User login successful",
  "request": {"id": "req-123", "path": "/api/auth/login"},
  "user": {"id": "user-xyz", "email": "user@example.com"},
  "duration": 87,
  "statusCode": 200
}
```

### Log: Friend Request Sent
```json
{
  "timestamp": "2026-01-11T22:31:00.456Z",
  "level": "info",
  "message": "Friend request sent",
  "request": {"id": "req-124"},
  "user": {"id": "user-xyz"},
  "data": {
    "targetUserId": "user-abc",
    "friendshipId": "friendship-uuid"
  },
  "duration": 34,
  "statusCode": 201
}
```

### Log: Error 404
```json
{
  "timestamp": "2026-01-11T22:32:00.789Z",
  "level": "warn",
  "message": "User not found",
  "request": {"id": "req-125", "path": "/api/users/invalid-id"},
  "user": {"id": "user-xyz"},
  "error": {
    "code": "USER_NOT_FOUND",
    "details": {"userId": "invalid-id"}
  },
  "statusCode": 404
}
```

### Log: Error 500
```json
{
  "timestamp": "2026-01-11T22:33:00.999Z",
  "level": "error",
  "message": "Database connection error",
  "request": {"id": "req-126"},
  "error": {
    "code": "DATABASE_ERROR",
    "message": "Connection timeout",
    "stack": "Error: Connection timeout\n  at ..."
  },
  "statusCode": 500
}
```

**Stratégie de corrélation**:
- `request.id` (X-Request-ID) : suit une requête à travers tous les logs
- `user.id` : regroupe actions d'un utilisateur
- Logs enrichis avec context (service, host, version)

---

## Diagramme 27: Métriques Prometheus

**Type de diagramme souhaité**: Metrics Dashboard Specification

**Description**:
Métriques exposées pour monitoring Grafana.

**Endpoint**: `GET /metrics` (format Prometheus)

**Métriques exposées**:

### 1. HTTP Metrics

**http_requests_total** (Counter)
- Labels: method, route, status_code
- Description: Total requêtes HTTP
```
http_requests_total{method="GET",route="/friends",status="200"} 15420
http_requests_total{method="POST",route="/auth/login",status="401"} 87
```

**http_request_duration_seconds** (Histogram)
- Labels: method, route
- Buckets: 0.01, 0.05, 0.1, 0.5, 1, 2, 5
- Description: Latence requêtes
```
http_request_duration_seconds_bucket{method="GET",route="/friends",le="0.1"} 14230
http_request_duration_seconds_sum{method="GET",route="/friends"} 892.34
http_request_duration_seconds_count{method="GET",route="/friends"} 15420
```

**http_responses_total** (Counter)
- Labels: status_code
- Description: Réponses par code HTTP
```
http_responses_total{status="200"} 98543
http_responses_total{status="401"} 234
http_responses_total{status="500"} 12
```

---

### 2. Database Metrics

**db_query_duration_seconds** (Histogram)
- Labels: query_type (SELECT/INSERT/UPDATE/DELETE), table
- Description: Temps exécution requêtes SQL
```
db_query_duration_seconds_bucket{query="SELECT",table="users",le="0.01"} 5432
```

**db_connections_active** (Gauge)
- Description: Connexions DB actives
```
db_connections_active 12
```

**db_connections_idle** (Gauge)
- Description: Connexions DB idle (pool)
```
db_connections_idle 8
```

**db_errors_total** (Counter)
- Labels: error_type (timeout, connection_error)
- Description: Erreurs base de données
```
db_errors_total{error="timeout"} 3
```

---

### 3. Business Metrics

**users_registered_total** (Counter)
- Description: Total utilisateurs enregistrés
```
users_registered_total 1523
```

**users_active_online** (Gauge)
- Description: Utilisateurs actuellement online
```
users_active_online 234
```

**friend_requests_sent_total** (Counter)
- Description: Total demandes d'ami envoyées
```
friend_requests_sent_total 5643
```

**matches_completed_total** (Counter)
- Labels: match_type (ranked/normal)
- Description: Parties terminées
```
matches_completed_total{type="ranked"} 8765
```

---

### 4. Cache Metrics (Futur - Redis)

**cache_hits_total** (Counter)
- Labels: cache_name
```
cache_hits_total{cache="friends_list"} 45321
```

**cache_misses_total** (Counter)
```
cache_misses_total{cache="friends_list"} 1234
```

**cache_hit_rate** (Gauge) - Calculé
- Formule: hits / (hits + misses)
```
cache_hit_rate{cache="friends_list"} 0.973
```

---

## Diagramme 28: Configuration Environment Variables

**Type de diagramme souhaité**: Configuration Schema

**Description**:
Variables d'environnement nécessaires au backend.

**Fichier**: `/etc/nexa/api.env`

```bash
# Application
NODE_ENV=production
APP_PORT=3000
APP_VERSION=1.2.3

# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=nexa_db
DATABASE_USER=nexa_api_user
DATABASE_PASSWORD=super_secret_password
DATABASE_POOL_MIN=5
DATABASE_POOL_MAX=20
DATABASE_SSL=true

# JWT
JWT_SECRET=your-256-bit-secret-key-change-me
JWT_EXPIRES_IN=15m
JWT_REFRESH_SECRET=your-refresh-secret-key
JWT_REFRESH_EXPIRES_IN=7d

# Rate Limiting
RATE_LIMIT_TTL=60
RATE_LIMIT_MAX=50

# CORS
CORS_ORIGIN=https://nexa-client.com
CORS_CREDENTIALS=true

# Logging
LOG_LEVEL=info
LOG_FILE_PATH=/var/log/nexa-api/app.log
LOG_MAX_FILES=10
LOG_MAX_SIZE=10m

# Metrics
METRICS_ENABLED=true
METRICS_PORT=9090

# External Services (futur)
S3_BUCKET=nexa-avatars
S3_REGION=eu-west-1
SMTP_HOST=smtp.example.com
```

**Validation au démarrage**:
```typescript
import { ConfigService } from '@nestjs/config';

@Injectable()
export class ConfigValidationService {
  constructor(private config: ConfigService) {
    this.validate();
  }

  validate() {
    const required = [
      'DATABASE_HOST',
      'JWT_SECRET',
      'CORS_ORIGIN'
    ];

    for (const key of required) {
      if (!this.config.get(key)) {
        throw new Error(`Missing required env var: ${key}`);
      }
    }
  }
}
```

---

## Métadonnées pour génération

### Palette couleurs modules
- Controllers: #4CAF50 (vert)
- Services: #2196F3 (bleu)
- Repositories: #FF9800 (orange)
- Guards/Middleware: #9C27B0 (violet)
- Utilities: #607D8B (gris)

### Icônes suggérées
- 🔒 Guards/Auth
- 📊 Metrics
- 📝 Logs
- 🗄️ Database
- ⚡ Cache

### Style diagrammes
- UML Component Diagram pour modules
- Sequence Diagram pour flows
- Flowchart pour pipelines


