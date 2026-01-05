# 🚀 NexA Backend - Architecture Node.js

## 📋 Vue d'ensemble

**Stack technique :**
- **Framework** : Express.js (simple, rapide) ou NestJS (structure, TypeScript)
- **Base de données** : PostgreSQL 14+
- **ORM** : Prisma (type-safe, migrations faciles)
- **Auth** : JWT (access + refresh tokens)
- **Logs** : Winston (JSON structured)
- **Métriques** : prom-client (Prometheus)
- **Validation** : Joi ou class-validator

**Hébergement** : Debian VPS (AWS EC2, DigitalOcean, Hetzner, etc.)

---

## 🏗️ Structure du Projet (Express)

```
nexa-backend/
├── src/
│   ├── controllers/          # Route handlers
│   │   ├── auth.controller.ts
│   │   ├── users.controller.ts
│   │   ├── friends.controller.ts
│   │   └── matches.controller.ts
│   ├── services/             # Business logic
│   │   ├── auth.service.ts
│   │   ├── users.service.ts
│   │   ├── friends.service.ts
│   │   └── matches.service.ts
│   ├── repositories/         # Database access
│   │   ├── users.repository.ts
│   │   ├── friends.repository.ts
│   │   └── matches.repository.ts
│   ├── middleware/           # Express middleware
│   │   ├── auth.middleware.ts
│   │   ├── rateLimit.middleware.ts
│   │   ├── validation.middleware.ts
│   │   ├── logging.middleware.ts
│   │   └── errorHandler.middleware.ts
│   ├── routes/               # API routes
│   │   ├── index.ts
│   │   ├── auth.routes.ts
│   │   ├── users.routes.ts
│   │   ├── friends.routes.ts
│   │   └── matches.routes.ts
│   ├── models/               # TypeScript types/interfaces
│   │   ├── User.ts
│   │   ├── Friend.ts
│   │   └── Match.ts
│   ├── utils/                # Utilitaires
│   │   ├── logger.ts
│   │   ├── jwt.ts
│   │   ├── bcrypt.ts
│   │   └── validator.ts
│   ├── config/               # Configuration
│   │   ├── database.ts
│   │   ├── jwt.ts
│   │   └── server.ts
│   └── app.ts                # Express app setup
├── prisma/
│   ├── schema.prisma         # Database schema
│   └── migrations/           # Migration history
├── tests/
│   ├── unit/
│   └── integration/
├── .env.example
├── package.json
├── tsconfig.json
└── README.md
```

---

## 📡 API Contracts (Endpoints)

### Auth Endpoints

#### POST `/api/auth/register`
```typescript
Request:
{
  "username": "string (3-20 chars)",
  "email": "string (valid email)",
  "password": "string (8+ chars)"
}

Response 201:
{
  "success": true,
  "data": {
    "user": {
      "id": "uuid",
      "username": "string",
      "email": "string",
      "avatar": "string | null",
      "level": 1,
      "elo": 1000,
      "createdAt": "ISO 8601"
    },
    "tokens": {
      "accessToken": "jwt",
      "refreshToken": "jwt",
      "expiresIn": 3600
    }
  }
}

Errors:
- 400 BAD_REQUEST: "Email already exists"
- 400 VALIDATION_ERROR: "Invalid email format"
```

#### POST `/api/auth/login`
```typescript
Request:
{
  "email": "string",
  "password": "string"
}

Response 200: (same as register)

Errors:
- 401 UNAUTHORIZED: "Invalid credentials"
- 429 TOO_MANY_REQUESTS: "Too many login attempts"
```

#### POST `/api/auth/refresh`
```typescript
Request:
{
  "refreshToken": "jwt"
}

Response 200:
{
  "success": true,
  "data": {
    "accessToken": "jwt",
    "expiresIn": 3600
  }
}

Errors:
- 401 UNAUTHORIZED: "Invalid refresh token"
```

#### POST `/api/auth/logout`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": null
}
```

---

### User Endpoints

#### GET `/api/me`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": {
    "id": "uuid",
    "username": "string",
    "email": "string",
    "avatar": "string | null",
    "level": "number",
    "elo": "number",
    "stats": {
      "totalMatches": "number",
      "wins": "number",
      "losses": "number"
    },
    "status": "online",
    "createdAt": "ISO 8601",
    "lastSeenAt": "ISO 8601"
  }
}
```

