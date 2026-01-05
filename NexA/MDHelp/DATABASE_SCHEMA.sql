# 🗄️ PostgreSQL Schema - NexA Backend

## 📐 Schema Complet (MVP)

```sql
-- ============================================
-- EXTENSIONS
-- ============================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- Pour la recherche fulltext

-- ============================================
-- TABLE: users
-- ============================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    avatar_url VARCHAR(500) DEFAULT 'https://cdn.nexa.game/avatars/default.png',
    level INTEGER DEFAULT 1 CHECK (level >= 1),
    elo INTEGER DEFAULT 1000 CHECK (elo >= 0),
    status VARCHAR(20) DEFAULT 'offline' CHECK (status IN ('online', 'offline', 'in-game')),
    is_banned BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_seen_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour recherche username (ILIKE + trigram)
CREATE INDEX idx_users_username_trgm ON users USING gin (username gin_trgm_ops);
CREATE INDEX idx_users_email ON users (email);
CREATE INDEX idx_users_status ON users (status) WHERE status != 'offline'; -- Partial index
CREATE INDEX idx_users_created_at ON users (created_at DESC);

-- ============================================
-- TABLE: refresh_tokens
-- ============================================
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens (user_id);
CREATE INDEX idx_refresh_tokens_token_hash ON refresh_tokens (token_hash);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens (expires_at) WHERE NOT revoked;

-- ============================================
-- TABLE: friend_requests
-- ============================================
CREATE TABLE friend_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    from_user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    to_user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'declined')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT no_self_request CHECK (from_user_id != to_user_id),
    CONSTRAINT unique_friend_request UNIQUE (from_user_id, to_user_id)
);

CREATE INDEX idx_friend_requests_to_user ON friend_requests (to_user_id) WHERE status = 'pending';
CREATE INDEX idx_friend_requests_from_user ON friend_requests (from_user_id) WHERE status = 'pending';
CREATE INDEX idx_friend_requests_status ON friend_requests (status);

-- ============================================
-- TABLE: friendships
-- ============================================
CREATE TABLE friendships (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user1_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user2_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT no_self_friendship CHECK (user1_id != user2_id),
    CONSTRAINT unique_friendship UNIQUE (user1_id, user2_id),
    -- S'assurer que user1_id < user2_id pour éviter les doublons inversés
    CONSTRAINT ordered_users CHECK (user1_id < user2_id)
);

CREATE INDEX idx_friendships_user1 ON friendships (user1_id);
CREATE INDEX idx_friendships_user2 ON friendships (user2_id);
CREATE INDEX idx_friendships_both ON friendships (user1_id, user2_id);

-- ============================================
-- TABLE: matches
-- ============================================
CREATE TABLE matches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    mode VARCHAR(50) NOT NULL CHECK (mode IN ('casual', 'ranked', 'custom')),
    map VARCHAR(100),
    duration INTEGER NOT NULL CHECK (duration > 0), -- en secondes
    winning_team VARCHAR(20), -- 'blue', 'red', 'draw', etc.
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ended_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_matches_created_at ON matches (created_at DESC);
CREATE INDEX idx_matches_mode ON matches (mode);

-- ============================================
-- TABLE: match_participants
-- ============================================
CREATE TABLE match_participants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_id UUID NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    team VARCHAR(20) NOT NULL,
    result VARCHAR(20) NOT NULL CHECK (result IN ('victory', 'defeat', 'draw')),
    score INTEGER DEFAULT 0 CHECK (score >= 0),
    kills INTEGER DEFAULT 0 CHECK (kills >= 0),
    deaths INTEGER DEFAULT 0 CHECK (deaths >= 0),
    assists INTEGER DEFAULT 0 CHECK (assists >= 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Contraintes
    CONSTRAINT unique_match_participant UNIQUE (match_id, user_id)
);

CREATE INDEX idx_match_participants_match ON match_participants (match_id);
CREATE INDEX idx_match_participants_user ON match_participants (user_id, created_at DESC);
CREATE INDEX idx_match_participants_user_match ON match_participants (user_id, match_id);

-- ============================================
-- TRIGGERS: updated_at auto-update
-- ============================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_friend_requests_updated_at BEFORE UPDATE ON friend_requests
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- VIEWS: Simplifier les requêtes courantes
-- ============================================

-- Vue pour obtenir les amis d'un user
CREATE OR REPLACE VIEW user_friends AS
SELECT 
    f.id AS friendship_id,
    CASE 
        WHEN f.user1_id = u.id THEN f.user2_id
        ELSE f.user1_id
    END AS friend_id,
    u.id AS user_id,
    f.created_at AS friends_since
FROM friendships f
CROSS JOIN users u
WHERE u.id IN (f.user1_id, f.user2_id);

-- Vue pour les stats d'un joueur
CREATE OR REPLACE VIEW user_stats AS
SELECT 
    user_id,
    COUNT(*) AS total_matches,
    SUM(CASE WHEN result = 'victory' THEN 1 ELSE 0 END) AS wins,
    SUM(CASE WHEN result = 'defeat' THEN 1 ELSE 0 END) AS losses,
    SUM(CASE WHEN result = 'draw' THEN 1 ELSE 0 END) AS draws,
    ROUND(
        (SUM(CASE WHEN result = 'victory' THEN 1 ELSE 0 END)::NUMERIC / 
         NULLIF(COUNT(*), 0)) * 100, 
        2
    ) AS win_rate,
    SUM(kills) AS total_kills,
    SUM(deaths) AS total_deaths,
    SUM(assists) AS total_assists,
    ROUND(
        SUM(kills)::NUMERIC / NULLIF(SUM(deaths), 0),
        2
    ) AS kd_ratio
FROM match_participants
GROUP BY user_id;

-- ============================================
-- FONCTIONS UTILITAIRES
-- ============================================

-- Fonction pour créer une amitié (gère l'ordre user1 < user2)
CREATE OR REPLACE FUNCTION create_friendship(
    p_user1_id UUID,
    p_user2_id UUID
) RETURNS UUID AS $$
DECLARE
    v_friendship_id UUID;
    v_smaller_id UUID;
    v_larger_id UUID;
BEGIN
    -- S'assurer que user1_id < user2_id
    IF p_user1_id < p_user2_id THEN
        v_smaller_id := p_user1_id;
        v_larger_id := p_user2_id;
    ELSE
        v_smaller_id := p_user2_id;
        v_larger_id := p_user1_id;
    END IF;
    
    -- Insérer la friendship
    INSERT INTO friendships (user1_id, user2_id)
    VALUES (v_smaller_id, v_larger_id)
    RETURNING id INTO v_friendship_id;
    
    RETURN v_friendship_id;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour vérifier si deux users sont amis
CREATE OR REPLACE FUNCTION are_friends(
    p_user1_id UUID,
    p_user2_id UUID
) RETURNS BOOLEAN AS $$
DECLARE
    v_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM friendships
    WHERE (user1_id = LEAST(p_user1_id, p_user2_id) 
       AND user2_id = GREATEST(p_user1_id, p_user2_id));
    
    RETURN v_count > 0;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- DONNÉES DE TEST (optionnel, pour dev)
-- ============================================

-- User test (password: 'TestPass123!')
-- Hash bcrypt de 'TestPass123!' avec salt 10
INSERT INTO users (username, email, password_hash, level, elo, status) VALUES
('TestPlayer', 'test@nexa.game', '$2b$10$N9qo8uLOickgx2ZMRZoMye.IZRbuCl4jBGHZJdG6FvCh5YgbCQGLm', 10, 1200, 'online'),
('ProGamer', 'pro@nexa.game', '$2b$10$N9qo8uLOickgx2ZMRZoMye.IZRbuCl4jBGHZJdG6FvCh5YgbCQGLm', 25, 1800, 'offline'),
('CasualPlayer', 'casual@nexa.game', '$2b$10$N9qo8uLOickgx2ZMRZoMye.IZRbuCl4jBGHZJdG6FvCh5YgbCQGLm', 5, 950, 'in-game');

-- Friendships test
DO $$
DECLARE
    v_user1_id UUID;
    v_user2_id UUID;
BEGIN
    SELECT id INTO v_user1_id FROM users WHERE username = 'TestPlayer';
    SELECT id INTO v_user2_id FROM users WHERE username = 'ProGamer';
    
    IF v_user1_id IS NOT NULL AND v_user2_id IS NOT NULL THEN
        PERFORM create_friendship(v_user1_id, v_user2_id);
    END IF;
END $$;

-- Match test
DO $$
DECLARE
    v_match_id UUID;
    v_user1_id UUID;
    v_user2_id UUID;
    v_user3_id UUID;
BEGIN
    SELECT id INTO v_user1_id FROM users WHERE username = 'TestPlayer';
    SELECT id INTO v_user2_id FROM users WHERE username = 'ProGamer';
    SELECT id INTO v_user3_id FROM users WHERE username = 'CasualPlayer';
    
    -- Créer un match
    INSERT INTO matches (mode, map, duration, winning_team, created_at, ended_at)
    VALUES ('ranked', 'Arena Alpha', 1847, 'blue', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '30 minutes')
    RETURNING id INTO v_match_id;
    
    -- Ajouter les participants
    INSERT INTO match_participants (match_id, user_id, team, result, score, kills, deaths, assists) VALUES
    (v_match_id, v_user1_id, 'blue', 'victory', 2450, 12, 5, 8),
    (v_match_id, v_user2_id, 'blue', 'victory', 2800, 15, 3, 10),
    (v_match_id, v_user3_id, 'red', 'defeat', 1900, 8, 12, 4);
END $$;

-- ============================================
-- MAINTENANCE & OPTIMISATION
-- ============================================

-- Nettoyer les refresh tokens expirés (à exécuter régulièrement via cron)
CREATE OR REPLACE FUNCTION cleanup_expired_tokens()
RETURNS INTEGER AS $$
DECLARE
    v_deleted_count INTEGER;
BEGIN
    DELETE FROM refresh_tokens
    WHERE expires_at < NOW() OR revoked = TRUE;
    
    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    RETURN v_deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Mettre à jour last_seen_at d'un user (appelé par le backend régulièrement)
CREATE OR REPLACE FUNCTION update_user_last_seen(p_user_id UUID)
RETURNS VOID AS $$
BEGIN
    UPDATE users
    SET last_seen_at = NOW()
    WHERE id = p_user_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- MIGRATIONS SETUP
-- ============================================
-- Pour gérer les migrations, utiliser un outil comme:
-- - node-pg-migrate (npm package)
-- - TypeORM migrations
-- - Prisma Migrate
-- - Flyway

-- Exemple de table de suivi de migrations (si custom)
CREATE TABLE IF NOT EXISTS schema_migrations (
    id SERIAL PRIMARY KEY,
    version VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

INSERT INTO schema_migrations (version, description) VALUES
('001', 'Initial schema - users, friends, matches');

```

