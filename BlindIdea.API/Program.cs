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

            // ---------------- Services ----------------
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.Configure<JwtOptions>(
                configuration.GetSection("Jwt") // ✅ fixed section name
            );
            builder.Services.Configure<EmailOptions>(
                configuration.GetSection("Email"));

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

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

            // ---------------- Controllers & Swagger ----------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ---------------- DbContext ----------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );

            var app = builder.Build();

            // ---------------- Middleware ----------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.ApplyMigrations(); // optional: applies DB migrations automatically
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // ✅ required
            app.UseAuthorization();  // ✅ required

            app.MapControllers();

            app.Run();
        }
    }
}
