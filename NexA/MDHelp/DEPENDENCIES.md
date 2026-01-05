# 📦 Dependencies & Installation

## Backend (Node.js/NestJS)

### package.json Dependencies

```json
{
  "name": "nexa-backend",
  "version": "1.0.0",
  "description": "NexA Game Backend API",
  "author": "",
  "private": true,
  "license": "MIT",
  "scripts": {
    "build": "nest build",
    "start": "nest start",
    "start:dev": "nest start --watch",
    "start:debug": "nest start --debug --watch",
    "start:prod": "node dist/main",
    "test": "jest",
    "test:watch": "jest --watch",
    "test:cov": "jest --coverage",
    "test:e2e": "jest --config ./test/jest-e2e.json"
  },
  "dependencies": {
    "@nestjs/common": "^10.0.0",
    "@nestjs/core": "^10.0.0",
    "@nestjs/platform-express": "^10.0.0",
    "@nestjs/config": "^3.1.1",
    "@nestjs/jwt": "^10.2.0",
    "@nestjs/passport": "^10.0.3",
    "@nestjs/typeorm": "^10.0.1",
    "@nestjs/throttler": "^5.1.1",
    "@willsoto/nestjs-prometheus": "^6.0.0",
    "typeorm": "^0.3.17",
    "pg": "^8.11.3",
    "passport": "^0.7.0",
    "passport-jwt": "^4.0.1",
    "passport-local": "^1.0.0",
    "bcrypt": "^5.1.1",
    "class-validator": "^0.14.0",
    "class-transformer": "^0.5.1",
    "prom-client": "^15.1.0",
    "winston": "^3.11.0",
    "helmet": "^7.1.0",
    "reflect-metadata": "^0.1.13",
    "rxjs": "^7.8.1"
  },
  "devDependencies": {
    "@nestjs/cli": "^10.0.0",
    "@nestjs/schematics": "^10.0.0",
    "@nestjs/testing": "^10.0.0",
    "@types/express": "^4.17.17",
    "@types/node": "^20.3.1",
    "@types/passport-jwt": "^3.0.13",
    "@types/passport-local": "^1.0.38",
    "@types/bcrypt": "^5.0.2",
    "@typescript-eslint/eslint-plugin": "^6.0.0",
    "@typescript-eslint/parser": "^6.0.0",
    "eslint": "^8.42.0",
    "eslint-config-prettier": "^9.0.0",
    "eslint-plugin-prettier": "^5.0.0",
    "jest": "^29.5.0",
    "prettier": "^3.0.0",
    "source-map-support": "^0.5.21",
    "ts-jest": "^29.1.0",
    "ts-loader": "^9.4.3",
    "ts-node": "^10.9.1",
    "tsconfig-paths": "^4.2.0",
    "typescript": "^5.1.3"
  }
}
```

### Installation Commands

```bash
# Installer NestJS CLI globalement
npm install -g @nestjs/cli

# Créer le projet
nest new backend --skip-git

cd backend

# Installer toutes les dépendances
npm install --save \
  @nestjs/config \
  @nestjs/jwt \
  @nestjs/passport \
  @nestjs/typeorm \
  @nestjs/throttler \
  @willsoto/nestjs-prometheus \
  typeorm \
  pg \
  passport \
  passport-jwt \
  passport-local \
  bcrypt \
  class-validator \
  class-transformer \
  prom-client \
  winston \
  helmet

# Dev dependencies
npm install --save-dev \
  @types/passport-jwt \
  @types/passport-local \
  @types/bcrypt
```

---

## Unity (Packages)

### Via Package Manager (Window → Package Manager)

#### 1. DOTween (Animations)
**Option A: Git URL** (recommandé)
```
https://github.com/Demigiant/dotween.git
```

**Option B: Asset Store**
- Rechercher "DOTween" (gratuit)
- Download + Import

#### 2. Newtonsoft.Json (JSON Serialization)
**Via Package Manager: Add package by name**
```
com.unity.nuget.newtonsoft-json
```

#### 3. TextMeshPro (UI Text)
**Inclus par défaut dans Unity 2022+**
- Si popup "Import TMP Essentials" → Cliquer Import

---

### manifest.json (Packages/manifest.json)

