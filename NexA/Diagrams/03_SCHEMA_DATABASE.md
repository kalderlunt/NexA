# Schéma de Base de Données PostgreSQL

## Vue d'ensemble

Ce document décrit le schéma complet de la base de données PostgreSQL pour NexA, avec relations, index, et requêtes types.

---

## Diagramme 13: ERD (Entity Relationship Diagram) - MVP

**Type de diagramme souhaité**: Entity Relationship Diagram (ERD)

**Description**:
Schéma relationnel complet des tables avec clés primaires, étrangères, et cardinalités.

**Tables**:

### Table: `users`
**Description**: Stocke les comptes utilisateurs

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| id | UUID | PK, DEFAULT uuid_generate_v4() | Identifiant unique |
| email | VARCHAR(255) | UNIQUE, NOT NULL | Email (login) |
| username | VARCHAR(50) | UNIQUE, NOT NULL | Pseudo public |
| password_hash | VARCHAR(255) | NOT NULL | Bcrypt hash |
| avatar_url | VARCHAR(500) | NULL | URL avatar (CDN) |
| level | INTEGER | DEFAULT 1 | Niveau joueur |
| elo | INTEGER | DEFAULT 1000 | Rating compétitif |
| created_at | TIMESTAMP | DEFAULT NOW() | Date création |
| last_login | TIMESTAMP | NULL | Dernière connexion |
| is_active | BOOLEAN | DEFAULT TRUE | Compte actif |
| status | VARCHAR(20) | DEFAULT 'offline' | offline/online/in-game |

**Index**:
- `idx_users_email` (email) - recherche login
- `idx_users_username` (username) - recherche publique
- `idx_users_status` (status) - filtrage amis online

---

### Table: `friendships`
**Description**: Gère les relations d'amitié et demandes

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| id | UUID | PK, DEFAULT uuid_generate_v4() | Identifiant unique |
| requester_id | UUID | FK → users(id), NOT NULL | Utilisateur qui envoie |
| receiver_id | UUID | FK → users(id), NOT NULL | Utilisateur qui reçoit |
| status | VARCHAR(20) | NOT NULL | pending/accepted/declined |
| created_at | TIMESTAMP | DEFAULT NOW() | Date demande |
| updated_at | TIMESTAMP | DEFAULT NOW() | Date mise à jour |

**Contraintes**:
- `CHECK (requester_id != receiver_id)` - pas de self-friend
- `UNIQUE (requester_id, receiver_id)` - pas de doublons

**Index**:
- `idx_friendships_requester` (requester_id, status) - mes demandes
- `idx_friendships_receiver` (receiver_id, status) - demandes reçues
- `idx_friendships_accepted` (requester_id, receiver_id) WHERE status='accepted' - liste amis

**Triggers**:
- `updated_at` auto-update on UPDATE

---

### Table: `matches`
**Description**: Stocke l'historique des parties

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| id | UUID | PK, DEFAULT uuid_generate_v4() | Identifiant unique |
| match_type | VARCHAR(20) | NOT NULL | ranked/normal/custom |
| status | VARCHAR(20) | NOT NULL | pending/in_progress/completed/cancelled |
| winner_team | INTEGER | NULL | 1 ou 2 (NULL si pas terminé) |
| duration_seconds | INTEGER | NULL | Durée partie (secondes) |
| started_at | TIMESTAMP | NULL | Date début |
| ended_at | TIMESTAMP | NULL | Date fin |
| created_at | TIMESTAMP | DEFAULT NOW() | Date création match |

**Index**:
- `idx_matches_created_at` (created_at DESC) - historique récent
- `idx_matches_status` (status) - matches en cours

---

### Table: `match_participants`
**Description**: Joueurs dans une partie (relation many-to-many)

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| id | UUID | PK, DEFAULT uuid_generate_v4() | Identifiant unique |
| match_id | UUID | FK → matches(id), NOT NULL | Référence match |
| user_id | UUID | FK → users(id), NOT NULL | Référence joueur |
| team | INTEGER | NOT NULL | 1 ou 2 |
| champion_id | INTEGER | NULL | ID champion joué (futur) |
| kills | INTEGER | DEFAULT 0 | Kills |
| deaths | INTEGER | DEFAULT 0 | Morts |
| assists | INTEGER | DEFAULT 0 | Assistances |
| gold_earned | INTEGER | DEFAULT 0 | Or gagné |
| damage_dealt | INTEGER | DEFAULT 0 | Dégâts infligés |
| is_mvp | BOOLEAN | DEFAULT FALSE | MVP de la partie |

