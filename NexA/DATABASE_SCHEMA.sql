-- NexA Database Schema (PostgreSQL)
-- Version: 1.0.0

-- Activer l'extension UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========================================
-- TABLE: users
-- Description: Comptes utilisateurs
-- ========================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    avatar_url VARCHAR(500),
    level INTEGER DEFAULT 1 CHECK (level >= 1),
    elo INTEGER DEFAULT 1000 CHECK (elo >= 0),
    created_at TIMESTAMP DEFAULT NOW() NOT NULL,
    last_login TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    status VARCHAR(20) DEFAULT 'offline' CHECK (status IN ('offline', 'online', 'in-game'))
);

-- Index pour optimiser les recherches
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_status ON users(status) WHERE status != 'offline';
CREATE INDEX idx_users_created_at ON users(created_at DESC);

-- ========================================
-- TABLE: friendships
-- Description: Relations d'amitié
-- ========================================
CREATE TABLE friendships (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    requester_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    receiver_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'declined', 'blocked')),
    created_at TIMESTAMP DEFAULT NOW() NOT NULL,
    updated_at TIMESTAMP DEFAULT NOW() NOT NULL,
    CONSTRAINT no_self_friend CHECK (requester_id != receiver_id),
    CONSTRAINT unique_friendship UNIQUE (requester_id, receiver_id)
);

-- Index pour les requêtes de friendships
CREATE INDEX idx_friendships_requester ON friendships(requester_id, status);
CREATE INDEX idx_friendships_receiver ON friendships(receiver_id, status);
CREATE INDEX idx_friendships_accepted ON friendships(requester_id, receiver_id) WHERE status = 'accepted';

-- Trigger pour mettre à jour updated_at automatiquement
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_friendships_updated_at
BEFORE UPDATE ON friendships
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- ========================================
-- TABLE: matches
-- Description: Historique des parties
-- ========================================
CREATE TABLE matches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_type VARCHAR(20) NOT NULL CHECK (match_type IN ('ranked', 'normal', 'custom')),
    status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'in_progress', 'completed', 'cancelled')),
    winner_team INTEGER CHECK (winner_team IN (1, 2)),
    duration_seconds INTEGER CHECK (duration_seconds >= 0),
    started_at TIMESTAMP,
    ended_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW() NOT NULL
);

-- Index pour l'historique
CREATE INDEX idx_matches_created_at ON matches(created_at DESC);
CREATE INDEX idx_matches_status ON matches(status);
CREATE INDEX idx_matches_type ON matches(match_type);

-- ========================================
-- TABLE: match_participants
-- Description: Joueurs dans une partie
-- ========================================
CREATE TABLE match_participants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_id UUID NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    team INTEGER NOT NULL CHECK (team IN (1, 2)),
    champion_id INTEGER,
    kills INTEGER DEFAULT 0 CHECK (kills >= 0),
    deaths INTEGER DEFAULT 0 CHECK (deaths >= 0),
    assists INTEGER DEFAULT 0 CHECK (assists >= 0),
    gold_earned INTEGER DEFAULT 0 CHECK (gold_earned >= 0),
    damage_dealt INTEGER DEFAULT 0 CHECK (damage_dealt >= 0),
    CONSTRAINT unique_match_user UNIQUE (match_id, user_id)
);

-- Index pour les participants
CREATE INDEX idx_participants_match ON match_participants(match_id);
CREATE INDEX idx_participants_user ON match_participants(user_id);
CREATE INDEX idx_participants_team ON match_participants(match_id, team);

-- ========================================
-- TABLE: user_stats (statistiques agrégées)
-- Description: Cache des statistiques joueur
-- ========================================
CREATE TABLE user_stats (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    total_matches INTEGER DEFAULT 0 CHECK (total_matches >= 0),
    wins INTEGER DEFAULT 0 CHECK (wins >= 0),
    losses INTEGER DEFAULT 0 CHECK (losses >= 0),
    total_kills INTEGER DEFAULT 0 CHECK (total_kills >= 0),
    total_deaths INTEGER DEFAULT 0 CHECK (total_deaths >= 0),
    total_assists INTEGER DEFAULT 0 CHECK (total_assists >= 0),
    win_rate DECIMAL(5,2) DEFAULT 0.0 CHECK (win_rate >= 0 AND win_rate <= 100),
    kda_ratio DECIMAL(5,2) DEFAULT 0.0 CHECK (kda_ratio >= 0),
    updated_at TIMESTAMP DEFAULT NOW() NOT NULL
);

