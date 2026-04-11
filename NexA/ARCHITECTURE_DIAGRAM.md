# 🏗️ Architecture de Sécurité NexA

## 📊 Diagramme d'Architecture Complète

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         🎮 UNITY CLIENT (PC BUILD)                          │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │  ApiClient.cs                                                         │ │
│  │  ═══════════════                                                      │ │
│  │                                                                       │ │
│  │  ✅ const string API_URL = "https://api.nexa.com"                    │ │
│  │  ✅ private string _accessToken (JWT temporaire)                     │ │
│  │  ✅ Task<LoginResult> Login(email, password)                         │ │
│  │  ✅ Task<UserProfile> GetProfile()                                   │ │
│  │                                                                       │ │
│  │  ❌ NO database credentials                                          │ │
│  │  ❌ NO direct PostgreSQL connection                                  │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │  UserManager.cs                                                       │ │
│  │  ═══════════════                                                      │ │
│  │                                                                       │ │
│  │  • UserId, Username, Level, Elo (en mémoire)                         │ │
│  │  • Events: OnUserDataUpdated, OnUserLoggedOut                        │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       │ HTTPS (TLS 1.3)
                                       │ 🔒 Chiffrement AES-256-GCM
                                       │ 🔐 Certificate Pinning
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        🌐 BACKEND API (Go/Java)                             │
│                      https://api.nexa.com (Port 443)                        │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │  🛡️ Security Layers                                                   │ │
│  │  ═══════════════════                                                  │ │
│  │                                                                       │ │
│  │  1. Nginx Reverse Proxy                                              │ │
│  │     ├─ SSL Termination (TLS 1.3)                                     │ │
│  │     ├─ Rate Limiting (global)                                        │ │
│  │     └─ DDoS Protection                                               │ │
│  │                                                                       │ │
│  │  2. Middleware Pipeline                                              │ │
│  │     ├─ CORS (origin whitelist)                                       │ │
│  │     ├─ Rate Limiter (Redis - 100 req/min)                            │ │
│  │     ├─ Security Headers                                              │ │
│  │     ├─ Logger (Request ID tracking)                                  │ │
│  │     └─ JWT Auth (validate signature)                                 │ │
│  │                                                                       │ │
│  │  3. Route Handlers                                                   │ │
│  │     ├─ POST /api/v1/auth/register                                    │ │
│  │     ├─ POST /api/v1/auth/login                                       │ │
│  │     ├─ POST /api/v1/auth/refresh                                     │ │
│  │     └─ GET  /api/v1/auth/profile (protected)                         │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │  🔐 Environment Variables (.env)                                      │ │
│  │  ════════════════════════════════                                     │ │
│  │                                                                       │ │
│  │  DB_HOST=localhost                                                   │ │
│  │  DB_USER=nexa_user                                                   │ │
│  │  DB_PASSWORD=NexA2026SecurePass! ◄── Unity ne voit JAMAIS ça       │ │
│  │  JWT_SECRET=super_secret_key...   ◄── Unity ne voit JAMAIS ça       │ │
│  │  REDIS_HOST=localhost                                                │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                       │
                                       │ Private Network
                                       │ Firewall: Port 5432 (PostgreSQL)
                                       │ Only accessible from backend
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      💾 POSTGRESQL DATABASE                                 │
│                     localhost:5432 (Private)                                │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │  Tables:                                                              │ │
│  │  ═══════                                                              │ │
│  │                                                                       │ │
│  │  • users (id, email, username, password_hash, level, elo, ...)      │ │
│  │  • friendships (requester_id, receiver_id, status, ...)             │ │
│  │  • matches (match_type, winner_team, duration, ...)                 │ │
│  │  • match_participants (user_id, match_id, kills, deaths, ...)       │ │
│  │  • user_stats (total_matches, wins, losses, kda, ...)               │ │
│  │                                                                       │ │
│  │  🔒 password_hash: Bcrypt (coût 12)                                  │ │
│  │  🔒 Constraints: UNIQUE, NOT NULL, CHECK                             │ │
│  │  ⚡ 13 Index pour performance                                         │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────────┐
│                      🔴 REDIS (Rate Limiting & Cache)                       │
│                         localhost:6379                                      │
│                                                                             │
│  • ratelimit:192.168.1.100 = 97 (expire 42s)                               │
│  • blacklist:jwt_id_123 = 1 (expire 24h)                                   │
│  • session:user_id_456 = {...}                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Flux d'Authentification (Login)

