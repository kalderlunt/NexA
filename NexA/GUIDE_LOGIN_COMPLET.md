# 🎮 NexA - Guide Complet Login Sécurisé avec Auto-Login (30 jours)

## 🎯 Objectif

Implémenter un système de login sécurisé entre Unity et le backend, avec une option "Se souvenir de moi" qui permet de reconnecter automatiquement l'utilisateur pendant 30 jours.

---

## 📋 Plan d'Action

1. **Backend** : Gérer les refresh tokens avec expiration 30 jours
2. **Unity** : Stocker le refresh token de manière sécurisée
3. **Unity** : Auto-login au démarrage si token valide
4. **Test** : Vérifier que tout fonctionne

---

## PARTIE 1: BACKEND (Déjà Prêt ✅)

### Votre Backend Est Déjà Configuré !

Le backend Java/Gradle que vous avez supporte déjà :
- ✅ JWT avec expiration 24h (access token)
- ✅ Refresh token avec expiration 7 jours
- ✅ Endpoint `/api/v1/auth/refresh` pour renouveler

### Modification Nécessaire: Étendre Refresh Token à 30 Jours

**Fichier:** `backend-java/src/main/resources/application.properties`

```properties
# JWT Configuration
jwt.secret=super_secret_key_change_this_in_production_min_32_characters_long
jwt.expiration=86400000
jwt.refresh-expiration=2592000000

# 86400000 = 24 heures (access token)
# 2592000000 = 30 jours (refresh token) ← MODIFIÉ
```

**C'est tout pour le backend ! ✅**

---

## PARTIE 2: UNITY - CLIENT API SÉCURISÉ

### Fichier 1: PlayerPrefsEncryption.cs

Créer un système de chiffrement pour PlayerPrefs (sécurité supplémentaire).

**Emplacement:** `Assets/Scripts/Core/PlayerPrefsEncryption.cs`

```csharp
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NexA.Core
{
    /// <summary>
    /// Chiffrement des données sensibles dans PlayerPrefs
    /// </summary>
    public static class PlayerPrefsEncryption
    {
        // Clé de chiffrement (changez-la pour votre jeu)
        private static readonly byte[] KEY = Encoding.UTF8.GetBytes("NexA2026SecureGameKeyChange32Char");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("NexA2026IV123456");

        /// <summary>
        /// Sauvegarder une valeur chiffrée
        /// </summary>
        public static void SetEncryptedString(string key, string value)
        {
            string encrypted = Encrypt(value);
            PlayerPrefs.SetString(key, encrypted);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Récupérer une valeur chiffrée
        /// </summary>
        public static string GetEncryptedString(string key, string defaultValue = "")
        {
            string encrypted = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(encrypted))
                return defaultValue;

            try
            {
                return Decrypt(encrypted);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Supprimer une clé
        /// </summary>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        private static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = KEY;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
        }

        private static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = KEY;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
```

### Fichier 2: ApiClient.cs (Mis à Jour avec Auto-Login)

**Emplacement:** `Assets/Scripts/Core/ApiClient.cs`

Modifiez votre `ApiClient.cs` existant pour ajouter :

