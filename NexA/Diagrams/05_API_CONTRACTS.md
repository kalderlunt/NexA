# Contrats API REST

## Vue d'ensemble

Ce document définit tous les endpoints de l'API avec payloads, réponses, codes d'erreur, et exemples.

---

## Diagramme 29: API Endpoints Map

**Type de diagramme souhaité**: API Map / Endpoint Tree

**Description**:
Vue d'ensemble de tous les endpoints organisés par resource.

**Structure**:

```
/api
├── /auth
│   ├── POST   /register          (public)
│   ├── POST   /login             (public)
│   └── POST   /refresh           (public)
│
├── /users
│   ├── GET    /me                (authenticated)
│   ├── GET    /search            (authenticated)
│   └── GET    /:id               (authenticated)
│
├── /friends
│   ├── GET    /                  (authenticated) - Liste amis
│   ├── GET    /requests          (authenticated) - Demandes pending
│   ├── POST   /request           (authenticated) - Envoyer demande
│   ├── POST   /accept/:id        (authenticated) - Accepter demande
│   ├── POST   /decline/:id       (authenticated) - Refuser demande
│   └── DELETE /:id               (authenticated) - Supprimer ami
│
├── /matches
│   ├── GET    /                  (authenticated) - Historique
│   └── GET    /:id               (authenticated) - Détails match
│
└── /health
    └── GET    /                  (public) - Health check
```

**Statistiques**:
- Total endpoints: 14
- Public: 4
- Authenticated: 10
- Methods: GET (8), POST (5), DELETE (1)

---

## Diagramme 30: API Contract - Auth Module

**Type de diagramme souhaité**: API Documentation

### Endpoint: POST /auth/register

**Description**: Création d'un nouveau compte utilisateur

**Request**:
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "JohnDoe",
  "password": "SecurePass123!"
}
```

**Request DTO**:
```typescript
class RegisterDto {
  @IsEmail()
  email: string;

  @Length(3, 50)
  @Matches(/^[a-zA-Z0-9_-]+$/)
  username: string;

  @Length(8, 100)
  @Matches(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/)
  password: string;
}
```

**Response Success (201 Created)**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "username": "JohnDoe",
      "avatarUrl": null,
      "level": 1,
      "elo": 1000,
      "createdAt": "2026-01-11T22:00:00.000Z"
    }
  },
  "meta": {
    "timestamp": "2026-01-11T22:00:00.000Z",
    "requestId": "req-abc123"
  }
}
```

**Response Errors**:

**409 Conflict** - Email déjà utilisé
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_ALREADY_EXISTS",
    "message": "An account with this email already exists",
    "details": {
      "field": "email",
      "value": "user@example.com"
    },
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/register",
    "requestId": "req-abc123"
  }
}
```

**409 Conflict** - Username déjà utilisé
```json
{
  "success": false,
  "error": {
    "code": "USERNAME_ALREADY_EXISTS",
    "message": "This username is already taken",
    "details": {
      "field": "username",
      "value": "JohnDoe"
    },
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/register",
    "requestId": "req-abc123"
  }
}
```

**400 Bad Request** - Validation error
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {
      "errors": [
        {
          "field": "password",
          "message": "Password must contain at least one uppercase letter, one lowercase letter, and one number",
          "value": "weak"
        }
      ]
    },
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/register",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: POST /auth/login

**Description**: Authentification utilisateur

**Request**:
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "username": "JohnDoe",
      "avatarUrl": "https://cdn.nexa.com/avatars/johndoe.png",
      "level": 15,
      "elo": 1450,
      "status": "online"
    }
  }
}
```

**Response Errors**:

**401 Unauthorized** - Credentials invalides
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/login",
    "requestId": "req-abc123"
  }
}
```

**429 Too Many Requests** - Rate limit dépassé
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many login attempts. Please try again in 60 seconds.",
    "details": {
      "retryAfter": 60
    },
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/login",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: POST /auth/refresh

**Description**: Renouveler l'access token avec un refresh token

**Request**:
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." // rotated
  }
}
```

**Response Errors**:

**401 Unauthorized** - Refresh token invalide ou expiré
```json
{
  "success": false,
  "error": {
    "code": "INVALID_REFRESH_TOKEN",
    "message": "Invalid or expired refresh token",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/auth/refresh",
    "requestId": "req-abc123"
  }
}
```

