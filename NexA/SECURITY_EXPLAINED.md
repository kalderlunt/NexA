# 🔐 SÉCURITÉ: Pourquoi un Hacker NE PEUT PAS Sniffer ou Modifier les Données

Ce document explique en détail comment les données de login/register sont protégées contre les attaques.

---

## 🎯 Votre Question

> "Comment je sais qu'un hacker ne peut pas sniffer et modifier les données ?"

**Réponse courte**: Grâce à **5 couches de sécurité** combinées :
1. 🔒 **HTTPS/TLS** → Chiffrement en transit (anti-sniffing)
2. 🔑 **Bcrypt** → Hashing des mots de passe (protection DB)
3. ✍️ **JWT Signé** → Impossible de modifier le token
4. ⏱️ **Rate Limiting** → Limite les attaques par force brute
5. 🛡️ **Validation Serveur** → Vérification stricte de toutes les entrées

---

## 📡 Scénario 1: Sniffing du Réseau (Wireshark, tcpdump)

### ❌ Attaque Sans Protection (HTTP):

```
[Hacker avec Wireshark]
    ↓ Capture le trafic
    
POST http://api.nexa.com/auth/login
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "supersecret123"
}

→ Le hacker voit TOUT en clair !
→ Il peut voler le mot de passe
```

### ✅ Protection avec HTTPS/TLS 1.3:

```
[Hacker avec Wireshark]
    ↓ Capture le trafic
    
Paquet réseau capturé:
16 03 03 00 A4 ��ڱ���#�]��ݿ��B8�...
   ^
   └─ Tête TLS, mais contenu CHIFFRÉ

→ Impossible de lire sans la clé privée du serveur
→ La clé privée ne quitte JAMAIS le serveur
→ Chiffrement AES-256-GCM (standard militaire)
```

**Métaphore**: C'est comme envoyer une lettre dans un coffre-fort verrouillé. 
Même si le hacker intercepte le colis, il ne peut pas l'ouvrir sans la clé.

### Comment Activer HTTPS:

#### Développement (Certificat auto-signé):
```powershell
# Générer un certificat SSL local
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"

# Dans le code Go:
srv.ListenAndServeTLS("cert.pem", "key.pem")
```

#### Production (Let's Encrypt - Gratuit):
```bash
# Installer Certbot
apt install certbot

# Obtenir un certificat gratuit
certbot certonly --standalone -d api.nexa.com

# Certificats générés dans:
/etc/letsencrypt/live/api.nexa.com/fullchain.pem
/etc/letsencrypt/live/api.nexa.com/privkey.pem
```

---

## 🗄️ Scénario 2: Vol de la Base de Données

### ❌ Sans Protection (Mot de passe en clair):

```sql
SELECT * FROM users;

id  | email              | password
----|--------------------|--------------
123 | alice@example.com  | supersecret123
456 | bob@example.com    | password456

→ Si un hacker accède à la DB, TOUS les mots de passe sont exposés
→ Il peut se connecter aux comptes
```

### ✅ Protection avec Bcrypt Hash:

```sql
SELECT * FROM users;

id  | email              | password_hash
----|--------------------|-------------------------------------------------
123 | alice@example.com  | $2a$12$KIXw7qXlQ9v.N6FZK3p3eOv8Yv4J8...
456 | bob@example.com    | $2a$12$QzN9Y3p8F2w1R6m4T7k3L9h2D5v8...

→ Le hacker voit seulement des hashs
→ IMPOSSIBLE de récupérer le mot de passe original
→ Chaque hash est unique (salt aléatoire inclus)
```

### Pourquoi Bcrypt est Incassable:

#### Test de Brute Force:

```powershell
# Hasher "password123" avec Bcrypt (coût 12)
Measure-Command { bcrypt.GenerateFromPassword("password123", 12) }

# Résultat: ~300ms pour UN SEUL hash
```

**Calcul d'attaque**:
- 1 tentative = 300ms
- 1 million de tentatives = 300,000 secondes = **83 heures**
- 1 milliard de tentatives = **95 ans !**

**Comparaison**: SHA256 (non sécurisé pour mots de passe):
- 1 milliard de tentatives = quelques secondes sur GPU

#### Rainbow Tables (Tables Précalculées):

```
Sans Salt:
password123 → MD5 → 482c811da5d5b4bc6d497ffa98491e38
→ Chercher dans rainbow table → Trouvé!

Avec Bcrypt (Salt inclus):
password123 + salt_random_1 → $2a$12$KIXw7qXlQ9v...
password123 + salt_random_2 → $2a$12$QzN9Y3p8F2w...
                                ^
                                └─ Différent à chaque fois!

→ Rainbow tables INUTILES (il faudrait une table par salt)
```

