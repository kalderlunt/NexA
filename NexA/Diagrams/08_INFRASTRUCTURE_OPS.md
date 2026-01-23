# Infrastructure & Observabilité

## Vue d'ensemble

Ce document décrit l'infrastructure de déploiement sur Debian et la stack d'observabilité complète.

---

## Diagramme 53: Déploiement Debian

**Type de diagramme souhaité**: Infrastructure Diagram

**Description**:
Architecture de déploiement complète sur serveur Debian.

### Serveur Debian (Single Server MVP)

**OS**: Debian 12 (Bookworm)
**Specs minimales**:
- CPU: 2 vCPU
- RAM: 4 GB
- Storage: 50 GB SSD
- Network: 1 Gbps

### Services Stack

```
┌─────────────────────────────────────────────────────┐
│                   SERVEUR DEBIAN                     │
├─────────────────────────────────────────────────────┤
│                                                       │
│  ┌─────────────────────────────────────────────┐   │
│  │              Nginx (Port 80/443)             │   │
│  │          (Reverse Proxy + SSL/TLS)           │   │
│  └──────────────────┬──────────────────────────┘   │
│                     │                                │
│  ┌──────────────────┴──────────────────────────┐   │
│  │                                               │   │
│  │  ┌────────────────┐    ┌──────────────────┐ │   │
│  │  │ NestJS API     │    │   Grafana        │ │   │
│  │  │ (Port 3000)    │    │   (Port 3001)    │ │   │
│  │  └────────┬───────┘    └──────────────────┘ │   │
│  │           │                                   │   │
│  │  ┌────────┴───────┐    ┌──────────────────┐ │   │
│  │  │ PostgreSQL     │    │  Prometheus      │ │   │
│  │  │ (Port 5432)    │    │  (Port 9090)     │ │   │
│  │  └────────────────┘    └──────────────────┘ │   │
│  │                                               │   │
│  │  ┌──────────────────┐  ┌──────────────────┐ │   │
│  │  │    Loki          │  │   Promtail       │ │   │
│  │  │  (Port 3100)     │  │   (Log Agent)    │ │   │
│  │  └──────────────────┘  └──────────────────┘ │   │
│  │                                               │   │
│  └───────────────────────────────────────────────┘  │
│                                                       │
└─────────────────────────────────────────────────────┘
```

### Filesystem Structure

```
/
├── opt/
│   └── nexa-api/                    # Application
│       ├── dist/                    # Compiled JS
│       ├── node_modules/
│       ├── package.json
│       └── ecosystem.config.js      # PM2 config
│
├── etc/
│   ├── nginx/
│   │   ├── nginx.conf
│   │   └── sites-available/
│   │       └── nexa-api.conf
│   │
│   ├── nexa/
│   │   └── api.env                  # Secrets (chmod 600)
│   │
│   ├── prometheus/
│   │   └── prometheus.yml
│   │
│   └── loki/
│       └── loki-config.yml
│
├── var/
│   ├── log/
│   │   └── nexa-api/
│   │       ├── app.log
│   │       ├── error.log
│   │       └── access.log
│   │
│   └── lib/
│       ├── postgresql/
│       │   └── data/                # DB data
│       │
│       ├── prometheus/
│       │   └── data/                # Metrics storage
│       │
│       └── loki/
│           └── chunks/              # Log storage
│
└── home/
    └── nexa/                        # Service user home
```

---

## Diagramme 54: Nginx Configuration

**Type de diagramme souhaité**: Configuration Diagram

**Description**:
Configuration Nginx pour reverse proxy.

**File**: `/etc/nginx/sites-available/nexa-api.conf`