**Contraintes**:
- `UNIQUE (match_id, user_id)` - un joueur une fois par match
- `CHECK (team IN (1, 2))` - seulement 2 équipes

**Index**:
- `idx_participants_match` (match_id) - participants d'un match
- `idx_participants_user` (user_id, match_id DESC) - historique joueur
- `idx_participants_user_recent` (user_id, match_id) WHERE matches.created_at > NOW() - INTERVAL '30 days' - optimisation historique récent

---

## Relations (Cardinalités):

```
users (1) ─────< (N) friendships (requester_id)
users (1) ─────< (N) friendships (receiver_id)

users (1) ─────< (N) match_participants
matches (1) ────< (N) match_participants

Relation amitié bidirectionnelle:
User A ←→ User B nécessite une row avec status='accepted'
```

---

## Diagramme 14: Stratégie d'Index et Performances

**Type de diagramme souhaité**: Index Strategy Diagram

**Description**:
Vue des index et stratégies d'optimisation pour les requêtes fréquentes.

**Requêtes critiques à optimiser**:

### Q1: Liste d'amis avec statut
```sql
SELECT u.id, u.username, u.avatar_url, u.status
FROM users u
JOIN friendships f ON (
    (f.requester_id = $1 AND f.receiver_id = u.id) OR
    (f.receiver_id = $1 AND f.requester_id = u.id)
)
WHERE f.status = 'accepted'
ORDER BY u.status DESC, u.username ASC;
```
**Index utilisés**:
- `idx_friendships_accepted`
- `idx_users_status`

**Performance cible**: < 10ms pour 100 amis

---

### Q2: Historique parties (pagination cursor)
```sql
SELECT m.*, COUNT(mp.id) as player_count
FROM matches m
JOIN match_participants mp ON m.id = mp.match_id
WHERE mp.user_id = $1 AND m.created_at < $2
GROUP BY m.id
ORDER BY m.created_at DESC
LIMIT 20;
```
**Index utilisés**:
- `idx_participants_user_recent`
- `idx_matches_created_at`

**Performance cible**: < 20ms pour 1000 matches

---

### Q3: Détails match avec participants
```sql
SELECT 
    m.*,
    json_agg(
        json_build_object(
            'user_id', u.id,
            'username', u.username,
            'team', mp.team,
            'kills', mp.kills,
            'deaths', mp.deaths,
            'assists', mp.assists
        )
    ) as participants
FROM matches m
JOIN match_participants mp ON m.id = mp.match_id
JOIN users u ON mp.user_id = u.id
WHERE m.id = $1
GROUP BY m.id;
```
**Index utilisés**:
- `idx_participants_match`

**Performance cible**: < 15ms

---

### Q4: Search users
```sql
SELECT id, username, avatar_url
FROM users
WHERE username ILIKE $1 || '%'
AND is_active = TRUE
LIMIT 10;
```
**Index utilisés**:
- `idx_users_username` (B-tree supporte LIKE prefix)

**Performance cible**: < 5ms

---

## Diagramme 15: Stratégie de Migration

**Type de diagramme souhaité**: Timeline / Migration Strategy

**Description**:
Plan de migration et évolution du schéma.

**Phases**:

### Phase 0: MVP (Initial)
**Tables**: users, friendships, matches, match_participants
**Fonctionnalités**: Auth, Friends, Match History
**Contraintes**: Basiques
**Index**: Essentiels uniquement

### Phase 1: Beta (Semaine 2-4)
**Ajouts**:
- Table `notifications`
  - id, user_id, type, content, read, created_at
- Table `user_settings`
  - user_id (PK), language, theme, notifications_enabled
- Colonne `last_seen` dans users (pour "Away" status)

**Optimisations**:
- Partitionnement `matches` par date (si > 1M rows)
- Index GIN sur jsonb (si données match complexes)

### Phase 2: Production (Mois 2-3)
**Ajouts**:
- Table `matchmaking_queue`
- Table `chat_messages`
- Table `achievements`
- Table `user_stats_daily` (agrégats pré-calculés)

**Optimisations**:
- Materialized view `user_stats_summary`
- Partitionnement `chat_messages` par mois
- Archive `matches` > 6 mois dans table séparée

