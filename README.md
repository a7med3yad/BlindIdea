<p align="center">
  <img src="Images/logo.png" alt="BlindIdea Logo" width="500"/>
</p>

<p align="center">
  <b>Innovation without ego.</b>
</p>

---

# BlindIdea API

> **Innovation without ego.**

A platform designed to encourage **anonymous idea sharing** within teams while maintaining **privacy and security**. Built with ASP.NET Core, Clean Architecture, JWT Authentication, and AES-256 encryption.

---

## 🔴 Table of Contents

- [BlindIdea API](#blindidea-api)
  - [🔴 Table of Contents](#-table-of-contents)
  - [Overview](#overview)
  - [Architecture](#architecture)
    - [Dependency Rules](#dependency-rules)
  - [Project Structure](#project-structure)
  - [Tech Stack](#tech-stack)
  - [Features](#features)
    - [Authentication](#authentication)
    - [Team Management](#team-management)
    - [Anonymous Idea Sharing](#anonymous-idea-sharing)
    - [Anonymous Rating System](#anonymous-rating-system)
    - [Dashboard \& Insights](#dashboard--insights)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Access Scalar API Docs](#access-scalar-api-docs)
  - [Environment Variables](#environment-variables)
  - [API Endpoints](#api-endpoints)
    - [Auth](#auth)
    - [Team](#team)
    - [Ideas](#ideas)
    - [Dashboard](#dashboard)
  - [Authentication Flow](#authentication-flow)
    - [Register + Verify](#register--verify)
    - [Login](#login)
    - [Refresh Token](#refresh-token)
    - [Google / GitHub OAuth](#google--github-oauth)
    - [Using Bearer Token in Requests](#using-bearer-token-in-requests)
  - [Security](#security)
    - [JWT Tokens](#jwt-tokens)
    - [OTP Protection](#otp-protection)
    - [Idea Encryption](#idea-encryption)
    - [Anonymity](#anonymity)
  - [Database Schema](#database-schema)
  - [Design Patterns Used](#design-patterns-used)
  - [Branding](#branding)

---

## Overview

BlindIdea allows teams to share ideas **anonymously** — no one knows who submitted what. Ideas are **AES-256 encrypted** in the database and only decrypted for team members. Ratings are also anonymous to prevent bias.

```
Register → Verify Email (OTP) → Create/Join Team → Submit Ideas → Rate Ideas → View Dashboard
```

---

## Architecture

Clean Architecture with 4 layers — each layer only depends on the layer inside it:

```
┌──────────────────────────────────────┐
│           BlindIdea.API              │  Controllers, Program.cs, Middleware
├──────────────────────────────────────┤
│       BlindIdea.Application          │  Services, DTOs, Interfaces
├──────────────────────────────────────┤
│         BlindIdea.Domain             │  Entities, Repository Interfaces
├──────────────────────────────────────┤
│      BlindIdea.Infrastructure        │  DB, Repositories, Email, Encryption
└──────────────────────────────────────┘
```

### Dependency Rules

```
Domain          →  depends on NOTHING
Application     →  depends on Domain
Infrastructure  →  depends on Domain + Application
API             →  depends on Application + Infrastructure
```

---

## Project Structure

```
BlindIdea/
│
├── BlindIdea.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── TeamController.cs
│   │   ├── IdeaController.cs
│   │   └── DashboardController.cs
│   └── Program.cs
│
├── BlindIdea.Application/
│   ├── Dtos/
│   │   ├── Auth/
│   │   ├── Teams/
│   │   ├── Ideas/
│   │   └── Dashboards/
│   └── Services/
│       ├── Abstraction/
│       │   ├── IAuthService.cs
│       │   ├── ITeamService.cs
│       │   ├── IIdeaService.cs
│       │   └── IDashboardService.cs
│       └── Implementation/
│           ├── Auth/
│           ├── Teams/
│           ├── Ideas/
│           └── Dashboards/
│
├── BlindIdea.Domain/
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── Team.cs
│   │   ├── Idea.cs
│   │   ├── Rating.cs
│   │   └── RefreshToken.cs
│   └── Abstraction/
│       ├── Repositories/
│       │   ├── IGenericRepository.cs
│       │   ├── ITeamRepository.cs
│       │   ├── IIdeaRepository.cs
│       │   └── IRatingRepository.cs
│       ├── IUnitOfWork.cs
│       └── Services/
│
└── BlindIdea.Infrastructure/
    ├── Implementation/
    │   ├── Auth/
    │   │   ├── AuthService.cs
    │   │   ├── TokenService.cs
    │   │   ├── EmailService.cs
    │   │   ├── OtpService.cs
    │   │   └── OAuthService.cs
    │   ├── Encryption/
    │   │   └── EncryptionService.cs
    │   ├── Repositories/
    │   │   ├── GenericRepository.cs
    │   │   ├── TeamRepository.cs
    │   │   ├── IdeaRepository.cs
    │   │   └── RatingRepository.cs
    │   └── UnitOfWorks/
    │       └── UnitOfWork.cs
    └── Persistence/
        └── AppDbContext.cs
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| Language | C# |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Identity + JWT Bearer |
| OAuth | Google + GitHub |
| Email | MailKit + Gmail SMTP |
| Encryption | AES-256 |
| API Docs | Scalar |
| Architecture | Clean Architecture |
| Pattern | Repository + Unit of Work |

---

## Features

### Authentication
- Register with email + OTP verification
- Login with JWT access token + refresh token
- Forgot password via OTP email
- Change password
- Google OAuth login
- GitHub OAuth login
- Role-based authorization (Admin / User)
- OTP rate limiting (max 3 per 10 minutes)
- OTP expiration (5 minutes)
- Refresh token rotation (7 days)

### Team Management
- Create a team → automatically become Admin
- Join team via unique invite code
- View team info and members
- Regenerate invite code
- Remove members (Admin only)
- Leave team
- Delete team (Admin only)

### Anonymous Idea Sharing
- Submit ideas with title and content
- Ideas encrypted with AES-256 before storing
- Ideas displayed anonymously — no author shown
- Delete your own ideas

### Anonymous Rating System
- Rate ideas on a scale of 1 to 5
- Ratings are anonymous
- Update or remove your rating
- Cannot rate your own idea

### Dashboard & Insights
- Total ideas and ratings
- Overall average rating
- Top 5 rated ideas
- 5 most recent ideas
- Personal stats (ideas submitted, ideas rated)

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (Express or full)
- Gmail account with App Password
- Google OAuth credentials
- GitHub OAuth credentials

### Installation

```bash
# Clone the repository
git clone https://github.com/a7med3yad/BlindIdea.git
cd BlindIdea

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update --project BlindIdea.Infrastructure --startup-project BlindIdea.API

# Run the API
dotnet run --project BlindIdea.API
```

### Access Scalar API Docs

```
https://localhost:7286/scalar
```

---

## Environment Variables

Add these to `appsettings.json` or User Secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BlindIdeaDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters!!",
    "Issuer": "BlindIdea",
    "Audience": "BlindIdeaUsers"
  },
  "Encryption": {
    "Key": "your-32-character-encryption-key!!"
  },
  "EmailSettings": {
    "Email": "theblindidea@gmail.com",
    "Password": "your-gmail-app-password",
    "DisplayName": "BlindIdea"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  }
}
```

> **Never commit real credentials to source control.**
> Use `dotnet user-secrets` for local development.

```bash
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

---

## API Endpoints

### Auth

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/Auth/register` | Register with email + password | Public |
| POST | `/api/Auth/verify-email` | Submit OTP → get tokens | Public |
| POST | `/api/Auth/login` | Login → get tokens | Public |
| POST | `/api/Auth/forgot-password` | Send OTP to email | Public |
| POST | `/api/Auth/verify-reset` | Verify OTP → get tokens | Public |
| POST | `/api/Auth/refresh-token` | Refresh access token | Public |
| POST | `/api/Auth/logout` | Revoke refresh token | Bearer |
| POST | `/api/Auth/change-password` | Change password | Bearer |
| POST | `/api/Auth/assign-role` | Assign role to user | Admin |
| GET | `/api/Auth/profile` | Get current user profile | Bearer |
| GET | `/api/Auth/login/google` | Google OAuth login | Public |
| GET | `/api/Auth/login/github` | GitHub OAuth login | Public |

### Team

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/Team/create` | Create team → become Admin | Bearer |
| POST | `/api/Team/join` | Join team with invite code | Bearer |
| GET | `/api/Team/my-team` | Get my team info | Bearer |
| GET | `/api/Team/members` | Get team members | Bearer |
| POST | `/api/Team/leave` | Leave team | Bearer |
| DELETE | `/api/Team/delete` | Delete team | Admin |
| POST | `/api/Team/regenerate-invite` | New invite code | Admin |
| DELETE | `/api/Team/remove-member/{id}` | Remove a member | Admin |

### Ideas

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/Idea/submit` | Submit anonymous idea | Bearer |
| GET | `/api/Idea/team-ideas` | Get all team ideas | Bearer |
| GET | `/api/Idea/{ideaId}` | Get single idea | Bearer |
| DELETE | `/api/Idea/{ideaId}` | Delete idea (author only) | Bearer |
| POST | `/api/Idea/{ideaId}/rate` | Rate idea (1-5) | Bearer |
| DELETE | `/api/Idea/{ideaId}/rate` | Remove rating | Bearer |

### Dashboard

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/Dashboard` | Get team dashboard | Bearer |

---

## Authentication Flow

### Register + Verify

```
POST /register  { email, password }
        ↓
OTP sent to email (valid 5 minutes, max 3 requests per 10 minutes)
        ↓
POST /verify-email  { email, otp }
        ↓
{ accessToken, refreshToken }
```

### Login

```
POST /login  { email, password }
        ↓
{ accessToken, refreshToken }
```

### Refresh Token

```
POST /refresh-token  { refreshToken }
        ↓
{ accessToken, refreshToken }  ← new tokens issued, old token revoked
```

### Google / GitHub OAuth

```
GET /login/google
        ↓
Google login page
        ↓
/signin-google  (middleware handles automatically)
        ↓
/external-callback
        ↓
{ accessToken, refreshToken }
```

### Using Bearer Token in Requests

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Security

### JWT Tokens

| Token | Lifetime | Purpose |
|---|---|---|
| Access Token | 15 minutes | Authenticate API requests |
| Refresh Token | 7 days | Get new access token |

### OTP Protection

| Rule | Value |
|---|---|
| OTP expiration | 5 minutes |
| Max requests | 3 per 10 minutes |
| Storage | ASP.NET Identity tokens (hashed) |

### Idea Encryption

```
Submit idea:
"My amazing idea"
        ↓  AES-256 Encrypt (random IV per encryption)
"aGVsbG8gd29ybGQ..."  ← stored in DB

View idea:
"aGVsbG8gd29ybGQ..."
        ↓  AES-256 Decrypt
"My amazing idea"  ← shown to team members only
```

### Anonymity

- `UserId` is stored in the database but **never returned** in API responses
- `IdeaResponseDto` contains no author field
- Ratings are shown as averages only — never linked to a specific user

---

## Database Schema

```
AspNetUsers
├── Id
├── Email
├── TeamId (FK → Teams, nullable)
├── IsVerified
├── OtpExpiration
├── OtpRequestCount
└── OtpRequestWindowStart

Teams
├── Id
├── Name
├── InviteCode (unique index)
├── AdminId (FK → AspNetUsers)
└── CreatedAt

Ideas
├── Id
├── EncryptedTitle
├── EncryptedContent
├── TeamId (FK → Teams)
├── UserId (FK → AspNetUsers)
└── CreatedAt

Ratings
├── Id
├── Score (1 to 5)
├── IdeaId (FK → Ideas)
├── UserId (FK → AspNetUsers)
└── Unique constraint on (UserId + IdeaId)

RefreshTokens
├── Id
├── Token
├── ExpiresAt
├── CreatedAt
├── IsRevoked
└── UserId (FK → AspNetUsers)
```

---

## Design Patterns Used

| Pattern | Where |
|---|---|
| Repository Pattern | `IGenericRepository`, `TeamRepository`, `IdeaRepository`, `RatingRepository` |
| Unit of Work | `IUnitOfWork`, `UnitOfWork` |
| Dependency Injection | All services registered via interfaces in `Program.cs` |
| Interface Segregation | `IAuthService`, `ITeamService`, `IIdeaService`, `IDashboardService` |
| Clean Architecture | 4 separate projects with strict dependency rules |
| Options Pattern | `EmailSettings`, `JwtSettings` from `appsettings.json` |

---

## Branding

**BlindIdea** — *Innovation without ego.*

| Color | Hex | Usage |
|---|---|---|
| Primary Red | `#E8003D` | Brand color, buttons, accents |
| Background | `#000000` | App background |
| White | `#FFFFFF` | Primary text |
| Dark Card | `#1A1A1A` | Cards, surfaces |
| Border | `#2A2A2A` | Subtle borders |
| Muted | `#AAAAAA` | Secondary text |

---

*Built with ❤️ — Ahmed Ayad*