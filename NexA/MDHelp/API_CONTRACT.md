# 🌐 API Contract - NexA Backend

Base URL: `https://api.nexa.game/v1`

## 📐 Format de Réponse Standard

### Success (2xx)
```json
{
  "success": true,
  "data": { /* payload */ },
  "meta": { /* pagination, etc */ }
}
```

### Error (4xx/5xx)
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": { /* optional */ }
  }
}
```

---

## 🔐 Authentication

### POST /auth/register
**Description**: Créer un nouveau compte

**Body**:
```json
{
  "username": "PlayerOne",
  "email": "player@example.com",
  "password": "SecurePass123!"
}
```

**Response 201**:
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "uuid-v4",
      "username": "PlayerOne",
      "email": "player@example.com",
      "createdAt": "2026-01-04T10:00:00Z"
    },
    "tokens": {
      "accessToken": "eyJhbGc...",
      "refreshToken": "eyJhbGc...",
      "expiresIn": 3600
    }
  }
}
```

**Errors**:
- `400 USERNAME_TAKEN` - Username already exists
- `400 EMAIL_TAKEN` - Email already exists
- `400 INVALID_PASSWORD` - Password too weak (min 8 chars, 1 uppercase, 1 number)
- `400 VALIDATION_ERROR` - Invalid input format

---

### POST /auth/login
**Description**: Se connecter

**Body**:
```json
{
  "email": "player@example.com",
  "password": "SecurePass123!"
}
```

**Response 200**:
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "uuid-v4",
      "username": "PlayerOne",
      "email": "player@example.com",
      "avatar": "https://cdn.nexa.game/avatars/default.png",
      "level": 15,
      "elo": 1250
    },
    "tokens": {
      "accessToken": "eyJhbGc...",
      "refreshToken": "eyJhbGc...",
      "expiresIn": 3600
    }
  }
}
```

**Errors**:
- `401 INVALID_CREDENTIALS` - Email or password incorrect
- `403 ACCOUNT_BANNED` - Account is banned

---

### POST /auth/refresh
**Description**: Renouveler l'access token

**Body**:
```json
{
  "refreshToken": "eyJhbGc..."
}
```

**Response 200**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "expiresIn": 3600
  }
}
```

**Errors**:
- `401 INVALID_TOKEN` - Refresh token invalid or expired
- `401 TOKEN_REVOKED` - Token has been revoked

---

### POST /auth/logout
**Description**: Se déconnecter (révoque le refresh token)

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "message": "Logged out successfully"
  }
}
```

---

## 👤 User Profile

### GET /me
**Description**: Obtenir le profil de l'utilisateur connecté

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "id": "uuid-v4",
    "username": "PlayerOne",
    "email": "player@example.com",
    "avatar": "https://cdn.nexa.game/avatars/abc123.png",
    "level": 15,
    "elo": 1250,
    "stats": {
      "totalMatches": 142,
      "wins": 78,
      "losses": 64,
      "winRate": 54.93
    },
    "status": "online",
    "createdAt": "2025-06-15T10:00:00Z",
    "lastSeenAt": "2026-01-04T15:30:00Z"
  }
}
```

**Errors**:
- `401 UNAUTHORIZED` - Token missing or invalid

---

### GET /users/:userId
**Description**: Obtenir le profil public d'un autre joueur

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "id": "uuid-v4",
    "username": "PlayerTwo",
    "avatar": "https://cdn.nexa.game/avatars/def456.png",
    "level": 22,
    "elo": 1580,
    "stats": {
      "totalMatches": 256,
      "wins": 145,
      "losses": 111,
      "winRate": 56.64
    },
    "status": "in-game",
    "isFriend": true
  }
}
```

**Errors**:
- `404 USER_NOT_FOUND` - User does not exist

---

### GET /users/search
**Description**: Rechercher des utilisateurs par username

**Headers**: `Authorization: Bearer {accessToken}`

**Query Params**:
- `q` (required): Search term (min 3 chars)
- `limit` (optional): Max results (default 20, max 50)

**Example**: `/users/search?q=Play&limit=10`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "users": [
      {
        "id": "uuid-1",
        "username": "PlayerOne",
        "avatar": "https://cdn.nexa.game/avatars/abc.png",
        "level": 15,
        "status": "online",
        "isFriend": false,
        "hasPendingRequest": false
      },
      {
        "id": "uuid-2",
        "username": "PlayerTwo",
        "avatar": "https://cdn.nexa.game/avatars/def.png",
        "level": 22,
        "status": "offline",
        "isFriend": true,
        "hasPendingRequest": false
      }
    ]
  },
  "meta": {
    "total": 2
  }
}
```

