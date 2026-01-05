# 📊 Observabilité & Ops - NexA Backend

## 🎯 Stack Observabilité

```
┌─────────────────────────────────────────────────┐
│            NestJS Backend (Node.js)              │
│  ├─ Winston/Pino (logs JSON)                    │
│  ├─ prom-client (métriques Prometheus)          │
│  └─ Correlation ID middleware                   │
└─────────────────────────────────────────────────┘
              ↓                    ↓
    ┌─────────────────┐  ┌─────────────────┐
    │  Loki (logs)    │  │  Prometheus     │
    │  (Pull mode)    │  │  (Pull /metrics)│
    └─────────────────┘  └─────────────────┘
              ↓                    ↓
         ┌──────────────────────────────┐
         │      Grafana Dashboards      │
         │  - Logs Explorer             │
         │  - API Metrics               │
         │  - Database Performance      │
         └──────────────────────────────┘
```

---

## 📝 Logs Structurés (JSON)

### Format Standard

```json
{
  "timestamp": "2026-01-04T15:30:45.123Z",
  "level": "info",
  "message": "User login successful",
  "service": "nexa-api",
  "environment": "production",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userId": "user-uuid-123",
  "context": {
    "module": "AuthController",
    "method": "login",
    "ip": "192.168.1.100",
    "userAgent": "NexA-Client/1.0.0"
  },
  "metadata": {
    "duration": 234,
    "statusCode": 200
  }
}
```

### Niveaux de Log

- **`debug`**: Informations de débogage (désactivé en prod)
- **`info`**: Événements normaux (login, logout, API calls)
- **`warn`**: Situations anormales mais non-critiques (rate limit approché, cache miss)
- **`error`**: Erreurs applicatives (erreur DB, API externe down)
- **`fatal`**: Erreurs critiques (serveur ne peut pas démarrer)

### Exemples de Logs par Type

#### 1. API Request Log (info)
```json
{
  "timestamp": "2026-01-04T15:30:45.123Z",
  "level": "info",
  "message": "HTTP GET /api/v1/friends",
  "correlationId": "req-123",
  "userId": "user-abc",
  "context": {
    "method": "GET",
    "path": "/api/v1/friends",
    "statusCode": 200,
    "duration": 45,
    "ip": "10.0.0.5"
  }
}
```

#### 2. Database Query Log (debug)
```json
{
  "timestamp": "2026-01-04T15:30:45.150Z",
  "level": "debug",
  "message": "Database query executed",
  "correlationId": "req-123",
  "context": {
    "query": "SELECT * FROM friendships WHERE user1_id = $1",
    "duration": 12,
    "rowCount": 5
  }
}
```

#### 3. Error Log (error)
```json
{
  "timestamp": "2026-01-04T15:35:22.456Z",
  "level": "error",
  "message": "Failed to send friend request",
  "correlationId": "req-456",
  "userId": "user-xyz",
  "context": {
    "module": "FriendsService",
    "method": "sendFriendRequest",
    "errorCode": "USER_NOT_FOUND",
    "targetUserId": "user-nonexistent"
  },
  "stack": "Error: User not found\n  at FriendsService.sendFriendRequest..."
}
```

#### 4. Business Event Log (info)
```json
{
  "timestamp": "2026-01-04T16:00:00.789Z",
  "level": "info",
  "message": "Match completed",
  "correlationId": "match-789",
  "context": {
    "matchId": "match-uuid-789",
    "mode": "ranked",
    "duration": 1847,
    "playerCount": 8,
    "winningTeam": "blue"
  }
}
```

---

## 📈 Métriques Prometheus

### Métriques Custom à Implémenter

#### 1. **API Metrics**
```typescript
// Counter: Nombre total de requêtes HTTP
http_requests_total{method="GET", path="/friends", status="200"}

// Histogram: Latence des requêtes HTTP (en ms)
http_request_duration_ms{method="GET", path="/friends", quantile="0.5"} 45
http_request_duration_ms{method="GET", path="/friends", quantile="0.95"} 120
http_request_duration_ms{method="GET", path="/friends", quantile="0.99"} 250

// Counter: Erreurs HTTP
http_errors_total{method="POST", path="/auth/login", status="401"}
```