#### GET `/api/users/search?q={query}`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "username": "string",
      "avatar": "string | null",
      "level": "number",
      "elo": "number",
      "isFriend": "boolean"
    }
  ]
}

Notes:
- Max 20 résultats
- Recherche insensible à la casse
- Minimum 2 caractères
```

---

### Friends Endpoints

#### GET `/api/friends`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "username": "string",
      "avatar": "string | null",
      "level": "number",
      "elo": "number",
      "status": "online | offline | in-game",
      "lastSeenAt": "ISO 8601",
      "friendsSince": "ISO 8601"
    }
  ]
}
```

#### GET `/api/friends/requests`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "from": {
        "id": "uuid",
        "username": "string",
        "avatar": "string | null"
      },
      "createdAt": "ISO 8601"
    }
  ]
}
```

#### POST `/api/friends/request`
```typescript
Headers: Authorization: Bearer {accessToken}

Request:
{
  "targetUserId": "uuid"
}

Response 201:
{
  "success": true,
  "data": {
    "id": "uuid",
    "to": { "id": "uuid", "username": "string" },
    "createdAt": "ISO 8601"
  }
}

Errors:
- 400 BAD_REQUEST: "Already friends"
- 400 BAD_REQUEST: "Request already sent"
- 404 NOT_FOUND: "User not found"
```

#### POST `/api/friends/accept`
```typescript
Headers: Authorization: Bearer {accessToken}

Request:
{
  "requestId": "uuid"
}

Response 200:
{
  "success": true,
  "data": {
    "friendship": {
      "id": "uuid",
      "friend": { "id": "uuid", "username": "string" },
      "createdAt": "ISO 8601"
    }
  }
}
```

#### POST `/api/friends/decline`
```typescript
Headers: Authorization: Bearer {accessToken}

Request:
{
  "requestId": "uuid"
}

Response 200:
{
  "success": true,
  "data": null
}
```

#### DELETE `/api/friends/:userId`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": null
}
```

---

### Matches Endpoints

#### GET `/api/matches?limit=20&cursor={cursor}`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "gameMode": "ranked | casual",
      "result": "victory | defeat | draw",
      "duration": 1234, // seconds
      "createdAt": "ISO 8601",
      "participants": null // Pas de détails dans la liste
    }
  ],
  "meta": {
    "nextCursor": "base64_encoded_cursor",
    "hasMore": true,
    "total": 150
  }
}