```nginx
# Rate limiting zones
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/s;
limit_req_zone $binary_remote_addr zone=login_limit:10m rate=10r/m;

# Upstream API
upstream nexa_api {
    server localhost:3000 max_fails=3 fail_timeout=30s;
    keepalive 32;
}

# HTTP to HTTPS redirect
server {
    listen 80;
    server_name api.nexa.com;
    
    location / {
        return 301 https://$server_name$request_uri;
    }
}

# HTTPS server
server {
    listen 443 ssl http2;
    server_name api.nexa.com;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/api.nexa.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.nexa.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # Request ID (for correlation)
    set $request_id $http_x_request_id;
    if ($request_id = "") {
        set $request_id $request_id;
    }
    add_header X-Request-ID $request_id always;

    # Logging
    access_log /var/log/nginx/nexa-api-access.log combined;
    error_log /var/log/nginx/nexa-api-error.log warn;

    # API routes
    location /api {
        # Rate limiting
        limit_req zone=api_limit burst=20 nodelay;

        # CORS headers (if needed)
        add_header Access-Control-Allow-Origin "https://nexa-client.com" always;
        add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
        add_header Access-Control-Allow-Headers "Authorization, Content-Type, X-Request-ID" always;
        add_header Access-Control-Max-Age "3600" always;

        # Preflight requests
        if ($request_method = OPTIONS) {
            return 204;
        }

        # Proxy settings
        proxy_pass http://nexa_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Request-ID $request_id;
        
        proxy_cache_bypass $http_upgrade;
        proxy_connect_timeout 10s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
    }

    # Auth routes (stricter rate limit)
    location ~* ^/api/auth/(login|register) {
        limit_req zone=login_limit burst=5 nodelay;
        
        proxy_pass http://nexa_api;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Request-ID $request_id;
    }

    # Grafana (optional - admin only)
    location /grafana/ {
        # IP whitelist (admin only)
        allow 192.168.1.0/24;
        deny all;

        proxy_pass http://localhost:3001/;
        proxy_set_header Host $host;
    }

    # Health check (no rate limit)
    location /health {
        proxy_pass http://nexa_api/health;
        access_log off;
    }
}
```

---

## Diagramme 55: Systemd Services

**Type de diagramme souhaité**: Service Configuration

**Description**:
Configuration des services systemd.

### Service 1: NestJS API

**File**: `/etc/systemd/system/nexa-api.service`

```ini
[Unit]
Description=NexA API Server (NestJS)
Documentation=https://nexa.com/docs
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
User=nexa
Group=nexa
WorkingDirectory=/opt/nexa-api

# Environment
EnvironmentFile=/etc/nexa/api.env

# Command
ExecStart=/usr/bin/node dist/main.js

# Restart policy
Restart=always
RestartSec=10
StartLimitInterval=60
StartLimitBurst=3

# Security
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/log/nexa-api

# Logging
StandardOutput=journal
StandardError=journal
SyslogIdentifier=nexa-api

# Resource limits
LimitNOFILE=65536
MemoryLimit=1G
CPUQuota=200%

[Install]
WantedBy=multi-user.target
```

**Commandes**:
```bash
# Enable et start
sudo systemctl enable nexa-api
sudo systemctl start nexa-api

# Status
sudo systemctl status nexa-api

# Logs
sudo journalctl -u nexa-api -f

# Restart
sudo systemctl restart nexa-api
```

---

### Service 2: Prometheus

**File**: `/etc/systemd/system/prometheus.service`

```ini
[Unit]
Description=Prometheus Monitoring
Documentation=https://prometheus.io/docs
After=network.target

[Service]
Type=simple
User=prometheus
Group=prometheus
WorkingDirectory=/var/lib/prometheus

ExecStart=/usr/local/bin/prometheus \
  --config.file=/etc/prometheus/prometheus.yml \
  --storage.tsdb.path=/var/lib/prometheus/data \
  --storage.tsdb.retention.time=30d \
  --web.console.templates=/etc/prometheus/consoles \
  --web.console.libraries=/etc/prometheus/console_libraries \
  --web.listen-address=:9090

Restart=always
RestartSec=5

NoNewPrivileges=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/prometheus

[Install]
WantedBy=multi-user.target
```

**Prometheus Config**: `/etc/prometheus/prometheus.yml`

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'nexa-prod'
    environment: 'production'

# Alerting (futur)
alerting:
  alertmanagers:
    - static_configs:
        - targets: []

