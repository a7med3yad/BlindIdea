# BlindIdea Authentication System - Complete Implementation Guide

## Overview

This document describes the production-grade JWT-based authentication system implemented for the BlindIdea API. The system includes:

- ✅ User registration with email verification
- ✅ Login with JWT access tokens (15 minutes)
- ✅ Refresh token system with token rotation (7 days)
- ✅ Email verification with 24-hour tokens
- ✅ Logout and token revocation (single device & all devices)
- ✅ Comprehensive audit logging
- ✅ Security breach detection (token reuse)
- ✅ Hash-based token storage (SHA-256)

---

## Architecture Overview

### Core Components

#### 1. **Entities** (`BlindIdea.Core/Entities/`)
- **User** - ASP.NET Identity user with soft delete and email verification
- **RefreshToken** - Long-lived tokens for obtaining new access tokens
  - Fields: Id, TokenHash (SHA-256), JwtId, UserId, ExpiresAt, CreatedAt, RevokedAt, ReplacedByTokenId, IsUsed
  - Implements token rotation and security breach detection
- **EmailVerificationToken** - Short-lived tokens for email verification
  - Fields: Id, TokenHash (SHA-256), UserId, ExpiresAt, CreatedAt, VerifiedAt

#### 2. **Services** (`BlindIdea.Application/Services/`)
- **IAuthService / AuthService** - Core authentication logic
  - RegisterAsync() → Creates user with email verification
  - LoginAsync() → Authenticates and returns tokens
  - RefreshTokenAsync() → Token rotation with breach detection
  - LogoutAsync() → Revoke single refresh token
  - RevokeAllTokensAsync() → Logout from all devices
  - VerifyEmailAsync() → Email confirmation
  - ResendVerificationEmailAsync() → Rate-limited resend
  - SendVerificationEmailAsync() → Helper for email sending

- **IJwtService / JwtService** - Token generation
  - CreateAccessToken() → 15-minute JWT with JTI claim
  - GenerateRefreshToken() → 32-byte cryptographically secure token
  - HashToken() → SHA-256 hashing for storage
  - ExtractJwtId() → Extract JTI claim from token

- **IEmailService / EmailService** - Email delivery
  - SendEmailAsync() → SMTP-based email sending
  - Retry logic with 3 attempts
  - Comprehensive error logging

#### 3. **Controllers** (`BlindIdea.API/Controllers/`)
- **AuthController** - REST endpoints
  - POST /api/auth/register → User registration
  - POST /api/auth/login → Authentication
  - POST /api/auth/refresh → Token refresh
  - POST /api/auth/logout → Single device logout
  - POST /api/auth/revoke-all → All devices logout (requires auth)
  - GET /api/auth/verify-email → Email verification
  - POST /api/auth/resend-verification → Resend verification email

#### 4. **DTOs** (`BlindIdea.Application/Dtos/`)
- **AuthResponseDto** - Token response with user info and expiry times
- **LoginDto** - Login credentials
- **RegisterDto** - Registration data with password confirmation
- **RefreshTokenRequestDto** - Refresh token request
- **ResendVerificationEmailDto** - Email resend request
- **UserDto** - User information in responses

---

## Security Features

### Token Security
1. **Access Tokens**
   - 15-minute expiry (short-lived)
   - HS512 (HMAC-SHA512) signing
   - Contains claims: userId, email, name, jti (unique ID)
   - Validated on every request

2. **Refresh Tokens**
   - 7-day expiry (configurable)
   - 32-byte cryptographically secure (256-bits entropy)
   - SHA-256 hashed in database (never stored plain)
   - Single-use with token rotation
   - Family tracking for breach detection

3. **Token Rotation Pattern**
   - Old refresh token marked as "used"
   - New refresh token issued with new access token
   - Old token linked to new via `ReplacedByTokenId`
   - Prevents token reuse attacks

4. **Breach Detection**
   - If revoked token is reused → Security alert
   - All tokens for user revoked immediately
   - Incident logged with IP address and timestamp

