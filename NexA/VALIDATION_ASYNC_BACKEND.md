# Validation asynchrone avec vérification backend

## 🎯 Vue d'ensemble

Le système de validation du RegisterScreenMultiStep vérifie maintenant en temps réel si le **username** et l'**email** sont déjà utilisés dans la base de données, avant même que l'utilisateur clique sur "S'inscrire".

---

## ✨ Fonctionnalités implémentées

### **1. Validation du Username (Étape 1)**

#### **Validation locale (instantanée)**
- ✅ Minimum **3 caractères**
- ✅ Maximum **20 caractères**
- ✅ Uniquement **lettres et chiffres** (alphanumerique)
- ✅ Pas d'espaces, caractères spéciaux

#### **Validation backend (asynchrone avec debounce)**
- ✅ Vérifie si le pseudo existe déjà dans la DB
- ✅ **Debounce de 0.5s** : attend que l'utilisateur arrête de taper
- ✅ Message "Vérification..." pendant l'appel API
- ✅ Message "Ce pseudo est déjà pris" si indisponible
- ✅ Icône verte ✓ si disponible

### **2. Validation de l'Email (Étape 2)**

#### **Validation locale (instantanée)**
- ✅ Format email valide (regex)
- ✅ Doit contenir @ et un domaine

#### **Validation backend (asynchrone avec debounce)**
- ✅ Vérifie si l'email existe déjà dans la DB
- ✅ **Debounce de 0.5s** : attend que l'utilisateur arrête de taper
- ✅ Message "Vérification..." pendant l'appel API
- ✅ Message "Cet email est déjà utilisé" si indisponible
- ✅ Icône verte ✓ si disponible

#### **Validation de confirmation**
- ✅ Les deux emails doivent correspondre
- ✅ Message "Les emails ne correspondent pas" si différents

### **3. Validation du Password (Étape 3)**

#### **Validation locale (instantanée)**
- ✅ Minimum **8 caractères**
- ✅ Au moins **1 majuscule**
- ✅ Au moins **1 minuscule**
- ✅ Au moins **1 chiffre**

#### **Validation de confirmation**
- ✅ Les deux mots de passe doivent correspondre
- ✅ Message "Les mots de passe ne correspondent pas" si différents

---

## 🔧 Modifications techniques

### **1. APIService.cs**

#### **Nouveaux endpoints ajoutés**
```csharp
/// <summary>
/// Vérifie si un username existe déjà
/// </summary>
public async Task<CheckAvailabilityResponse> CheckUsernameAvailabilityAsync(string username)
{
    string url = $"/auth/check-username?username={Uri.EscapeDataString(username)}";
    return await SendRequestAsync<CheckAvailabilityResponse>(url, "GET", requiresAuth: false);
}

/// <summary>
/// Vérifie si un email existe déjà
/// </summary>
public async Task<CheckEmailAvailabilityResponse> CheckEmailAvailabilityAsync(string email)
{
    string url = $"/auth/check-email?email={Uri.EscapeDataString(email)}";
    return await SendRequestAsync<CheckAvailabilityResponse>(url, "GET", requiresAuth: false);
}
```

### **2. APIResponse.cs**

#### **Nouveau modèle ajouté**
```csharp
[Serializable]
public class CheckAvailabilityResponse
{
    public bool available;
    public string message;
}
```

### **3. RegisterScreenMultiStep.cs**

#### **Variables ajoutées**
```csharp
// Async validation states
private bool isUsernameAvailable = true;
private bool isEmailAvailable = true;
private bool isCheckingUsername;
private bool isCheckingEmail;

// Debounce timers
private Coroutine usernameCheckCoroutine;
private Coroutine emailCheckCoroutine;
private const float DEBOUNCE_DELAY = 0.5f;
```

