using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BlindIdea.API.Extensions;

/// <summary>
/// Injects the JWT Bearer security scheme into the OpenAPI document.
/// Uses IAuthenticationSchemeProvider so the scheme name is never hard-coded.
/// Compatible with Microsoft.OpenApi v2 / .NET 10 — no Microsoft.OpenApi.Models dependency.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (!authSchemes.Any(s => s.Name == "Bearer"))
            return;

        // Register the Bearer security scheme at document level
        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Enter your JWT access token."
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securitySchemes;

        // Apply as a requirement on every operation
        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
    }
}