```csharp
// AJOUTER ces constantes au début de la classe ApiClient
private const string PREF_REFRESH_TOKEN = "nexa_refresh_token";
private const string PREF_REMEMBER_ME = "nexa_remember_me";
private const string PREF_TOKEN_EXPIRY = "nexa_token_expiry";

// AJOUTER cette méthode pour sauvegarder le refresh token
private void SaveRefreshToken(string refreshToken, bool rememberMe)
{
    if (rememberMe)
    {
        // Sauvegarder de manière chiffrée
        PlayerPrefsEncryption.SetEncryptedString(PREF_REFRESH_TOKEN, refreshToken);
        PlayerPrefs.SetInt(PREF_REMEMBER_ME, 1);
        PlayerPrefs.SetString(PREF_TOKEN_EXPIRY, DateTime.Now.AddDays(30).ToString("o"));
        PlayerPrefs.Save();
        
        Debug.Log("✅ Refresh token sauvegardé (30 jours)");
    }
    else
    {
        // Ne pas sauvegarder
        ClearSavedCredentials();
    }
}

// AJOUTER cette méthode pour supprimer les credentials
public void ClearSavedCredentials()
{
    PlayerPrefsEncryption.DeleteKey(PREF_REFRESH_TOKEN);
    PlayerPrefs.DeleteKey(PREF_REMEMBER_ME);
    PlayerPrefs.DeleteKey(PREF_TOKEN_EXPIRY);
    Debug.Log("🗑️ Credentials supprimés");
}

// AJOUTER cette méthode pour vérifier si on peut auto-login
public bool CanAutoLogin()
{
    // Vérifier si "remember me" est activé
    if (PlayerPrefs.GetInt(PREF_REMEMBER_ME, 0) == 0)
        return false;

    // Vérifier si le token n'est pas expiré
    string expiryStr = PlayerPrefs.GetString(PREF_TOKEN_EXPIRY, "");
    if (string.IsNullOrEmpty(expiryStr))
        return false;

    try
    {
        DateTime expiry = DateTime.Parse(expiryStr);
        if (DateTime.Now > expiry)
        {
            Debug.Log("⏰ Token expiré, suppression");
            ClearSavedCredentials();
            return false;
        }
    }
    catch
    {
        ClearSavedCredentials();
        return false;
    }

    // Vérifier qu'on a un refresh token
    string refreshToken = PlayerPrefsEncryption.GetEncryptedString(PREF_REFRESH_TOKEN, "");
    return !string.IsNullOrEmpty(refreshToken);
}

// AJOUTER cette méthode pour auto-login
public async Task<bool> TryAutoLogin()
{
    if (!CanAutoLogin())
    {
        Debug.Log("❌ Auto-login impossible");
        return false;
    }

    Debug.Log("🔄 Tentative d'auto-login...");

    string refreshToken = PlayerPrefsEncryption.GetEncryptedString(PREF_REFRESH_TOKEN, "");
    
    try
    {
        // Utiliser le refresh token pour obtenir un nouveau access token
        var request = new RefreshTokenRequest { refreshToken = refreshToken };
        var response = await PostAsync<RefreshTokenResponse>("/auth/refresh", request, false);

        if (response != null)
        {
            _accessToken = response.accessToken;
            _tokenExpiration = DateTime.Now.AddHours(24);
            
            Debug.Log("✅ Auto-login réussi!");
            return true;
        }
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"⚠️ Auto-login échoué: {ex.Message}");
        ClearSavedCredentials();
    }

    return false;
}

// MODIFIER la méthode Login pour supporter "remember me"
public async Task<LoginResult> Login(string email, string password, bool rememberMe = false)
{
    var request = new LoginRequest
    {
        email = email,
        password = password
    };

    var response = await PostAsync<LoginResponse>("/auth/login", request, false);

    if (response != null)
    {
        _accessToken = response.accessToken;
        _refreshToken = response.refreshToken;
        _tokenExpiration = DateTime.Now.AddSeconds(response.expiresIn);

        // Sauvegarder le refresh token si "remember me" est coché
        SaveRefreshToken(response.refreshToken, rememberMe);

        return new LoginResult
        {
            success = true,
            user = response.user,
            accessToken = _accessToken
        };
    }

    return new LoginResult { success = false, error = "Login failed" };
}

// AJOUTER au Logout
public void Logout()
{
    _accessToken = null;
    _refreshToken = null;
    _tokenExpiration = DateTime.MinValue;

    // Supprimer les credentials sauvegardés
    ClearSavedCredentials();
}
```

### Fichier 3: LoginScreen.cs (Avec Checkbox Remember Me)

**Emplacement:** `Assets/Scripts/UI/LoginScreen.cs`

Modifiez votre LoginScreen pour ajouter la checkbox :

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NexA.Core;

