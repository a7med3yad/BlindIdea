using BlindIdea.API.Middleware;
using BlindIdea.Application.Services.Implementations;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Common.Options;
using BlindIdea.Infrastructure.Data;
using BlindIdea.Infrastructure.Implementation;
using BlindIdea.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace BlindIdea.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // ================= SERVICES =================

            // Application Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITeamService, TeamService>();
            builder.Services.AddScoped<IIdeaService, IdeaService>();
            builder.Services.AddScoped<IRatingService, RatingService>();
            
            // Infrastructure Services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            
            // Repository & Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Configuration Options
            builder.Services.Configure<JwtOptions>(
                configuration.GetSection("Jwt"));

            builder.Services.Configure<EmailOptions>(
                configuration.GetSection("Email"));

            // ================= DATABASE =================

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            // ================= IDENTITY =================

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Configure password rules
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                
                // User settings
                options.User.RequireUniqueEmail = true;
            });

            // ================= JWT =================

            var jwtKey = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new Exception("JWT Key is missing in appsettings.json");

            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero // No clock skew tolerance
                };
            });

            builder.Services.AddAuthorization();

            // ================= CORS =================

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // ================= CONTROLLERS =================

            builder.Services.AddControllers();

            // ================= LOGGING =================

            builder.Services.AddLogging(config =>
            {
                config.ClearProviders();
                config.AddConsole();
                if (builder.Environment.IsDevelopment())
                    config.AddDebug();
            });

            // ================= OPENAPI (.NET 10 BUILT-IN) =================

            builder.Services.AddOpenApi();

            // ================= SWAGGER =================

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ================= MIDDLEWARE =================

            // Exception handling - must be first
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Built-in OpenAPI endpoint
                app.MapOpenApi();

                // Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BlindIdea API v1");
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ================= DATABASE INITIALIZATION =================

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    // Apply pending migrations
                    dbContext.Database.Migrate();
                    
                    // Seed roles
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    SeedRolesAsync(roleManager).Wait();
                    
                    logger.LogInformation("Database initialization completed successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize database");
                    throw;
                }
            }

            app.Run();
        }

        /// <summary>
        /// Seeds initial roles into the database if they don't exist.
        /// </summary>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "TeamAdmin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
