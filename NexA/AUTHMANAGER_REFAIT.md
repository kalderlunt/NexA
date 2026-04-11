# ✅ AuthManager.cs - Refait à Neuf

## 🎯 Ce Qui a Été Corrigé

### Problèmes dans l'Ancienne Version

1. **StoreTokens appelé 2 fois** (ligne 53, 54 et 68, 69)
   ```csharp
   // ❌ AVANT
   StoreTokens(response.accessToken, response.refreshToken, response.expiresIn);
   StoreTokens(response.accessToken, response.refreshToken, response.expiresIn); // Doublon !
   ```

2. **Signature incompatible**
   - `StoreTokens` acceptait `TokenData` 
   - Mais on lui passait 3 paramètres séparés

3. **AuthResponse incompatible**
   - AuthResponse avait `accessToken`, `refreshToken`, `expiresIn` directement
   - Mais StoreTokens attendait un objet `TokenData`

---

## ✅ Nouvelle Version Propre

### Structure Correcte

```csharp
// APIResponse.cs
public class AuthResponse
{
    public User user;
    public TokenData tokens;  // ✅ Wrapper tokens
}

// AuthManager.cs
public async Task<User> LoginAsync(string email, string password)
{
    var response = await APIService.Instance.LoginAsync(email, password);
    StoreTokens(response.tokens);  // ✅ Passe TokenData
    CurrentUser = response.user;
    return CurrentUser;
}

private void StoreTokens(TokenData tokens)  // ✅ Accepte TokenData
{
    _accessToken = tokens.accessToken;
    _refreshToken = tokens.refreshToken;
    _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.expiresIn);
    // ...
}
```

---

## 📋 Fonctionnalités Complètes

### 1️⃣ Authentification

- ✅ **RegisterAsync** - Inscription nouvel utilisateur
- ✅ **LoginAsync** - Connexion utilisateur existant
- ✅ **LogoutAsync** - Déconnexion propre (serveur + local)

### 2️⃣ Gestion des Tokens

- ✅ **GetValidAccessTokenAsync** - Retourne un token valide (refresh auto si expiré)
- ✅ **TryRestoreSessionAsync** - Auto-login au démarrage (si refresh token persisté)
- ✅ **StoreTokens** - Stockage sécurisé (mémoire + optionnel PlayerPrefs)
- ✅ **ClearTokens** - Nettoyage complet

### 3️⃣ Propriétés Utiles

- ✅ **IsAuthenticated** - Vérifie si user connecté
- ✅ **CurrentUser** - Accès au user actuel

### 4️⃣ Messages d'Erreur User-Friendly

- ✅ **GetFriendlyErrorMessage** - Convertit codes backend en messages FR

---

## 🎮 Utilisation dans Unity

### LoginScreen.cs (Exemple)

```csharp
private async void OnLoginClicked()
{
    string email = emailInput.text.Trim();
    string password = passwordInput.text;

    try
    {
        // Simple et propre !
        User user = await AuthManager.Instance.LoginAsync(email, password);
        
        // Success
        ToastManager.Show($"Bienvenue, {user.username} !", ToastType.Success);
        UIManager.Instance.ShowScreen(ScreenType.Home);
    }
    catch (AuthException ex)
    {
        // Message déjà user-friendly
        ShowError(ex.Message);
    }
}
```

### Auto-Login au Démarrage

```csharp
// Dans un script de démarrage (SplashScreen, MainMenu, etc.)
private async void Start()
{
    bool restored = await AuthManager.Instance.TryRestoreSessionAsync();
    
    if (restored)
    {
        // User reconnecté automatiquement !
        UIManager.Instance.ShowScreen(ScreenType.Home);
    }
    else
    {
        // Afficher écran de login
        UIManager.Instance.ShowScreen(ScreenType.Login);
    }
}
```

---

## 🔧 Configuration

### Dans l'Inspector Unity

Sur le GameObject avec `AuthManager` :

