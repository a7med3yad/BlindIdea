using BlindIdea.Application.Services.Implementations;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Common.Options;
using BlindIdea.Infrastructure.Data;
using BlindIdea.Infrastructure.Extensions;
using BlindIdea.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.Configure<JwtOptions>(
                configuration.GetSection("Jwt")
            );

            builder.Services.Configure<EmailOptions>(
                configuration.GetSection("Email")
            );

            // ================= IDENTITY =================

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ================= JWT AUTH =================

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtKey = configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    throw new InvalidOperationException("JWT Key is missing in configuration");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    )
                };
            });

            // ================= CONTROLLERS =================

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ================= SWAGGER =================

            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellation) =>
                {
                    document.Info = new()
                    {
                        Title = "BlindIdea API",
                        Version = "v1",
                        Description = "BlindIdea Backend API Documentation"
                    };
                    return Task.CompletedTask;
                });
            });

            // ================= DATABASE =================

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"))
            );

            var app = builder.Build();

            // ================= MIDDLEWARE =================

            // Add exception handling middleware
            app.UseExceptionHandler("/error");

            // Enable Swagger in development only (disable in production for security)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlindIdea API v1");
                    c.RoutePrefix = "swagger"; // URL: /swagger
                });
            }

            // Apply migrations only in development
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    app.ApplyMigrations();
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while applying migrations");
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
