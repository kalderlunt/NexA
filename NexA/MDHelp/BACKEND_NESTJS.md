# 🔧 Backend NestJS - Guide d'Implémentation

## 📦 Installation & Setup

```bash
# Créer le projet NestJS
npm i -g @nestjs/cli
nest new nexa-backend
cd nexa-backend

# Dépendances principales
npm install --save \
  @nestjs/config \
  @nestjs/jwt \
  @nestjs/passport \
  @nestjs/typeorm \
  typeorm \
  pg \
  passport \
  passport-jwt \
  passport-local \
  bcrypt \
  class-validator \
  class-transformer \
  @nestjs/throttler \
  @willsoto/nestjs-prometheus \
  prom-client \
  winston

# Dev dependencies
npm install --save-dev \
  @types/passport-jwt \
  @types/passport-local \
  @types/bcrypt \
  @types/node
```

---

## 📁 Structure du Projet

```
src/
├── main.ts                      # Entry point
├── app.module.ts                # Module racine
├── common/                      # Code partagé
│   ├── decorators/
│   │   ├── current-user.decorator.ts
│   │   └── public.decorator.ts
│   ├── filters/
│   │   └── http-exception.filter.ts
│   ├── guards/
│   │   └── jwt-auth.guard.ts
│   ├── interceptors/
│   │   ├── logging.interceptor.ts
│   │   ├── correlation-id.interceptor.ts
│   │   └── transform.interceptor.ts
│   ├── middleware/
│   │   └── correlation-id.middleware.ts
│   └── dto/
│       └── pagination.dto.ts
├── config/                      # Configuration
│   ├── database.config.ts
│   └── jwt.config.ts
├── modules/
│   ├── auth/                    # Module authentification
│   │   ├── auth.controller.ts
│   │   ├── auth.service.ts
│   │   ├── auth.module.ts
│   │   ├── strategies/
│   │   │   ├── jwt.strategy.ts
│   │   │   └── local.strategy.ts
│   │   └── dto/
│   │       ├── login.dto.ts
│   │       ├── register.dto.ts
│   │       └── refresh-token.dto.ts
│   ├── users/                   # Module utilisateurs
│   │   ├── users.controller.ts
│   │   ├── users.service.ts
│   │   ├── users.module.ts
│   │   ├── entities/
│   │   │   ├── user.entity.ts
│   │   │   └── refresh-token.entity.ts
│   │   └── dto/
│   │       └── search-users.dto.ts
│   ├── friends/                 # Module amis
│   │   ├── friends.controller.ts
│   │   ├── friends.service.ts
│   │   ├── friends.module.ts
│   │   ├── entities/
│   │   │   ├── friend-request.entity.ts
│   │   │   └── friendship.entity.ts
│   │   └── dto/
│   │       ├── send-request.dto.ts
│   │       ├── accept-request.dto.ts
│   │       └── decline-request.dto.ts
│   └── matches/                 # Module matchs
│       ├── matches.controller.ts
│       ├── matches.service.ts
│       ├── matches.module.ts
│       ├── entities/
│       │   ├── match.entity.ts
│       │   └── match-participant.entity.ts
│       └── dto/
│           └── get-matches.dto.ts
└── shared/                      # Services partagés
    ├── logger/
    │   └── logger.service.ts
    └── metrics/
        └── metrics.service.ts
```

---

## 🔐 Auth Module (Exemple Complet)

### auth.controller.ts