---

## 📊 Exemples de Requêtes SQL Optimisées

### 1. Obtenir les amis d'un user avec leur statut
```sql
SELECT 
    u.id,
    u.username,
    u.avatar_url,
    u.level,
    u.elo,
    u.status,
    u.last_seen_at,
    f.created_at AS friends_since
FROM user_friends uf
JOIN users u ON u.id = uf.friend_id
JOIN friendships f ON f.id = uf.friendship_id
WHERE uf.user_id = $1
ORDER BY 
    CASE u.status
        WHEN 'online' THEN 1
        WHEN 'in-game' THEN 2
        WHEN 'offline' THEN 3
    END,
    u.username ASC;
```

### 2. Recherche utilisateurs (fulltext avec trigram)
```sql
SELECT 
    u.id,
    u.username,
    u.avatar_url,
    u.level,
    u.status,
    EXISTS(
        SELECT 1 FROM friendships f
        WHERE (f.user1_id = $1 AND f.user2_id = u.id)
           OR (f.user2_id = $1 AND f.user1_id = u.id)
    ) AS is_friend,
    EXISTS(
        SELECT 1 FROM friend_requests fr
        WHERE fr.from_user_id = $1 
          AND fr.to_user_id = u.id 
          AND fr.status = 'pending'
    ) AS has_pending_request
FROM users u
WHERE u.username ILIKE $2 || '%'
  AND u.id != $1
  AND u.is_banned = FALSE
ORDER BY similarity(u.username, $2) DESC
LIMIT $3;
```