---

## Diagramme 31: API Contract - Users Module

**Type de diagramme souhaité**: API Documentation

### Endpoint: GET /users/me

**Description**: Récupérer le profil de l'utilisateur connecté

**Request**:
```http
GET /api/users/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "username": "JohnDoe",
    "avatarUrl": "https://cdn.nexa.com/avatars/johndoe.png",
    "level": 15,
    "elo": 1450,
    "status": "online",
    "stats": {
      "totalMatches": 245,
      "wins": 132,
      "losses": 113,
      "winRate": 0.538,
      "totalKills": 1823,
      "totalDeaths": 1654,
      "kda": 1.102
    },
    "createdAt": "2025-06-15T10:30:00.000Z",
    "lastLogin": "2026-01-11T21:45:00.000Z"
  }
}
```

**Response Errors**:

**401 Unauthorized** - Token manquant ou invalide
```json
{
  "success": false,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Missing or invalid authentication token",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/users/me",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: GET /users/search

**Description**: Rechercher des utilisateurs par username

**Request**:
```http
GET /api/users/search?q=John&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `q` (string, required): Requête de recherche (min 2 caractères)
- `limit` (integer, optional): Nombre max de résultats (default: 10, max: 50)

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "users": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "username": "JohnDoe",
        "avatarUrl": "https://cdn.nexa.com/avatars/johndoe.png",
        "level": 15,
        "status": "online"
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "username": "JohnnyBoy",
        "avatarUrl": null,
        "level": 8,
        "status": "offline"
      }
    ],
    "total": 2
  }
}
```

**Response Errors**:

**400 Bad Request** - Query trop courte
```json
{
  "success": false,
  "error": {
    "code": "INVALID_SEARCH_QUERY",
    "message": "Search query must be at least 2 characters long",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/users/search",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: GET /users/:id

**Description**: Récupérer le profil public d'un utilisateur

**Request**:
```http
GET /api/users/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "JohnDoe",
    "avatarUrl": "https://cdn.nexa.com/avatars/johndoe.png",
    "level": 15,
    "elo": 1450,
    "status": "online",
    "stats": {
      "totalMatches": 245,
      "wins": 132,
      "losses": 113,
      "winRate": 0.538
    },
    "isFriend": true,
    "createdAt": "2025-06-15T10:30:00.000Z"
  }
}
```

**Response Errors**:

**404 Not Found** - Utilisateur inexistant
```json
{
  "success": false,
  "error": {
    "code": "USER_NOT_FOUND",
    "message": "User with ID 550e8400-e29b-41d4-a716-446655440000 not found",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/users/550e8400-e29b-41d4-a716-446655440000",
    "requestId": "req-abc123"
  }
}
```

---

## Diagramme 32: API Contract - Friends Module

**Type de diagramme souhaité**: API Documentation

### Endpoint: GET /friends

**Description**: Récupérer la liste d'amis de l'utilisateur connecté

**Request**:
```http
GET /api/friends
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "friends": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "username": "JohnDoe",
        "avatarUrl": "https://cdn.nexa.com/avatars/johndoe.png",
        "level": 15,
        "status": "online",
        "lastSeen": "2026-01-11T22:00:00.000Z",
        "friendsSince": "2025-07-20T14:30:00.000Z"
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "username": "JaneDoe",
        "avatarUrl": null,
        "level": 22,
        "status": "in-game",
        "currentMatch": {
          "matchId": "match-uuid-123",
          "matchType": "ranked"
        },
        "lastSeen": "2026-01-11T21:55:00.000Z",
        "friendsSince": "2025-08-10T10:00:00.000Z"
      },
      {
        "id": "770e8400-e29b-41d4-a716-446655440002",
        "username": "BobSmith",
        "avatarUrl": "https://cdn.nexa.com/avatars/bobsmith.png",
        "level": 8,
        "status": "offline",
        "lastSeen": "2026-01-10T18:30:00.000Z",
        "friendsSince": "2025-12-01T09:15:00.000Z"
      }
    ],
    "total": 3,
    "online": 1,
    "inGame": 1,
    "offline": 1
  }
}
```

---

### Endpoint: GET /friends/requests

**Description**: Récupérer les demandes d'ami en attente

**Request**:
```http
GET /api/friends/requests
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "received": [
      {
        "requestId": "req-uuid-abc",
        "from": {
          "id": "880e8400-e29b-41d4-a716-446655440003",
          "username": "AliceWonder",
          "avatarUrl": null,
          "level": 12
        },
        "createdAt": "2026-01-11T20:00:00.000Z"
      }
    ],
    "sent": [
      {
        "requestId": "req-uuid-def",
        "to": {
          "id": "990e8400-e29b-41d4-a716-446655440004",
          "username": "CharlieB",
          "avatarUrl": "https://cdn.nexa.com/avatars/charlieb.png",
          "level": 18
        },
        "createdAt": "2026-01-11T19:30:00.000Z"
      }
    ],
    "totalReceived": 1,
    "totalSent": 1
  }
}
```

---

### Endpoint: POST /friends/request

**Description**: Envoyer une demande d'ami

**Request**:
```http
POST /api/friends/request
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "targetUserId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response Success (201 Created)**:
```json
{
  "success": true,
  "data": {
    "requestId": "req-uuid-xyz",
    "targetUser": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "JohnDoe"
    },
    "status": "pending",
    "createdAt": "2026-01-11T22:00:00.000Z"
  }
}
```

**Response Errors**:

**404 Not Found** - Utilisateur cible inexistant
```json
{
  "success": false,
  "error": {
    "code": "USER_NOT_FOUND",
    "message": "Target user not found",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/request",
    "requestId": "req-abc123"
  }
}
```

**409 Conflict** - Déjà amis
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_FRIENDS",
    "message": "You are already friends with this user",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/request",
    "requestId": "req-abc123"
  }
}
```

**409 Conflict** - Demande déjà envoyée
```json
{
  "success": false,
  "error": {
    "code": "REQUEST_ALREADY_SENT",
    "message": "Friend request already sent to this user",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/request",
    "requestId": "req-abc123"
  }
}
```

**400 Bad Request** - Tentative de s'ajouter soi-même
```json
{
  "success": false,
  "error": {
    "code": "CANNOT_ADD_SELF",
    "message": "You cannot send a friend request to yourself",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/request",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: POST /friends/accept/:id

**Description**: Accepter une demande d'ami

**Request**:
```http
POST /api/friends/accept/req-uuid-abc
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "friendship": {
      "id": "friendship-uuid-123",
      "friend": {
        "id": "880e8400-e29b-41d4-a716-446655440003",
        "username": "AliceWonder",
        "avatarUrl": null,
        "level": 12,
        "status": "online"
      },
      "friendsSince": "2026-01-11T22:00:00.000Z"
    }
  }
}
```

**Response Errors**:

**404 Not Found** - Demande inexistante
```json
{
  "success": false,
  "error": {
    "code": "REQUEST_NOT_FOUND",
    "message": "Friend request not found",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/accept/req-uuid-abc",
    "requestId": "req-abc123"
  }
}
```

**403 Forbidden** - Pas le destinataire de la demande
```json
{
  "success": false,
  "error": {
    "code": "NOT_REQUEST_RECEIVER",
    "message": "You are not authorized to accept this friend request",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/accept/req-uuid-abc",
    "requestId": "req-abc123"
  }
}
```

---

### Endpoint: POST /friends/decline/:id

**Description**: Refuser une demande d'ami

**Request**:
```http
POST /api/friends/decline/req-uuid-abc
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "message": "Friend request declined"
  }
}
```

---

### Endpoint: DELETE /friends/:id

**Description**: Supprimer un ami

**Request**:
```http
DELETE /api/friends/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "message": "Friend removed successfully"
  }
}
```

**Response Errors**:

**404 Not Found** - Amitié inexistante
```json
{
  "success": false,
  "error": {
    "code": "FRIENDSHIP_NOT_FOUND",
    "message": "Friendship not found",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/friends/550e8400-e29b-41d4-a716-446655440000",
    "requestId": "req-abc123"
  }
}
```

---

## Diagramme 33: API Contract - Matches Module

**Type de diagramme souhaité**: API Documentation

### Endpoint: GET /matches

**Description**: Récupérer l'historique de parties avec pagination cursor

**Request**:
```http
GET /api/matches?limit=20&cursor=2026-01-10T18:00:00.000Z
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `limit` (integer, optional): Nombre de résultats (default: 20, max: 50)
- `cursor` (ISO timestamp, optional): Cursor pour pagination (created_at du dernier match de la page précédente)

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "matches": [
      {
        "id": "match-uuid-001",
        "matchType": "ranked",
        "status": "completed",
        "winnerTeam": 1,
        "durationSeconds": 1823,
        "startedAt": "2026-01-11T21:00:00.000Z",
        "endedAt": "2026-01-11T21:30:23.000Z",
        "playerTeam": 1,
        "playerStats": {
          "kills": 10,
          "deaths": 3,
          "assists": 8,
          "isVictory": true,
          "isMvp": false
        },
        "participantsCount": 10
      },
      {
        "id": "match-uuid-002",
        "matchType": "normal",
        "status": "completed",
        "winnerTeam": 2,
        "durationSeconds": 2145,
        "startedAt": "2026-01-11T19:30:00.000Z",
        "endedAt": "2026-01-11T20:05:45.000Z",
        "playerTeam": 1,
        "playerStats": {
          "kills": 5,
          "deaths": 7,
          "assists": 12,
          "isVictory": false,
          "isMvp": false
        },
        "participantsCount": 10
      }
    ],
    "pagination": {
      "limit": 20,
      "hasMore": true,
      "nextCursor": "2026-01-11T19:30:00.000Z"
    },
    "total": 245
  }
}
```

---

### Endpoint: GET /matches/:id

**Description**: Récupérer les détails complets d'un match

**Request**:
```http
GET /api/matches/match-uuid-001
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Success (200 OK)**:
```json
{
  "success": true,
  "data": {
    "match": {
      "id": "match-uuid-001",
      "matchType": "ranked",
      "status": "completed",
      "winnerTeam": 1,
      "durationSeconds": 1823,
      "startedAt": "2026-01-11T21:00:00.000Z",
      "endedAt": "2026-01-11T21:30:23.000Z",
      "teams": {
        "team1": {
          "isWinner": true,
          "totalKills": 35,
          "totalGold": 58400,
          "players": [
            {
              "userId": "user-uuid-me",
              "username": "MyUsername",
              "avatarUrl": "https://cdn.nexa.com/avatars/me.png",
              "championId": 12,
              "kills": 10,
              "deaths": 3,
              "assists": 8,
              "goldEarned": 14200,
              "damageDealt": 42350,
              "isMvp": true
            },
            {
              "userId": "user-uuid-1",
              "username": "Teammate1",
              "avatarUrl": null,
              "championId": 5,
              "kills": 8,
              "deaths": 4,
              "assists": 15,
              "goldEarned": 11800,
              "damageDealt": 28450,
              "isMvp": false
            }
            // ... 3 autres joueurs
          ]
        },
        "team2": {
          "isWinner": false,
          "totalKills": 22,
          "totalGold": 48200,
          "players": [
            // ... 5 joueurs
          ]
        }
      }
    }
  }
}
```