```typescript
import { Controller, Post, Body, UseGuards, Get, Req } from '@nestjs/common';
import { AuthService } from './auth.service';
import { RegisterDto, LoginDto, RefreshTokenDto } from './dto';
import { JwtAuthGuard } from '@/common/guards/jwt-auth.guard';
import { CurrentUser } from '@/common/decorators/current-user.decorator';
import { Public } from '@/common/decorators/public.decorator';
import { User } from '@/modules/users/entities/user.entity';
import { Throttle } from '@nestjs/throttler';

@Controller('auth')
export class AuthController {
  constructor(private readonly authService: AuthService) {}

  @Public()
  @Throttle(5, 3600) // 5 requêtes par heure
  @Post('register')
  async register(@Body() registerDto: RegisterDto) {
    return this.authService.register(registerDto);
  }

  @Public()
  @Throttle(10, 60) // 10 requêtes par minute
  @Post('login')
  async login(@Body() loginDto: LoginDto) {
    return this.authService.login(loginDto);
  }

  @Public()
  @Throttle(20, 60) // 20 requêtes par minute
  @Post('refresh')
  async refresh(@Body() refreshTokenDto: RefreshTokenDto) {
    return this.authService.refreshToken(refreshTokenDto.refreshToken);
  }

  @UseGuards(JwtAuthGuard)
  @Post('logout')
  async logout(@CurrentUser() user: User, @Req() req: any) {
    const token = req.headers.authorization?.split(' ')[1];
    return this.authService.logout(user.id, token);
  }
}
```

### auth.service.ts

```typescript
import { Injectable, UnauthorizedException, BadRequestException } from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from '@/modules/users/entities/user.entity';
import { RefreshToken } from '@/modules/users/entities/refresh-token.entity';
import { RegisterDto, LoginDto } from './dto';
import * as bcrypt from 'bcrypt';
import { AppLogger } from '@/shared/logger/logger.service';
import { MetricsService } from '@/shared/metrics/metrics.service';

@Injectable()
export class AuthService {
  constructor(
    @InjectRepository(User)
    private usersRepository: Repository<User>,
    @InjectRepository(RefreshToken)
    private refreshTokensRepository: Repository<RefreshToken>,
    private jwtService: JwtService,
    private logger: AppLogger,
    private metrics: MetricsService,
  ) {}

  async register(registerDto: RegisterDto) {
    const { username, email, password } = registerDto;

    // Vérifier si username/email existe
    const existingUser = await this.usersRepository.findOne({
      where: [{ username }, { email }],
    });

    if (existingUser) {
      if (existingUser.username === username) {
        throw new BadRequestException('USERNAME_TAKEN');
      }
      throw new BadRequestException('EMAIL_TAKEN');
    }

    // Hash password
    const passwordHash = await bcrypt.hash(password, 10);

    // Créer user
    const user = this.usersRepository.create({
      username,
      email,
      passwordHash,
    });

    await this.usersRepository.save(user);

    this.logger.info('User registered', { userId: user.id, username });
    this.metrics.incrementCounter('user_registrations_total');

    // Générer tokens
    const tokens = await this.generateTokens(user);

    return {
      success: true,
      data: {
        user: this.sanitizeUser(user),
        tokens,
      },
    };
  }

  async login(loginDto: LoginDto) {
    const { email, password } = loginDto;

    // Trouver user
    const user = await this.usersRepository.findOne({ where: { email } });

    if (!user) {
      throw new UnauthorizedException('INVALID_CREDENTIALS');
    }

    // Vérifier si banni
    if (user.isBanned) {
      throw new UnauthorizedException('ACCOUNT_BANNED');
    }

    // Vérifier password
    const isPasswordValid = await bcrypt.compare(password, user.passwordHash);

    if (!isPasswordValid) {
      throw new UnauthorizedException('INVALID_CREDENTIALS');
    }

    // Mettre à jour lastSeenAt
    user.lastSeenAt = new Date();
    user.status = 'online';
    await this.usersRepository.save(user);

    this.logger.info('User logged in', { userId: user.id, username: user.username });
    this.metrics.incrementCounter('user_logins_total');

    // Générer tokens
    const tokens = await this.generateTokens(user);

    return {
      success: true,
      data: {
        user: this.sanitizeUser(user),
        tokens,
      },
    };
  }

  async refreshToken(refreshToken: string) {
    try {
      // Vérifier le refresh token
      const payload = this.jwtService.verify(refreshToken, {
        secret: process.env.JWT_REFRESH_SECRET,
      });

      // Vérifier si révoqué
      const tokenRecord = await this.refreshTokensRepository.findOne({
        where: { tokenHash: this.hashToken(refreshToken) },
      });

      if (!tokenRecord || tokenRecord.revoked || tokenRecord.expiresAt < new Date()) {
        throw new UnauthorizedException('INVALID_TOKEN');
      }

      // Générer nouveau access token
      const user = await this.usersRepository.findOne({ where: { id: payload.sub } });

      if (!user) {
        throw new UnauthorizedException('INVALID_TOKEN');
      }

      const accessToken = this.jwtService.sign(
        { sub: user.id, username: user.username },
        { expiresIn: process.env.JWT_EXPIRES_IN || '3600s' },
      );

      return {
        success: true,
        data: {
          accessToken,
          expiresIn: parseInt(process.env.JWT_EXPIRES_IN || '3600'),
        },
      };
    } catch (error) {
      throw new UnauthorizedException('INVALID_TOKEN');
    }
  }

  async logout(userId: string, accessToken: string) {
    // Révoquer tous les refresh tokens de l'utilisateur
    await this.refreshTokensRepository.update(
      { userId },
      { revoked: true },
    );

    this.logger.info('User logged out', { userId });

    return {
      success: true,
      data: { message: 'Logged out successfully' },
    };
  }

  private async generateTokens(user: User) {
    const payload = { sub: user.id, username: user.username };

    const accessToken = this.jwtService.sign(payload, {
      expiresIn: process.env.JWT_EXPIRES_IN || '3600s',
    });

    const refreshToken = this.jwtService.sign(payload, {
      secret: process.env.JWT_REFRESH_SECRET,
      expiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '7d',
    });

    // Stocker refresh token en DB
    const tokenHash = this.hashToken(refreshToken);
    const expiresIn = parseInt(process.env.JWT_REFRESH_EXPIRES_IN || '604800');
    
    await this.refreshTokensRepository.save({
      userId: user.id,
      tokenHash,
      expiresAt: new Date(Date.now() + expiresIn * 1000),
    });

    return {
      accessToken,
      refreshToken,
      expiresIn: parseInt(process.env.JWT_EXPIRES_IN || '3600'),
    };
  }

  private hashToken(token: string): string {
    return bcrypt.hashSync(token, 10);
  }

  private sanitizeUser(user: User) {
    const { passwordHash, ...sanitized } = user;
    return sanitized;
  }
}
```