### 3. Historique matches avec cursor-based pagination
```sql
SELECT 
    m.id,
    m.mode,
    m.map,
    m.duration,
    m.created_at,
    mp.result,
    mp.score AS my_score,
    mp.kills AS my_kills,
    mp.deaths AS my_deaths,
    (SELECT COUNT(*) FROM match_participants mp2 WHERE mp2.match_id = m.id) AS player_count
FROM match_participants mp
JOIN matches m ON m.id = mp.match_id
WHERE mp.user_id = $1
  AND ($2::UUID IS NULL OR m.created_at < (
      SELECT created_at FROM matches WHERE id = $2
  ))
ORDER BY m.created_at DESC
LIMIT $3;
```

### 4. Détails d'un match avec tous les participants
```sql
SELECT 
    m.id AS match_id,
    m.mode,
    m.map,
    m.duration,
    m.winning_team,
    m.created_at,
    json_agg(
        json_build_object(
            'userId', u.id,
            'username', u.username,
            'avatar', u.avatar_url,
            'team', mp.team,
            'result', mp.result,
            'score', mp.score,
            'kills', mp.kills,
            'deaths', mp.deaths,
            'assists', mp.assists,
            'isMe', mp.user_id = $2
        ) ORDER BY mp.score DESC
    ) AS participants
FROM matches m
JOIN match_participants mp ON mp.match_id = m.id
JOIN users u ON u.id = mp.user_id
WHERE m.id = $1
GROUP BY m.id;
```