**Response Errors**:

**404 Not Found** - Match inexistant
```json
{
  "success": false,
  "error": {
    "code": "MATCH_NOT_FOUND",
    "message": "Match not found",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/matches/match-uuid-001",
    "requestId": "req-abc123"
  }
}
```

**403 Forbidden** - Utilisateur n'était pas dans le match (futur - privacy)
```json
{
  "success": false,
  "error": {
    "code": "MATCH_ACCESS_DENIED",
    "message": "You don't have access to this match",
    "timestamp": "2026-01-11T22:00:00.000Z",
    "path": "/api/matches/match-uuid-001",
    "requestId": "req-abc123"
  }
}
```

---

## Diagramme 34: Codes d'Erreur Complets

**Type de diagramme souhaité**: Error Code Matrix

**Description**:
Référence complète de tous les codes d'erreur possibles.

| HTTP Status | Error Code | Message | Contexte |
|-------------|------------|---------|----------|
| **400 Bad Request** |
| 400 | VALIDATION_ERROR | Validation failed | Champs invalides dans DTO |
| 400 | INVALID_INPUT | Invalid input data | Données non conformes |
| 400 | CANNOT_ADD_SELF | Cannot add yourself as friend | Friend request à soi-même |
| 400 | INVALID_SEARCH_QUERY | Search query too short | Query < 2 caractères |
| **401 Unauthorized** |
| 401 | UNAUTHORIZED | Missing or invalid token | Token JWT absent ou invalide |
| 401 | INVALID_CREDENTIALS | Invalid email or password | Login échoué |
| 401 | TOKEN_EXPIRED | Authentication token expired | Access token expiré |
| 401 | INVALID_REFRESH_TOKEN | Invalid or expired refresh token | Refresh token invalide |
| **403 Forbidden** |
| 403 | FORBIDDEN | Access denied | Pas les permissions |
| 403 | NOT_REQUEST_RECEIVER | Not authorized to accept | Pas le destinataire |
| 403 | MATCH_ACCESS_DENIED | No access to match | Pas participant au match |
| **404 Not Found** |
| 404 | USER_NOT_FOUND | User not found | Utilisateur inexistant |
| 404 | MATCH_NOT_FOUND | Match not found | Match inexistant |
| 404 | FRIENDSHIP_NOT_FOUND | Friendship not found | Amitié inexistante |
| 404 | REQUEST_NOT_FOUND | Friend request not found | Demande inexistante |
| 404 | RESOURCE_NOT_FOUND | Resource not found | Resource générique |
| **409 Conflict** |
| 409 | EMAIL_ALREADY_EXISTS | Email already registered | Email dupliqué |
| 409 | USERNAME_ALREADY_EXISTS | Username already taken | Username dupliqué |
| 409 | ALREADY_FRIENDS | Already friends | Déjà amis |
| 409 | REQUEST_ALREADY_SENT | Friend request already sent | Demande déjà envoyée |
| 409 | DUPLICATE_ENTRY | Duplicate entry | Entrée dupliquée générique |
| **429 Too Many Requests** |
| 429 | RATE_LIMIT_EXCEEDED | Too many requests | Rate limit dépassé |
| **500 Internal Server Error** |
| 500 | INTERNAL_SERVER_ERROR | Internal server error | Erreur serveur générique |
| 500 | DATABASE_ERROR | Database error | Erreur base de données |
| 500 | UNEXPECTED_ERROR | An unexpected error occurred | Erreur inattendue |
| **503 Service Unavailable** |
| 503 | SERVICE_UNAVAILABLE | Service temporarily unavailable | Service down |
| 503 | DATABASE_UNAVAILABLE | Database unavailable | DB inaccessible |