```json
{
  "dependencies": {
    "com.unity.ide.rider": "3.0.24",
    "com.unity.ide.visualstudio": "2.0.18",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.ugui": "1.0.0",
    "com.demigiant.dotween": "https://github.com/Demigiant/dotween.git"
  }
}
```

---

## Database (PostgreSQL)

### Extensions Required

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

### PostgreSQL Installation

#### Windows
1. Download: https://www.enterprisedb.com/downloads/postgres-postgresql-downloads
2. Installer PostgreSQL 15+
3. Lors de l'installation:
   - Port: 5432 (défaut)
   - Password: choisir un mot de passe pour `postgres`
   - Locale: French ou English

#### Linux (Debian/Ubuntu)
```bash
sudo apt update
sudo apt install postgresql-15 postgresql-contrib-15
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

---

## Observabilité (Monitoring Stack)

### Prometheus

#### Windows
1. Download: https://prometheus.io/download/
2. Extraire dans `C:\prometheus\`
3. Créer `prometheus.yml` (voir OBSERVABILITY_OPS.md)

#### Linux
```bash
# Debian/Ubuntu
wget https://github.com/prometheus/prometheus/releases/download/v2.48.0/prometheus-2.48.0.linux-amd64.tar.gz
tar xvfz prometheus-2.48.0.linux-amd64.tar.gz
sudo mv prometheus-2.48.0.linux-amd64 /opt/prometheus
sudo useradd --no-create-home --shell /bin/false prometheus
sudo chown -R prometheus:prometheus /opt/prometheus
```

---

### Grafana

#### Windows
1. Download: https://grafana.com/grafana/download?platform=windows
2. Installer (MSI installer)
3. Démarrer le service Grafana

#### Linux
```bash
# Debian/Ubuntu
sudo apt-get install -y adduser libfontconfig1
wget https://dl.grafana.com/oss/release/grafana_10.2.2_amd64.deb
sudo dpkg -i grafana_10.2.2_amd64.deb
sudo systemctl start grafana-server
sudo systemctl enable grafana-server
```

---

### Loki (Logs)

#### Windows
1. Download: https://github.com/grafana/loki/releases
2. Extraire `loki-windows-amd64.exe`
3. Créer `loki-config.yml` (voir OBSERVABILITY_OPS.md)

#### Linux
```bash
wget https://github.com/grafana/loki/releases/download/v2.9.3/loki-linux-amd64.zip
unzip loki-linux-amd64.zip
sudo mv loki-linux-amd64 /usr/local/bin/loki
sudo chmod +x /usr/local/bin/loki
```

---

## Infrastructure (Production - Debian)

### Nginx

```bash
sudo apt update
sudo apt install nginx
sudo systemctl start nginx
sudo systemctl enable nginx
```

### SSL/TLS (Let's Encrypt)

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.nexa.game
```

### Node.js (via NVM)

```bash
# Installer NVM
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.5/install.sh | bash
source ~/.bashrc

# Installer Node.js 20
nvm install 20
nvm use 20
nvm alias default 20

# Vérifier
node --version  # v20.x.x
npm --version   # 10.x.x
```

---

## Vérification des Installations

### Backend Dependencies Check

```bash
cd backend
npm list --depth=0
```

**Output attendu:**
```
backend@1.0.0
├── @nestjs/common@10.3.0
├── @nestjs/config@3.1.1
├── @nestjs/jwt@10.2.0
├── bcrypt@5.1.1
├── pg@8.11.3
├── typeorm@0.3.17
└── ... (autres packages)
```

### Unity Packages Check

**Dans Unity Editor:**
1. Window → Package Manager
2. Vérifier la présence de:
   - ✅ Newtonsoft.Json (3.2.1+)
   - ✅ DOTween (1.2+)
   - ✅ TextMeshPro (3.0+)

### PostgreSQL Check

```bash
psql --version
# Output: psql (PostgreSQL) 15.5

psql -U postgres -c "SELECT version();"
# Output: PostgreSQL 15.5 on x86_64-pc-linux-gnu...
```

### Prometheus/Grafana Check

```bash
# Prometheus
curl http://localhost:9090/-/healthy
# Output: Prometheus is Healthy.

# Grafana
curl http://localhost:3000/api/health
# Output: {"database":"ok","version":"10.2.2"}
```