-- ========================================
-- VUES (Views)
-- ========================================

-- Vue pour obtenir facilement la liste d'amis
CREATE OR REPLACE VIEW friends_list AS
SELECT 
    CASE 
        WHEN f.requester_id = u1.id THEN u2.id
        ELSE u1.id
    END AS user_id,
    CASE 
        WHEN f.requester_id = u1.id THEN u2.username
        ELSE u1.username
    END AS friend_username,
    CASE 
        WHEN f.requester_id = u1.id THEN u2.avatar_url
        ELSE u1.avatar_url
    END AS friend_avatar,
    CASE 
        WHEN f.requester_id = u1.id THEN u2.status
        ELSE u1.status
    END AS friend_status,
    CASE 
        WHEN f.requester_id = u1.id THEN u2.level
        ELSE u1.level
    END AS friend_level,
    f.created_at AS friends_since
FROM friendships f
JOIN users u1 ON f.requester_id = u1.id
JOIN users u2 ON f.receiver_id = u2.id
WHERE f.status = 'accepted';

-- Vue pour le profil utilisateur complet
CREATE OR REPLACE VIEW user_profiles AS
SELECT 
    u.id,
    u.email,
    u.username,
    u.avatar_url,
    u.level,
    u.elo,
    u.status,
    u.created_at,
    u.last_login,
    COALESCE(s.total_matches, 0) AS total_matches,
    COALESCE(s.wins, 0) AS wins,
    COALESCE(s.losses, 0) AS losses,
    COALESCE(s.win_rate, 0.0) AS win_rate,
    COALESCE(s.kda_ratio, 0.0) AS kda_ratio,
    (SELECT COUNT(*) FROM friendships 
     WHERE (requester_id = u.id OR receiver_id = u.id) 
     AND status = 'accepted') AS friends_count
FROM users u
LEFT JOIN user_stats s ON u.id = s.user_id;

-- ========================================
-- FONCTIONS UTILITAIRES
-- ========================================

-- Fonction pour calculer le KDA ratio
CREATE OR REPLACE FUNCTION calculate_kda(kills INT, deaths INT, assists INT)
RETURNS DECIMAL(5,2) AS $$
BEGIN
    IF deaths = 0 THEN
        RETURN (kills + assists)::DECIMAL(5,2);
    ELSE
        RETURN ((kills + assists)::DECIMAL / deaths::DECIMAL)::DECIMAL(5,2);
    END IF;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Fonction pour mettre à jour les statistiques utilisateur
CREATE OR REPLACE FUNCTION update_user_stats(p_user_id UUID)
RETURNS VOID AS $$
DECLARE
    v_total_matches INT;
    v_wins INT;
    v_losses INT;
    v_total_kills INT;
    v_total_deaths INT;
    v_total_assists INT;
    v_win_rate DECIMAL(5,2);
    v_kda_ratio DECIMAL(5,2);