Notes:
- Pagination cursor-based (pas d'offset)
- Limit max: 50
```

#### GET `/api/matches/:matchId`
```typescript
Headers: Authorization: Bearer {accessToken}

Response 200:
{
  "success": true,
  "data": {
    "id": "uuid",
    "gameMode": "ranked",
    "result": "victory",
    "duration": 1234,
    "createdAt": "ISO 8601",
    "participants": [
      {
        "userId": "uuid",
        "username": "string",
        "avatar": "string | null",
        "team": 1,
        "score": 1500,
        "isCurrentUser": true
      },
      // ... autres joueurs
    ]
  }
}
```

---

## 🗄️ Database Schema (PostgreSQL + Prisma)

### prisma/schema.prisma

```prisma
generator client {
  provider = "prisma-client-js"
}

datasource db {
  provider = "postgresql"
  url      = env("DATABASE_URL")
}

model User {
  id        String   @id @default(uuid())
  username  String   @unique @db.VarChar(20)
  email     String   @unique
  password  String   // bcrypt hash
  avatar    String?
  level     Int      @default(1)
  elo       Int      @default(1000)
  status    String   @default("offline") // online, offline, in-game
  
  createdAt DateTime @default(now())
  updatedAt DateTime @updatedAt
  lastSeenAt DateTime @default(now())
  
  // Relations
  friendshipsInitiated Friendship[] @relation("InitiatedFriendships")
  friendshipsReceived  Friendship[] @relation("ReceivedFriendships")
  friendRequestsSent   FriendRequest[] @relation("SentRequests")
  friendRequestsReceived FriendRequest[] @relation("ReceivedRequests")
  matchParticipations  MatchParticipant[]
  refreshTokens        RefreshToken[]
  
  @@index([email])
  @@index([username])
  @@map("users")
}

model Friendship {
  id        String   @id @default(uuid())
  user1Id   String
  user2Id   String
  createdAt DateTime @default(now())
  
  user1 User @relation("InitiatedFriendships", fields: [user1Id], references: [id], onDelete: Cascade)
  user2 User @relation("ReceivedFriendships", fields: [user2Id], references: [id], onDelete: Cascade)
  
  @@unique([user1Id, user2Id])
  @@index([user1Id])
  @@index([user2Id])
  @@map("friendships")
}

model FriendRequest {
  id        String   @id @default(uuid())
  fromId    String
  toId      String
  status    String   @default("pending") // pending, accepted, declined
  createdAt DateTime @default(now())
  
  from User @relation("SentRequests", fields: [fromId], references: [id], onDelete: Cascade)
  to   User @relation("ReceivedRequests", fields: [toId], references: [id], onDelete: Cascade)
  
  @@unique([fromId, toId])
  @@index([toId, status])
  @@map("friend_requests")
}

model Match {
  id        String   @id @default(uuid())
  gameMode  String   // ranked, casual
  duration  Int      // seconds
  createdAt DateTime @default(now())
  
  participants MatchParticipant[]
  
  @@index([createdAt])
  @@map("matches")
}

model MatchParticipant {
  id          String   @id @default(uuid())
  matchId     String
  userId      String
  team        Int      // 1 or 2
  score       Int
  result      String   // victory, defeat, draw
  
  match Match @relation(fields: [matchId], references: [id], onDelete: Cascade)
  user  User  @relation(fields: [userId], references: [id], onDelete: Cascade)
  
  @@unique([matchId, userId])
  @@index([userId, matchId])
  @@map("match_participants")
}

model RefreshToken {
  id        String   @id @default(uuid())
  token     String   @unique
  userId    String
  expiresAt DateTime
  createdAt DateTime @default(now())
  
  user User @relation(fields: [userId], references: [id], onDelete: Cascade)
  
  @@index([token])
  @@index([userId])
  @@map("refresh_tokens")
}
```

### Migrations

```bash
# Créer une migration
npx prisma migrate dev --name init

# Appliquer en production
npx prisma migrate deploy

# Générer le client
npx prisma generate
```

---

## 📊 Observabilité

### Logs Structurés (Winston)

```typescript
// src/utils/logger.ts
import winston from 'winston';

export const logger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  defaultMeta: { service: 'nexa-api' },
  transports: [
    new winston.transports.File({ filename: 'logs/error.log', level: 'error' }),
    new winston.transports.File({ filename: 'logs/combined.log' }),
    new winston.transports.Console({
      format: winston.format.simple()
    })
  ]
});

// Middleware de logging
export function loggingMiddleware(req, res, next) {
  const correlationId = req.headers['x-correlation-id'] || uuidv4();
  req.correlationId = correlationId;
  
  const start = Date.now();
  
  res.on('finish', () => {
    const duration = Date.now() - start;
    logger.info({
      type: 'http_request',
      correlationId,
      method: req.method,
      path: req.path,
      statusCode: res.statusCode,
      duration,
      userId: req.user?.id,
      ip: req.ip
    });
  });
  
  next();
}
```

### Métriques (Prometheus)

```typescript
// src/utils/metrics.ts
import promClient from 'prom-client';

const register = new promClient.Registry();

// Default metrics (CPU, memory, etc.)
promClient.collectDefaultMetrics({ register });

// Custom metrics
export const httpRequestDuration = new promClient.Histogram({
  name: 'http_request_duration_seconds',
  help: 'Duration of HTTP requests in seconds',
  labelNames: ['method', 'route', 'status_code'],
  registers: [register]
});

export const dbQueryDuration = new promClient.Histogram({
  name: 'db_query_duration_seconds',
  help: 'Duration of database queries',
  labelNames: ['operation', 'table'],
  registers: [register]
});

