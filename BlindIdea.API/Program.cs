using BlindIdea.API.Extensions;
using BlindIdea.API.Middleware;
using BlindIdea.Application.Extensions;
using BlindIdea.Core.Entities;
using BlindIdea.Infrastructure.Extensions;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ═══════════════════════════════════════════════════════════
//  1. LAYER REGISTRATIONS
// ═══════════════════════════════════════════════════════════
builder.Services.AddApplication(config);
builder.Services.AddInfrastructure(config);

// ═══════════════════════════════════════════════════════════
//  2. IDENTITY
// ═══════════════════════════════════════════════════════════
builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false; // handled manually in AuthService
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ═══════════════════════════════════════════════════════════
//  3. JWT AUTHENTICATION
// ═══════════════════════════════════════════════════════════
var jwtKey = config["Jwt:Key"]
    ?? config["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Key/Secret is not configured. Add 'Jwt:Key' to appsettings.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════
//  4. CORS
// ═══════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",  // React (CRA)
                "https://localhost:3000",
                "http://localhost:5173",  // Vite
                "https://localhost:5173",
                "http://localhost:4200",  // Angular
                "https://localhost:4200",
                "http://localhost:5218",  // .NET dev server
                "https://localhost:7024"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ═══════════════════════════════════════════════════════════
//  5. CONTROLLERS & JSON
// ═══════════════════════════════════════════════════════════
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ═══════════════════════════════════════════════════════════
//  6. API DOCS — Scalar (replaces Swashbuckle/Swagger UI)
//     Visit: /scalar/v1
// ═══════════════════════════════════════════════════════════
builder.Services.AddScalarDocs();

// ═══════════════════════════════════════════════════════════
//  7. LOGGING
// ═══════════════════════════════════════════════════════════
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    if (builder.Environment.IsDevelopment())
        logging.AddDebug();
});

// ───────────────────────────────────────────────────────────
var app = builder.Build();
// ───────────────────────────────────────────────────────────

// ═══════════════════════════════════════════════════════════
//  8. MIDDLEWARE PIPELINE
//     Order matters — do not rearrange without good reason.
// ═══════════════════════════════════════════════════════════
app.UseMiddleware<ExceptionHandlingMiddleware>(); // must be first to catch all errors

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");                     // before auth so pre-flight OPTIONS works

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();                             // serves /openapi/v1.json
    app.MapScalarApiReference();                  // serves /scalar/v1
}
else
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ═══════════════════════════════════════════════════════════
//  9. DATABASE INITIALISATION  (runs once on startup)
// ═══════════════════════════════════════════════════════════
await InitialiseDatabaseAsync(app);

Console.WriteLine("BlindIdea API is running.");
await app.RunAsync();

// ── Local function keeps the startup block clean ───────────
static async Task InitialiseDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        await db.Database.MigrateAsync();

        var roles = new[] { "Admin", "TeamAdmin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        logger.LogInformation("Database initialised successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialisation failed.");
        throw;
    }
}