**Errors**:
- `400 QUERY_TOO_SHORT` - Query must be at least 3 characters

---

## 👥 Friends

### GET /friends
**Description**: Liste de tous mes amis

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "friends": [
      {
        "id": "uuid-1",
        "username": "BestFriend",
        "avatar": "https://cdn.nexa.game/avatars/abc.png",
        "level": 18,
        "elo": 1400,
        "status": "online",
        "friendsSince": "2025-08-10T12:00:00Z"
      },
      {
        "id": "uuid-2",
        "username": "OldFriend",
        "avatar": "https://cdn.nexa.game/avatars/def.png",
        "level": 10,
        "elo": 1100,
        "status": "offline",
        "lastSeenAt": "2026-01-03T18:00:00Z",
        "friendsSince": "2025-09-20T15:00:00Z"
      }
    ]
  },
  "meta": {
    "total": 2
  }
}
```

---

### GET /friends/requests
**Description**: Liste des demandes d'amis (envoyées + reçues)

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "incoming": [
      {
        "id": "request-uuid-1",
        "from": {
          "id": "user-uuid-1",
          "username": "NewPlayer",
          "avatar": "https://cdn.nexa.game/avatars/new.png",
          "level": 5
        },
        "createdAt": "2026-01-04T10:00:00Z"
      }
    ],
    "outgoing": [
      {
        "id": "request-uuid-2",
        "to": {
          "id": "user-uuid-2",
          "username": "CoolPlayer",
          "avatar": "https://cdn.nexa.game/avatars/cool.png",
          "level": 20
        },
        "createdAt": "2026-01-03T15:00:00Z"
      }
    ]
  }
}
```

---

### POST /friends/request
**Description**: Envoyer une demande d'ami

**Headers**: `Authorization: Bearer {accessToken}`

**Body**:
```json
{
  "userId": "uuid-target-user"
}
```

**Response 201**:
```json
{
  "success": true,
  "data": {
    "requestId": "request-uuid",
    "to": {
      "id": "uuid-target-user",
      "username": "TargetPlayer",
      "avatar": "https://cdn.nexa.game/avatars/target.png"
    },
    "createdAt": "2026-01-04T15:45:00Z"
  }
}
```

**Errors**:
- `404 USER_NOT_FOUND` - Target user does not exist
- `400 ALREADY_FRIENDS` - Already friends with this user
- `400 REQUEST_ALREADY_SENT` - Request already pending
- `400 CANNOT_ADD_SELF` - Cannot add yourself as friend

---

### POST /friends/accept
**Description**: Accepter une demande d'ami

**Headers**: `Authorization: Bearer {accessToken}`

**Body**:
```json
{
  "requestId": "request-uuid"
}
```

**Response 200**:
```json
{
  "success": true,
  "data": {
    "friendship": {
      "id": "friendship-uuid",
      "friend": {
        "id": "user-uuid",
        "username": "NewFriend",
        "avatar": "https://cdn.nexa.game/avatars/new.png",
        "level": 5,
        "status": "online"
      },
      "createdAt": "2026-01-04T15:50:00Z"
    }
  }
}
```

**Errors**:
- `404 REQUEST_NOT_FOUND` - Request does not exist or not addressed to you
- `400 ALREADY_ACCEPTED` - Request already accepted

---

### POST /friends/decline
**Description**: Refuser une demande d'ami

**Headers**: `Authorization: Bearer {accessToken}`

**Body**:
```json
{
  "requestId": "request-uuid"
}
```

**Response 200**:
```json
{
  "success": true,
  "data": {
    "message": "Friend request declined"
  }
}
```

**Errors**:
- `404 REQUEST_NOT_FOUND` - Request does not exist

