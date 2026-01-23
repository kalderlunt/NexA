# 📊 Guide d'Utilisation - dbdiagram.io

Ce guide explique comment visualiser le schéma de base de données NexA sur dbdiagram.io.

---

## 🚀 Méthode 1: Import Direct (Recommandé)

### Étape 1: Ouvrir dbdiagram.io
Visitez: **https://dbdiagram.io/home**

### Étape 2: Créer un nouveau diagramme
- Cliquez sur **"Go to App"** (ou **"Start for Free"**)
- Cliquez sur **"New Diagram"**

### Étape 3: Copier le contenu
```powershell
# Copier le contenu du fichier dans le presse-papiers
Get-Content "E:\.Dev\.ProjetsPerso\NexA\NexA\DATABASE_DIAGRAM.dbml" | Set-Clipboard

# Ou ouvrir le fichier
notepad "E:\.Dev\.ProjetsPerso\NexA\NexA\DATABASE_DIAGRAM.dbml"
```

### Étape 4: Coller dans l'éditeur
- Sélectionnez tout le texte par défaut (Ctrl+A)
- Collez le contenu du fichier DBML (Ctrl+V)
- Le diagramme se génère automatiquement ! ✨

---

## 🎨 Méthode 2: Import de Fichier

### Étape 1: Sur dbdiagram.io
- Cliquez sur l'icône **☰** (menu hamburger) en haut à gauche
- Cliquez sur **"Import"**
- Sélectionnez **"DBML"**

### Étape 2: Charger le fichier
- Cliquez sur **"Choose File"**
- Sélectionnez `DATABASE_DIAGRAM.dbml`
- Cliquez sur **"Import"**

---

## 📸 Résultat Attendu

Vous verrez un diagramme avec:

### Tables Principales:
```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│   users     │────▶│  friendships     │◀────│   users     │
└─────────────┘     └──────────────────┘     └─────────────┘
       │                                               │
       │                                               │
       ▼                                               ▼
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│ user_stats  │     │match_participants│◀────│   matches   │
└─────────────┘     └──────────────────┘     └─────────────┘
```

### Légende des Relations:
- **─────▶** : Foreign Key (Many-to-One)
- **◀────** : Inverse relation (One-to-Many)
- **─────** : One-to-One

---

## 🎯 Fonctionnalités de dbdiagram.io

### 1. Zoom et Navigation
- **Molette de la souris** : Zoom in/out
- **Clic + Glisser** : Déplacer la vue
- **Clic sur table** : Voir les détails

### 2. Réorganiser les Tables
- **Glisser-déposer** les tables pour une meilleure disposition
- **Ctrl + Z** : Annuler
- **Ctrl + Y** : Refaire

### 3. Exporter le Diagramme

#### Export PNG/PDF
1. Cliquez sur **"Export"** (en haut à droite)
2. Choisissez le format:
   - **PNG** : Image haute résolution
   - **PDF** : Document imprimable
   - **SVG** : Vectoriel (éditable)

#### Export SQL
1. Cliquez sur **"Export"** → **"PostgreSQL"**
2. Téléchargez le script SQL généré
3. Comparez avec votre `DATABASE_SCHEMA.sql`

#### Export Code
Vous pouvez également exporter vers:
- **MySQL**
- **SQL Server**
- **Oracle**
- **Rails (Ruby on Rails)**
- **Django (Python)**
- **Laravel (PHP)**

### 4. Partager le Diagramme

#### Lien Public
1. Cliquez sur **"Share"**
2. Activez **"Public link"**
3. Copiez l'URL et partagez

#### Collaboration
1. Créez un compte (gratuit)
2. Invitez des collaborateurs
3. Éditez en temps réel

---

## 🎨 Personnalisation

### Changer les Couleurs des Tables

Ajoutez après la déclaration de table:

```dbml
Table users [headercolor: #3498db] {
  // colonnes...
}

Table matches [headercolor: #e74c3c] {
  // colonnes...
}
```

**Codes couleur suggérés:**
- `#3498db` - Bleu (users)
- `#e74c3c` - Rouge (matches)
- `#2ecc71` - Vert (friendships)
- `#f39c12` - Orange (stats)

### Grouper les Tables

```dbml
TableGroup "Authentication" {
  users
  user_stats
}

TableGroup "Social" {
  friendships
}

TableGroup "Gaming" {
  matches
  match_participants
}
```

---

## 🔍 Analyse du Schéma

### Relations Identifiées:

#### 1. Users ↔ Friendships (Many-to-Many)
```
users ──1:N──▶ friendships (requester_id)
users ──1:N──▶ friendships (receiver_id)
```
Un utilisateur peut avoir plusieurs amis, et chaque relation implique 2 utilisateurs.