export const activeConnections = new promClient.Gauge({
  name: 'active_connections',
  help: 'Number of active WebSocket connections',
  registers: [register]
});

// Endpoint pour Prometheus
app.get('/metrics', async (req, res) => {
  res.set('Content-Type', register.contentType);
  res.end(await register.metrics());
});
```

### Dashboards Grafana

**Panneaux recommandés :**
1. **HTTP Requests** : rate, latency (p50, p95, p99), errors 4xx/5xx
2. **Database** : query duration, connections, slow queries
3. **Auth** : login attempts, failed logins, active sessions
4. **Business Metrics** : new users/day, matches/day, active players

---

## 🚀 Déploiement Debian

### Setup Initial

```bash
# 1. Installer Node.js 20+ (via nvm)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
nvm install 20
nvm use 20

# 2. Installer PostgreSQL 14+
sudo apt update
sudo apt install postgresql postgresql-contrib

# 3. Créer la base de données
sudo -u postgres psql
CREATE DATABASE nexa;
CREATE USER nexauser WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE nexa TO nexauser;
\q

# 4. Installer PM2 (process manager)
npm install -g pm2
```

### Configuration

```bash
# .env (PRODUCTION)
NODE_ENV=production
PORT=3000
DATABASE_URL="postgresql://nexauser:secure_password@localhost:5432/nexa"
JWT_SECRET="generate_with_openssl_rand_base64_32"
JWT_REFRESH_SECRET="another_secure_secret"
JWT_EXPIRES_IN="1h"
JWT_REFRESH_EXPIRES_IN="7d"
LOG_LEVEL=info
```

### Service systemd

```bash
# /etc/systemd/system/nexa-api.service
[Unit]
Description=NexA API Server
After=network.target postgresql.service

[Service]
Type=simple
User=nexa
WorkingDirectory=/opt/nexa-backend
ExecStart=/usr/bin/node dist/app.js
Restart=always
RestartSec=10
StandardOutput=append:/var/log/nexa/api.log
StandardError=append:/var/log/nexa/error.log
Environment=NODE_ENV=production

[Install]
WantedBy=multi-user.target

# Activer et démarrer
sudo systemctl enable nexa-api
sudo systemctl start nexa-api
sudo systemctl status nexa-api
```

### Nginx Reverse Proxy

```nginx
# /etc/nginx/sites-available/nexa-api
server {
    listen 80;
    server_name api.nexa.game;
    
    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    limit_req zone=api burst=20 nodelay;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    location /metrics {
        deny all; # Accessible seulement en interne
    }
}

# Activer
sudo ln -s /etc/nginx/sites-available/nexa-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### SSL avec Let's Encrypt

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.nexa.game
# Auto-renewal
sudo systemctl enable certbot.timer
```

---

## 🔒 Sécurité

### Rate Limiting (Express)

```typescript
import rateLimit from 'express-rate-limit';

const loginLimiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 5, // 5 tentatives
  message: { error: { code: 'TOO_MANY_REQUESTS', message: 'Too many login attempts' } }
});

app.post('/api/auth/login', loginLimiter, authController.login);
```

### Helmet (Headers de sécurité)

```typescript
import helmet from 'helmet';
app.use(helmet());
```

### CORS

```typescript
import cors from 'cors';
app.use(cors({
  origin: process.env.ALLOWED_ORIGINS?.split(',') || ['http://localhost'],
  credentials: true
}));
```

---

## 📝 Notes Finales

### MVP (à implémenter en priorité)
- [x] Auth (register, login, refresh, logout)
- [x] Users (me, search)
- [x] Friends (list, request, accept, decline, remove)
- [x] Matches (history, details)
- [x] Logs structurés
- [x] Métriques Prometheus

### Post-MVP
- [ ] WebSocket (présence temps réel, notifications)
- [ ] Matchmaking (création de parties)
- [ ] Rate limiting avancé (par endpoint)
- [ ] Cache Redis (sessions, friends list)
- [ ] Tests unitaires + E2E
- [ ] CI/CD (GitHub Actions)
- [ ] Monitoring (Sentry pour erreurs)

**Prêt à coder ! 🎮**