public class LoginScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle rememberMeToggle; // ← NOUVEAU
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private async void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);

        // Tenter l'auto-login au démarrage
        await TryAutoLogin();
    }

    /// <summary>
    /// Tentative d'auto-login au démarrage
    /// </summary>
    private async Task TryAutoLogin()
    {
        if (ApiClient.Instance.CanAutoLogin())
        {
            ShowStatus("Reconnexion automatique...", Color.yellow);

            bool success = await ApiClient.Instance.TryAutoLogin();

            if (success)
            {
                ShowStatus("✅ Reconnecté automatiquement!", Color.green);
                
                // Charger le profil utilisateur
                var profile = await ApiClient.Instance.GetProfile();
                if (profile != null)
                {
                    UserManager.Instance.SetCurrentUser(profile);
                    GoToMainMenu();
                }
            }
            else
            {
                ShowStatus("Veuillez vous connecter", Color.white);
            }
        }
    }

    /// <summary>
    /// Login avec support "Remember Me"
    /// </summary>
    private async void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        bool rememberMe = rememberMeToggle.isOn; // ← NOUVEAU

        // Validations
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Veuillez remplir tous les champs", Color.red);
            return;
        }

        loginButton.interactable = false;
        ShowStatus("Connexion en cours...", Color.yellow);

        try
        {
            // Login avec l'option "remember me"
            var result = await ApiClient.Instance.Login(email, password, rememberMe);

            if (result.success)
            {
                ShowStatus($"Bienvenue {result.user.username}!", Color.green);
                
                // Charger le profil
                var profile = await ApiClient.Instance.GetProfile();
                if (profile != null)
                {
                    UserManager.Instance.SetCurrentUser(profile);
                    GoToMainMenu();
                }
            }
            else
            {
                ShowStatus($"Erreur: {result.error}", Color.red);
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Erreur: {ex.Message}", Color.red);
        }
        finally
        {
            loginButton.interactable = true;
        }
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"[LoginScreen] {message}");
    }

    private void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
