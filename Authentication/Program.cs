using BlindIdea.Application.Implementation.Auth;
using BlindIdea.Application.Services.Abstraction.Auth;
using BlindIdea.Application.Services.Abstraction.Dashboards;
using BlindIdea.Application.Services.Abstraction.Ideas;
using BlindIdea.Application.Services.Abstraction.Teams;
using BlindIdea.Application.Services.Implementation.Dashboards;
using BlindIdea.Application.Services.Implementation.Ideas;
using BlindIdea.Application.Services.Implementation.Teams;
using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Auth;
using BlindIdea.Infrastructure.Implementation.Cache;
using BlindIdea.Infrastructure.Implementation.Encryption;
using BlindIdea.Infrastructure.Implementation.UnitOfWorks;
using BlindIdea.Infrastructure.Persistence;
using BlindIdea.API.Health;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Net.Mime;
using System.Text;
using System.Threading.RateLimiting;

namespace BlindIdea.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
                options.UseUtcTimestamp = true;
            });

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
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestMethod
                                       | HttpLoggingFields.RequestPath
                                       | HttpLoggingFields.ResponseStatusCode
                                       | HttpLoggingFields.Duration;
            });

            builder.Services.AddProblemDetails();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });

            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("db");

            var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
            if (!builder.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(dataProtectionKeysPath))
            {
                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                    .SetApplicationName("BlindIdea");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keyValue = builder.Configuration["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyValue))
                {
                    throw new InvalidOperationException("Missing configuration value: Jwt:Key (set via environment variable Jwt__Key in Production).");
                }

                var key = Encoding.UTF8.GetBytes(keyValue);
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
                options.ClientId =
                    builder.Configuration["Authentication:GitHub:ClientId"]
                    ?? builder.Configuration["Authentication:Github:ClientId"]
                    ?? throw new InvalidOperationException("Missing configuration value: Authentication:GitHub:ClientId");

                options.ClientSecret =
                    builder.Configuration["Authentication:GitHub:ClientSecret"]
                    ?? builder.Configuration["Authentication:Github:ClientSecret"]
                    ?? throw new InvalidOperationException("Missing configuration value: Authentication:GitHub:ClientSecret");

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
            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddEndpointsApiExplorer();

            // Fixed CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("BlindIdeaPolicy", policy =>
                {
                    var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                    if (configuredOrigins is { Length: > 0 })
                    {
                        policy
                            .WithOrigins(configuredOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                    else
                    {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://localhost:3001",
                            "https://blindidea.duckdns.org",
                            "http://blindidea.duckdns.org",
                            "http://blindidea-frontend-557643339293.s3-website-us-east-1.amazonaws.com"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    }
                });
            });

            var app = builder.Build();

            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            else
            {
                app.UseHsts();
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;
                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context,
                            Exception = exceptionHandler?.Error,
                            ProblemDetails =
                            {
                                Title = "An unexpected error occurred.",
                                Status = StatusCodes.Status500InternalServerError
                            }
                        });
                    });
                });
            }

            app.UseHttpLogging();
            app.UseRateLimiter();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = new[] { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            // IMPORTANT: Disable HTTPS redirection in Production (Nginx handles HTTPS)
            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            // Use CORS - only one policy
            app.UseCors("BlindIdeaPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Add health check endpoint for monitoring
            app.MapGet("/api/health", () => Results.Ok(new 
            { 
                status = "Healthy", 
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName
            }));
            app.MapHealthChecks("/api/health").AllowAnonymous();
            app.MapGet("/api/health", () => Results.Ok(new 
            { 
                status = "Healthy", 
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName
            }));

            app.Run();
        }
    }
}