#### 2. **Database Metrics**
```typescript
// Histogram: Temps des requêtes DB
db_query_duration_ms{query="get_friends", quantile="0.95"} 15

// Gauge: Connexions actives
db_connections_active 12
db_connections_idle 8
db_connections_total 20

// Counter: Erreurs DB
db_errors_total{type="connection_timeout"} 3
```

#### 3. **Business Metrics**
```typescript
// Counter: Nombre de logins/registrations
user_registrations_total 1542
user_logins_total 8923

// Counter: Demandes d'amis
friend_requests_sent_total 234
friend_requests_accepted_total 189
friend_requests_declined_total 45

// Counter: Matches créés
matches_created_total{mode="ranked"} 456
matches_created_total{mode="casual"} 1203

// Gauge: Utilisateurs connectés
users_online 342
```

#### 4. **System Metrics** (auto par prom-client)
```typescript
// Memory
process_heap_bytes
nodejs_heap_size_used_bytes

// CPU
process_cpu_user_seconds_total
process_cpu_system_seconds_total

// Event Loop
nodejs_eventloop_lag_seconds
```

---

## 🔍 Correlation ID Tracking

### Flow du Correlation ID

```
Client Unity
  ↓ (génère UUID → X-Correlation-ID header)
Backend NestJS (middleware)
  ↓ (extrait/génère correlation ID → logger context)
Tous les logs + métriques
  ↓ (include correlation ID)
PostgreSQL (comment dans queries)
  ↓
Logs centralisés (Loki/ELK)
  ↓ (filtrable par correlation ID)
Grafana (trace complète d'une requête)
```

### Middleware NestJS (correlation-id.middleware.ts)
```typescript
import { Injectable, NestMiddleware } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';
import { v4 as uuidv4 } from 'uuid';

@Injectable()
export class CorrelationIdMiddleware implements NestMiddleware {
  use(req: Request, res: Response, next: NextFunction) {
    const correlationId = req.headers['x-correlation-id'] as string || uuidv4();
    
    // Stocker dans request pour usage ultérieur
    req['correlationId'] = correlationId;
    
    // Retourner dans response headers
    res.setHeader('X-Correlation-ID', correlationId);
    
    next();
  }
}
```

### Logger avec Correlation ID
```typescript
import { Injectable, Scope } from '@nestjs/common';
import * as winston from 'winston';

@Injectable({ scope: Scope.TRANSIENT })
export class AppLogger {
  private logger: winston.Logger;
  private context: any = {};

  constructor() {
    this.logger = winston.createLogger({
      format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.json()
      ),
      transports: [
        new winston.transports.Console(),
        new winston.transports.File({ 
          filename: '/var/log/nexa-api/app.log',
          maxsize: 10485760, // 10MB
          maxFiles: 10
        })
      ]
    });
  }

  setContext(context: any) {
    this.context = context;
  }

  info(message: string, meta?: any) {
    this.logger.info(message, { ...this.context, ...meta });
  }

  error(message: string, trace?: string, meta?: any) {
    this.logger.error(message, { ...this.context, stack: trace, ...meta });
  }

  warn(message: string, meta?: any) {
    this.logger.warn(message, { ...this.context, ...meta });
  }

  debug(message: string, meta?: any) {
    this.logger.debug(message, { ...this.context, ...meta });
  }
}
```

---

## 🎨 Dashboards Grafana

### Dashboard 1: **API Overview**

**Panels**:
1. **Requêtes/s** (Graph)
   - Query: `rate(http_requests_total[5m])`
   - Legend: par `method` et `path`

2. **Latence P95** (Graph)
   - Query: `histogram_quantile(0.95, rate(http_request_duration_ms_bucket[5m]))`
   - Threshold: warn à 200ms, critical à 500ms

3. **Error Rate** (Stat)
   - Query: `sum(rate(http_errors_total[5m])) / sum(rate(http_requests_total[5m])) * 100`
   - Unit: %
   - Threshold: warn à 1%, critical à 5%

4. **Top 10 Slowest Endpoints** (Table)
   - Query: `topk(10, histogram_quantile(0.95, rate(http_request_duration_ms_bucket[5m])))`

5. **Status Code Distribution** (Pie Chart)
   - Query: `sum(rate(http_requests_total[5m])) by (status)`

---