### Phase 3: Scale (Mois 6+)
**Évolutions**:
- Read replicas PostgreSQL
- Redis cache layer (friends list, user session)
- TimescaleDB extension pour time-series (métriques)
- Sharding par région (si multi-régions)

---

## Diagramme 16: Data Flow - Création de Match

**Type de diagramme souhaité**: Data Flow Diagram

**Description**:
Flux de données lors de la création et sauvegarde d'un match.

**Étapes**:

```
[Game Server] 
    ↓ (1) Match terminé
[POST /matches/complete]
    ↓
[API Backend]
    ↓ (2) Validation payload
    ├─→ Vérifie users existent
    ├─→ Vérifie match type valide
    └─→ Vérifie teams équilibrées
    ↓ (3) Transaction START
    ├─→ INSERT INTO matches (...)
    │       ↓
    │   [Obtient match.id]
    │       ↓
    ├─→ INSERT INTO match_participants (...) [bulk insert x10]
    │       ↓
    ├─→ UPDATE users SET level = level + 1 WHERE ... (XP gain)
    │       ↓
    ├─→ UPDATE users SET elo = elo + X WHERE ... (ELO ajustement)
    │       ↓
    └─→ INSERT INTO notifications (type='match_completed') [x10]
    ↓ (4) Transaction COMMIT
[Return match.id]
    ↓
[Cache Service]
    ├─→ Invalidate cache: friends list (statut change)
    ├─→ Invalidate cache: match history (nouveau match)
    └─→ Invalidate cache: user profile (level/elo change)
```

**Gestion d'erreurs**:
- Timeout → ROLLBACK complet
- Duplicate match_id → Idempotence (déjà créé)
- User not found → ROLLBACK + 404 error

**Performance**:
- Bulk insert participants (1 requête au lieu de 10)
- Index sur match_id pour JOIN rapide
- Transaction < 100ms target

---

## Diagramme 17: Archivage et Rétention

**Type de diagramme souhaité**: Flowchart / Data Lifecycle

**Description**:
Politique de rétention et archivage des données.

**Règles de rétention**:

### Données HOT (Active)
**Tables**: users, friendships
**Durée**: Permanent (tant que compte actif)
**Stockage**: PostgreSQL principal
**Backup**: Daily

### Données WARM (Recent)
**Tables**: matches, match_participants
**Durée**: 6 mois
**Stockage**: PostgreSQL principal
**Backup**: Weekly
**Accès**: Fréquent (historique récent)

### Données COLD (Archive)
**Tables**: matches_archive, match_participants_archive
**Durée**: 6 mois - 2 ans
**Stockage**: PostgreSQL séparé ou S3 (Parquet)
**Backup**: Monthly
**Accès**: Rare (stats historiques)

### Données DELETED
**Tables**: users (soft delete: is_active=false)
**Durée**: 30 jours avant hard delete (RGPD)
**Anonymisation**: Remplacer email/username par hash

**Processus d'archivage** (cron job):
```
[Daily 2AM]
    ↓
[SELECT matches WHERE created_at < NOW() - INTERVAL '6 months']
    ↓
[INSERT INTO matches_archive SELECT * FROM matches WHERE ...]
    ↓
[INSERT INTO match_participants_archive ...]
    ↓
[DELETE FROM match_participants WHERE match_id IN (...)]
    ↓
[DELETE FROM matches WHERE created_at < NOW() - INTERVAL '6 months']
    ↓
[VACUUM ANALYZE]
```

---

## Diagramme 18: Gestion des Sessions et Tokens

**Type de diagramme souhaité**: Data Model + State Diagram

**Description**:
Stockage et gestion des sessions JWT.

**Option 1: Stateless JWT (MVP)**
**Stockage**: Aucun (JWT auto-suffisant)
**Avantages**: Simple, scalable
**Inconvénients**: Pas de révocation immédiate

**Payload JWT**:
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "iat": 1234567890,
  "exp": 1234571490
}
```

**Option 2: JWT + Whitelist (Beta)**
**Ajout table**: `refresh_tokens`

| Colonne | Type | Contraintes | Description |
|---------|------|-------------|-------------|
| id | UUID | PK | Token ID (jti) |
| user_id | UUID | FK → users(id) | Utilisateur |
| token_hash | VARCHAR(255) | UNIQUE | Hash du refresh token |
| expires_at | TIMESTAMP | NOT NULL | Expiration |
| created_at | TIMESTAMP | DEFAULT NOW() | Création |
| last_used | TIMESTAMP | NULL | Dernier usage |
| is_revoked | BOOLEAN | DEFAULT FALSE | Révoqué |

**Avantages**: 
- Révocation immédiate possible
- Tracking usage
- Limite nombre de sessions

**Flow**:
```
[Login] 
    ↓