---

## Diagramme 35: Headers HTTP Standard

**Type de diagramme souhaité**: HTTP Headers Specification

**Description**:
Headers HTTP utilisés par l'API.

**Request Headers**:

| Header | Required | Description | Exemple |
|--------|----------|-------------|---------|
| Authorization | Auth routes | JWT Bearer token | `Bearer eyJhbGc...` |
| Content-Type | POST/PUT | Type de contenu | `application/json` |
| X-Request-ID | Optional | ID de requête (si absent, généré) | `req-uuid-abc123` |
| User-Agent | Recommended | Client info | `Unity/2021.3 (Windows)` |
| Accept-Language | Optional | Langue préférée (futur i18n) | `fr-FR` |

**Response Headers**:

| Header | Always | Description | Exemple |
|--------|--------|-------------|---------|
| Content-Type | Yes | Type de réponse | `application/json` |
| X-Request-ID | Yes | ID de corrélation | `req-uuid-abc123` |
| X-Response-Time | Yes | Temps traitement (ms) | `87` |
| X-RateLimit-Limit | Auth | Limite rate | `50` |
| X-RateLimit-Remaining | Auth | Requêtes restantes | `47` |
| X-RateLimit-Reset | Auth | Timestamp reset | `1673472000` |
| Retry-After | 429 | Secondes avant retry | `60` |

---

## Métadonnées pour génération

### Format de documentation
- Style: OpenAPI/Swagger
- Format payload: JSON
- Encoding: UTF-8

### Outils recommandés
- Swagger UI
- Postman Collection
- Insomnia
- Redoc

### Palette couleurs par méthode
- GET: #61AFFE (bleu)
- POST: #49CC90 (vert)
- DELETE: #F93E3E (rouge)
- PUT: #FCA130 (orange)