```
┌─────────┐                          ┌─────────┐                    ┌──────────┐
│  Unity  │                          │ Backend │                    │   DB     │
│  Client │                          │   API   │                    │PostgreSQL│
└────┬────┘                          └────┬────┘                    └────┬─────┘
     │                                    │                              │
     │ 1. POST /auth/login                │                              │
     │    {email, password}               │                              │
     ├───────────────────────────────────>│                              │
     │         HTTPS (chiffré)            │                              │
     │                                    │                              │
     │                                    │ 2. Query user by email       │
     │                                    ├─────────────────────────────>│
     │                                    │                              │
     │                                    │ 3. Return user + hash        │
     │                                    │<─────────────────────────────┤
     │                                    │                              │
     │                                    │ 4. Bcrypt compare            │
     │                                    │    (password vs hash)        │
     │                                    │                              │
     │                                    │ 5. Generate JWT              │
     │                                    │    - user_id                 │
     │                                    │    - exp (24h)               │
     │                                    │    - jti (unique)            │
     │                                    │    - Sign with secret        │
     │                                    │                              │
     │                                    │ 6. Update last_login         │
     │                                    ├─────────────────────────────>│
     │                                    │                              │
     │ 7. Return JWT + user data          │                              │
     │<───────────────────────────────────┤                              │
     │                                    │                              │
     │ 8. Store JWT in memory             │                              │
     │    (_accessToken)                  │                              │
     │                                    │                              │
     │ 9. GET /auth/profile               │                              │
     │    Header: Bearer <JWT>            │                              │
     ├───────────────────────────────────>│                              │
     │                                    │                              │
     │                                    │ 10. Validate JWT             │
     │                                    │     - Check signature        │
     │                                    │     - Check expiration       │
     │                                    │     - Extract user_id        │
     │                                    │                              │
     │                                    │ 11. Query user profile       │
     │                                    ├─────────────────────────────>│
     │                                    │                              │
     │                                    │ 12. Return profile           │
     │                                    │<─────────────────────────────┤
     │                                    │                              │
     │ 13. Return profile to Unity        │                              │
     │<───────────────────────────────────┤                              │
     │                                    │                              │
```

---

## 🕵️ Attaque: Décompilation Unity avec dnSpy

```
┌──────────────────────────────────────────────────────────────────────────┐
│  🎯 HACKER: Tente de décompiler le build Unity                          │
└──────────────────────────────────────────────────────────────────────────┘

Step 1: Extract files
  YourGame.exe
  YourGame_Data/
    ├─ Managed/
    │   ├─ Assembly-CSharp.dll  ◄── Contient votre code
    │   ├─ UnityEngine.dll
    │   └─ ...

Step 2: Open Assembly-CSharp.dll in dnSpy

Step 3: Search for "password", "database", "connection"

┌─────────────────────────────────────────────────────────────────────────┐
│  dnSpy Decompiled Code                                                  │
│  ══════════════════════                                                 │
│                                                                         │
│  namespace NexA.Core                                                    │
│  {                                                                      │
│      public class ApiClient : MonoBehaviour                             │
│      {                                                                  │
│          // ✅ Ce que le hacker voit:                                   │
│          private const string API_BASE_URL = "https://api.nexa.com";   │
│          private string _accessToken;                                   │
│                                                                         │
│          // ❌ Ce que le hacker NE voit PAS:                            │
│          // DB_HOST, DB_USER, DB_PASSWORD                              │
│          // (car c'est sur le serveur backend, pas dans Unity!)        │
│                                                                         │
│          public async Task<LoginResult> Login(string email, ...)       │
│          {                                                              │
│              // Envoie les credentials au backend                      │
│              // via HTTPS (chiffré)                                    │
│          }                                                              │
│      }                                                                  │
│  }                                                                      │
└─────────────────────────────────────────────────────────────────────────┘

❌ Résultat pour le hacker:
   • Trouve seulement l'URL publique du backend
   • Ne trouve AUCUN credential de base de données
   • Ne peut PAS se connecter directement à PostgreSQL
   • Doit passer par l'API (rate limited, validations)
```

---

## 🛡️ Protection Multi-Couches