---

## 🎫 Scénario 3: Modification du Token JWT

### ❌ Tentative d'Attaque:

```powershell
# 1. Hacker intercepte le token
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMTIzNCIsImV4cCI6MTY3ODkwMDAwMH0.5xK7QzN9Y3p8F2w1R6m4T7k3L9h2D5v8J1x6Z4n3M0s"

# 2. Décoder le payload (sans vérifier la signature)
$parts = $token.Split('.')
$payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($parts[1]))

Write-Host $payload
# Résultat: {"user_id":"1234","exp":1678900000}

# 3. Modifier le user_id pour se faire passer pour admin
$fakePayload = '{"user_id":"9999","exp":9999999999}'  # user_id admin
$fakePayloadBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($fakePayload))

# 4. Reconstruire le token avec le payload modifié
$fakeToken = "$($parts[0]).$fakePayloadBase64.$($parts[2])"

# 5. Envoyer la requête avec le token modifié
Invoke-RestMethod -Uri "http://localhost:8080/api/v1/auth/profile" `
    -Headers @{"Authorization"="Bearer $fakeToken"}
```

### ✅ Résultat de la Protection:

```json
{
  "error": "Token invalide"
}

HTTP Status: 401 Unauthorized
```

**Pourquoi ça échoue:**

```
Token JWT Structure:
HEADER.PAYLOAD.SIGNATURE

Signature = HMAC-SHA256(
    base64(header) + "." + base64(payload),
    SECRET_KEY  ← Seulement le serveur connaît cette clé
)

Vérification côté serveur:
1. Recalculer la signature avec la clé secrète
2. Comparer avec la signature du token

Si payload modifié:
  Signature calculée: ABC123...
  Signature du token:  XYZ789...
  ↓
  MISMATCH → 401 Unauthorized
```

### Protection Additionnelle: JWT ID (jti)

```go
// Lors de la génération du token
claims := jwt.MapClaims{
    "user_id": "1234",
    "jti": uuid.New().String(),  // ID unique
    "exp": time.Now().Add(24*time.Hour).Unix(),
}

// Côté serveur, on peut blacklister un jti dans Redis:
redis.Set(ctx, "blacklist:"+jti, "1", 24*time.Hour)

// Lors de la validation:
if redis.Exists(ctx, "blacklist:"+jti).Val() == 1 {
    return errors.New("token révoqué")
}
```

**Utilité**: 
- Si un token est compromis, on peut le révoquer
- Empêche les replay attacks (rejouer un token volé)

---

## 🔥 Scénario 4: Attaque par Force Brute

### ❌ Sans Rate Limiting:

```python
# Script d'attaque
import requests

passwords = open('10million_passwords.txt').readlines()

for pwd in passwords:
    response = requests.post('http://api.nexa.com/auth/login', json={
        'email': 'alice@example.com',
        'password': pwd.strip()
    })
    
    if response.status_code == 200:
        print(f"Trouvé! {pwd}")
        break

# Peut essayer 10,000 mots de passe en quelques secondes !
```

### ✅ Avec Rate Limiting (Redis):

```
Requête 1-100: ✅ OK
Requête 101:   ❌ 429 Too Many Requests

Headers de réponse:
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1678900042
Retry-After: 60

Body:
{
  "error": "Trop de requêtes, veuillez réessayer plus tard",
  "retry_after": 60
}
```

**Protection Multi-Niveaux**:

```
1. Nginx (Load Balancer)
   └─ limit_req_zone: 10 req/s par IP

2. Backend (Redis)
   └─ 100 req/minute par IP

3. Fail2Ban (Système)
   └─ Ban automatique après 10 échecs de login
```

**Résultat**:
- Attaque par force brute = **impossible**
- Maximum 100 tentatives/minute
- Pour tester 10,000 mots de passe = 100 minutes minimum
- Pour 1 million = **694 jours !**

---

## 🛡️ Scénario 5: Injection SQL

### ❌ Code Vulnérable (Concaténation de Strings):

```go
// ❌ NE JAMAIS FAIRE CELA
query := "SELECT * FROM users WHERE email = '" + email + "' AND password = '" + password + "'"
db.Query(query)

// Attaque:
email = "admin' OR '1'='1'; --"
password = "anything"

// Requête générée:
SELECT * FROM users WHERE email = 'admin' OR '1'='1'; --' AND password = 'anything'
                                            ^
                                            └─ Toujours vrai!

→ L'attaquant se connecte sans mot de passe!
```

### ✅ Code Sécurisé (Parameterized Queries):

```go
// ✅ TOUJOURS utiliser des requêtes préparées
query := "SELECT * FROM users WHERE email = $1 AND password = $2"
db.QueryRow(query, email, password)