# Scrape targets
scrape_configs:
  # NestJS API metrics
  - job_name: 'nexa-api'
    static_configs:
      - targets: ['localhost:3000']
    metrics_path: '/metrics'
    scrape_interval: 10s

  # Node exporter (system metrics)
  - job_name: 'node'
    static_configs:
      - targets: ['localhost:9100']

  # PostgreSQL exporter
  - job_name: 'postgres'
    static_configs:
      - targets: ['localhost:9187']

  # Prometheus self-monitoring
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
```

---

### Service 3: Loki (Logs)

**File**: `/etc/systemd/system/loki.service`

```ini
[Unit]
Description=Loki Log Aggregation
After=network.target

[Service]
Type=simple
User=loki
Group=loki
WorkingDirectory=/var/lib/loki

ExecStart=/usr/local/bin/loki \
  -config.file=/etc/loki/loki-config.yml

Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

**Loki Config**: `/etc/loki/loki-config.yml`

```yaml
auth_enabled: false

server:
  http_listen_port: 3100

ingester:
  lifecycler:
    address: 127.0.0.1
    ring:
      kvstore:
        store: inmemory
      replication_factor: 1
  chunk_idle_period: 5m
  chunk_retain_period: 30s

schema_config:
  configs:
    - from: 2024-01-01
      store: boltdb-shipper
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 24h

storage_config:
  boltdb_shipper:
    active_index_directory: /var/lib/loki/boltdb-shipper-active
    cache_location: /var/lib/loki/boltdb-shipper-cache
    shared_store: filesystem
  filesystem:
    directory: /var/lib/loki/chunks

limits_config:
  retention_period: 30d
  ingestion_rate_mb: 10
  ingestion_burst_size_mb: 20

chunk_store_config:
  max_look_back_period: 0s

table_manager:
  retention_deletes_enabled: true
  retention_period: 30d
```

---

### Service 4: Promtail (Log Shipper)

**File**: `/etc/systemd/system/promtail.service`

```ini
[Unit]
Description=Promtail Log Shipper
After=network.target loki.service

[Service]
Type=simple
User=root
Group=root

ExecStart=/usr/local/bin/promtail \
  -config.file=/etc/loki/promtail-config.yml

Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

**Promtail Config**: `/etc/loki/promtail-config.yml`

```yaml
server:
  http_listen_port: 9080
  grpc_listen_port: 0

positions:
  filename: /tmp/positions.yaml

clients:
  - url: http://localhost:3100/loki/api/v1/push