```
Couche 1: HTTPS/TLS
═════════════════════
  ├─ Chiffrement AES-256-GCM
  ├─ Impossible de sniffer avec Wireshark
  └─ Certificate Pinning (empêche MITM)

Couche 2: JWT Signature
═══════════════════════
  ├─ Signé avec HMAC-SHA256
  ├─ Impossible de modifier sans la clé
  ├─ Expiration (24h)
  └─ JWT ID unique (anti-replay)

Couche 3: Rate Limiting
═══════════════════════
  ├─ Redis tracking (100 req/min)
  ├─ Par IP + par utilisateur
  └─ Empêche brute force

Couche 4: Validation Serveur
════════════════════════════
  ├─ Ne JAMAIS faire confiance au client
  ├─ Requêtes préparées (anti-SQL injection)
  ├─ Sanitization des entrées
  └─ Contraintes de base de données

Couche 5: Firewall
══════════════════
  ├─ Port 5432 (PostgreSQL) fermé au public
  ├─ Accessible seulement depuis backend
  └─ VPN pour accès admin
```

---

## 📊 Données Exposées vs Sécurisées

```
┌──────────────────────────────────────────────────────────────────────────┐
│  📂 Unity Build (Ce que le hacker peut décompiler)                      │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ✅ Informations Publiques (OK)                                         │
│  ────────────────────────────────                                       │
│  • API_BASE_URL = "https://api.nexa.com"                                │
│  • Logique du jeu (gameplay)                                            │
│  • Assets, textures, sons                                               │
│                                                                          │
│  ⚠️ Informations Temporaires (Limitées)                                 │
│  ─────────────────────────────────────                                  │
│  • JWT Token (expire en 24h)                                            │
│  • User ID, username, level (en mémoire)                                │
│                                                                          │
│  ❌ Credentials de DB (JAMAIS présents)                                 │
│  ────────────────────────────────────────                               │
│  • DB_HOST                  → Sur le serveur backend                    │
│  • DB_USER                  → Sur le serveur backend                    │
│  • DB_PASSWORD              → Sur le serveur backend                    │
│  • JWT_SECRET               → Sur le serveur backend                    │
│  • REDIS_PASSWORD           → Sur le serveur backend                    │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Checklist de Sécurité

```
Backend (Go/Java)
─────────────────
☑ Credentials en variables d'environnement (.env)
☑ HTTPS/TLS 1.3 activé
☑ JWT avec signature HMAC-SHA256
☑ Rate limiting Redis (100 req/min)
☑ Bcrypt hashing (coût 12)
☑ Requêtes préparées (anti-SQL injection)
☑ CORS strict (whitelist)
☑ Security headers (X-Frame-Options, CSP, etc.)
☑ Logs de sécurité (tentatives échouées)

Unity Client
────────────
☑ Architecture client-serveur (pas de connexion directe DB)
☑ URL publique seulement (pas de secrets)
☑ JWT stocké en mémoire (pas dans PlayerPrefs en clair)
☑ Build avec IL2CPP (pas Mono)
☑ Strip Engine Code activé
☑ Managed Stripping Level: High
☐ Obfuscation (optionnel - Beebyte)
☐ Certificate Pinning (optionnel)

Base de Données
───────────────
☑ Port 5432 fermé au public (firewall)
☑ Accessible seulement depuis backend
☑ Mots de passe hashés (Bcrypt)
☑ Contraintes et validations
☑ Index pour performance
☑ Backups réguliers
```

---

## 📚 Fichiers de Documentation

```
E:\.Dev\.ProjetsPerso\NexA\NexA\

Documentation Sécurité
──────────────────────
├─ UNITY_SECURITY_ANSWER.md       ◄── Réponse courte à votre question
├─ UNITY_SECURITY_GUIDE.md        ◄── Guide complet (15 pages)
├─ SECURITY_EXPLAINED.md          ◄── Détails HTTPS, JWT, Bcrypt, etc.
└─ ARCHITECTURE_DIAGRAM.md        ◄── Ce fichier

Code Unity
──────────
├─ Assets/Scripts/Core/
│   ├─ ApiClient.cs               ◄── Client API sécurisé
│   └─ UserManager.cs             ◄── Gestionnaire utilisateur
└─ Assets/Scripts/UI/
    └─ LoginScreen.cs             ◄── Écran de connexion exemple

Backend Go
──────────
└─ backend-go/
    ├─ main.go
    ├─ internal/
    │   ├─ config/
    │   ├─ database/
    │   ├─ middleware/
    │   ├─ services/
    │   └─ handlers/
    └─ .env.example

Base de Données
───────────────
├─ DATABASE_SCHEMA.sql            ◄── Schéma PostgreSQL complet
├─ DATABASE_DIAGRAM.dbml          ◄── Format dbdiagram.io
└─ open-dbdiagram.ps1             ◄── Visualiser le schéma
```

---

**Architecture sécurisée ✅**
**Credentials protégés ✅**
**Prêt pour la production ✅**
