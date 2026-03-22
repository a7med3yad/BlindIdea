using BlindIdea.Application.Implementation.Auth;
using BlindIdea.Application.Services.Abstraction.Auth;
using BlindIdea.Application.Services.Abstraction.Dashboards;
using BlindIdea.Application.Services.Abstraction.Ideas;
using BlindIdea.Application.Services.Abstraction.Teams;
using BlindIdea.Application.Services.Implementation.Dashboards;
using BlindIdea.Application.Services.Implementation.Ideas;
using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Auth;
using BlindIdea.Infrastructure.Implementation.Cache;
using BlindIdea.Infrastructure.Implementation.Encryption;
using BlindIdea.Infrastructure.Implementation.UnitOfWorks;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;



namespace BlindIdea.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
            builder.Services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(3,
                            TimeSpan.FromSeconds(5), null);
                        sqlOptions.CommandTimeout(60);
                    }
                );
            }, poolSize: 128);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            }).AddGoogle(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                options.CallbackPath = "/signin-google"; 
            })
            .AddGitHub(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
                options.CallbackPath = "/signin-github";
                options.Scope.Add("user:email");
            });


            builder.Services.AddAuthorization(); 

            // Dependency injection
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IOAuthService, OAuthService>();
            builder.Services.AddScoped<IEncryptionService, EncryptionService>();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITeamService, TeamService>();
            builder.Services.AddScoped<IIdeaService, IdeaService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ICacheService,CacheService>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("BlindIdeaPolicy", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",    
                            "http://localhost:5173",    
                            "https://yourdomain.com"    
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }


            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string [] roles = new[] { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
            app.UseCors("BlindIdeaPolicy");


            app.UseHttpsRedirection();

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}