scrape_configs:
  # NestJS API logs
  - job_name: nexa-api
    static_configs:
      - targets:
          - localhost
        labels:
          job: nexa-api
          environment: production
          __path__: /var/log/nexa-api/*.log

    # Pipeline for JSON logs
    pipeline_stages:
      - json:
          expressions:
            timestamp: timestamp
            level: level
            message: message
            request_id: request.id
            user_id: user.id
            duration: duration
            status_code: statusCode
      
      - labels:
          level:
          request_id:
          user_id:
      
      - timestamp:
          source: timestamp
          format: RFC3339

  # Nginx access logs
  - job_name: nginx
    static_configs:
      - targets:
          - localhost
        labels:
          job: nginx
          __path__: /var/log/nginx/nexa-api-*.log

  # System logs
  - job_name: syslog
    journal:
      max_age: 12h
      labels:
        job: systemd-journal
    relabel_configs:
      - source_labels: ['__journal__systemd_unit']
        target_label: 'unit'
```

---

## Diagramme 56: Grafana Dashboards

**Type de diagramme souhaité**: Dashboard Layout

**Description**:
Structure des dashboards Grafana.

### Dashboard 1: API Overview

**Panneaux**:

1. **Requests per Second** (Graph)
   - Metric: `rate(http_requests_total[1m])`
   - Type: Time series
   - Colors: Green

2. **Response Times** (Graph)
   - Metric: `histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))`
   - Type: Time series
   - Legend: p50, p95, p99

3. **Error Rate** (Stat)
   - Metric: `rate(http_responses_total{status=~"5.."}[5m])`
   - Type: Single stat
   - Threshold: > 1% red

4. **Success Rate** (Gauge)
   - Metric: `(sum(rate(http_responses_total{status=~"2.."}[5m])) / sum(rate(http_responses_total[5m]))) * 100`
   - Type: Gauge
   - Range: 0-100%

5. **Top Endpoints** (Table)
   - Metric: `topk(10, sum by (route) (rate(http_requests_total[5m])))`
   - Type: Table

6. **Status Code Distribution** (Pie)
   - Metric: `sum by (status_code) (rate(http_responses_total[5m]))`
   - Type: Pie chart

---

### Dashboard 2: Database Performance

**Panneaux**:

1. **Query Duration** (Graph)
   - Metric: `rate(db_query_duration_seconds_sum[5m]) / rate(db_query_duration_seconds_count[5m])`
   - Type: Time series

2. **Active Connections** (Graph)
   - Metric: `db_connections_active`
   - Type: Time series

3. **Slow Queries** (Table)
   - Metric: `topk(10, db_query_duration_seconds{query_type="SELECT"})`
   - Threshold: > 100ms

4. **Connection Pool** (Stat)
   - Metrics: `db_connections_active`, `db_connections_idle`
   - Type: Stat panel

---

### Dashboard 3: Business Metrics

**Panneaux**:

1. **Users Online** (Graph)
   - Metric: `users_active_online`
   - Type: Time series

2. **New Registrations** (Graph)
   - Metric: `increase(users_registered_total[1h])`
   - Type: Time series

3. **Matches Completed** (Stat)
   - Metric: `increase(matches_completed_total[24h])`
   - Type: Big number

4. **Friend Requests** (Graph)
   - Metric: `increase(friend_requests_sent_total[1h])`
   - Type: Time series

---

### Dashboard 4: Logs Explorer

**Panneaux**:

1. **Log Stream** (Logs)
   - Query: `{job="nexa-api"}`
   - Type: Logs panel
   - Filters: level, request_id, user_id

2. **Error Logs** (Logs)
   - Query: `{job="nexa-api"} |= "error"`
   - Type: Logs panel

3. **Log Volume** (Graph)
   - Query: `sum(count_over_time({job="nexa-api"}[1m]))`
   - Type: Time series

---

## Diagramme 57: Alerting Rules

**Type de diagramme souhaité**: Alert Configuration

**Description**:
Règles d'alerting Prometheus.

**File**: `/etc/prometheus/alerts.yml`

```yaml
groups:
  - name: api_alerts
    interval: 30s
    rules:
      # High error rate
      - alert: HighErrorRate
        expr: |
          (sum(rate(http_responses_total{status=~"5.."}[5m])) 
          / sum(rate(http_responses_total[5m]))) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value | humanizePercentage }} (threshold: 5%)"

      # API down
      - alert: APIDown
        expr: up{job="nexa-api"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "API is down"
          description: "NexA API has been down for more than 1 minute"

      # High response time
      - alert: HighResponseTime
        expr: |
          histogram_quantile(0.95, 
            rate(http_request_duration_seconds_bucket[5m])
          ) > 1
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "High response time"
          description: "95th percentile response time is {{ $value }}s"

      # Database connection issues
      - alert: DatabaseConnectionLow
        expr: db_connections_active / db_connections_pool_max > 0.9
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Database connection pool nearly exhausted"
          description: "{{ $value | humanizePercentage }} of connections used"

      # Disk space
      - alert: DiskSpaceLow
        expr: |
          (node_filesystem_avail_bytes{mountpoint="/"} 
          / node_filesystem_size_bytes{mountpoint="/"}) < 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Disk space low"
          description: "Only {{ $value | humanizePercentage }} disk space remaining"

  - name: business_alerts
    interval: 1m
    rules:
      # No users online (suspicious)
      - alert: NoUsersOnline
        expr: users_active_online == 0
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "No users online"
          description: "No active users for 10 minutes (possible issue?)"
```

---

## Diagramme 58: Backup Strategy

**Type de diagramme souhaité**: Backup Flow

**Description**:
Stratégie de sauvegarde complète.

### Backup Scripts

**Database Backup**: `/opt/scripts/backup-db.sh`

```bash
#!/bin/bash
set -e

# Config
BACKUP_DIR="/var/backups/nexa-db"
RETENTION_DAYS=7
DB_NAME="nexa_db"
DB_USER="nexa_backup_user"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
S3_BUCKET="s3://nexa-backups"

# Create backup dir
mkdir -p "$BACKUP_DIR"

# Dump database
echo "[$(date)] Starting database backup..."
pg_dump -U "$DB_USER" -Fc "$DB_NAME" > "$BACKUP_DIR/nexa_db_$TIMESTAMP.dump"

# Compress
gzip "$BACKUP_DIR/nexa_db_$TIMESTAMP.dump"

# Upload to S3 (optional)
if command -v aws &> /dev/null; then
    echo "[$(date)] Uploading to S3..."
    aws s3 cp "$BACKUP_DIR/nexa_db_$TIMESTAMP.dump.gz" "$S3_BUCKET/daily/"
fi

# Delete old backups
echo "[$(date)] Cleaning old backups..."
find "$BACKUP_DIR" -name "*.dump.gz" -mtime +$RETENTION_DAYS -delete

echo "[$(date)] Backup completed successfully"
```

**Cron job**: `/etc/cron.d/nexa-backups`

```cron
# Daily DB backup at 3 AM
0 3 * * * nexa /opt/scripts/backup-db.sh >> /var/log/nexa-backup.log 2>&1

# Weekly full backup (Sundays at 2 AM)
0 2 * * 0 nexa /opt/scripts/backup-full.sh >> /var/log/nexa-backup.log 2>&1
```

---

## Diagramme 59: Deployment Checklist

**Type de diagramme souhaité**: Checklist

**Description**:
Checklist complète pour déploiement production.

### Pre-Deployment

- [ ] **Server setup**
  - [ ] Debian 12 installé
  - [ ] User `nexa` créé
  - [ ] SSH key-based auth configuré
  - [ ] Firewall configuré (ufw)
  - [ ] Fail2ban installé

- [ ] **Dependencies installées**
  - [ ] Node.js 20 LTS
  - [ ] PostgreSQL 15
  - [ ] Nginx
  - [ ] Prometheus
  - [ ] Loki + Promtail
  - [ ] Grafana

- [ ] **DNS configuré**
  - [ ] api.nexa.com → Server IP
  - [ ] SSL certificate (Let's Encrypt)

### Deployment

- [ ] **Database**
  - [ ] PostgreSQL initialisée
  - [ ] User `nexa_api_user` créé
  - [ ] Database `nexa_db` créée
  - [ ] Migrations exécutées
  - [ ] Backup initial créé

- [ ] **Application**
  - [ ] Code compilé (`npm run build`)
  - [ ] node_modules installés (production only)
  - [ ] /opt/nexa-api permissions correctes
  - [ ] .env configuré (/etc/nexa/api.env)
  - [ ] JWT secrets générés

- [ ] **Services**
  - [ ] nexa-api.service enabled
  - [ ] nginx configuré et testé
  - [ ] prometheus.service running
  - [ ] loki.service running
  - [ ] promtail.service running
  - [ ] grafana.service running

### Post-Deployment

- [ ] **Tests**
  - [ ] Health check OK (GET /health)
  - [ ] Login endpoint fonctionnel
  - [ ] Database queries OK
  - [ ] Metrics exposés (/metrics)
  - [ ] Logs collectés (Loki)

- [ ] **Monitoring**
  - [ ] Grafana dashboards configurés
  - [ ] Alerting rules actives
  - [ ] Notification channels configurés (email/Slack)

- [ ] **Security**
  - [ ] Rate limiting testé
  - [ ] SSL A+ rating (ssllabs.com)
  - [ ] Headers sécurité présents
  - [ ] Secrets rotés (production passwords)

- [ ] **Documentation**
  - [ ] Runbook créé
  - [ ] Incident procedures documentées
  - [ ] Access control documenté

---

## Métadonnées pour génération

### Icônes Infrastructure
- 🖥️ Server
- 🔒 Security
- 📊 Monitoring
- 💾 Database
- 🔄 Backup
- ⚠️ Alerts

### Outils recommandés
- Terraform (Infrastructure as Code)
- Ansible (Configuration management)
- Docker (containerization alternative)
- Kubernetes (scaling futur)

### Références
- Nginx Best Practices
- Prometheus Operator
- 12-Factor App methodology


