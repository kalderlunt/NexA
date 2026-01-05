# 🚀 NexA - Commandes Essentielles

## 📦 Unity

### Setup Initial
```bash
# Installer DOTween via Asset Store ou Package Manager
# Window > Package Manager > My Assets > DOTween

# Installer Newtonsoft.Json
# Window > Package Manager > Add package by name
# com.unity.nuget.newtonsoft-json
```

### Tester le Projet
```bash
# Ouvrir Unity
# File > Open Project > Sélectionner E:\.Dev\.ProjetsPerso\NexA\NexA

# Play Mode
# Assets/Hub/Scenes/MainHub.unity (à créer)
# Appuyer sur Play

# Build
# File > Build Settings
# Add Open Scenes
# Build (Windows Standalone)
```

---

## 🖥️ Backend Node.js

### Setup Initial
```bash
# Créer le projet
mkdir nexa-backend
cd nexa-backend
npm init -y

# Installer les dépendances
npm install express prisma @prisma/client bcrypt jsonwebtoken \
  winston prom-client cors helmet express-rate-limit joi dotenv

# Dev dependencies
npm install -D typescript @types/node @types/express \
  @types/bcrypt @types/jsonwebtoken ts-node nodemon

# TypeScript config
npx tsc --init

# Prisma setup
npx prisma init
```

### Scripts package.json
```json
{
  "scripts": {
    "dev": "nodemon --exec ts-node src/app.ts",
    "build": "tsc",
    "start": "node dist/app.js",
    "prisma:generate": "npx prisma generate",
    "prisma:migrate": "npx prisma migrate dev",
    "prisma:studio": "npx prisma studio"
  }
}
```

### Développement
```bash
# Démarrer en mode dev (avec hot-reload)
npm run dev

# Générer le client Prisma (après modification du schema)
npm run prisma:generate

# Créer une migration
npm run prisma:migrate

# Ouvrir Prisma Studio (GUI pour voir la DB)
npm run prisma:studio
```

### Build Production
```bash
# Compiler TypeScript
npm run build

# Démarrer en production
NODE_ENV=production npm start
```

---

## 🗄️ PostgreSQL

### Installation

#### Debian/Ubuntu
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

#### Mac
```bash
brew install postgresql@14
brew services start postgresql@14
```

#### Windows
```
Télécharger PostgreSQL Installer depuis postgresql.org
Utiliser pgAdmin pour gérer la DB
```

### Créer la Base de Données
```bash
# Se connecter en tant que postgres
sudo -u postgres psql

# Créer la DB et l'utilisateur
CREATE DATABASE nexa;
CREATE USER nexauser WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE nexa TO nexauser;

# Quitter
\q
```

### Commandes Utiles
```bash
# Se connecter à la DB
psql -U nexauser -d nexa

# Lister les tables
\dt

# Voir le schéma d'une table
\d users

# Requête SQL
SELECT * FROM users LIMIT 10;

# Quitter
\q
```

### Backup & Restore
```bash
# Backup
pg_dump -U nexauser nexa > backup.sql

# Restore
psql -U nexauser nexa < backup.sql
```

---

## 🔧 Prisma

### Commandes Principales
```bash
# Init (créer prisma/schema.prisma)
npx prisma init

# Générer le client TypeScript
npx prisma generate

# Créer une migration (dev)
npx prisma migrate dev --name add_users_table

# Appliquer les migrations (production)
npx prisma migrate deploy

# Reset la DB (DANGER: supprime toutes les données)
npx prisma migrate reset

# Ouvrir Prisma Studio (GUI)
npx prisma studio

# Pull schema depuis une DB existante
npx prisma db pull

# Push schema vers la DB (sans migration)
npx prisma db push
```

### Générer des Données Fake
```bash
# Créer prisma/seed.ts
# Puis:
npx ts-node prisma/seed.ts
```

---

## 🐳 Docker (Optionnel)

### PostgreSQL avec Docker
```bash
# Démarrer PostgreSQL
docker run --name nexa-postgres \
  -e POSTGRES_USER=nexauser \
  -e POSTGRES_PASSWORD=secure_password \
  -e POSTGRES_DB=nexa \
  -p 5432:5432 \
  -d postgres:14

# Arrêter
docker stop nexa-postgres

# Redémarrer
docker start nexa-postgres

# Supprimer (DANGER: perte de données)
docker rm -f nexa-postgres
```

### Backend avec Docker
```dockerfile
# Dockerfile
FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
CMD ["npm", "start"]
```

```bash
# Build
docker build -t nexa-backend .

# Run
docker run -p 3000:3000 --env-file .env nexa-backend
```

---

## 🚀 Déploiement Debian

### Setup Serveur
```bash
# Mettre à jour le système
sudo apt update && sudo apt upgrade -y

# Installer Node.js via nvm
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
source ~/.bashrc
nvm install 20
nvm use 20

# Installer PostgreSQL
sudo apt install postgresql postgresql-contrib

# Installer Nginx
sudo apt install nginx

# Installer Certbot (Let's Encrypt)
sudo apt install certbot python3-certbot-nginx
```