### Email Verification
- 24-hour token expiry
- SHA-256 hashed in database
- Single-use tokens
- Rate-limited resend (max once per 2 minutes)
- Must verify before login

### Audit Trail
- All auth events logged with IP address
- Token creation/revocation tracked
- Failed login attempts recorded
- Email verification attempts logged
- Security incidents flagged as errors

---

## Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",      // Environment variable: JWT_SECRET
    "Issuer": "BlindIdeaAPI",                     // Environment variable: JWT_ISSUER
    "Audience": "BlindIdeaClient",                // Environment variable: JWT_AUDIENCE
    "AccessTokenExpiryMinutes": 15,               // Environment variable: JWT_ACCESS_EXPIRY_MINUTES
    "RefreshTokenExpiryDays": 7                   // Environment variable: JWT_REFRESH_EXPIRY_DAYS
  },
  "Email": {
    "From": "noreply@blindidea.com",             // Environment variable: SMTP_FROM
    "SmtpServer": "smtp.office365.com",          // Environment variable: SMTP_HOST
    "Port": 587,                                  // Environment variable: SMTP_PORT
    "Username": "your-email@outlook.com",        // Environment variable: SMTP_USER
    "Password": "your-app-password",             // Environment variable: SMTP_PASSWORD
    "EnableSsl": true                            // Environment variable: SMTP_ENABLE_SSL
  },
  "AppBaseUrl": "https://yourdomain.com"         // Environment variable: APP_BASE_URL
}
```

### Environment Variables (Production)
```bash
JWT_SECRET=your_secure_secret_key_min_32_chars
JWT_ISSUER=BlindIdeaAPI
JWT_AUDIENCE=BlindIdeaClient
JWT_ACCESS_EXPIRY_MINUTES=15
JWT_REFRESH_EXPIRY_DAYS=7
JWT_REFRESH_EXPIRY_DAYS=7
SMTP_HOST=smtp.office365.com
SMTP_PORT=587
SMTP_USER=your-email@outlook.com
SMTP_PASSWORD=your-app-password
SMTP_ENABLE_SSL=true
APP_BASE_URL=https://yourdomain.com
```

---

## API Endpoints

### 1. Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "qR8vN2...",
  "accessTokenExpiration": "2024-01-01T10:15:00Z",
  "refreshTokenExpiration": "2024-01-08T10:00:00Z",
  "user": {
    "id": "user-uuid",
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

**Note:** User receives verification email. Email must be verified before login.

### 2. Verify Email
```http
GET /api/auth/verify-email?userId=user-uuid&token=verification_token
```

**Response (200):**
```json
{
  "message": "Email verified successfully. You can now login."
}
```

### 3. Resend Verification Email
```http
POST /api/auth/resend-verification
Content-Type: application/json

{
  "email": "john@example.com"
}
```

**Rate Limit:** Maximum once per 2 minutes per user

### 4. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "rememberMe": false
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "qR8vN2...",
  "accessTokenExpiration": "2024-01-01T10:15:00Z",
  "refreshTokenExpiration": "2024-01-08T10:00:00Z",
  "user": {
    "id": "user-uuid",
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

**Errors:**
- 401: Invalid credentials or email not verified
- 400: Validation error

### 5. Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "qR8vN2..."
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGc...",  // New token
  "refreshToken": "kL9mP5...",  // New token (rotation)
  "accessTokenExpiration": "2024-01-01T10:15:00Z",
  "refreshTokenExpiration": "2024-01-08T10:00:00Z",
  "user": { ... }
}
```

**Features:**
- Old refresh token is revoked
- New tokens issued
- Detects token reuse (breach indicator)

### 6. Logout (Single Device)
```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "qR8vN2..."
}
```

**Response (204):** No content

### 7. Revoke All Tokens (Logout from All Devices)
```http
POST /api/auth/revoke-all
Authorization: Bearer eyJhbGc...
```

**Response (204):** No content