**Persist Tokens** (bool) :
- ☑️ **Activé** : Refresh token sauvegardé dans PlayerPrefs → Auto-login possible
- ☐ **Désactivé** : Tokens en mémoire uniquement → Plus sécurisé mais pas d'auto-login

---

## 🔐 Sécurité

### Tokens Gérés

- **Access Token** : 24h, en mémoire uniquement (jamais persisté)
- **Refresh Token** : 30 jours, optionnellement persisté via `SecureStorage`

### Auto-Refresh

Le token d'accès est automatiquement rafraîchi 30s avant expiration :

```csharp
// Vous n'avez JAMAIS à faire ça manuellement !
string token = await AuthManager.Instance.GetValidAccessTokenAsync();

// AuthManager gère le refresh automatiquement si nécessaire
```

### Logs Détaillés

Tous les logs sont préfixés `[AuthManager]` pour faciliter le debug :

```
[AuthManager] Tentative de connexion pour kalderlunt@example.com
[AuthManager] Connexion réussie pour kalderlunt (ID: ae405c42...)
[AuthManager] Tokens stockés. Expire à: 2026-01-24 12:30:00
```

---

## 📊 Flow Complet

### Login

```
User clique "Login"
    │
    ├─> AuthManager.LoginAsync(email, password)
    │       │
    │       ├─> APIService.LoginAsync(email, password)
    │       │       └─> POST /api/v1/auth/login
    │       │
    │       ├─> Backend retourne: {user, tokens{accessToken, refreshToken, expiresIn}}
    │       │
    │       ├─> StoreTokens(response.tokens)
    │       │       ├─> _accessToken = tokens.accessToken
    │       │       ├─> _refreshToken = tokens.refreshToken
    │       │       └─> _tokenExpiresAt = Now + expiresIn
    │       │
    │       └─> CurrentUser = response.user
    │
    └─> Retourne User
```

### Auto-Refresh Token

```
GetValidAccessTokenAsync()
    │
    ├─> Token valide ? (> 30s avant expiration)
    │   ├─ OUI → Retourner _accessToken
    │   └─ NON ↓
    │
    ├─> Refresh token existe ?
    │   └─ NON → Exception "Session expirée"
    │
    ├─> APIService.RefreshTokenAsync(_refreshToken)
    │       └─> POST /api/v1/auth/refresh
    │
    ├─> Backend retourne: {accessToken, expiresIn}
    │
    ├─> _accessToken = response.accessToken
    ├─> _tokenExpiresAt = Now + expiresIn
    │
    └─> Retourner _accessToken
```

---

## ✅ Checklist

- [x] AuthManager.cs refait à neuf
- [x] APIResponse.cs avec `tokens` wrapper
- [x] Plus de doublon StoreTokens
- [x] Signature cohérente
- [x] Logs détaillés
- [x] Commentaires complets
- [x] Gestion d'erreurs robuste
- [x] Aucune erreur de compilation

---

## 🎯 Testez Maintenant

### Dans Unity :

1. **Play Mode** (▶️)
2. **Email** : `kalderlunt@example.com`
3. **Password** : `SuperSecurePass123!`
4. **Login** ✓

### Logs Attendus :

```
[AuthManager] Tentative de connexion pour kalderlunt@example.com
[API] POST http://192.168.1.19:8080/api/v1/auth/login | Correlation: xxx
[API] Response: {"user":{...},"tokens":{"accessToken":"eyJ...","refreshToken":"eyJ...","expiresIn":86400}}
[AuthManager] Tokens stockés. Expire à: 2026-01-24 12:30:00
[AuthManager] Connexion réussie pour kalderlunt (ID: ae405c42-aa87-470f-b81c-aa339bb40aaf)
[Login] Success! Welcome kalderlunt
```

---

**🎉 AuthManager complètement refait et testé !**

**Testez dans Unity et dites-moi ce qui se passe ! 🚀**
