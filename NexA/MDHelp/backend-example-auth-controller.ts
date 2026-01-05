// ============================================
// EXEMPLE: src/controllers/auth.controller.ts
// ============================================

import { Request, Response } from 'express';
import bcrypt from 'bcrypt';
import jwt from 'jsonwebtoken';
import { prisma } from '../app';
import { logger } from '../utils/logger';

const JWT_SECRET = process.env.JWT_SECRET!;
const JWT_REFRESH_SECRET = process.env.JWT_REFRESH_SECRET!;
const JWT_EXPIRES_IN = process.env.JWT_EXPIRES_IN || '1h';
const JWT_REFRESH_EXPIRES_IN = process.env.JWT_REFRESH_EXPIRES_IN || '7d';

// ============================================
// REGISTER
// ============================================

export async function register(req: Request, res: Response) {
  try {
    const { username, email, password } = req.body;

    // Validation
    if (!username || !email || !password) {
      return res.status(400).json({
        success: false,
        error: { code: 'VALIDATION_ERROR', message: 'Missing required fields' }
      });
    }

    if (password.length < 8) {
      return res.status(400).json({
        success: false,
        error: { code: 'VALIDATION_ERROR', message: 'Password must be at least 8 characters' }
      });
    }

    // Vérifier si email existe
    const existingUser = await prisma.user.findUnique({ where: { email } });
    if (existingUser) {
      return res.status(400).json({
        success: false,
        error: { code: 'EMAIL_EXISTS', message: 'Email already exists' }
      });
    }

    // Hash password
    const hashedPassword = await bcrypt.hash(password, 10);

    // Créer user
    const user = await prisma.user.create({
      data: {
        username,
        email,
        password: hashedPassword
      }
    });

    // Générer tokens
    const tokens = generateTokens(user.id);

    // Stocker refresh token
    await prisma.refreshToken.create({
      data: {
        token: tokens.refreshToken,
        userId: user.id,
        expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000) // 7 jours
      }
    });

    logger.info({
      type: 'user_registered',
      correlationId: req.correlationId,
      userId: user.id,
      username: user.username
    });

    res.status(201).json({
      success: true,
      data: {
        user: sanitizeUser(user),
        tokens
      }
    });
  } catch (error: any) {
    logger.error({
      type: 'register_error',
      correlationId: req.correlationId,
      error: error.message
    });
    res.status(500).json({
      success: false,
      error: { code: 'INTERNAL_ERROR', message: 'Failed to register' }
    });
  }
}

// ============================================
// LOGIN
// ============================================

export async function login(req: Request, res: Response) {
  try {
    const { email, password } = req.body;

    // Validation
    if (!email || !password) {
      return res.status(400).json({
        success: false,
        error: { code: 'VALIDATION_ERROR', message: 'Missing email or password' }
      });
    }

    // Trouver user
    const user = await prisma.user.findUnique({ where: { email } });
    if (!user) {
      return res.status(401).json({
        success: false,
        error: { code: 'INVALID_CREDENTIALS', message: 'Invalid email or password' }
      });
    }

    // Vérifier password
    const isPasswordValid = await bcrypt.compare(password, user.password);
    if (!isPasswordValid) {
      return res.status(401).json({
        success: false,
        error: { code: 'INVALID_CREDENTIALS', message: 'Invalid email or password' }
      });
    }

    // Générer tokens
    const tokens = generateTokens(user.id);

    // Stocker refresh token
    await prisma.refreshToken.create({
      data: {
        token: tokens.refreshToken,
        userId: user.id,
        expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)
      }
    });

    // Update lastSeenAt
    await prisma.user.update({
      where: { id: user.id },
      data: { lastSeenAt: new Date(), status: 'online' }
    });

    logger.info({
      type: 'user_login',
      correlationId: req.correlationId,
      userId: user.id,
      username: user.username
    });

    res.json({
      success: true,
      data: {
        user: sanitizeUser(user),
        tokens
      }
    });
  } catch (error: any) {
    logger.error({
      type: 'login_error',
      correlationId: req.correlationId,
      error: error.message
    });
    res.status(500).json({
      success: false,
      error: { code: 'INTERNAL_ERROR', message: 'Failed to login' }
    });
  }
}

// ============================================
// REFRESH TOKEN
// ============================================

export async function refresh(req: Request, res: Response) {
  try {
    const { refreshToken } = req.body;

    if (!refreshToken) {
      return res.status(400).json({
        success: false,
        error: { code: 'VALIDATION_ERROR', message: 'Refresh token required' }
      });
    }

    // Vérifier token
    let decoded: any;
    try {
      decoded = jwt.verify(refreshToken, JWT_REFRESH_SECRET);
    } catch {
      return res.status(401).json({
        success: false,
        error: { code: 'INVALID_TOKEN', message: 'Invalid refresh token' }
      });
    }

    // Vérifier si token existe en DB
    const storedToken = await prisma.refreshToken.findUnique({
      where: { token: refreshToken }
    });

    if (!storedToken || storedToken.expiresAt < new Date()) {
      return res.status(401).json({
        success: false,
        error: { code: 'INVALID_TOKEN', message: 'Refresh token expired or invalid' }
      });
    }

    // Générer nouveau access token
    const accessToken = jwt.sign({ userId: decoded.userId }, JWT_SECRET, {
      expiresIn: JWT_EXPIRES_IN
    });

    res.json({
      success: true,
      data: {
        accessToken,
        expiresIn: 3600 // 1 heure en secondes
      }
    });
  } catch (error: any) {
    logger.error({
      type: 'refresh_error',
      correlationId: req.correlationId,
      error: error.message
    });
    res.status(500).json({
      success: false,
      error: { code: 'INTERNAL_ERROR', message: 'Failed to refresh token' }
    });
  }
}

// ============================================
// LOGOUT
// ============================================

export async function logout(req: Request, res: Response) {
  try {
    const userId = req.user?.id;

    if (!userId) {
      return res.status(401).json({
        success: false,
        error: { code: 'UNAUTHORIZED', message: 'Not authenticated' }
      });
    }

    // Supprimer tous les refresh tokens de l'utilisateur
    await prisma.refreshToken.deleteMany({
      where: { userId }
    });

    // Update status
    await prisma.user.update({
      where: { id: userId },
      data: { status: 'offline', lastSeenAt: new Date() }
    });

    logger.info({
      type: 'user_logout',
      correlationId: req.correlationId,
      userId
    });

    res.json({
      success: true,
      data: null
    });
  } catch (error: any) {
    logger.error({
      type: 'logout_error',
      correlationId: req.correlationId,
      error: error.message
    });
    res.status(500).json({
      success: false,
      error: { code: 'INTERNAL_ERROR', message: 'Failed to logout' }
    });
  }
}

// ============================================
// HELPERS
// ============================================

function generateTokens(userId: string) {
  const accessToken = jwt.sign({ userId }, JWT_SECRET, {
    expiresIn: JWT_EXPIRES_IN
  });

  const refreshToken = jwt.sign({ userId }, JWT_REFRESH_SECRET, {
    expiresIn: JWT_REFRESH_EXPIRES_IN
  });

  return {
    accessToken,
    refreshToken,
    expiresIn: 3600 // 1 heure en secondes
  };
}

function sanitizeUser(user: any) {
  const { password, ...userWithoutPassword } = user;
  return {
    ...userWithoutPassword,
    stats: {
      totalMatches: 0, // À calculer depuis match_participants
      wins: 0,
      losses: 0
    }
  };
}