**Note:** Requires valid access token

---

## Database Schema

### RefreshTokens Table
```sql
CREATE TABLE [dbo].[RefreshTokens] (
  [Id] uniqueidentifier NOT NULL PRIMARY KEY,
  [TokenHash] nvarchar(256) NOT NULL,           -- SHA-256 hash
  [JwtId] nvarchar(256) NOT NULL,               -- JWT ID for linking
  [UserId] nvarchar(450) NOT NULL,              -- Foreign key to AspNetUsers
  [ExpiresAt] datetime2 NOT NULL,
  [CreatedAt] datetime2 NOT NULL,
  [CreatedByIp] nvarchar(45),                   -- IPv6 max length
  [RevokedAt] datetime2 NULL,                   -- NULL if not revoked
  [RevokedByIp] nvarchar(45),
  [ReplacedByTokenId] uniqueidentifier NULL,    -- Token rotation tracking
  [IsUsed] bit NOT NULL,                        -- Single-use enforcement
  CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId])
    REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
  INDEX [IX_RefreshTokens_UserId],
  INDEX [IX_RefreshTokens_JwtId],
  INDEX [IX_RefreshTokens_ExpiresAt]
);
```

### EmailVerificationTokens Table
```sql
CREATE TABLE [dbo].[EmailVerificationTokens] (
  [Id] uniqueidentifier NOT NULL PRIMARY KEY,
  [TokenHash] nvarchar(256) NOT NULL,           -- SHA-256 hash
  [UserId] nvarchar(450) NOT NULL,              -- Foreign key to AspNetUsers
  [ExpiresAt] datetime2 NOT NULL,
  [CreatedAt] datetime2 NOT NULL,
  [VerifiedAt] datetime2 NULL,                  -- NULL if not used
  CONSTRAINT [FK_EmailVerificationTokens_Users] FOREIGN KEY ([UserId])
    REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
  INDEX [IX_EmailVerificationTokens_UserId],
  INDEX [IX_EmailVerificationTokens_ExpiresAt]
);
```

---

## Files Modified/Created

### Created Files
- `BlindIdea.Core/Entities/RefreshToken.cs` - Token rotation entity
- `BlindIdea.Core/Entities/EmailVerificationToken.cs` - Email verification entity
- `BlindIdea.Application/Dtos/RefreshTokenRequestDto.cs` - Token refresh request
- `BlindIdea.Application/Dtos/ResendVerificationEmailDto.cs` - Verification resend request
- `BlindIdea.Infrastructure/Migrations/AddRefreshTokenAndEmailVerificationToken.cs` - Database migration

### Modified Files
- `BlindIdea.Core/Entities/User.cs` - Already supports email verification
- `BlindIdea.Core/Interfaces/IJwtService.cs` - Added ExtractJwtId() method
- `BlindIdea.Infrastructure/Services/JwtService.cs` - Full implementation with token hashing
- `BlindIdea.Infrastructure/Common/Options/JwtOptions.cs` - Added AccessTokenExpiryMinutes & RefreshTokenExpiryDays
- `BlindIdea.Application/Services/Interfaces/IAuthService.cs` - Added all auth methods
- `BlindIdea.Application/Services/Implementations/AuthService.cs` - Full implementation with email verification
- `BlindIdea.Application/Dtos/AuthResponseDto.cs` - Added refresh token fields
- `BlindIdea.API/Controllers/AuthController.cs` - All endpoints (register, login, refresh, logout, verify, resend)
- `BlindIdea.API/Program.cs` - EmailService registration
- `BlindIdea.API/appsettings.json` - JWT expiry configuration
- `BlindIdea.Infrastructure/Persistence/AppDbContext.cs` - Entity configurations for new entities

---

## Testing the System

### 1. Register a New User
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "TestPass123!",
    "confirmPassword": "TestPass123!"
  }'
