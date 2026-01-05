# ⚡ Quick Start Guide - NexA

Guide de démarrage rapide pour mettre en place l'environnement complet en 15 minutes.

---

## 📋 Prérequis

- **Node.js** 20+ ([Download](https://nodejs.org/))
- **PostgreSQL** 15+ ([Download](https://www.postgresql.org/download/))
- **Unity** 2022.3+ ([Download](https://unity.com/download))
- **Git** ([Download](https://git-scm.com/downloads))

---

## 🚀 Setup Rapide (Local Dev)

### 1️⃣ Database Setup (5 min)

```bash
# Démarrer PostgreSQL (Windows)
# Le service devrait être démarré automatiquement après installation

# Créer la base de données
psql -U postgres
```

```sql
-- Dans psql:
CREATE DATABASE nexa_db;
CREATE USER nexa_user WITH PASSWORD 'dev_password_123';
GRANT ALL PRIVILEGES ON DATABASE nexa_db TO nexa_user;
\q
```

```bash
# Charger le schema
cd E:\.Dev\.ProjetsPerso\NexA\NexA
psql -U nexa_user -d nexa_db -f DATABASE_SCHEMA.sql
```

✅ **Test**: `psql -U nexa_user -d nexa_db -c "SELECT * FROM users;"`

---

### 2️⃣ Backend Setup (5 min)

```bash
# Créer le dossier backend
mkdir backend
cd backend

# Initialiser NestJS
npm i -g @nestjs/cli
nest new . --skip-git

# Installer les dépendances
npm install --save @nestjs/config @nestjs/jwt @nestjs/passport @nestjs/typeorm typeorm pg passport passport-jwt passport-local bcrypt class-validator class-transformer @nestjs/throttler @willsoto/nestjs-prometheus prom-client winston

npm install --save-dev @types/passport-jwt @types/passport-local @types/bcrypt
```

**Créer `.env`**:
```bash
# .env
NODE_ENV=development
PORT=3000
LOG_LEVEL=debug

# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=nexa_db
DB_USER=nexa_user
DB_PASSWORD=dev_password_123
DB_POOL_MIN=2
DB_POOL_MAX=10

# JWT
JWT_SECRET=your_super_secret_jwt_key_min_32_chars_here_dev_only
JWT_REFRESH_SECRET=your_refresh_secret_key_min_32_chars_here_dev_only
JWT_EXPIRES_IN=3600
JWT_REFRESH_EXPIRES_IN=604800

# CORS
CORS_ORIGIN=*

# Rate Limiting
RATE_LIMIT_WINDOW_MS=60000
RATE_LIMIT_MAX_REQUESTS=100

# Monitoring
PROMETHEUS_ENABLED=true
```

**Copier les fichiers backend** depuis `BACKEND_NESTJS.md` (modules Auth, Users, Friends, Matches)

```bash
# Démarrer le backend
npm run start:dev
```

✅ **Test**: Ouvrir `http://localhost:3000/health` dans le navigateur

---

### 3️⃣ Unity Setup (5 min)

```bash
# Ouvrir Unity Hub
# Add → Sélectionner le dossier E:\.Dev\.ProjetsPerso\NexA\NexA
# Unity Editor 2022.3+
```

**Installer DOTween**:
1. Window → Package Manager
2. `+` → Add package from git URL
3. Coller: `https://github.com/Demigiant/dotween.git`
4. Ou installer depuis l'Asset Store (gratuit)

**Installer Newtonsoft.Json**:
1. Window → Package Manager
2. `+` → Add package by name
3. `com.unity.nuget.newtonsoft-json`

**Configurer l'API URL**:
1. Créer `Assets/Resources/Config/`
2. Créer un ScriptableObject `APIConfig`
3. BaseURL = `http://localhost:3000/v1`

**Créer la scène de base**:
1. Créer `Assets/Hub/Scenes/MainHub.unity`
2. Ajouter Canvas + EventSystem
3. Ajouter GameObject vide `_Managers`:
   - APIService
   - AuthManager
   - UIManager
   - CacheManager

✅ **Test**: Play → Vérifier les logs `[UIManager] Loaded screen: ...`

---

## 🧪 Test du Flow Complet

### Backend API Test (cURL)

```bash
# 1. Register
curl -X POST http://localhost:3000/v1/auth/register \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"TestUser\",\"email\":\"test@nexa.game\",\"password\":\"TestPass123!\"}"

# 2. Login (copier le accessToken de la réponse)
curl -X POST http://localhost:3000/v1/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"test@nexa.game\",\"password\":\"TestPass123!\"}"

# 3. Get Me (remplacer YOUR_TOKEN)
curl http://localhost:3000/v1/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"

# 4. Search users
curl "http://localhost:3000/v1/users/search?q=Test" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

### Unity Test

1. **Play dans Unity**
2. Écran Login devrait apparaître avec animation
3. Entrer credentials: `test@nexa.game` / `TestPass123!`
4. Cliquer Login
5. ✅ Success si transition vers Home screen

---

## 📊 Monitoring Setup (Optionnel)

### Prometheus (Windows)

```bash
# Télécharger: https://prometheus.io/download/
# Extraire dans C:\prometheus\

# Créer prometheus.yml:
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'nexa-api'
    static_configs:
      - targets: ['localhost:3000']
    metrics_path: '/metrics'
```

```bash
# Démarrer
cd C:\prometheus
.\prometheus.exe --config.file=prometheus.yml

# Ouvrir http://localhost:9090
```

### Grafana (Windows)

```bash
# Télécharger: https://grafana.com/grafana/download?platform=windows
# Installer et démarrer le service

# Ouvrir http://localhost:3000 (default: admin/admin)
# Ajouter Prometheus datasource: http://localhost:9090
# Importer les dashboards depuis OBSERVABILITY_OPS.md
```

---

## 🔍 Troubleshooting

### ❌ Backend ne démarre pas

**Erreur**: `Cannot connect to database`
```bash
# Vérifier PostgreSQL
pg_isready -h localhost -p 5432

# Vérifier les credentials dans .env
# Tester la connexion
psql -U nexa_user -d nexa_db
```

**Erreur**: `Port 3000 already in use`
```bash
# Changer le port dans .env
PORT=3001

# Ou tuer le processus sur le port 3000
netstat -ano | findstr :3000
taskkill /PID <PID> /F
```

### ❌ Unity: Compilation errors

**Erreur**: `The type or namespace 'DOTween' could not be found`
```
→ Réinstaller DOTween depuis Package Manager
→ Vérifier Window → Package Manager → DOTween
```

**Erreur**: `The type or namespace 'Newtonsoft' could not be found`
```
→ Package Manager → Add package by name → com.unity.nuget.newtonsoft-json
```

### ❌ Unity: Can't connect to API

**Erreur**: Console: `[API] ConnectionError`
```csharp
// Vérifier l'URL dans APIService.cs
[SerializeField] private string baseURL = "http://localhost:3000/v1";

// Vérifier que le backend est démarré
// Tester dans le navigateur: http://localhost:3000/health
```

---

## 📁 Structure Finale

```
E:\.Dev\.ProjetsPerso\NexA\NexA\
├── backend/                    # Backend NestJS
│   ├── src/
│   │   ├── modules/
│   │   │   ├── auth/
│   │   │   ├── users/
│   │   │   ├── friends/
│   │   │   └── matches/
│   │   ├── main.ts
│   │   └── app.module.ts
│   ├── .env
│   └── package.json
├── Assets/                     # Unity Client
│   └── Hub/
│       └── Script/
│           ├── Core/
│           ├── Screens/
│           ├── Services/
│           ├── Models/
│           ├── Components/
│           └── Utils/
├── DATABASE_SCHEMA.sql
├── README.md
├── ARCHITECTURE.md
├── API_CONTRACT.md
└── ... (autres docs)
```

---

## 🎯 Next Steps

### MVP Week 1 Checklist

- [x] Database schema créé
- [x] Backend: Auth module (register/login/refresh)
- [x] Backend: Users module (me/search)
- [x] Unity: APIService + AuthManager
- [x] Unity: LoginScreen + animations
- [ ] **TODO**: Backend: Friends module complet
- [ ] **TODO**: Unity: FriendsScreen complet
- [ ] **TODO**: Backend: Matches module
- [ ] **TODO**: Unity: MatchHistoryScreen

### Commencer le Dev

1. **Backend**: Implémenter le module Friends
   ```bash
   cd backend
   nest g module friends
   nest g controller friends
   nest g service friends
   ```

2. **Unity**: Créer les prefabs UI
   - FriendListItem
   - ToastNotification
   - LoadingSpinner

3. **Test**: Flow complet Register → Login → Friends → Add Friend

---

## 🚀 Commandes Utiles

### Backend

```bash
# Dev mode (hot reload)
npm run start:dev

# Build production
npm run build

# Tests
npm run test
npm run test:e2e

# Migrations (TODO: setup TypeORM migrations)
npm run migration:generate
npm run migration:run
```

### Database

```bash
# Backup
pg_dump -U nexa_user nexa_db > backup.sql

# Restore
psql -U nexa_user -d nexa_db < backup.sql

# Reset (DANGER: efface tout)
psql -U nexa_user -d nexa_db -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
psql -U nexa_user -d nexa_db -f DATABASE_SCHEMA.sql
```

### Monitoring

```bash
# Logs backend (Windows PowerShell)
Get-Content -Path ".\logs\app.log" -Wait -Tail 50

# Métriques Prometheus
curl http://localhost:3000/metrics

# Health check
curl http://localhost:3000/health
```

---

## 📚 Documentation Complète

| Fichier | Description |
|---------|-------------|
| [README.md](./README.md) | Vue d'ensemble du projet |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Architecture + Roadmap |
| [API_CONTRACT.md](./API_CONTRACT.md) | Contrats API complets |
| [DATABASE_SCHEMA.sql](./DATABASE_SCHEMA.sql) | Schema PostgreSQL |
| [UNITY_ARCHITECTURE.md](./UNITY_ARCHITECTURE.md) | Architecture Unity |
| [BACKEND_NESTJS.md](./BACKEND_NESTJS.md) | Backend NestJS |
| [OBSERVABILITY_OPS.md](./OBSERVABILITY_OPS.md) | Logs/Métriques/Ops |
| **QUICK_START.md** (ce fichier) | Guide démarrage rapide |

---

## 💬 Support

Pour toute question, consulter d'abord:
1. Les docs ci-dessus
2. Les logs backend (`npm run start:dev`)
3. Les logs Unity (Console)
4. Créer une issue si problème persistant

---

**Bon développement ! 🎮**