### Dashboard 2: **Database Performance**

**Panels**:
1. **Query Duration P95** (Graph)
   - Query: `histogram_quantile(0.95, rate(db_query_duration_ms_bucket[5m]))`

2. **Active Connections** (Graph)
   - Query: `db_connections_active`
   - Max: 100 (alerte si > 80)

3. **Top 10 Slowest Queries** (Table)
   - Query: `topk(10, histogram_quantile(0.95, rate(db_query_duration_ms_bucket[5m])))`

4. **DB Errors** (Graph)
   - Query: `rate(db_errors_total[5m])`

---

### Dashboard 3: **Business Metrics**

**Panels**:
1. **Users Online** (Gauge)
   - Query: `users_online`

2. **Registrations/Logins (24h)** (Stat)
   - Query: `increase(user_registrations_total[24h])`
   - Query: `increase(user_logins_total[24h])`

3. **Friend Requests** (Graph)
   - Query: `rate(friend_requests_sent_total[5m])`
   - Query: `rate(friend_requests_accepted_total[5m])`

4. **Matches Created** (Graph)
   - Query: `rate(matches_created_total[5m]) by (mode)`

---

### Dashboard 4: **System Health**

**Panels**:
1. **CPU Usage** (Graph)
   - Query: `rate(process_cpu_user_seconds_total[5m]) * 100`

2. **Memory Usage** (Graph)
   - Query: `process_heap_bytes / 1024 / 1024` (MB)

3. **Event Loop Lag** (Graph)
   - Query: `nodejs_eventloop_lag_seconds * 1000` (ms)
   - Threshold: warn à 100ms, critical à 500ms

4. **Uptime** (Stat)
   - Query: `time() - process_start_time_seconds`
   - Unit: seconds → duration

---

## 🐧 Déploiement Debian

### Structure des Dossiers

```
/opt/nexa/
├── backend/              # Code backend (npm install ici)
│   ├── dist/
│   ├── node_modules/
│   ├── package.json
│   └── .env
├── scripts/              # Scripts utilitaires
│   ├── backup_db.sh
│   ├── deploy.sh
│   └── health_check.sh
└── logs/                 # Logs applicatifs
    ├── app.log
    ├── error.log
    └── access.log

/etc/nginx/
└── sites-available/
    └── nexa-api

/etc/systemd/system/
├── nexa-api.service
└── nexa-worker.service (optionnel, pour jobs async)

/var/log/nexa-api/       # Logs symlink
```

---

### 1. Systemd Service (nexa-api.service)

```ini
[Unit]
Description=NexA API Backend
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
User=nexa
Group=nexa
WorkingDirectory=/opt/nexa/backend
EnvironmentFile=/opt/nexa/backend/.env
ExecStart=/usr/bin/node dist/main.js
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal

# Limits
LimitNOFILE=4096
MemoryLimit=1G
CPUQuota=200%

# Security
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/nexa/logs

[Install]
WantedBy=multi-user.target
```

**Commandes**:
```bash
sudo systemctl daemon-reload
sudo systemctl enable nexa-api
sudo systemctl start nexa-api
sudo systemctl status nexa-api
sudo journalctl -u nexa-api -f  # Suivre les logs
```

---

### 2. Nginx Reverse Proxy

**/etc/nginx/sites-available/nexa-api**:
```nginx
upstream nexa_backend {
    least_conn;
    server 127.0.0.1:3000 max_fails=3 fail_timeout=30s;
    # Pour load balancing multi-instance:
    # server 127.0.0.1:3001 max_fails=3 fail_timeout=30s;
}

# Rate limiting
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
limit_req_zone $binary_remote_addr zone=auth_limit:10m rate=10r/m;

server {
    listen 80;
    server_name api.nexa.game;

    # Redirect HTTP → HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.nexa.game;

    # SSL
    ssl_certificate /etc/letsencrypt/live/api.nexa.game/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.nexa.game/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # Logs
    access_log /var/log/nginx/nexa-api-access.log;
    error_log /var/log/nginx/nexa-api-error.log;

    # API routes
    location /v1/ {
        limit_req zone=api_limit burst=20 nodelay;
        
        proxy_pass http://nexa_backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 10s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
    }

    # Auth routes (rate limit plus strict)
    location /v1/auth/ {
        limit_req zone=auth_limit burst=5 nodelay;
        proxy_pass http://nexa_backend;
        # ... (mêmes headers que ci-dessus)
    }

    # Prometheus metrics (protégé, accès local uniquement)
    location /metrics {
        allow 127.0.0.1;
        deny all;
        proxy_pass http://nexa_backend;
    }

    # Health check
    location /health {
        proxy_pass http://nexa_backend;
        access_log off;
    }
}
```