```

### 2. Check Email for Verification Link
The email will contain a link like:
```
https://localhost:7001/auth/verify-email?userId=XXX&token=YYY
```

### 3. Verify Email (Click link or use API)
```bash
curl -X GET "https://localhost:7001/api/auth/verify-email?userId=USER_ID&token=TOKEN"
```

### 4. Login After Verification
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPass123!",
    "rememberMe": false
  }'
```

### 5. Use Access Token in Requests
```bash
curl -X GET https://localhost:7001/api/team \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

### 6. Refresh Token When Expired
```bash
curl -X POST https://localhost:7001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "REFRESH_TOKEN"
  }'
```

### 7. Logout
```bash
curl -X POST https://localhost:7001/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "REFRESH_TOKEN"
  }'
```

---

## Logging

All authentication events are logged using ASP.NET Core's ILogger:

```
[Information] User registered successfully: user-id (email@test.com)
[Information] Verification email sent to user: user-id
[Information] User logged in successfully: user-id from IP: 192.168.1.1
[Information] Token refreshed for user: user-id (Token rotation)
[Information] Email verified successfully for user: user-id
[Warning] Login attempt with unverified email: email@test.com from IP: 192.168.1.1
[Warning] Failed login attempt for user: user-id from IP: 192.168.1.1
[Error] SECURITY ALERT: Revoked token reused by user user-id from IP 192.168.1.1. Revoking all tokens.
```

View logs in:
- Development: Console output
- Production: Application Insights / Logging provider

---

## Security Best Practices

1. **Never Log Tokens** - Tokens are never written to logs
2. **Hash All Tokens** - Tokens are hashed with SHA-256 before storage
3. **HTTPS Only** - All endpoints require HTTPS in production
4. **Secure Headers** - Use HttpOnly, Secure, SameSite cookies if storing tokens there
5. **Environment Variables** - Never hardcode secrets in appsettings.json
6. **Breach Detection** - Token reuse triggers immediate revocation
7. **Rate Limiting** - Email verification resend limited to 1 per 2 minutes
8. **Email Verification** - Users cannot log in until email is verified

---

## Rate Limits

- **Email Verification Resend**: 1 request per 2 minutes per user
- **Password Reset** (future): Not yet implemented
- **Login Attempts** (future): Rate limiting not yet implemented

---

## Future Enhancements

1. [ ] Add rate limiting on login attempts (e.g., 5 failed attempts → 15 min lockout)
2. [ ] Implement password reset flow with secure token
3. [ ] Add refresh token to HttpOnly secure cookie (optional, body-based is fine too)
4. [ ] Add IP whitelisting for sensitive operations
5. [ ] Implement account lockout after failed login attempts
6. [ ] Add two-factor authentication (2FA) support
7. [ ] Token introspection endpoint for client validation
8. [ ] Revoke by JWT ID (jti) endpoint
9. [ ] Add role-based claims to access tokens
10. [ ] Implement token binding (IP affinity)

---

## Troubleshooting

### "Please verify your email before logging in"
- User email is not verified
- Solution: Send verification email via POST /api/auth/resend-verification

### "The refresh token is invalid, expired, or has been revoked"
- Token has expired (>7 days old)
- Token was revoked
- Token doesn't exist in database
- Solution: Re-login to get new tokens

### "SECURITY ALERT: Revoked token reused"
- A revoked token was used again
- All user's tokens have been revoked
- Solution: Re-login, check for account compromise

### "Email verification token invalid, expired, or already used"
- Token expired (>24 hours old)
- Token already used
- Wrong token/user combination
- Solution: Request new verification email

---

## Summary

The authentication system is **production-ready** with:

✅ Secure JWT tokens with short expiry
✅ Refresh token rotation with breach detection
✅ Email verification requirements
✅ Comprehensive audit logging
✅ Proper error handling and validation
✅ Configurable via environment variables
✅ Hashbased token storage (SHA-256)
✅ Token family tracking for security

The system is backward-compatible and can be extended with additional features like 2FA, password reset, and IP-based rate limiting.