---

## Troubleshooting

### ❌ npm install fails (backend)

**Erreur**: `EACCES: permission denied`
```bash
# Changer le propriétaire du dossier npm global (Linux/Mac)
sudo chown -R $USER:$GROUP ~/.npm
sudo chown -R $USER:$GROUP /usr/local/lib/node_modules

# Ou utiliser nvm (recommandé)
```

**Erreur**: `bcrypt@5.1.1 install: node-pre-gyp install --fallback-to-build`
```bash
# Windows: Installer windows-build-tools
npm install --global windows-build-tools

# Linux: Installer build-essential
sudo apt-get install build-essential
```

### ❌ Unity: Package installation fails

**Erreur**: "Cannot resolve package"
```
1. Window → Package Manager → Advanced → Reset packages to defaults
2. Restart Unity
3. Réessayer l'installation
```

**Erreur**: "Git dependency could not be resolved"
```
1. Installer Git: https://git-scm.com/downloads
2. Ajouter Git au PATH
3. Restart Unity
```

### ❌ PostgreSQL: Connection refused

```bash
# Vérifier le service
# Windows:
services.msc → "postgresql-x64-15" → Start

# Linux:
sudo systemctl status postgresql
sudo systemctl start postgresql

# Vérifier le port
netstat -an | findstr 5432  # Windows
ss -tuln | grep 5432        # Linux
```

---

## Versions Recommandées (Testées)

| Component | Version | Notes |
|-----------|---------|-------|
| Node.js | 20.10+ | LTS |
| NestJS | 10.3+ | |
| TypeScript | 5.3+ | |
| PostgreSQL | 15.5+ | |
| Unity | 2022.3 LTS | |
| DOTween | 1.2.765+ | |
| Newtonsoft.Json | 3.2.1+ | Unity package |
| Prometheus | 2.48+ | |
| Grafana | 10.2+ | |
| Loki | 2.9+ | |
| Nginx | 1.24+ | |
| Debian | 12 (Bookworm) | Production |

---

## Scripts d'Installation Rapide

### backend-install.sh (Linux)

```bash
#!/bin/bash
set -e

echo "📦 Installing NexA Backend Dependencies..."

# Installer Node.js via NVM si pas déjà installé
if ! command -v node &> /dev/null; then
    echo "Installing Node.js..."
    curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.5/install.sh | bash
    source ~/.nvm/nvm.sh
    nvm install 20
fi

# Installer NestJS CLI
npm install -g @nestjs/cli

# Créer le projet backend
nest new backend --skip-git
cd backend

# Installer les dépendances
npm install --save \
  @nestjs/config \
  @nestjs/jwt \
  @nestjs/passport \
  @nestjs/typeorm \
  @nestjs/throttler \
  @willsoto/nestjs-prometheus \
  typeorm \
  pg \
  passport \
  passport-jwt \
  passport-local \
  bcrypt \
  class-validator \
  class-transformer \
  prom-client \
  winston \
  helmet

npm install --save-dev \
  @types/passport-jwt \
  @types/passport-local \
  @types/bcrypt

echo "✅ Backend dependencies installed!"
echo "Next: Copy modules from BACKEND_NESTJS.md"
```

### db-setup.sh (Linux)

```bash
#!/bin/bash
set -e

echo "🗄️ Setting up NexA Database..."

# Créer la base de données
sudo -u postgres psql << EOF
CREATE DATABASE nexa_db;
CREATE USER nexa_user WITH PASSWORD 'dev_password_123';
GRANT ALL PRIVILEGES ON DATABASE nexa_db TO nexa_user;
EOF

# Charger le schema
psql -U nexa_user -d nexa_db -f DATABASE_SCHEMA.sql

echo "✅ Database setup complete!"
echo "Test: psql -U nexa_user -d nexa_db -c 'SELECT * FROM users;'"
```

---

## 📚 Resources

- **NestJS**: https://docs.nestjs.com/
- **TypeORM**: https://typeorm.io/
- **DOTween**: http://dotween.demigiant.com/documentation.php
- **PostgreSQL**: https://www.postgresql.org/docs/
- **Prometheus**: https://prometheus.io/docs/
- **Grafana**: https://grafana.com/docs/


