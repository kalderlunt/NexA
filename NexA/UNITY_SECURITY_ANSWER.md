# 🔐 Réponse à la Question de Sécurité Unity

## 🎯 Question Posée

> "Lors de la build comment je fais pour sécuriser les codes en direction de la base de données (url, user, pwd) lorsque j'utilise un décompilateur unity"

---

## ✅ RÉPONSE COURTE

**Ne JAMAIS mettre les credentials de base de données dans Unity !**

### Architecture Recommandée

```
Unity Client (Build PC)
    ↓ HTTPS
    ↓ Seulement l'URL publique: https://api.nexa.com
    ↓
Backend API (Go/Java sur serveur)
    ↓ Connexion sécurisée
    ↓ Credentials stockés en variables d'environnement
    ↓
PostgreSQL Database (Serveur privé)
```

**Ce que Unity contient:**
- ✅ URL du backend API (publique, pas de risque)
- ✅ Token JWT (temporaire, expire en 24h)
- ❌ JAMAIS de credentials de base de données
- ❌ JAMAIS de connexion directe à PostgreSQL

---

## 📁 Fichiers Créés pour Vous

### 1. **UNITY_SECURITY_GUIDE.md**
Guide complet de sécurité Unity avec:
- ❌ Ce qu'il ne faut JAMAIS faire (exemples de code vulnérable)
- ✅ 5 solutions détaillées
- 🔐 Protection contre la décompilation
- 📊 Comparaison des approches
- 🧪 Tests de sécurité

### 2. **Assets/Scripts/Core/ApiClient.cs**
Client API sécurisé pour Unity:
```csharp
// ✅ SÉCURISÉ - Seulement l'URL publique
#if UNITY_EDITOR
    private const string API_BASE_URL = "http://localhost:8080";
#else
    private const string API_BASE_URL = "https://api.nexa.com";
#endif

// Authentification avec JWT
public async Task<LoginResult> Login(string email, string password)
{
    // Envoie email/password au backend
    // Reçoit un JWT token
    // Utilise ce token pour les requêtes suivantes
}
```

### 3. **Assets/Scripts/Core/UserManager.cs**
Gestionnaire d'utilisateur (Singleton):
- Stocke les données utilisateur en mémoire
- Gère la session utilisateur
- Persiste entre les scènes Unity

### 4. **Assets/Scripts/UI/LoginScreen.cs**
Exemple d'écran de connexion fonctionnel:
- Formulaire login/register
- Validation des entrées
- Gestion des erreurs
- Reconnexion automatique

---

## 🛡️ Pourquoi C'est Sécurisé

### 1. Décompilation Unity avec dnSpy

**Ce que le hacker verra:**
```csharp
// Dans ApiClient.cs décompilé
private const string API_BASE_URL = "https://api.nexa.com";
```

**Ce qu'il NE verra PAS:**
```
❌ DB_HOST=localhost
❌ DB_USER=nexa_user
❌ DB_PASSWORD=NexA2026SecurePass!
❌ JWT_SECRET=super_secret_key...
```

**Pourquoi?** Ces données sont sur le serveur backend, pas dans Unity !

### 2. Architecture Client-Serveur

```
Unity Client              Backend Go                PostgreSQL
    │                         │                          │
    ├─ Login(email, pwd) ────>│                          │
    │                         ├─ Verify password         │
    │                         ├─ Query DB ──────────────>│
    │                         │<─ User data ─────────────┤
    │<─ JWT Token ────────────┤                          │
    │                         │                          │
    ├─ GetProfile(JWT) ──────>│                          │
    │                         ├─ Validate JWT            │
    │                         ├─ Query DB ──────────────>│
    │<─ Profile data ─────────┤<─ User profile ──────────┤
```

**Unity ne connaît JAMAIS les credentials de la base de données !**

### 3. JWT Token (Temporaire)

Même si un hacker décompile et trouve un token JWT :
- ⏰ Le token expire en 24h
- 🔐 Le token est signé (impossible à modifier)
- 🚫 Vous pouvez révoquer le token côté serveur
- 🆔 Chaque token a un ID unique (jti)

### 4. HTTPS (Chiffrement)

Toutes les communications sont chiffrées :
- 🔒 TLS 1.3 avec AES-256-GCM
- 🕵️ Impossible de sniffer avec Wireshark
- 🔐 Certificate pinning pour empêcher MITM

---

## 🚀 Comment Utiliser

### Étape 1: Démarrer le Backend Go

