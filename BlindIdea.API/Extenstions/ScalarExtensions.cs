using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace BlindIdea.API.Extensions;

public static class ScalarExtensions
{
    /// <summary>
    /// Registers the OpenAPI document generator with Bearer security
    /// and maps the Scalar UI middleware.
    /// </summary>
    public static IServiceCollection AddScalarDocs(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title       = "BlindIdea API";
                document.Info.Version     = "v1";
                document.Info.Description = "Blind idea submission and rating platform.";
                return Task.CompletedTask;
            });

            // Service-activated transformer — IAuthenticationSchemeProvider is
            // resolved from DI automatically; no manual registration needed.
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }

    /// <summary>
    /// Maps the Scalar API reference UI at /scalar/{documentName}.
    /// Call this after app.MapOpenApi().
    /// </summary>
    public static WebApplication UseScalarDocs(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("BlindIdea API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