```

---

## PARTIE 3: UNITY UI - ÉCRAN DE LOGIN

### Créer l'Interface de Login

1. **Créer une nouvelle scène:** `Scenes/LoginScene`

2. **Hiérarchie UI:**
```
Canvas
├── Panel_Login (Background)
│   ├── Text_Title ("NexA - Connexion")
│   ├── InputField_Email
│   ├── InputField_Password
│   ├── Toggle_RememberMe ("Se souvenir de moi (30 jours)")  ← NOUVEAU
│   ├── Button_Login ("Se connecter")
│   ├── Button_Register ("Créer un compte")
│   └── Text_Status (messages d'erreur/succès)
└── Loading_Panel (optionnel)
```

3. **Configurer le Toggle Remember Me:**
   - Ajouter un GameObject `Toggle_RememberMe`
   - Component: Toggle
   - Ajouter un Label: "Se souvenir de moi pendant 30 jours"

4. **Assigner le Script:**
   - Sélectionner `Panel_Login`
   - Ajouter Component → `LoginScreen`
   - Glisser les références UI dans l'inspecteur

---

## PARTIE 4: TESTER LE SYSTÈME

### Test 1: Login Normal (Sans Remember Me)

```csharp
// Dans Unity, lancer LoginScene
// Ne pas cocher "Remember Me"
// Se connecter avec: test@example.com / password123
// Fermer et relancer → Doit redemander login
```

### Test 2: Login avec Remember Me

```csharp
// Cocher "Remember Me"
// Se connecter avec: test@example.com / password123
// Fermer Unity
// Relancer → Doit se reconnecter automatiquement ! ✅
```

### Test 3: Expiration du Token

```csharp
// Pour tester l'expiration (sans attendre 30 jours):
// PlayerPrefs.SetString("nexa_token_expiry", DateTime.Now.AddSeconds(-1).ToString("o"));
// Relancer → Doit redemander login
```

### Test 4: Logout Manuel

```csharp
// Se connecter avec Remember Me
// Cliquer sur "Logout"
// Relancer → Doit redemander login (credentials supprimés)
```

---

## PARTIE 5: SÉCURITÉ

### Ce Qui Protège Votre Système

#### 1. Chiffrement PlayerPrefs
```csharp
✅ Refresh token chiffré avec AES-256
✅ Impossible de lire en clair dans le registre Windows
✅ Clé de chiffrement unique à votre jeu
```

#### 2. Expiration Automatique
```csharp
✅ Token expire après 30 jours
✅ Vérification à chaque démarrage
✅ Suppression auto si expiré
```

#### 3. JWT Signature
```csharp
✅ Refresh token signé par le serveur
✅ Impossible de modifier sans la clé secrète
✅ Backend vérifie la signature
```

#### 4. HTTPS en Production
```csharp
✅ Trafic réseau chiffré
✅ Impossible de sniffer le refresh token
✅ Certificate pinning possible (avancé)
```

### Risques Résiduels

#### ⚠️ Si l'ordinateur de l'utilisateur est compromis
```
Un malware peut:
- Lire la mémoire de Unity (token en clair)
- Déchiffrer PlayerPrefs (clé dans le code)
- Voler le refresh token

Solution: C'est acceptable pour un jeu
(Même Steam, Epic Games fonctionnent ainsi)
```

#### ⚠️ Décompilation Unity
```
Un hacker peut:
- Décompiler le .exe avec dnSpy
- Voir la clé AES de chiffrement

Solution: Obfuscation + IL2CPP (rend très difficile)
```

---

## PARTIE 6: AMÉLIORATIONS FUTURES (Optionnel)

### Option 1: Device ID Binding

Lier le refresh token à l'appareil:

```csharp
// Backend: Stocker device_id avec le refresh token
// Si device_id change → Invalider le token
string deviceId = SystemInfo.deviceUniqueIdentifier;
```

### Option 2: IP Tracking

```csharp
// Backend: Détecter changement d'IP suspect
// Si IP change de pays → Demander re-login
```

### Option 3: Token Rotation

```csharp
// Backend: Générer un nouveau refresh token à chaque utilisation
// Invalider l'ancien
// = Plus sécurisé (détecte le vol de token)
```

### Option 4: 2FA (Two-Factor Authentication)

```csharp
// Pour les comptes sensibles
// Email/SMS avec code à 6 chiffres
```

---

## 🚀 CHECKLIST FINALE

### Backend
- [ ] Modifier `jwt.refresh-expiration=2592000000` (30 jours)
- [ ] Lancer le backend: `.\gradlew bootRun`
- [ ] Tester endpoint: `curl http://localhost:8080/health`

### Unity
- [ ] Créer `PlayerPrefsEncryption.cs`
- [ ] Modifier `ApiClient.cs` (ajouter auto-login)
- [ ] Modifier `LoginScreen.cs` (ajouter checkbox)
- [ ] Créer la scène `LoginScene`
- [ ] Ajouter le Toggle "Remember Me" dans l'UI
- [ ] Assigner les références dans l'inspecteur

### Tests
- [ ] Login normal (sans remember me)
- [ ] Login avec remember me
- [ ] Fermer/Relancer Unity → Auto-login fonctionne
- [ ] Logout → Credentials supprimés
- [ ] Expiration → Token expiré redirige au login

---

## 📝 COMMANDES RAPIDES

### Lancer le Backend
```powershell
cd E:\.Dev\.ProjetsPerso\NexA\NexA\backend-java
.\gradlew bootRun
```

### Tester l'API
```powershell
# Login
$login = @{ email = "test@example.com"; password = "password123" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:8080/api/v1/auth/login" `
    -Method POST -ContentType "application/json" -Body $login

# Afficher tokens
Write-Host "Access Token: $($response.accessToken.Substring(0,50))..."
Write-Host "Refresh Token: $($response.refreshToken.Substring(0,50))..."
```

### Debugger PlayerPrefs (Windows)
```powershell
# Voir les PlayerPrefs
regedit
# HKEY_CURRENT_USER\Software\[CompanyName]\[ProductName]
```

---

## 🎯 RÉSULTAT FINAL

**Vous aurez:**
- ✅ Login sécurisé avec JWT
- ✅ Option "Remember Me" 30 jours
- ✅ Auto-login au démarrage si coché
- ✅ Refresh token chiffré dans PlayerPrefs
- ✅ Expiration automatique après 30 jours
- ✅ Protection contre le sniffing réseau (HTTPS)
- ✅ Logout qui supprime les credentials

**Comme Steam, Epic Games, Battle.net !** 🎮

---

**Temps estimé:** 2-3 heures pour tout implémenter et tester

**Difficulté:** Moyenne (copier-coller + configuration UI)

Besoin d'aide sur une étape spécifique ? 😊