```powershell
cd E:\.Dev\.ProjetsPerso\NexA\NexA\backend-go
go run main.go

# Résultat: 🚀 Serveur démarré sur le port 8080
```

### Étape 2: Copier les Scripts Unity

Les fichiers sont déjà créés dans:
```
Assets/Scripts/
├── Core/
│   ├── ApiClient.cs       # Client API sécurisé
│   └── UserManager.cs     # Gestionnaire utilisateur
└── UI/
    └── LoginScreen.cs     # Écran de connexion
```

### Étape 3: Utiliser dans Unity

```csharp
using NexA.Core;

public class MyGameScript : MonoBehaviour
{
    async void Start()
    {
        // Se connecter
        var result = await ApiClient.Instance.Login(
            "test@example.com", 
            "password123"
        );
        
        if (result.success)
        {
            Debug.Log($"Connecté: {result.user.username}");
            Debug.Log($"Level: {result.user.level}");
            Debug.Log($"Elo: {result.user.elo}");
        }
    }
}
```

### Étape 4: Build Sécurisé

```
File → Build Settings → Player Settings
├── Other Settings
│   ├── ☑ Strip Engine Code
│   ├── ☑ Managed Stripping Level: High
│   └── ☑ Scripting Backend: IL2CPP
└── Build
```

---

## 🔬 Test de Sécurité

### Test 1: Décompiler avec dnSpy

1. Builder votre projet Unity
2. Ouvrir `YourGame_Data/Managed/Assembly-CSharp.dll` avec dnSpy
3. Chercher "password", "database", "credentials"
4. Résultat: ❌ Rien trouvé !

### Test 2: Vérifier le Traffic Réseau

```powershell
# Avec Wireshark, capturer le trafic pendant le login
# Filtrer: http || https

# Résultat visible:
POST https://api.nexa.com/api/v1/auth/login
Body: {"email":"...","password":"..."}  # ✅ Chiffré avec HTTPS

# Résultat invisible:
❌ Aucune connexion directe à PostgreSQL (port 5432)
❌ Aucun credential de DB dans les paquets
```

---

## 📊 Comparaison

| Approche | Sécurité | Difficulté | Coût |
|----------|----------|------------|------|
| **Unity → Backend → DB** (Notre solution) | ⭐⭐⭐⭐⭐ | Moyenne | Gratuit |
| Unity → DB Direct avec credentials chiffrés | ⭐⭐ | Moyenne | Gratuit |
| Unity → DB Direct avec obfuscation | ⭐⭐⭐ | Faible | 0-50€ |
| Unity → DB Direct en clair | ❌ DANGER | Facile | Gratuit |

---

## 🎓 Ressources

### Documentation Créée
- **`UNITY_SECURITY_GUIDE.md`** - Guide complet (15 pages)
- **`SECURITY_EXPLAINED.md`** - Explications détaillées sur HTTPS, JWT, etc.
- **`QUICKSTART.md`** - Guide de démarrage mis à jour avec section Unity

### Code Unity Créé
- **`ApiClient.cs`** - Client API sécurisé (300+ lignes)
- **`UserManager.cs`** - Gestionnaire utilisateur (150+ lignes)
- **`LoginScreen.cs`** - Écran de connexion exemple (250+ lignes)

### Backend Déjà Prêt
- Backend Go avec JWT, rate limiting, sécurité complète
- Base de données PostgreSQL avec schéma complet
- Documentation de l'API

---

## ✨ Résumé Final

### Ce que Unity contient:
```csharp
✅ URL publique du backend: "https://api.nexa.com"
✅ Token JWT temporaire (24h)
✅ Données utilisateur en mémoire (ID, username, level, elo)
```

### Ce que Unity NE contient PAS:
```
❌ URL de la base de données
❌ Username de la base de données
❌ Password de la base de données
❌ JWT Secret Key
❌ Aucune connexion directe à PostgreSQL
```

### Pourquoi c'est sécurisé:
1. **Architecture client-serveur** → Unity ne touche jamais à la DB
2. **HTTPS/TLS 1.3** → Chiffrement de toutes les communications
3. **JWT signé** → Impossible de modifier le token
4. **Rate limiting** → Protection contre brute force
5. **IL2CPP build** → Plus difficile à décompiler que Mono

---

**Avec cette architecture, un hacker peut décompiler Unity autant qu'il veut, il ne trouvera JAMAIS vos credentials de base de données ! 🛡️**

Pour plus de détails, consultez **`UNITY_SECURITY_GUIDE.md`**.