### DTOs avec Validation

```typescript
// register.dto.ts
import { IsEmail, IsString, MinLength, MaxLength, Matches } from 'class-validator';

export class RegisterDto {
  @IsString()
  @MinLength(3)
  @MaxLength(20)
  @Matches(/^[a-zA-Z0-9_-]+$/, {
    message: 'Username can only contain letters, numbers, underscore and dash',
  })
  username: string;

  @IsEmail()
  email: string;

  @IsString()
  @MinLength(8)
  @Matches(/^(?=.*[A-Z])(?=.*[0-9])/, {
    message: 'Password must contain at least 1 uppercase letter and 1 number',
  })
  password: string;
}

// login.dto.ts
export class LoginDto {
  @IsEmail()
  email: string;

  @IsString()
  password: string;
}
```

---

## 👥 Friends Module (Snippets)

### friends.service.ts (requêtes SQL optimisées)

```typescript
@Injectable()
export class FriendsService {
  constructor(
    @InjectRepository(Friendship)
    private friendshipsRepository: Repository<Friendship>,
    @InjectRepository(FriendRequest)
    private friendRequestsRepository: Repository<FriendRequest>,
    @InjectRepository(User)
    private usersRepository: Repository<User>,
    private logger: AppLogger,
    private metrics: MetricsService,
  ) {}

  async getFriends(userId: string) {
    // Utiliser la vue user_friends (cf. DATABASE_SCHEMA.sql)
    const query = `
      SELECT 
        u.id, u.username, u.avatar_url, u.level, u.elo, u.status, u.last_seen_at,
        uf.friends_since
      FROM user_friends uf
      JOIN users u ON u.id = uf.friend_id
      WHERE uf.user_id = $1
      ORDER BY 
        CASE u.status
          WHEN 'online' THEN 1
          WHEN 'in-game' THEN 2
          WHEN 'offline' THEN 3
        END,
        u.username ASC
    `;

    const friends = await this.usersRepository.query(query, [userId]);

    return {
      success: true,
      data: { friends },
      meta: { total: friends.length },
    };
  }

  async sendFriendRequest(fromUserId: string, toUserId: string) {
    if (fromUserId === toUserId) {
      throw new BadRequestException('CANNOT_ADD_SELF');
    }

    // Vérifier si déjà amis
    const areFriends = await this.areFriends(fromUserId, toUserId);
    if (areFriends) {
      throw new BadRequestException('ALREADY_FRIENDS');
    }

    // Vérifier si demande existante
    const existingRequest = await this.friendRequestsRepository.findOne({
      where: [
        { fromUserId, toUserId, status: 'pending' },
        { fromUserId: toUserId, toUserId: fromUserId, status: 'pending' },
      ],
    });

    if (existingRequest) {
      throw new BadRequestException('REQUEST_ALREADY_SENT');
    }

    // Créer demande
    const request = this.friendRequestsRepository.create({
      fromUserId,
      toUserId,
    });

    await this.friendRequestsRepository.save(request);

    this.logger.info('Friend request sent', { fromUserId, toUserId, requestId: request.id });
    this.metrics.incrementCounter('friend_requests_sent_total');

    const toUser = await this.usersRepository.findOne({ where: { id: toUserId } });

    return {
      success: true,
      data: {
        requestId: request.id,
        to: this.sanitizeUser(toUser),
        createdAt: request.createdAt,
      },
    };
  }

  async acceptFriendRequest(userId: string, requestId: string) {
    const request = await this.friendRequestsRepository.findOne({
      where: { id: requestId, toUserId: userId, status: 'pending' },
    });

    if (!request) {
      throw new NotFoundException('REQUEST_NOT_FOUND');
    }

    // Créer friendship (via fonction SQL pour gérer l'ordre user1 < user2)
    await this.usersRepository.query('SELECT create_friendship($1, $2)', [
      request.fromUserId,
      request.toUserId,
    ]);

    // Mettre à jour request
    request.status = 'accepted';
    await this.friendRequestsRepository.save(request);

    this.logger.info('Friend request accepted', { requestId, userId });
    this.metrics.incrementCounter('friend_requests_accepted_total');

    const friend = await this.usersRepository.findOne({ where: { id: request.fromUserId } });

    return {
      success: true,
      data: {
        friendship: {
          friend: this.sanitizeUser(friend),
          createdAt: new Date(),
        },
      },
    };
  }

  private async areFriends(user1Id: string, user2Id: string): Promise<boolean> {
    const result = await this.usersRepository.query('SELECT are_friends($1, $2) as are_friends', [
      user1Id,
      user2Id,
    ]);
    return result[0]?.are_friends || false;
  }

  private sanitizeUser(user: User) {
    const { passwordHash, ...sanitized } = user;
    return sanitized;
  }
}
```

