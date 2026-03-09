using BlindIdea.Application.Services.Implementations;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Common;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlindIdea.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIdeaService, IdeaService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IRatingService, RatingService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
