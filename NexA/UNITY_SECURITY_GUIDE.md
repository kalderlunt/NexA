# 🔐 Sécurité Unity: Protection des Credentials de Base de Données

## ⚠️ Le Problème

### Décompilation Unity
Les builds Unity (C#) sont facilement décompilables avec des outils comme:
- **dnSpy** - Décompilateur .NET gratuit
- **ILSpy** - Décompilateur open-source
- **dotPeek** - Décompilateur JetBrains
- **Reflexil** - Modification de DLL en direct

**Résultat:** Si vous mettez l'URL, user, password en dur dans le code Unity, un hacker peut les extraire en 2 minutes !

---

## ❌ CE QU'IL NE FAUT JAMAIS FAIRE

### Exemple de Code Vulnérable

```csharp
// ❌ DANGER - NE JAMAIS FAIRE CELA !
public class DatabaseManager : MonoBehaviour
{
    // Un hacker avec dnSpy verra TOUT ça en clair
    private string dbUrl = "postgresql://nexa_user:NexA2026SecurePass!@localhost:5432/nexa_db";
    private string apiKey = "sk_live_51K8h2k3L9...";
    private string jwtSecret = "super_secret_jwt_key_123";
    
    void Start()
    {
        ConnectToDatabase();
    }
}
```

**Après décompilation avec dnSpy:**
```csharp
// Le hacker voit exactement le même code !
private string dbUrl = "postgresql://nexa_user:NexA2026SecurePass!@localhost:5432/nexa_db";
```

---

## ✅ SOLUTION 1: Architecture Client-Serveur (RECOMMANDÉ)

### Principe: Ne JAMAIS connecter Unity directement à la base de données

```
❌ Architecture Dangereuse:
Unity Client → PostgreSQL Database
     ↑
     └─ Credentials exposés dans le build

✅ Architecture Sécurisée:
Unity Client → Backend API (Go/Java) → PostgreSQL Database
                     ↑
                     └─ Credentials seulement sur le serveur
```

### Implémentation

#### Unity (Client)
```csharp
// ✅ SÉCURISÉ - Seulement l'URL publique du backend
public class ApiClient : MonoBehaviour
{
    // L'URL du backend est publique (pas de risque)
    private const string API_BASE_URL = "https://api.nexa.com";
    
    private string accessToken; // JWT reçu après login
    
    // Login
    public async Task<bool> Login(string email, string password)
    {
        var payload = new { email, password };
        var json = JsonUtility.ToJson(payload);
        
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{API_BASE_URL}/api/v1/auth/login", json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                accessToken = response.access_token;
                return true;
            }
            return false;
        }
    }
    
    // Récupérer les données utilisateur
    public async Task<UserData> GetUserProfile()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{API_BASE_URL}/api/v1/auth/profile"))
        {
            // Le token JWT authentifie l'utilisateur
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<UserData>(request.downloadHandler.text);
            }
            return null;
        }
    }
}

[System.Serializable]
public class LoginResponse
{
    public string access_token;
    public string refresh_token;
    public int expires_in;
}
```

#### Backend (Go) - Les credentials sont ici, pas dans Unity
```go
// Ce code tourne SUR LE SERVEUR, pas dans Unity
func main() {
    // Credentials chargés depuis les variables d'environnement
    cfg := config.Load()
    
    // Connexion PostgreSQL (sécurisée côté serveur)
    db, _ := database.NewPostgresDB(cfg)
    
    // Unity ne voit JAMAIS ces credentials
}
```

**Avantages:**
- ✅ Credentials jamais exposés au client
- ✅ Décompiler Unity ne sert à rien
- ✅ Vous contrôlez 100% de l'accès aux données
- ✅ Possibilité de ban, rate limiting, etc.

---

## ✅ SOLUTION 2: Obfuscation du Code (Protection Partielle)

Si vous DEVEZ stocker des données sensibles dans Unity (API keys externes, etc.):

### 2.1 Utiliser un Obfuscateur

#### Beebyte Obfuscator (Payant - Recommandé)
```bash
# Asset Store: https://assetstore.unity.com/packages/tools/utilities/obfuscator-48919

# Protection offerte:
✅ Renommage des variables/méthodes
✅ Chiffrement des strings
✅ Anti-debugging
✅ Control flow obfuscation
```

**Avant obfuscation:**
```csharp
private string apiKey = "sk_live_51K8h2k3L9...";
```

**Après obfuscation (dnSpy):**
```csharp
private string a = "\x52\x1B\x3F\x7E\x2A..."; // String chiffrée
```

#### Alternatives Gratuites
- **ConfuserEx** (open-source pour .NET)
- **Eazfuscator.NET** (version gratuite limitée)

### 2.2 Chiffrement Manuel des Secrets

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

public class SecureConfig : MonoBehaviour
{
    // Clé de chiffrement (obfusquée elle aussi)
    private static readonly byte[] AES_KEY = new byte[] { 
        0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6,
        0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C 
    };
    
    // Credentials chiffrés (stockés en base64)
    private const string ENCRYPTED_DB_URL = "U2FsdGVkX1+ZqJ5K3p0w...";
    
    // Déchiffrer au runtime
    public static string GetDatabaseUrl()
    {
        return Decrypt(ENCRYPTED_DB_URL);
    }
    
    private static string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        
        using (Aes aes = Aes.Create())
        {
            aes.Key = AES_KEY;
            aes.IV = new byte[16]; // Simplification (utilisez un IV réel)
            
            using (var decryptor = aes.CreateDecryptor())
            {
                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
    }
}

// Utilisation
void Start()
{
    string dbUrl = SecureConfig.GetDatabaseUrl();
    // Utiliser dbUrl...
}
```

**Script PowerShell pour générer les credentials chiffrés:**
```powershell
# encrypt_credentials.ps1
$plainText = "postgresql://nexa_user:NexA2026SecurePass!@localhost:5432/nexa_db"
$key = [byte[]](0x2B,0x7E,0x15,0x16,0x28,0xAE,0xD2,0xA6,0xAB,0xF7,0x15,0x88,0x09,0xCF,0x4F,0x3C)

$aes = [System.Security.Cryptography.Aes]::Create()
$aes.Key = $key
$aes.IV = New-Object byte[] 16

$encryptor = $aes.CreateEncryptor()
$plainBytes = [System.Text.Encoding]::UTF8.GetBytes($plainText)
$encryptedBytes = $encryptor.TransformFinalBlock($plainBytes, 0, $plainBytes.Length)
$encryptedBase64 = [Convert]::ToBase64String($encryptedBytes)

Write-Host "Encrypted value for C#:"
Write-Host "private const string ENCRYPTED_DB_URL = `"$encryptedBase64`";"
```

**⚠️ Limitations:**
- La clé AES est toujours dans le code (décompilable)
- Un hacker déterminé peut toujours extraire les secrets
- **Solution partielle uniquement**

---

## ✅ SOLUTION 3: Remote Config (Firebase, PlayFab, etc.)

### Principe: Charger les configurations depuis un serveur distant

```csharp
using Firebase.RemoteConfig;

public class RemoteConfigManager : MonoBehaviour
{
    async void Start()
    {
        // Récupérer la config depuis Firebase
        await FirebaseRemoteConfig.DefaultInstance.FetchAsync();
        FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        
        // Récupérer l'URL du backend
        string apiUrl = FirebaseRemoteConfig.DefaultInstance.GetValue("api_base_url").StringValue;
        
        // Utiliser apiUrl...
    }
}
```

**Configuration Firebase Console:**
```json
{
  "api_base_url": "https://api.nexa.com",
  "enable_debug_mode": false,
  "max_retry_attempts": 3
}
```

**Avantages:**
- ✅ Config modifiable sans rebuild
- ✅ A/B testing possible
- ✅ Rollback instantané
- ⚠️ L'URL reste visible dans le build (mais pas les credentials DB)

---

## ✅ SOLUTION 4: Environnement Hybride (Développement vs Production)

### Unity Code
```csharp
public class ConfigManager : MonoBehaviour
{
    [System.Serializable]
    public class Config
    {
        public string apiUrl;
        public bool isDevelopment;
    }
    
    // Config pour développement (localhost)
    private static readonly Config DEV_CONFIG = new Config
    {
        apiUrl = "http://localhost:8080",
        isDevelopment = true
    };
    
    // Config pour production (URL publique seulement)
    private static readonly Config PROD_CONFIG = new Config
    {
        apiUrl = "https://api.nexa.com",
        isDevelopment = false
    };
    
    public static Config GetConfig()
    {
        #if UNITY_EDITOR
            return DEV_CONFIG; // Mode éditeur = dev
        #else
            return PROD_CONFIG; // Build = production
        #endif
    }
}
```

**Utilisation:**
```csharp
void Start()
{
    var config = ConfigManager.GetConfig();
    Debug.Log($"API URL: {config.apiUrl}");
    Debug.Log($"Is Dev: {config.isDevelopment}");
}
```

---

## ✅ SOLUTION 5: Certificate Pinning (Anti Man-in-the-Middle)

Empêcher l'interception HTTPS:

```csharp
using UnityEngine.Networking;

public class SecureApiClient : MonoBehaviour
{
    // Hash SHA256 du certificat SSL de votre serveur
    private const string EXPECTED_CERT_HASH = "AA:BB:CC:DD:EE:FF:00:11...";
    
    async Task<string> SecureRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Valider le certificat
            request.certificateHandler = new CertificateHandler();
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            return null;
        }
    }
}

public class CertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Calculer le hash du certificat reçu
        string receivedHash = ComputeSHA256(certificateData);
        
        // Comparer avec le hash attendu
        return receivedHash == EXPECTED_CERT_HASH;
    }
    
    private string ComputeSHA256(byte[] data)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", ":");
        }
    }
}
```

---

## 📊 Comparaison des Solutions

| Solution | Sécurité | Complexité | Coût | Recommandé |
|----------|----------|------------|------|------------|
| **Architecture Client-Serveur** | ⭐⭐⭐⭐⭐ | Moyenne | Gratuit | ✅ OUI |
| **Obfuscation** | ⭐⭐⭐ | Faible | 0-50€ | ⚠️ Partiel |
| **Chiffrement Manuel** | ⭐⭐ | Moyenne | Gratuit | ⚠️ Partiel |
| **Remote Config** | ⭐⭐⭐⭐ | Faible | Gratuit | ✅ Oui (config) |
| **Certificate Pinning** | ⭐⭐⭐⭐⭐ | Élevée | Gratuit | ✅ Oui (HTTPS) |

---

## 🎯 Recommandations pour NexA

### Architecture Finale Recommandée

```
Unity Client (PC)
    ↓ HTTPS (Certificate Pinning)
Backend API (Go/Java)
    ↓ VPN/Firewall
PostgreSQL Database (Serveur privé)
    ↑
    └─ Credentials en variables d'environnement
```

### Configuration Unity

```csharp
// Assets/Scripts/Core/ApiClient.cs
public class ApiClient : MonoBehaviour
{
    // ✅ Seulement l'URL publique (pas de secrets)
    #if UNITY_EDITOR
        private const string API_URL = "http://localhost:8080";
    #else
        private const string API_URL = "https://api.nexa.com";
    #endif
    
    private string accessToken;
    
    // Authentification
    public async Task<bool> Login(string email, string password)
    {
        var request = new LoginRequest { email = email, password = password };
        var response = await PostAsync<LoginResponse>("/api/v1/auth/login", request);
        
        if (response != null)
        {
            accessToken = response.access_token;
            PlayerPrefs.SetString("refresh_token", response.refresh_token);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
    
    // Récupérer le profil
    public async Task<UserProfile> GetProfile()
    {
        return await GetAsync<UserProfile>("/api/v1/auth/profile");
    }
    
    // Méthode générique POST
    private async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonUtility.ToJson(data);
        using (var request = UnityWebRequest.PostWwwForm(API_URL + endpoint, json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            }
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<T>(request.downloadHandler.text);
            }
            return default(T);
        }
    }
    
    // Méthode générique GET
    private async Task<T> GetAsync<T>(string endpoint)
    {
        using (var request = UnityWebRequest.Get(API_URL + endpoint))
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            }
            
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonUtility.FromJson<T>(request.downloadHandler.text);
            }
            return default(T);
        }
    }
}
```

---

## 🔒 Sécurité Additionnelle

### 1. Build Stripping (Unity)
```
File → Build Settings → Player Settings → Other Settings
☑ Strip Engine Code
☑ Managed Stripping Level: High
☑ IL2CPP Code Generation (au lieu de Mono)
```

**IL2CPP vs Mono:**
- **Mono**: C# → DLL → Facile à décompiler avec dnSpy
- **IL2CPP**: C# → C++ → Binaire natif → Beaucoup plus difficile

### 2. Code Protection dans Build Settings
```
Player Settings → Publishing Settings
☑ Split Application Binary (Android)
☑ Use App Bundle (Android)
```

### 3. Détection de Débogage
```csharp
public class AntiDebug : MonoBehaviour
{
    void Update()
    {
        // Détecter un débogueur attaché
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Debug.LogError("Debugger detected! Exiting...");
            Application.Quit();
        }
        
        // Détecter dnSpy/ILSpy (vérifie les processus)
        if (IsDebuggerRunning())
        {
            Application.Quit();
        }
    }
    
    private bool IsDebuggerRunning()
    {
        var processes = new[] { "dnSpy", "ILSpy", "dotPeek", "Reflexil" };
        foreach (var proc in System.Diagnostics.Process.GetProcesses())
        {
            foreach (var debugger in processes)
            {
                if (proc.ProcessName.Contains(debugger))
                    return true;
            }
        }
        return false;
    }
}
```

---

## 📝 Checklist Sécurité Unity

- [ ] ✅ Utiliser architecture client-serveur (Unity → Backend → DB)
- [ ] ✅ JAMAIS de credentials DB dans Unity
- [ ] ✅ HTTPS uniquement (TLS 1.3)
- [ ] ✅ Certificate Pinning pour empêcher MITM
- [ ] ✅ JWT pour authentification (pas de session cookies)
- [ ] ✅ Build avec IL2CPP (pas Mono)
- [ ] ✅ Obfuscation du code (Beebyte ou similaire)
- [ ] ✅ Remote Config pour les paramètres sensibles
- [ ] ✅ Anti-debugging dans les builds
- [ ] ✅ Validation côté serveur (JAMAIS faire confiance au client)
- [ ] ✅ Rate limiting sur le backend
- [ ] ✅ Logs côté serveur pour détecter les abus

---

## 🆘 Que Faire si les Credentials sont Compromis ?

1. **Changer immédiatement** les mots de passe DB
2. **Révoquer** tous les tokens JWT actifs
3. **Publier** un patch urgent
4. **Notifier** les utilisateurs
5. **Analyser** les logs pour détecter les abus
6. **Implémenter** certificate pinning
7. **Ajouter** 2FA pour les comptes critiques

---

## 🔗 Ressources

- **Unity Security Best Practices**: https://docs.unity3d.com/Manual/security-best-practices.html
- **OWASP Mobile Top 10**: https://owasp.org/www-project-mobile-top-10/
- **Beebyte Obfuscator**: https://assetstore.unity.com/packages/tools/utilities/obfuscator-48919
- **dnSpy (pour tester)**: https://github.com/dnSpy/dnSpy
- **Certificate Pinning Guide**: https://owasp.org/www-community/controls/Certificate_and_Public_Key_Pinning

---

## ✨ Résumé

**Règle d'or:** Unity ne doit JAMAIS se connecter directement à la base de données.

```
✅ Unity → Backend API → Database
❌ Unity → Database (credentials exposés!)
```

Avec l'architecture client-serveur que nous avons créée (backend Go), vous êtes déjà protégé ! 🛡️