---

## 📊 Métriques Prometheus (metrics.service.ts)

```typescript
import { Injectable } from '@nestjs/common';
import { Counter, Histogram, Gauge, register } from 'prom-client';

@Injectable()
export class MetricsService {
  private httpRequestsTotal: Counter;
  private httpRequestDuration: Histogram;
  private httpErrorsTotal: Counter;
  private dbQueryDuration: Histogram;
  private dbConnectionsActive: Gauge;
  private usersOnline: Gauge;

  constructor() {
    // HTTP metrics
    this.httpRequestsTotal = new Counter({
      name: 'http_requests_total',
      help: 'Total HTTP requests',
      labelNames: ['method', 'path', 'status'],
    });

    this.httpRequestDuration = new Histogram({
      name: 'http_request_duration_ms',
      help: 'HTTP request duration in milliseconds',
      labelNames: ['method', 'path'],
      buckets: [10, 50, 100, 200, 500, 1000, 2000, 5000],
    });

    this.httpErrorsTotal = new Counter({
      name: 'http_errors_total',
      help: 'Total HTTP errors',
      labelNames: ['method', 'path', 'status'],
    });

    // DB metrics
    this.dbQueryDuration = new Histogram({
      name: 'db_query_duration_ms',
      help: 'Database query duration in milliseconds',
      labelNames: ['query'],
      buckets: [1, 5, 10, 25, 50, 100, 250, 500],
    });

    this.dbConnectionsActive = new Gauge({
      name: 'db_connections_active',
      help: 'Active database connections',
    });

    // Business metrics
    this.usersOnline = new Gauge({
      name: 'users_online',
      help: 'Number of online users',
    });
  }

  incrementCounter(name: string, labels?: Record<string, string>) {
    switch (name) {
      case 'user_registrations_total':
        new Counter({ name, help: 'Total user registrations' }).inc();
        break;
      case 'user_logins_total':
        new Counter({ name, help: 'Total user logins' }).inc();
        break;
      case 'friend_requests_sent_total':
        new Counter({ name, help: 'Total friend requests sent' }).inc();
        break;
      // ... autres counters
    }
  }

  observeHttpRequest(method: string, path: string, statusCode: number, duration: number) {
    this.httpRequestsTotal.inc({ method, path, status: statusCode });
    this.httpRequestDuration.observe({ method, path }, duration);

    if (statusCode >= 400) {
      this.httpErrorsTotal.inc({ method, path, status: statusCode });
    }
  }

  observeDbQuery(query: string, duration: number) {
    this.dbQueryDuration.observe({ query }, duration);
  }

  setDbConnectionsActive(count: number) {
    this.dbConnectionsActive.set(count);
  }

  setUsersOnline(count: number) {
    this.usersOnline.set(count);
  }

  getMetrics() {
    return register.metrics();
  }
}
```