### 5. Stats d'un joueur
```sql
SELECT 
    u.id,
    u.username,
    u.level,
    u.elo,
    COALESCE(s.total_matches, 0) AS total_matches,
    COALESCE(s.wins, 0) AS wins,
    COALESCE(s.losses, 0) AS losses,
    COALESCE(s.win_rate, 0) AS win_rate,
    COALESCE(s.total_kills, 0) AS total_kills,
    COALESCE(s.total_deaths, 0) AS total_deaths,
    COALESCE(s.kd_ratio, 0) AS kd_ratio
FROM users u
LEFT JOIN user_stats s ON s.user_id = u.id
WHERE u.id = $1;
```

---

## 🔧 Configuration PostgreSQL Recommandée

### postgresql.conf (pour prod)
```ini
# Connexions
max_connections = 100
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 4MB
maintenance_work_mem = 64MB

# WAL
wal_level = replica
max_wal_size = 1GB
min_wal_size = 80MB

# Query planner
random_page_cost = 1.1
effective_io_concurrency = 200

# Logging
log_destination = 'stderr'
logging_collector = on
log_directory = 'log'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
log_rotation_age = 1d
log_rotation_size = 100MB
log_line_prefix = '%t [%p]: user=%u,db=%d,app=%a,client=%h '
log_min_duration_statement = 1000  # Log queries > 1s
```

### pg_hba.conf
```
# Local connections
local   all             postgres                                peer
local   all             all                                     scram-sha-256

# Remote connections (prod: restreindre aux IPs du backend)
host    nexa_db         nexa_user       10.0.0.0/8             scram-sha-256
host    all             all             0.0.0.0/0              reject
```

---

## 📦 Backup & Restore

### Backup automatique (cron quotidien)
```bash
#!/bin/bash
# /opt/nexa/scripts/backup_db.sh
BACKUP_DIR="/var/backups/postgresql"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="nexa_db"

pg_dump -U nexa_user -F c -b -v -f "${BACKUP_DIR}/${DB_NAME}_${DATE}.backup" ${DB_NAME}

# Garder seulement les 7 derniers jours
find ${BACKUP_DIR} -name "${DB_NAME}_*.backup" -mtime +7 -delete
```

### Restore
```bash
pg_restore -U nexa_user -d nexa_db -v /var/backups/postgresql/nexa_db_20260104.backup
```

---

## 🔍 Monitoring Queries

### pg_stat_statements (activer dans postgresql.conf)
```sql
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Top 10 requêtes les plus lentes
SELECT 
    query,
    calls,
    total_exec_time,
    mean_exec_time,
    max_exec_time
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;
```