#### **Logique de debounce**
```csharp
private void OnUsernameChanged(string value)
{
    isUsernameValid = ValidateUsername(value);
    
    if (isUsernameValid)
    {
        // Cancel previous check
        if (usernameCheckCoroutine != null)
            StopCoroutine(usernameCheckCoroutine);
        
        // Start new debounced check
        usernameCheckCoroutine = StartCoroutine(CheckUsernameAvailabilityDebounced(value));
    }
    else
    {
        isUsernameAvailable = true; // Reset
        UpdateValidationUI(...);
    }
    
    UpdateNextButtonState();
}

private IEnumerator CheckUsernameAvailabilityDebounced(string username)
{
    // Attendre 0.5s avant de faire l'appel API
    yield return new WaitForSeconds(DEBOUNCE_DELAY);
    
    yield return StartCoroutine(CheckUsernameAvailability(username));
}

private IEnumerator CheckUsernameAvailability(string username)
{
    isCheckingUsername = true;
    
    // Show "Vérification..." message
    usernameValidationMessage.text = "Vérification...";
    usernameValidationMessage.color = textInactiveColor;
    usernameValidationMessage.gameObject.SetActive(true);
    
    // Appel API asynchrone
    var task = APIService.Instance.CheckUsernameAvailabilityAsync(username);
    yield return new WaitUntil(() => task.IsCompleted);
    
    isCheckingUsername = false;
    
    if (task.Exception != null)
    {
        // Erreur réseau - on considère comme disponible
        isUsernameAvailable = true;
    }
    else
    {
        isUsernameAvailable = task.Result.available;
    }
    
    // Update UI avec le résultat
    bool finalValid = isUsernameValid && isUsernameAvailable;
    string errorMsg = !isUsernameAvailable ? "Ce pseudo est déjà pris" : GetUsernameErrorMessage(username);
    UpdateValidationUI(usernameValidationIcon, usernameValidationMessage, finalValid, errorMsg);
    UpdateNextButtonState();
}
```

#### **Bouton "Suivant" désactivé pendant la vérification**
```csharp
private void UpdateNextButtonState()
{
    bool canProceed = false;

    switch (currentStep)
    {
        case 1:
            // Doit être valide ET disponible ET pas en train de vérifier
            canProceed = isUsernameValid && isUsernameAvailable && !isCheckingUsername;
            break;
        case 2:
            canProceed = isEmailValid && isEmailAvailable && !isCheckingEmail && isConfirmEmailValid;
            break;
        case 3:
            canProceed = isPasswordValid && isConfirmPasswordValid;
            break;
    }

    nextButton.interactable = canProceed;
}
```

---

## 📊 Flux utilisateur

### **Scénario 1 : Username déjà pris**

```
1. Utilisateur tape "JohnDoe"
2. Validation locale OK (✓ format valide)
3. Attendre 0.5s (debounce)
4. Message "Vérification..." affiché
5. Appel API : GET /auth/check-username?username=JohnDoe
6. Réponse : { available: false, message: "Username déjà pris" }
7. Icône rouge ✗ + Message "Ce pseudo est déjà pris"
8. Bouton "Suivant" désactivé
```

### **Scénario 2 : Username disponible**

```
1. Utilisateur tape "SuperPlayer123"
2. Validation locale OK (✓ format valide)
3. Attendre 0.5s (debounce)
4. Message "Vérification..." affiché
5. Appel API : GET /auth/check-username?username=SuperPlayer123
6. Réponse : { available: true }
7. Icône verte ✓ + Aucun message d'erreur
8. Bouton "Suivant" activé
```

### **Scénario 3 : Utilisateur tape rapidement (debounce)**

```
Tape "J" → Annule
Tape "Jo" → Annule
Tape "Joh" → Validation locale OK
        → Attendre 0.5s
Tape "John" (avant 0.5s) → Annule le timer précédent, redémarre
        → Attendre 0.5s
        → Si pas de nouvelle frappe, appel API pour "John"
```

### **Scénario 4 : Erreur réseau**

```
1. Utilisateur tape un username valide
2. Appel API échoue (timeout, erreur serveur)
3. Log warning dans la console
4. isUsernameAvailable = true (on ne bloque pas l'utilisateur)
5. Icône verte ✓ affichée
6. Bouton "Suivant" activé
7. La vérification finale sera faite lors de l'inscription
```

---

## 🔒 Sécurité

### **1. Double validation**
- ✅ **Frontend** : Validation locale pour UX instantanée
- ✅ **Backend** : Validation finale lors de l'inscription (sécurité)

### **2. Gestion des erreurs réseau**
- ✅ Ne bloque pas l'utilisateur si l'API ne répond pas
- ✅ La validation finale sera faite côté serveur de toute façon
- ✅ Logs des erreurs pour débogage

