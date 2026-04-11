# Singleton Pattern - Documentation

## 📋 Vue d'ensemble

Le script `Singleton<T>` fournit un pattern Singleton générique et thread-safe pour Unity, éliminant la duplication de code dans vos managers.

## ✅ Caractéristiques

- **Thread-safe** : Utilise un lock pour éviter les race conditions
- **DontDestroyOnLoad** : Persiste entre les scènes
- **Protection contre les duplicatas** : Détruit automatiquement les instances supplémentaires
- **Auto-création** : Crée automatiquement l'instance si elle n'existe pas
- **Cleanup automatique** : Gère la destruction propre lors du quit

## 🚀 Utilisation

### Créer un nouveau Singleton

```csharp
using NexA.Hub.Core;
using UnityEngine;

namespace NexA.Hub.Services
{
    public class MyManager : Singleton<MyManager>
    {
        [Header("Configuration")]
        [SerializeField] private float someValue = 10f;

        // Optionnel : Override Awake si vous avez besoin d'initialisation
        protected override void Awake()
        {
            base.Awake(); // ⚠️ IMPORTANT : Toujours appeler base.Awake()
            
            // Votre initialisation ici
            Debug.Log("MyManager initialized!");
        }

        public void DoSomething()
        {
            Debug.Log($"Doing something with value: {someValue}");
        }
    }
}
```

### Accéder au Singleton depuis n'importe où

```csharp
// Accès à l'instance
MyManager.Instance.DoSomething();

// Vérifier si l'instance existe
if (MyManager.Instance != null)
{
    MyManager.Instance.DoSomething();
}
```

## 📦 Services qui utilisent Singleton<T>

Tous les services principaux utilisent maintenant le pattern Singleton centralisé :

### ✅ `APIService : Singleton<APIService>`
- Service centralisé pour les appels API REST
- Gestion des tokens, retry, timeout, correlation IDs

### ✅ `AuthManager : Singleton<AuthManager>`
- Gestion de l'authentification
- Login, register, refresh tokens, logout

### ✅ `CacheManager : Singleton<CacheManager>`
- Cache en mémoire pour éviter les appels API redondants
- TTL automatique, cleanup périodique

### ✅ `UIManager : Singleton<UIManager>`
- Navigation entre écrans avec state machine
- Transitions animées avec DOTween

## 🔧 Configuration Unity

### Option 1 : Auto-création (Recommandé)
Le singleton se crée automatiquement à la première utilisation :
```csharp
// Pas besoin de créer un GameObject manuellement
APIService.Instance.LoginAsync(...);
```

### Option 2 : Création manuelle
Vous pouvez créer manuellement un GameObject dans la scène :
1. Créer un Empty GameObject : `APIService`
2. Ajouter le script `APIService.cs`
3. Configurer les SerializeFields dans l'Inspector
4. Le singleton détectera et utilisera cette instance

## ⚠️ Points d'attention

### 1. Toujours appeler `base.Awake()`
Si vous override la méthode `Awake()`, n'oubliez pas d'appeler `base.Awake()` :
```csharp
protected override void Awake()
{
    base.Awake(); // ⚠️ NE PAS OUBLIER
    
    // Votre code ici
}
```

### 2. Ne pas accéder à Instance dans Awake()
L'instance n'est pas garantie d'être initialisée dans `Awake()` d'autres scripts. Utilisez `Start()` à la place :
```csharp
// ❌ MAUVAIS
void Awake()
{
    MyManager.Instance.DoSomething(); // Peut être null
}

// ✅ BON
void Start()
{
    MyManager.Instance.DoSomething(); // Instance garantie
}
```

### 3. Pas d'accès après OnApplicationQuit
Le singleton retourne `null` après le quit de l'application pour éviter les erreurs :
```csharp
protected override void OnApplicationQuit()
{
    base.OnApplicationQuit();
    
    // Ne pas utiliser d'autres singletons ici
}
```

## 🧪 Tests

### Tester qu'une seule instance existe
```csharp
[Test]
public void TestSingletonUniqueness()
{
    var obj1 = new GameObject("Manager1").AddComponent<MyManager>();
    var obj2 = new GameObject("Manager2").AddComponent<MyManager>();
    
    // obj2 devrait être détruit automatiquement
    Assert.IsTrue(obj1 != null);
    Assert.IsTrue(obj2 == null || obj2.gameObject == null);
    
    // Une seule instance
    Assert.AreEqual(obj1, MyManager.Instance);
}
```

## 🔍 Debugging

### Logs automatiques
Le Singleton affiche des logs pour le debugging :
```
[Singleton] Instance de 'APIService' créée automatiquement.
[Singleton] 'AuthManager' initialisé.
[Singleton] Instance duplicate de 'UIManager' détectée. Destruction de UIManager (1).
```

### Désactiver les logs
Commentez les `Debug.Log()` dans `Singleton.cs` si nécessaire.

## 📚 Références

- **Fichier** : `Assets/Hub/Script/Core/Singleton.cs`
- **Namespace** : `NexA.Hub.Core`
- **Pattern** : Singleton (Thread-safe avec Lazy Initialization)

## 💡 Bonnes pratiques

1. ✅ Utilisez Singleton pour les managers globaux (API, Auth, UI, Cache)
2. ✅ Évitez Singleton pour les objets de gameplay (ennemis, joueurs, etc.)
3. ✅ Préférez l'injection de dépendances pour les tests
4. ✅ Documentez les dépendances entre singletons
5. ✅ Évitez les références circulaires entre singletons