#### 2. Users ↔ User Stats (One-to-One)
```
users ──1:1──▶ user_stats
```
Chaque utilisateur a exactement un enregistrement de statistiques.

#### 3. Matches ↔ Participants (One-to-Many)
```
matches ──1:N──▶ match_participants
```
Une partie contient plusieurs joueurs.

#### 4. Users ↔ Match Participants (One-to-Many)
```
users ──1:N──▶ match_participants
```
Un joueur peut participer à plusieurs parties.

---

## 📊 Statistiques du Schéma

- **Total Tables**: 5
- **Total Relations**: 5 (Foreign Keys)
- **Total Index**: 13
- **Total Contraintes**: 8+

### Distribution des Colonnes:
- `users`: 11 colonnes
- `friendships`: 6 colonnes
- `matches`: 8 colonnes
- `match_participants`: 10 colonnes
- `user_stats`: 10 colonnes

**Total**: 45 colonnes

---

## 🛠️ Utilisation Avancée

### Générer le Schéma SQL depuis DBML

Si vous modifiez le DBML sur dbdiagram.io:

1. Cliquez sur **"Export"** → **"PostgreSQL"**
2. Téléchargez le SQL généré
3. Comparez avec votre fichier actuel:

```powershell
# Télécharger depuis dbdiagram.io → nexa_exported.sql

# Comparer les différences
code --diff "E:\.Dev\.ProjetsPerso\NexA\NexA\DATABASE_SCHEMA.sql" "nexa_exported.sql"
```

### Générer le DBML depuis SQL Existant

Si vous avez un SQL existant:

1. Sur dbdiagram.io, cliquez **"Import"** → **"From SQL"**
2. Collez votre SQL PostgreSQL
3. Le DBML est généré automatiquement

---

## 📝 Exemple de Modifications Courantes

### Ajouter une Nouvelle Table

```dbml
Table champions {
  id integer [pk, increment]
  name varchar(100) [not null, unique]
  role varchar(50) [not null]
  difficulty integer [default: 1]
  
  Note: 'Personnages jouables du jeu'
}

// Lien avec match_participants
Ref: match_participants.champion_id > champions.id
```

### Ajouter une Colonne

```dbml
Table users {
  // ...colonnes existantes...
  preferred_role varchar(50) [null, note: 'Rôle préféré du joueur']
}
```

### Ajouter un Index

```dbml
Table users {
  // ...
  Indexes {
    (username, email) [name: 'idx_user_search']
  }
}
```

---

## 🐛 Dépannage

### Erreur: "Syntax Error"
- Vérifiez les parenthèses `[]` et accolades `{}`
- Vérifiez que les références existent
- Les noms de tables sont sensibles à la casse

### Le Diagramme Ne Se Génère Pas
- Rafraîchissez la page (F5)
- Vérifiez la console (F12) pour les erreurs JavaScript
- Essayez un autre navigateur (Chrome recommandé)

### Les Relations Ne S'Affichent Pas
- Vérifiez que les colonnes référencées existent
- Le type de colonne doit correspondre (uuid → uuid)
- Utilisez `ref:` dans la déclaration de colonne ou `Ref:` séparément

---

## 🔗 Ressources

- **Site officiel**: https://dbdiagram.io/
- **Documentation DBML**: https://dbml.dbdiagram.io/docs/
- **Exemples**: https://dbdiagram.io/d (galerie publique)
- **Support**: support@dbdiagram.io

---

## 💡 Astuces

### 1. Raccourcis Clavier
- `Ctrl + /` : Commenter/Décommenter
- `Ctrl + F` : Rechercher
- `Ctrl + S` : Sauvegarder (si connecté)
- `Ctrl + Z` : Annuler
- `Tab` : Auto-complétion

### 2. Commentaires
```dbml
// Commentaire sur une ligne

/*
  Commentaire
  sur plusieurs
  lignes
*/
```

### 3. Notes Markdown
```dbml
Note table_name {
  '''
  # Titre
  **Gras** et *italique*
  
  - Liste
  - D'items
  
  ```sql
  SELECT * FROM table;
  ```
  '''
}
```

---

## 🎓 Prochaines Étapes

1. ✅ Visualiser le schéma actuel
2. ⏳ Exporter en PNG pour la documentation
3. ⏳ Partager avec l'équipe
4. ⏳ Ajouter les nouvelles tables (champions, items, etc.)
5. ⏳ Générer le SQL pour les migrations

---

**Bon diagramming ! 📊✨**