---

### DELETE /friends/:userId
**Description**: Supprimer un ami

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "message": "Friend removed successfully"
  }
}
```

**Errors**:
- `404 FRIENDSHIP_NOT_FOUND` - Not friends with this user

---

## 🎮 Matches

### GET /matches
**Description**: Historique de mes parties (cursor-based pagination)

**Headers**: `Authorization: Bearer {accessToken}`

**Query Params**:
- `limit` (optional): Number of results (default 20, max 50)
- `cursor` (optional): Pagination cursor (from previous response)

**Example**: `/matches?limit=20&cursor=eyJpZCI6MTIzNH0=`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "matches": [
      {
        "id": "match-uuid-1",
        "mode": "ranked",
        "result": "victory",
        "duration": 1847,
        "playerCount": 8,
        "myScore": 2450,
        "myKills": 12,
        "myDeaths": 5,
        "createdAt": "2026-01-04T14:00:00Z"
      },
      {
        "id": "match-uuid-2",
        "mode": "casual",
        "result": "defeat",
        "duration": 1245,
        "playerCount": 6,
        "myScore": 1800,
        "myKills": 8,
        "myDeaths": 9,
        "createdAt": "2026-01-03T19:00:00Z"
      }
    ]
  },
  "meta": {
    "nextCursor": "eyJpZCI6MTIzNH0=",
    "hasMore": true
  }
}
```

---

### GET /matches/:matchId
**Description**: Détails complets d'une partie

**Headers**: `Authorization: Bearer {accessToken}`

**Response 200**:
```json
{
  "success": true,
  "data": {
    "id": "match-uuid-1",
    "mode": "ranked",
    "result": "victory",
    "duration": 1847,
    "map": "Arena Alpha",
    "createdAt": "2026-01-04T14:00:00Z",
    "participants": [
      {
        "userId": "my-uuid",
        "username": "PlayerOne",
        "avatar": "https://cdn.nexa.game/avatars/me.png",
        "team": "blue",
        "score": 2450,
        "kills": 12,
        "deaths": 5,
        "assists": 8,
        "isMe": true
      },
      {
        "userId": "other-uuid-1",
        "username": "Teammate",
        "avatar": "https://cdn.nexa.game/avatars/tm.png",
        "team": "blue",
        "score": 2100,
        "kills": 10,
        "deaths": 6,
        "assists": 12,
        "isMe": false
      },
      {
        "userId": "other-uuid-2",
        "username": "Enemy",
        "avatar": "https://cdn.nexa.game/avatars/en.png",
        "team": "red",
        "score": 1950,
        "kills": 8,
        "deaths": 11,
        "assists": 5,
        "isMe": false
      }
    ]
  }
}
```

**Errors**:
- `404 MATCH_NOT_FOUND` - Match does not exist
- `403 FORBIDDEN` - You were not a participant in this match

---

## 🔧 Codes d'Erreur Communs

| Code | HTTP | Description |
|------|------|-------------|
| `UNAUTHORIZED` | 401 | Token missing, invalid, or expired |
| `FORBIDDEN` | 403 | Action not allowed (banned, insufficient permissions) |
| `NOT_FOUND` | 404 | Resource does not exist |
| `VALIDATION_ERROR` | 400 | Input validation failed |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Server error |
| `SERVICE_UNAVAILABLE` | 503 | Service temporarily down |

---

## 🔒 Authentication Flow

1. **Register/Login** → Receive `accessToken` (1h) + `refreshToken` (7d)
2. **Store tokens** securely in Unity (PlayerPrefs encrypted or in-memory)
3. **Every request** → Header: `Authorization: Bearer {accessToken}`
4. **If 401 UNAUTHORIZED** → Call `/auth/refresh` with refreshToken
5. **If refresh fails** → Redirect to login screen
6. **On logout** → Call `/auth/logout` + clear tokens

---

## 📊 Rate Limits (MVP)

| Endpoint | Limit |
|----------|-------|
| `/auth/register` | 5/hour per IP |
| `/auth/login` | 10/minute per IP |
| `/auth/refresh` | 20/minute per user |
| All other endpoints | 100/minute per user |

Headers retournés:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1641040800
```