BEGIN
    -- Calculer les statistiques
    SELECT 
        COUNT(DISTINCT mp.match_id),
        COUNT(DISTINCT CASE WHEN m.winner_team = mp.team THEN mp.match_id END),
        COUNT(DISTINCT CASE WHEN m.winner_team != mp.team THEN mp.match_id END),
        COALESCE(SUM(mp.kills), 0),
        COALESCE(SUM(mp.deaths), 0),
        COALESCE(SUM(mp.assists), 0)
    INTO 
        v_total_matches,
        v_wins,
        v_losses,
        v_total_kills,
        v_total_deaths,
        v_total_assists
    FROM match_participants mp
    JOIN matches m ON mp.match_id = m.id
    WHERE mp.user_id = p_user_id
      AND m.status = 'completed';

    -- Calculer win rate
    IF v_total_matches > 0 THEN
        v_win_rate := (v_wins::DECIMAL / v_total_matches::DECIMAL * 100)::DECIMAL(5,2);
    ELSE
        v_win_rate := 0.0;
    END IF;

    -- Calculer KDA ratio
    v_kda_ratio := calculate_kda(v_total_kills, v_total_deaths, v_total_assists);

    -- Insérer ou mettre à jour
    INSERT INTO user_stats (
        user_id, total_matches, wins, losses,
        total_kills, total_deaths, total_assists,
        win_rate, kda_ratio, updated_at
    ) VALUES (
        p_user_id, v_total_matches, v_wins, v_losses,
        v_total_kills, v_total_deaths, v_total_assists,
        v_win_rate, v_kda_ratio, NOW()
    )
    ON CONFLICT (user_id) DO UPDATE SET
        total_matches = EXCLUDED.total_matches,
        wins = EXCLUDED.wins,
        losses = EXCLUDED.losses,
        total_kills = EXCLUDED.total_kills,
        total_deaths = EXCLUDED.total_deaths,
        total_assists = EXCLUDED.total_assists,
        win_rate = EXCLUDED.win_rate,
        kda_ratio = EXCLUDED.kda_ratio,
        updated_at = NOW();
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- TRIGGERS
-- ========================================

-- Trigger pour mettre à jour les stats après une partie
CREATE OR REPLACE FUNCTION trigger_update_stats_after_match()
RETURNS TRIGGER AS $$
BEGIN
    -- Si le match vient d'être complété
    IF NEW.status = 'completed' AND OLD.status != 'completed' THEN
        -- Mettre à jour les stats de tous les participants
        PERFORM update_user_stats(user_id)
        FROM match_participants
        WHERE match_id = NEW.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_stats_on_match_complete
AFTER UPDATE ON matches
FOR EACH ROW
EXECUTE FUNCTION trigger_update_stats_after_match();

-- ========================================
-- DONNÉES DE TEST (Optionnel)
-- ========================================

-- Insérer un utilisateur de test (mot de passe: password123)
-- Hash bcrypt de "password123" avec coût 12
INSERT INTO users (email, username, password_hash, level, elo, status)
VALUES (
    'test@example.com',
    'testuser',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYfQqPh6Jm2',
    5,
    1250,
    'offline'
) ON CONFLICT (email) DO NOTHING;

-- ========================================
-- COMMENTAIRES
-- ========================================

COMMENT ON TABLE users IS 'Comptes utilisateurs du jeu';
COMMENT ON TABLE friendships IS 'Relations d''amitié entre utilisateurs';
COMMENT ON TABLE matches IS 'Historique des parties jouées';
COMMENT ON TABLE match_participants IS 'Joueurs ayant participé à une partie';
COMMENT ON TABLE user_stats IS 'Statistiques agrégées des joueurs (cache)';

COMMENT ON COLUMN users.password_hash IS 'Hash bcrypt du mot de passe (coût 12)';
COMMENT ON COLUMN users.status IS 'Statut actuel: offline, online, in-game';
COMMENT ON COLUMN friendships.status IS 'État de l''amitié: pending, accepted, declined, blocked';
COMMENT ON COLUMN matches.winner_team IS 'Équipe gagnante (1 ou 2), NULL si partie non terminée';

-- ========================================
-- PERMISSIONS (à ajuster selon vos besoins)
-- ========================================

-- Donner les permissions à nexa_user
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO nexa_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO nexa_user;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO nexa_user;

-- ========================================
-- FIN DU SCHÉMA
-- ========================================

-- Afficher un message de succès
DO $$
BEGIN
    RAISE NOTICE 'Schéma NexA créé avec succès!';
    RAISE NOTICE 'Tables: users, friendships, matches, match_participants, user_stats';
    RAISE NOTICE 'Vues: friends_list, user_profiles';
    RAISE NOTICE 'Utilisateur de test créé: test@example.com / password123';
END $$;
