# Architecture Singleton - Vue d'ensemble

## 🏗️ Structure des Singletons

```
┌─────────────────────────────────────────────────────────┐
│          Singleton<T> (Abstract Base Class)              │
│  ┌───────────────────────────────────────────────────┐  │
│  │ • Thread-safe Instance property                   │  │
│  │ • DontDestroyOnLoad comportement                  │  │
│  │ • Protection contre duplicatas                    │  │
│  │ • Auto-création lazy                              │  │
│  │ • Cleanup OnApplicationQuit                       │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │ Hérite
          ┌─────────────────┼─────────────────┐
          │                 │                  │
          │                 │                  │
    ┌─────▼──────┐   ┌─────▼──────┐   ┌──────▼─────┐
    │ APIService │   │ AuthManager│   │ UIManager  │
    │            │   │            │   │            │
    │ Instance ✓ │   │ Instance ✓ │   │ Instance ✓ │
    └────────────┘   └────────────┘   └────────────┘
          │                 │                  │
          ▼                 ▼                  ▼
    [HTTP Calls]      [Auth Logic]      [UI Navigation]
```

## 📦 Services utilisant Singleton<T>

### 🌐 APIService
```csharp
public class APIService : Singleton<APIService>
{
    // Gère tous les appels HTTP vers le backend
    // • Login/Register
    // • Friends management
    // • Match history
    // • Retry logic & timeouts
}

// Usage
await APIService.Instance.LoginAsync(email, password);
```

### 🔐 AuthManager
```csharp
public class AuthManager : Singleton<AuthManager>
{
    // Gère l'authentification
    // • Access/Refresh tokens
    // • CurrentUser state
    // • Auto-refresh tokens
}

// Usage
bool isAuth = AuthManager.Instance.IsAuthenticated;
User user = AuthManager.Instance.CurrentUser;
```

### 🎨 UIManager
```csharp
public class UIManager : Singleton<UIManager>
{
    // Gère la navigation UI
    // • Screen transitions
    // • State machine
    // • Fade animations
}

// Usage
UIManager.Instance.ShowScreen(ScreenType.Home);
```

### 💾 CacheManager
```csharp
public class CacheManager : Singleton<CacheManager>
{
    // Cache en mémoire
    // • TTL automatique
    // • Évite API calls redondants
}

// Usage
CacheManager.Instance.Set("friends", friendsList, ttl: 300);
```

## 🔄 Cycle de vie

```
┌──────────────────────────────────────────────────────┐
│                   APPLICATION START                   │
└────────────────────┬─────────────────────────────────┘
                     │
                     ▼
          ┌──────────────────────┐
          │  Première utilisation │
          │  Singleton.Instance   │
          └──────────┬────────────┘
                     │
                     ▼
          ┌──────────────────────┐
          │   FindObjectOfType    │
          │   (cherche existant)  │
          └──────────┬────────────┘
                     │
           ┌─────────▼─────────┐
           │ Existe ?          │
           └─────────┬─────────┘
                     │
        ┌────────────┼────────────┐
        │ OUI        │         NON│
        ▼            │            ▼
  ┌─────────┐       │      ┌────────────┐
  │ Retourne│       │      │ Créer new  │
  │ existant│       │      │ GameObject │
  └─────────┘       │      └──────┬─────┘
                    │             │
                    │      ┌──────▼─────┐
                    │      │ AddComponent│
                    │      └──────┬─────┘
                    │             │
                    └─────────────┘
                          │
                          ▼
                ┌──────────────────┐
                │ DontDestroyOnLoad│
                └─────────┬────────┘
                          │
                          ▼
                ┌──────────────────┐
                │  Instance prête  │
                └──────────────────┘
```

## ⚠️ Gestion des duplicatas

```
Scene A                        Scene B
┌──────────┐                  ┌──────────┐
│ Manager  │ ─── Load ────▶   │ Manager  │
│ (v1)     │    Scene B       │ (v2) NEW │
└──────────┘                  └────┬─────┘
     │                             │
     │ DontDestroyOnLoad           │ Awake()
     │                             │
     ▼                             ▼
┌──────────┐              ┌────────────────┐
│ Survit   │              │ if Instance != │
│          │              │    null        │
└──────────┘              └────────┬───────┘
                                   │
                                   ▼
                          ┌────────────────┐
                          │ Destroy(this)  │
                          └────────────────┘
                          
Résultat: Une seule instance (v1) survit
```

## 🎯 Best Practices

### ✅ À FAIRE
```csharp
// 1. Toujours appeler base.Awake()
protected override void Awake()
{
    base.Awake();
    // Votre code...
}

// 2. Vérifier null avant utilisation
if (MyManager.Instance != null)
{
    MyManager.Instance.DoSomething();
}

// 3. Utiliser dans Start(), pas dans Awake()
void Start()
{
    APIService.Instance.DoSomething(); // ✓ Safe
}
```

### ❌ À ÉVITER
```csharp
// 1. Ne pas appeler dans Awake d'autres scripts
void Awake()
{
    MyManager.Instance.DoSomething(); // ❌ Peut être null
}

// 2. Ne pas créer manuellement
void Start()
{
    new GameObject().AddComponent<MyManager>(); // ❌ Doublon
}

// 3. Ne pas override Awake sans base
protected override void Awake()
{
    // base.Awake(); // ❌ OUBLIÉ !
    DoSomething();
}
```

## 📊 Diagramme de dépendances

```
┌─────────────┐
│  UIManager  │
└──────┬──────┘
       │ uses
       ▼
┌─────────────┐     ┌──────────────┐
│ AuthManager │────▶│  APIService  │
└──────┬──────┘uses └──────────────┘
       │
       │ uses
       ▼
┌─────────────┐
│CacheManager │
└─────────────┘
```

**Règle importante** : Éviter les références circulaires !
- ✅ UIManager → AuthManager → APIService (OK)
- ❌ APIService → AuthManager → APIService (PAS OK)

## 🧪 Testing

```csharp
[Test]
public void TestSingletonInstance()
{
    // Arrange
    var instance1 = MyManager.Instance;
    var instance2 = MyManager.Instance;
    
    // Assert
    Assert.AreEqual(instance1, instance2);
    Assert.IsNotNull(instance1);
}

[Test]
public void TestNoDuplicates()
{
    // Arrange
    var obj1 = new GameObject().AddComponent<MyManager>();
    var obj2 = new GameObject().AddComponent<MyManager>();
    
    // Assert
    // obj2 devrait être détruit
    Assert.IsTrue(obj2 == null || !obj2.gameObject.activeSelf);
}
```

## 📝 Checklist d'implémentation

Quand vous créez un nouveau Singleton :

- [ ] Hériter de `Singleton<T>` au lieu de `MonoBehaviour`
- [ ] Supprimer l'ancien code singleton (`Instance { get; private set; }`)
- [ ] Override `Awake()` avec `base.Awake()` si nécessaire
- [ ] Tester qu'une seule instance existe
- [ ] Vérifier les dépendances circulaires
- [ ] Documenter l'utilisation
- [ ] Ajouter des exemples de code

## 🔗 Fichiers associés

- **Core** : `Assets/Hub/Script/Core/Singleton.cs`
- **Services** : 
  - `Assets/Hub/Script/Services/APIService.cs`
  - `Assets/Hub/Script/Services/AuthManager.cs`
  - `Assets/Hub/Script/Services/CacheManager.cs`
- **UI** : `Assets/Hub/Script/Core/UIManager.cs`
- **Examples** : `Assets/Hub/Script/Examples/ExampleManager.cs`
- **Docs** : `Diagrams/SINGLETON_PATTERN.md`

