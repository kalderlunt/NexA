# 🔧 Fix : Connexion HTTP Bloquée par Unity

## ❌ Erreur Actuelle

```
[Login] Unexpected error: System.InvalidOperationException: Insecure connection not allowed
at (wrapper managed-to-native) UnityEngine.Networking.UnityWebRequest.SendWebRequest()
```

**Cause** : Unity bloque les connexions HTTP non sécurisées (votre backend est en `http://192.168.1.19:8080`)

---

## ✅ Solution : Autoriser HTTP en Développement

### Étape 1 : Player Settings

1. **Unity** → `Edit` → `Project Settings`
2. **Player** (icône Unity)
3. Descendez jusqu'à **"Other Settings"**
4. Trouvez **"Allow downloads over HTTP"**
5. Changez de **"Not allowed"** à **"Allowed in Development builds"** ou **"Always allowed"**

![Player Settings](https://i.imgur.com/xxx.png)

### Options disponibles :

- **Not allowed** (défaut) ❌ - Bloque HTTP
- **Allowed in Development builds** ✅ - HTTP OK en Editor + Dev builds
- **Always allowed** ⚠️ - HTTP OK partout (pas recommandé en production)

---

## 🎯 Configuration Recommandée

```
Allow downloads over HTTP: Allowed in Development builds
```

**Pourquoi ?**
- ✅ HTTP fonctionne en **Unity Editor** (pour tester avec 192.168.1.19:8080)
- ✅ HTTP fonctionne en **Development Build** (pour tester sur mobile/tablette)
- ❌ HTTP bloqué en **Production Build** (force HTTPS)

---

## 🔄 Après Avoir Changé

1. **Sauvegardez** le projet (Ctrl+S)
2. **Relancez Play Mode** (▶️)
3. **Testez le login**

---

## ✅ Résultat Attendu

### Console Unity :

```
[AuthManager] Tentative de connexion pour kalderlunt@example.com
[API] POST http://192.168.1.19:8080/api/v1/auth/login | Correlation: xxx
[API] Response: {"user":{...},"tokens":{...}}
[AuthManager] Tokens stockés. Expire à: 2026-01-24 12:30:00
[AuthManager] Connexion réussie pour kalderlunt (ID: ae405c42...)
[Login] Success! Welcome kalderlunt
```

### UI :

```
✅ Toast VERT : "Bienvenue, kalderlunt !" 🎉
```

---

## 🔐 Sécurité en Production

### APIService.cs (déjà configuré) :

```csharp
[Header("Configuration")]
#if UNITY_EDITOR
    [SerializeField] private string baseURL = "http://192.168.1.19:8080/api/v1";  // HTTP en dev
#else
    [SerializeField] private string baseURL = "https://api.nexa.game/v1";  // HTTPS en prod
#endif
```

**En production** :
- Unity bloquera HTTP
- APIService utilisera `https://api.nexa.game/v1`
- Connexion sécurisée avec TLS 1.3

---

## 📋 Checklist

- [ ] Ouvrir Project Settings → Player
- [ ] Trouver "Allow downloads over HTTP"
- [ ] Changer à "Allowed in Development builds"
- [ ] Sauvegarder le projet
- [ ] Relancer Play Mode
- [ ] Tester le login

---

**🎯 C'était juste un paramètre Unity à changer !**

**Changez le paramètre et testez ! 🚀**