### Déployer le Backend
```bash
# Clone le repo
git clone https://github.com/yourusername/nexa-backend.git
cd nexa-backend

# Installer les dépendances
npm install

# Créer .env
cp .env.example .env
nano .env

# Build
npm run build

# Appliquer les migrations
npx prisma migrate deploy

# Démarrer avec PM2 (production)
npm install -g pm2
pm2 start dist/app.js --name nexa-api
pm2 startup
pm2 save
```

### Configurer Nginx
```bash
# Créer la config
sudo nano /etc/nginx/sites-available/nexa-api

# Contenu:
server {
    listen 80;
    server_name api.nexa.game;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_cache_bypass $http_upgrade;
    }
}

# Activer le site
sudo ln -s /etc/nginx/sites-available/nexa-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### SSL avec Let's Encrypt
```bash
# Obtenir le certificat
sudo certbot --nginx -d api.nexa.game

# Auto-renewal (vérifie que c'est activé)
sudo systemctl status certbot.timer
```

---

## 📊 Monitoring

### Prometheus
```bash
# Installer
wget https://github.com/prometheus/prometheus/releases/download/v2.40.0/prometheus-2.40.0.linux-amd64.tar.gz
tar xvfz prometheus-*.tar.gz
cd prometheus-*

# Configurer (prometheus.yml)
scrape_configs:
  - job_name: 'nexa-api'
    static_configs:
      - targets: ['localhost:3000']

# Démarrer
./prometheus --config.file=prometheus.yml
```

### Grafana
```bash
# Installer
sudo apt-get install -y software-properties-common
sudo add-apt-repository "deb https://packages.grafana.com/oss/deb stable main"
wget -q -O - https://packages.grafana.com/gpg.key | sudo apt-key add -
sudo apt-get update
sudo apt-get install grafana

# Démarrer
sudo systemctl start grafana-server
sudo systemctl enable grafana-server

# Accéder à http://localhost:3000 (admin/admin)
```

---

## 🧪 Tests

### Backend (Jest)
```bash
# Installer Jest
npm install -D jest @types/jest ts-jest supertest @types/supertest

# Créer jest.config.js
npx ts-jest config:init

# Lancer les tests
npm test

# Coverage
npm test -- --coverage
```

### Postman
```bash
# Créer une collection NexA API
# Endpoints à tester:
- POST /api/auth/register
- POST /api/auth/login
- GET /api/me (avec Authorization header)
- GET /api/friends
- GET /api/matches

# Exporter la collection
# File > Export > Collection v2.1
```

---

## 🔍 Debugging

### Unity Logs
```csharp
// Dans Unity Console (Window > General > Console)
// Filter par type: Log, Warning, Error

// Logs personnalisés
Debug.Log("[APIService] Request sent");
Debug.LogWarning("[AuthManager] Token expired");
Debug.LogError("[UIManager] Screen not found");
```

### Backend Logs
```bash
# Logs en temps réel (dev)
npm run dev

# Logs PM2 (production)
pm2 logs nexa-api

# Logs dans fichiers
tail -f logs/combined.log
tail -f logs/error.log

# Chercher un correlation ID
grep "abc-123-def" logs/combined.log
```

### PostgreSQL Logs
```bash
# Voir les logs PostgreSQL
sudo tail -f /var/log/postgresql/postgresql-14-main.log

# Activer le logging des slow queries (postgresql.conf)
log_min_duration_statement = 100  # Log queries > 100ms
```

---

## 🛠️ Outils Utiles

### Postman
- Tester l'API manuellement
- Créer des collections de requêtes
- Automatiser des tests

### pgAdmin
- GUI pour PostgreSQL
- Visualiser les tables
- Exécuter des requêtes SQL

### VS Code Extensions
- **Prisma** : Syntax highlighting pour schema.prisma
- **REST Client** : Tester des endpoints depuis VS Code
- **ESLint** : Linter TypeScript
- **Prettier** : Formatter le code

### Unity Extensions
- **Rider** : IDE JetBrains pour Unity (payant mais puissant)
- **VS Code with Unity Tools** : Alternative gratuite

---

## 📚 Ressources

### Documentation
- **Unity** : https://docs.unity3d.com/
- **DOTween** : http://dotween.demigiant.com/
- **Prisma** : https://www.prisma.io/docs
- **Express** : https://expressjs.com/
- **PostgreSQL** : https://www.postgresql.org/docs/

### Tutoriels
- **Unity Networking** : https://docs-multiplayer.unity3d.com/
- **Node.js Best Practices** : https://github.com/goldbergyoni/nodebestpractices
- **JWT Auth** : https://jwt.io/introduction

### Communautés
- **Discord Unity FR** : https://discord.gg/unityfr
- **Stack Overflow** : unity3d, nodejs, postgresql tags
- **Reddit** : r/Unity3D, r/node, r/webdev

---

**Bon développement ! 🚀**

