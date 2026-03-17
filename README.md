# 💡 IdeaVault — Backend API

> **Stack:** .NET 10 · Four-Layer Architecture (API · Core · Application · Infrastructure)

---

## Table of Contents

- [💡 IdeaVault — Backend API](#-ideavault--backend-api)
  - [Table of Contents](#table-of-contents)
  - [Project Overview](#project-overview)
  - [Architecture](#architecture)
  - [Project Structure](#project-structure)
  - [Layer Responsibilities](#layer-responsibilities)
    - [Layer 1 — API (`IdeaVault.API`)](#layer-1--api-ideavaultapi)
    - [Layer 2 — Core (`IdeaVault.Core`)](#layer-2--core-ideavaultcore)
    - [Layer 3 — Application (`IdeaVault.Application`)](#layer-3--application-ideavaultapplication)
    - [Layer 4 — Infrastructure (`IdeaVault.Infrastructure`)](#layer-4--infrastructure-ideavaultinfrastructure)
  - [Domain Models](#domain-models)
  - [Authentication Flow](#authentication-flow)
    - [Registration](#registration)
    - [Login](#login)
    - [Forgot Password](#forgot-password)
  - [API Endpoints](#api-endpoints)
    - [Auth](#auth)
    - [Teams](#teams)
    - [Ideas](#ideas)
    - [Dashboard](#dashboard)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Create the Solution](#create-the-solution)
    - [Install NuGet Packages](#install-nuget-packages)
  - [Configuration](#configuration)
  - [Database Setup](#database-setup)
  - [Running the Project](#running-the-project)
  - [Testing](#testing)

---

## Project Overview

IdeaVault is a collaborative idea management platform. Users register, join teams, and anonymously submit ideas that peers can rate. The backend is built with **.NET 10** using a clean four-layer architecture.

**Core Features:**
- Email-based registration with OTP verification
- JWT access + refresh token authentication
- Password reset via email OTP
- Team creation — one team per user, creator becomes admin
- Admin can add members to the team
- Anonymous idea submission — author identity is hidden from all consumers
- Idea rating (1–5 stars) by any user including the author
- Dashboard with aggregated idea analytics

---

## Architecture

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← Controllers, Middleware, DTOs, Filters
├─────────────────────────────────────────┤
│           Application Layer             │  ← Use Cases, CQRS Commands/Queries, Validators
├─────────────────────────────────────────┤
│             Core Layer                  │  ← Entities, Interfaces, Domain Events, Enums
├─────────────────────────────────────────┤
│         Infrastructure Layer            │  ← EF Core, Repositories, Email, JWT, Caching
└─────────────────────────────────────────┘
```

Dependencies flow **inward only**: API → Application → Core ← Infrastructure

---

## Project Structure

```
IdeaVault/
├── IdeaVault.sln
│
├── src/
│   ├── IdeaVault.API/                    # Layer 1 — Presentation
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── TeamsController.cs
│   │   │   ├── IdeasController.cs
│   │   │   └── DashboardController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Teams/
│   │   │   ├── Ideas/
│   │   │   └── Dashboard/
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   ├── IdeaVault.Core/                   # Layer 2 — Domain Core
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Team.cs
│   │   │   ├── TeamMember.cs
│   │   │   ├── Idea.cs
│   │   │   ├── IdeaRating.cs
│   │   │   └── OtpCode.cs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── ITeamRepository.cs
│   │   │   │   ├── IIdeaRepository.cs
│   │   │   │   └── IOtpRepository.cs
│   │   │   ├── Services/
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IJwtService.cs
│   │   │   │   └── IOtpService.cs
│   │   │   └── IUnitOfWork.cs
│   │   ├── Enums/
│   │   │   ├── OtpPurpose.cs
│   │   │   └── TeamRole.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── NotFoundException.cs
│   │       └── UnauthorizedException.cs
│   │
│   ├── IdeaVault.Application/            # Layer 3 — Application Logic
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── VerifyRegisterOtpCommand.cs
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── ForgotPasswordCommand.cs
│   │   │   │   ├── ResetPasswordCommand.cs
│   │   │   │   └── RefreshTokenCommand.cs
│   │   │   └── Handlers/
│   │   │       └── ...Handlers.cs
│   │   ├── Teams/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTeamCommand.cs
│   │   │   │   └── AddMemberCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetTeamQuery.cs
│   │   ├── Ideas/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateIdeaCommand.cs
│   │   │   │   └── RateIdeaCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetIdeasQuery.cs
│   │   │       └── GetIdeaByIdQuery.cs
│   │   ├── Dashboard/
│   │   │   └── Queries/
│   │   │       └── GetDashboardInsightsQuery.cs
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   └── DependencyInjection.cs
│   │
│   └── IdeaVault.Infrastructure/         # Layer 4 — Infrastructure
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/
│       │   │   ├── UserConfiguration.cs
│       │   │   ├── TeamConfiguration.cs
│       │   │   ├── IdeaConfiguration.cs
│       │   │   └── IdeaRatingConfiguration.cs
│       │   ├── Repositories/
│       │   │   ├── UserRepository.cs
│       │   │   ├── TeamRepository.cs
│       │   │   ├── IdeaRepository.cs
│       │   │   └── OtpRepository.cs
│       │   ├── Migrations/
│       │   └── UnitOfWork.cs
│       ├── Services/
│       │   ├── JwtService.cs
│       │   ├── OtpService.cs
│       │   └── EmailService.cs
│       ├── Caching/
│       │   └── CacheService.cs
│       └── DependencyInjection.cs
│
└── tests/
    ├── IdeaVault.UnitTests/
    └── IdeaVault.IntegrationTests/
```

---

## Layer Responsibilities

### Layer 1 — API (`IdeaVault.API`)

The entry point of the application. Handles HTTP concerns only.

- **Controllers** receive HTTP requests and delegate to MediatR commands/queries
- **DTOs** define request/response shapes (never expose domain entities)
- **Middleware** handles cross-cutting concerns: exceptions, logging, correlation IDs
- **Filters** run validation before hitting the controller action
- **Program.cs** wires up all services and pipeline

```csharp
// Program.cs — .NET 10 minimal hosting
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()           // Application layer DI
    .AddInfrastructure(builder.Configuration)  // Infrastructure layer DI
    .AddApiServices();          // API-specific: Swagger, CORS, Auth middleware

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

### Layer 2 — Core (`IdeaVault.Core`)

The **heart of the system**. Contains pure domain logic with zero external dependencies.

- **Entities** are plain C# classes with domain behavior
- **Interfaces** define contracts that Infrastructure must implement (Dependency Inversion)
- **Enums** represent domain states
- **Exceptions** model domain rule violations

```csharp
// Core/Entities/Idea.cs
public class Idea
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Tags { get; private set; }
    public Guid AuthorId { get; private set; }   // stored, but NEVER returned in queries
    public Guid TeamId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<IdeaRating> Ratings { get; private set; } = [];

    public double AverageRating => Ratings.Any()
        ? Ratings.Average(r => r.Score)
        : 0;

    public static Idea Create(string name, string description, string tags, Guid authorId, Guid teamId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Idea name is required.");
        return new Idea
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Tags = tags,
            AuthorId = authorId,
            TeamId = teamId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

### Layer 3 — Application (`IdeaVault.Application`)

Orchestrates use cases using the **CQRS + MediatR** pattern.

- **Commands** mutate state (Register, CreateIdea, RateIdea)
- **Queries** read state (GetIdeas, GetDashboard)
- **Handlers** implement the business logic for each command/query
- **Behaviors** add cross-cutting pipeline steps (validation, logging)
- **Validators** use FluentValidation

```csharp
// Application/Ideas/Commands/CreateIdeaCommand.cs
public record CreateIdeaCommand(
    string Name,
    string Description,
    string Tags,
    Guid TeamId
) : IRequest<Guid>;

// Application/Ideas/Handlers/CreateIdeaHandler.cs
public class CreateIdeaHandler(IIdeaRepository ideas, IUnitOfWork uow, ICurrentUser currentUser)
    : IRequestHandler<CreateIdeaCommand, Guid>
{
    public async Task<Guid> Handle(CreateIdeaCommand cmd, CancellationToken ct)
    {
        var idea = Idea.Create(cmd.Name, cmd.Description, cmd.Tags, currentUser.Id, cmd.TeamId);
        await ideas.AddAsync(idea, ct);
        await uow.SaveChangesAsync(ct);
        return idea.Id;
    }
}
```

---

### Layer 4 — Infrastructure (`IdeaVault.Infrastructure`)

Implements all external concerns: database, email, caching, JWT.

- **AppDbContext** — EF Core 10 with PostgreSQL (or SQL Server)
- **Repositories** implement `IRepository<T>` from Core
- **JwtService** issues access tokens + refresh tokens
- **OtpService** generates and validates 6-digit codes with TTL
- **EmailService** sends OTP emails via SMTP / SendGrid

```csharp
// Infrastructure/Services/JwtService.cs
public class JwtService(IOptions<JwtSettings> settings) : IJwtService
{
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Secret));
        var token = new JwtSecurityToken(
            issuer: settings.Value.Issuer,
            audience: settings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
```

---

## Domain Models

| Entity | Key Fields |
|---|---|
| `User` | Id, Email, PasswordHash, IsVerified, RefreshToken, RefreshTokenExpiry |
| `OtpCode` | Id, UserId, Code (hashed), Purpose (Register/ResetPassword), ExpiresAt, IsUsed |
| `Team` | Id, Name, AdminUserId, CreatedAt |
| `TeamMember` | TeamId, UserId, Role (Admin/Member) |
| `Idea` | Id, Name, Description, Tags, AuthorId (private), TeamId, CreatedAt |
| `IdeaRating` | Id, IdeaId, RaterId, Score (1–5), CreatedAt |

**Anonymity Rule:** `AuthorId` is stored in the database for auditing, but the application layer **never maps it to any response DTO**. Queries for ideas always omit the author field entirely.

---

## Authentication Flow

### Registration
```
POST /api/auth/register       → saves user (unverified), sends OTP email
POST /api/auth/verify-register → validates OTP, marks user verified, returns tokens
```

### Login
```
POST /api/auth/login          → validates credentials, returns access + refresh tokens
POST /api/auth/refresh        → validates refresh token, issues new token pair
```

### Forgot Password
```
POST /api/auth/forgot-password → sends OTP to registered email
POST /api/auth/reset-password  → validates OTP + new password, returns tokens
```

**Token Strategy:**
- Access Token: JWT, 15-minute TTL, signed with HS256
- Refresh Token: opaque random bytes, 7-day TTL, stored hashed in DB

---

## API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | None | Register with email + password |
| POST | `/api/auth/verify-register` | None | Submit OTP to verify email |
| POST | `/api/auth/login` | None | Login, receive tokens |
| POST | `/api/auth/refresh` | None | Refresh access token |
| POST | `/api/auth/forgot-password` | None | Request password reset OTP |
| POST | `/api/auth/reset-password` | None | Reset password with OTP |

### Teams
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/teams` | Required | Create team (user becomes admin) |
| GET | `/api/teams/me` | Required | Get my team |
| POST | `/api/teams/members` | Admin only | Add member by email |
| GET | `/api/teams/members` | Required | List team members |

**Business Rule:** A user can only belong to one team. Creating a second team returns `409 Conflict`.

### Ideas
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/ideas` | Required | Submit idea (anonymous) |
| GET | `/api/ideas` | Required | List ideas (no author field) |
| GET | `/api/ideas/{id}` | Required | Get idea details |
| POST | `/api/ideas/{id}/rate` | Required | Rate idea (1–5 stars) |

### Dashboard
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/dashboard` | Required | Team idea insights |

**Dashboard Response:**
```json
{
  "totalIdeas": 42,
  "averageRating": 3.8,
  "topRatedIdeas": [...],
  "ratingDistribution": { "1": 3, "2": 7, "3": 12, "4": 14, "5": 6 },
  "ideasPerDay": [...],
  "mostActiveDay": "2025-03-12"
}
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 16+ (or SQL Server 2022)
- SMTP credentials (or SendGrid API key)

### Create the Solution

```bash
# Create solution
dotnet new sln -n IdeaVault

# Create projects
dotnet new webapi -n IdeaVault.API -o src/IdeaVault.API
dotnet new classlib -n IdeaVault.Core -o src/IdeaVault.Core
dotnet new classlib -n IdeaVault.Application -o src/IdeaVault.Application
dotnet new classlib -n IdeaVault.Infrastructure -o src/IdeaVault.Infrastructure

# Add to solution
dotnet sln add src/IdeaVault.API
dotnet sln add src/IdeaVault.Core
dotnet sln add src/IdeaVault.Application
dotnet sln add src/IdeaVault.Infrastructure

# Reference chain: API → Application → Core ← Infrastructure
dotnet add src/IdeaVault.API reference src/IdeaVault.Application
dotnet add src/IdeaVault.Application reference src/IdeaVault.Core
dotnet add src/IdeaVault.Infrastructure reference src/IdeaVault.Core
dotnet add src/IdeaVault.API reference src/IdeaVault.Infrastructure
```

### Install NuGet Packages

```bash
# Application Layer
dotnet add src/IdeaVault.Application package MediatR
dotnet add src/IdeaVault.Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/IdeaVault.Application package AutoMapper

# Infrastructure Layer
dotnet add src/IdeaVault.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/IdeaVault.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/IdeaVault.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/IdeaVault.Infrastructure package MailKit

# API Layer
dotnet add src/IdeaVault.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/IdeaVault.API package Swashbuckle.AspNetCore
dotnet add src/IdeaVault.API package Serilog.AspNetCore
```

---

## Configuration

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=ideavault;Username=postgres;Password=secret"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-chars",
    "Issuer": "IdeaVault",
    "Audience": "IdeaVaultUsers"
  },
  "Email": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "Username": "apikey",
    "Password": "SG.xxxxx",
    "FromAddress": "noreply@ideavault.app",
    "FromName": "IdeaVault"
  },
  "Otp": {
    "ExpiryMinutes": 10
  }
}
```

Use `dotnet user-secrets` for local development to keep credentials out of source control:
```bash
dotnet user-secrets set "Jwt:Secret" "my-local-dev-secret-32-chars-minimum"
```

---

## Database Setup

```bash
# Add initial migration
dotnet ef migrations add InitialCreate \
  --project src/IdeaVault.Infrastructure \
  --startup-project src/IdeaVault.API

# Apply migration
dotnet ef database update \
  --project src/IdeaVault.Infrastructure \
  --startup-project src/IdeaVault.API
```

---

## Running the Project

```bash
cd src/IdeaVault.API
dotnet run
```

Swagger UI available at: `https://localhost:5001/swagger`

---

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Unit tests mock all interfaces from `IdeaVault.Core`. Integration tests use `WebApplicationFactory` with an in-memory or test PostgreSQL database.

---

> Built with ❤️ on .NET 10 — Clean Architecture, no shortcuts.