**Activation**:
```bash
sudo ln -s /etc/nginx/sites-available/nexa-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

### 3. Variables d'Environnement (.env)

```bash
# App
NODE_ENV=production
PORT=3000
LOG_LEVEL=info

# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=nexa_db
DB_USER=nexa_user
DB_PASSWORD=super_secure_password_here
DB_POOL_MIN=5
DB_POOL_MAX=20

# JWT
JWT_SECRET=your_jwt_secret_here_min_32_chars
JWT_REFRESH_SECRET=your_refresh_secret_here_min_32_chars
JWT_EXPIRES_IN=3600
JWT_REFRESH_EXPIRES_IN=604800

# CORS
CORS_ORIGIN=https://nexa.game,https://www.nexa.game

# Rate Limiting
RATE_LIMIT_WINDOW_MS=60000
RATE_LIMIT_MAX_REQUESTS=100

# Monitoring
PROMETHEUS_ENABLED=true
```

---

### 4. Rotation des Logs (logrotate)

**/etc/logrotate.d/nexa-api**:
```
/var/log/nexa-api/*.log {
    daily
    rotate 14
    compress
    delaycompress
    missingok
    notifempty
    create 0640 nexa nexa
    sharedscripts
    postrotate
        systemctl reload nexa-api > /dev/null 2>&1 || true
    endscript
}
```

---

### 5. Prometheus Configuration

**/etc/prometheus/prometheus.yml**:
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'nexa-api'
    static_configs:
      - targets: ['localhost:3000']
    metrics_path: '/metrics'
    scrape_interval: 10s
```

---

### 6. Loki Configuration (Pull mode)

**/etc/loki/config.yml**:
```yaml
auth_enabled: false

server:
  http_listen_port: 3100

ingester:
  lifecycler:
    ring:
      kvstore:
        store: inmemory
      replication_factor: 1

schema_config:
  configs:
    - from: 2020-10-24
      store: boltdb-shipper
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 24h

storage_config:
  boltdb_shipper:
    active_index_directory: /var/lib/loki/index
    cache_location: /var/lib/loki/cache
    shared_store: filesystem
  filesystem:
    directory: /var/lib/loki/chunks

limits_config:
  enforce_metric_name: false
  reject_old_samples: true
  reject_old_samples_max_age: 168h

chunk_store_config:
  max_look_back_period: 0s

table_manager:
  retention_deletes_enabled: true
  retention_period: 336h  # 14 jours
```

---

### 7. Script de Déploiement

**/opt/nexa/scripts/deploy.sh**:
```bash
#!/bin/bash
set -e

echo "🚀 Deploying NexA API..."

# Pull latest code
cd /opt/nexa/backend
git pull origin main

# Install dependencies
npm ci --production

# Build
npm run build

# Run migrations
npm run migration:run

# Restart service
sudo systemctl restart nexa-api

# Health check
sleep 5
curl -f http://localhost:3000/health || exit 1

echo "✅ Deployment successful!"
```

---

## 🚨 Alertes Grafana (exemples)

### Alerte 1: **High Error Rate**
```
WHEN avg() OF query(
  sum(rate(http_errors_total[5m])) / sum(rate(http_requests_total[5m])) * 100
) IS ABOVE 5
FOR 5m
```
**Action**: Notification Slack/Discord

### Alerte 2: **High API Latency**
```
WHEN avg() OF query(
  histogram_quantile(0.95, rate(http_request_duration_ms_bucket[5m]))
) IS ABOVE 500
FOR 10m
```

### Alerte 3: **Database Connections Critical**
```
WHEN last() OF query(db_connections_active) IS ABOVE 80
FOR 5m
```

### Alerte 4: **Service Down**
```
WHEN last() OF query(up{job="nexa-api"}) IS BELOW 1
FOR 1m
```