// Attaque tentée:
email = "admin' OR '1'='1'; --"
password = "anything"

// Ce qui est effectivement cherché:
SELECT * FROM users WHERE email = 'admin'' OR ''1''=''1''; --' AND password = 'anything'
                                            ^
                                            └─ Échappé automatiquement!

→ Aucun utilisateur trouvé → Login échoue
```

**Protection automatique**:
- Les paramètres sont **échappés** automatiquement
- Les guillemets sont doublés: `'` → `''`
- Impossible d'injecter du SQL

---

## 💡 Résumé: Pourquoi C'est Sécurisé

| Attaque | Protection | Résultat |
|---------|-----------|----------|
| **Sniffing Réseau** | HTTPS/TLS 1.3 | ❌ Données chiffrées (AES-256) |
| **Vol Base de Données** | Bcrypt (coût 12) | ❌ Hash impossible à inverser |
| **Modification Token** | JWT Signature (HMAC-SHA256) | ❌ Signature invalide = rejeté |
| **Brute Force** | Rate Limiting (100/min) | ❌ Bloqué après 100 tentatives |
| **Injection SQL** | Parameterized Queries | ❌ SQL échappé automatiquement |
| **Replay Attack** | JWT exp + jti | ❌ Token expire + peut être révoqué |
| **Timing Attack** | subtle.ConstantTimeCompare | ❌ Temps identique peu importe le résultat |

---

## 🧪 Tester Vous-Même

### Test 1: Vérifier le Chiffrement HTTPS

```powershell
# Sans HTTPS (développement)
$response = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/auth/login" `
    -Method POST -ContentType "application/json" `
    -Body '{"email":"test@example.com","password":"password123"}'

# Avec Wireshark, vous verriez le mot de passe en clair

# Avec HTTPS (production)
$response = Invoke-WebRequest -Uri "https://api.nexa.com/api/v1/auth/login" `
    -Method POST -ContentType "application/json" `
    -Body '{"email":"test@example.com","password":"password123"}'

# Avec Wireshark, vous verriez seulement: �x��ڱ���#�]��ݿ��...
```

### Test 2: Tenter de Casser le Hash Bcrypt

```powershell
# Générer un hash
go run -c '
package main
import (
    "fmt"
    "golang.org/x/crypto/bcrypt"
)
func main() {
    hash, _ := bcrypt.GenerateFromPassword([]byte("password123"), 12)
    fmt.Println(string(hash))
}
'

# Résultat: $2a$12$KIXw7qXlQ9v.N6FZK3p3eOv8Yv4...

# Tentative de craquage avec John the Ripper:
echo '$2a$12$KIXw7qXlQ9v.N6FZK3p3eOv8Yv4...' > hash.txt
john hash.txt --wordlist=rockyou.txt

# Résultat: Plusieurs heures/jours/semaines selon la puissance CPU
```

---

## 🚨 Ce Qu'il Faut TOUJOURS Faire

### ✅ Checklist Sécurité

- [ ] **HTTPS activé** en production (Let's Encrypt)
- [ ] **Bcrypt coût >= 12** pour les mots de passe
- [ ] **JWT_SECRET >= 32 caractères** aléatoires
- [ ] **Rate Limiting** activé (Redis)
- [ ] **Validation serveur** pour TOUTES les entrées
- [ ] **Requêtes préparées** (parameterized queries)
- [ ] **CORS strict** (liste blanche d'origines)
- [ ] **Security Headers** configurés
- [ ] **Logs** des tentatives de connexion
- [ ] **Monitoring** des anomalies (Grafana)

### ❌ Ne JAMAIS Faire

- [ ] Stocker les mots de passe en clair
- [ ] Utiliser MD5 ou SHA256 pour les mots de passe
- [ ] Concaténer des strings SQL
- [ ] Exposer les détails d'erreurs en production
- [ ] Utiliser HTTP en production
- [ ] Stocker le JWT_SECRET dans le code
- [ ] Faire confiance aux données client
- [ ] Négliger les logs de sécurité

---

## 📚 Ressources pour Approfondir

- **OWASP Top 10**: https://owasp.org/www-project-top-ten/
- **JWT Security**: https://tools.ietf.org/html/rfc8725
- **Bcrypt Deep Dive**: https://github.com/kelektiv/node.bcrypt.js
- **TLS 1.3 Spec**: https://tools.ietf.org/html/rfc8446
- **SQL Injection Prevention**: https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html

---

**En résumé**: Un hacker **NE PEUT PAS** sniffer ou modifier les données grâce à la combinaison de HTTPS (chiffrement), Bcrypt (hashing), JWT signé (intégrité), Rate Limiting (protection brute force) et validation serveur. 🔒