[Generate access token (15min) + refresh token (7 days)]
    ↓
[INSERT refresh_token]
    ↓
[Return both tokens]
    ↓
[Access token expire]
    ↓
[Client: POST /auth/refresh avec refresh_token]
    ↓
[Verify refresh_token in DB]
    ├─→ Revoked? → 401 Unauthorized
    ├─→ Expired? → 401 Unauthorized
    └─→ Valid → Generate new access token
```

**Nettoyage** (cron):
```sql
DELETE FROM refresh_tokens 
WHERE expires_at < NOW() 
OR (last_used < NOW() - INTERVAL '30 days' AND is_revoked = FALSE);
```

---

## Diagramme 19: Stratégie de Backup

**Type de diagramme souhaité**: Infrastructure + Timeline

**Description**:
Plan de sauvegarde et disaster recovery.

**Niveaux de backup**:

### Level 1: Continuous (WAL Archiving)
**Fréquence**: Temps réel
**Outil**: pg_wal archiving
**Stockage**: Local SSD + S3
**RPO**: < 1 minute (Recovery Point Objective)
**RTO**: < 30 minutes (Recovery Time Objective)

### Level 2: Daily Full Backup
**Fréquence**: Daily 3AM
**Outil**: pg_dump
**Stockage**: S3 (Standard)
**Rétention**: 7 jours
**Compression**: gzip

### Level 3: Weekly Full Backup
**Fréquence**: Sunday 2AM
**Outil**: pg_basebackup
**Stockage**: S3 (Glacier)
**Rétention**: 4 semaines

### Level 4: Monthly Archive
**Fréquence**: 1er du mois
**Outil**: pg_dump (archive format)
**Stockage**: S3 (Deep Archive)
**Rétention**: 1 an

**Test de restauration**: Monthly (automatisé)

---

## Requêtes SQL Exemples (pour documentation)

### Insertion match complet (transaction)
```sql
BEGIN;

-- 1. Créer le match
INSERT INTO matches (match_type, status, winner_team, duration_seconds, started_at, ended_at)
VALUES ('ranked', 'completed', 1, 1800, NOW() - INTERVAL '30 minutes', NOW())
RETURNING id;

-- 2. Insérer participants (bulk)
INSERT INTO match_participants (match_id, user_id, team, kills, deaths, assists)
VALUES 
    ('match-uuid', 'user1-uuid', 1, 10, 2, 5),
    ('match-uuid', 'user2-uuid', 1, 8, 3, 12),
    -- ... x10
ON CONFLICT DO NOTHING;

-- 3. Update ELO
UPDATE users 
SET elo = elo + 25 
WHERE id IN (SELECT user_id FROM match_participants WHERE match_id = 'match-uuid' AND team = 1);

COMMIT;
```

### Pagination cursor-based
```sql
-- Page 1 (initiale)
SELECT id, created_at, winner_team, duration_seconds
FROM matches m
JOIN match_participants mp ON m.id = mp.match_id
WHERE mp.user_id = $1
ORDER BY created_at DESC
LIMIT 20;

-- Page 2+ (avec cursor)
SELECT id, created_at, winner_team, duration_seconds
FROM matches m
JOIN match_participants mp ON m.id = mp.match_id
WHERE mp.user_id = $1 
  AND m.created_at < $2  -- cursor = created_at de la dernière row page précédente
ORDER BY created_at DESC
LIMIT 20;
```

---

## Métadonnées pour génération

### Conventions de nommage
- Tables: pluriel, snake_case
- Colonnes: snake_case
- Index: `idx_table_columns`
- Foreign keys: `fk_table_column`
- Constraints: `chk_table_description`

### Couleurs ERD suggérées
- Entités principales (users, matches): #2196F3 (bleu)
- Entités de liaison (friendships, match_participants): #FF9800 (orange)
- Clés primaires: #4CAF50 (vert)
- Clés étrangères: #9C27B0 (violet)

### Outils recommandés
- dbdiagram.io
- DrawSQL
- pgAdmin ERD
- PlantUML (avec @startuml @enduml)