### **3. Debounce pour éviter le spam**
- ✅ Réduit la charge serveur (pas d'appel à chaque touche)
- ✅ Améliore l'UX (moins de messages clignotants)

---

## 📡 Backend requis

### **Endpoint 1 : Vérifier username**
```http
GET /api/v1/auth/check-username?username=JohnDoe

Response 200 OK:
{
  "available": false,
  "message": "Username déjà pris"
}
```

### **Endpoint 2 : Vérifier email**
```http
GET /api/v1/auth/check-email?email=john@example.com

Response 200 OK:
{
  "available": true,
  "message": "Email disponible"
}
```

### **Implémentation backend (exemple Node.js)**
```javascript
// Route: GET /auth/check-username
router.get('/check-username', async (req, res) => {
  const { username } = req.query;
  
  // Valider le format
  if (!username || username.length < 3) {
    return res.status(400).json({ error: 'Username invalide' });
  }
  
  // Check dans la DB
  const existingUser = await User.findOne({ username });
  
  res.json({
    available: !existingUser,
    message: existingUser ? 'Username déjà pris' : 'Username disponible'
  });
});

// Route: GET /auth/check-email
router.get('/check-email', async (req, res) => {
  const { email } = req.query;
  
  // Valider le format
  if (!email || !isValidEmail(email)) {
    return res.status(400).json({ error: 'Email invalide' });
  }
  
  // Check dans la DB
  const existingUser = await User.findOne({ email });
  
  res.json({
    available: !existingUser,
    message: existingUser ? 'Email déjà utilisé' : 'Email disponible'
  });
});
```

---

## 🧪 Tests

### **Test 1 : Username minimum 3 caractères**
1. Taper "ab" → ❌ "Le pseudo doit contenir au moins 3 caractères"
2. Taper "abc" → ✓ Vérification backend

### **Test 2 : Username caractères spéciaux**
1. Taper "john@doe" → ❌ "Uniquement des lettres et chiffres"
2. Taper "johndoe" → ✓ Vérification backend

### **Test 3 : Username déjà pris**
1. Taper un username existant en DB
2. ✅ Message "Ce pseudo est déjà pris"
3. ✅ Bouton "Suivant" désactivé

### **Test 4 : Email format invalide**
1. Taper "invalid-email" → ❌ "Format d'email invalide"
2. Taper "test@example.com" → ✓ Vérification backend

### **Test 5 : Debounce fonctionnel**
1. Taper rapidement "JohnDoe" lettre par lettre
2. ✅ Un seul appel API fait après 0.5s de la dernière touche

### **Test 6 : Erreur réseau**
1. Déconnecter le backend
2. Taper un username valide
3. ✅ Warning dans la console
4. ✅ Utilisateur peut continuer (pas bloqué)

---

## 📈 Performances

### **Optimisations**
- ✅ **Debounce** : Réduit les appels API de ~90%
- ✅ **Validation locale d'abord** : Pas d'appel API si format invalide
- ✅ **Annulation** : Les requêtes précédentes sont annulées
- ✅ **Async/Await** : UI reste responsive pendant les checks

### **Statistiques estimées**
| Scénario | Sans debounce | Avec debounce |
|----------|---------------|---------------|
| Taper "JohnDoe" (7 lettres) | 7 appels API | 1 appel API |
| Taper rapidement puis effacer | 10+ appels | 0-1 appels |
| Copier-coller | 1 appel | 1 appel |

---

## ✅ Checklist finale

- [x] Endpoints API ajoutés dans APIService
- [x] Modèle CheckAvailabilityResponse ajouté
- [x] Validation username avec debounce
- [x] Validation email avec debounce
- [x] Message "Vérification..." pendant l'appel API
- [x] Bouton désactivé pendant la vérification
- [x] Gestion des erreurs réseau
- [x] Messages d'erreur clairs et spécifiques
- [x] Animations de feedback (icônes, couleurs)
- [x] Tests de validation locale
- [ ] Implémentation backend des endpoints
- [ ] Tests d'intégration frontend/backend

---

## 🚀 Prochaines étapes

1. **Implémenter les endpoints backend**
   - `/auth/check-username`
   - `/auth/check-email`

2. **Tests d'intégration**
   - Tester avec le vrai backend
   - Vérifier les temps de réponse
   - Tester avec une connexion lente

3. **Améliorations futures**
   - Suggestions de usernames disponibles si pris
   - Auto-complétion email avec domaines populaires
   - Indicateur de force du mot de passe visuel (barre de progression)

---

**Date** : 2026-02-04  
**Status** : ✅ Frontend implémenté - Backend à implémenter