---

## 🎯 Interceptor Logging + Metrics

```typescript
// logging.interceptor.ts
import { Injectable, NestInterceptor, ExecutionContext, CallHandler } from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AppLogger } from '@/shared/logger/logger.service';
import { MetricsService } from '@/shared/metrics/metrics.service';

@Injectable()
export class LoggingInterceptor implements NestInterceptor {
  constructor(
    private logger: AppLogger,
    private metrics: MetricsService,
  ) {}

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const request = context.switchToHttp().getRequest();
    const { method, url, headers, body } = request;
    const correlationId = request['correlationId'];
    const userId = request.user?.id;

    const startTime = Date.now();

    this.logger.setContext({
      correlationId,
      userId,
      module: context.getClass().name,
      method: context.getHandler().name,
    });

    this.logger.info(`HTTP ${method} ${url}`, {
      ip: request.ip,
      userAgent: headers['user-agent'],
    });

    return next.handle().pipe(
      tap({
        next: (data) => {
          const duration = Date.now() - startTime;
          const response = context.switchToHttp().getResponse();
          const statusCode = response.statusCode;

          this.logger.info(`HTTP ${method} ${url} - ${statusCode}`, {
            duration,
            statusCode,
          });

          this.metrics.observeHttpRequest(method, url, statusCode, duration);
        },
        error: (error) => {
          const duration = Date.now() - startTime;
          const statusCode = error.status || 500;

          this.logger.error(`HTTP ${method} ${url} - ${statusCode}`, error.stack, {
            duration,
            statusCode,
            errorCode: error.response?.error?.code,
          });

          this.metrics.observeHttpRequest(method, url, statusCode, duration);
        },
      }),
    );
  }
}
```

---

## 🚀 main.ts (Bootstrap avec tout configuré)

```typescript
import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import { AppModule } from './app.module';
import helmet from 'helmet';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Global prefix
  app.setGlobalPrefix('v1');

  // CORS
  app.enableCors({
    origin: process.env.CORS_ORIGIN?.split(',') || '*',
    credentials: true,
  });

  // Security
  app.use(helmet());

  // Validation
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      forbidNonWhitelisted: true,
      transform: true,
    }),
  );

  // Start
  const port = process.env.PORT || 3000;
  await app.listen(port);

  console.log(`🚀 NexA API running on http://localhost:${port}`);
}
bootstrap();
```


