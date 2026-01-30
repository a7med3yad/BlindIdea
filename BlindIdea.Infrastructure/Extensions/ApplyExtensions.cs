using System;
using BlindIdea.Infrastructure.Data;
using Microsoft.AspNetCore.Builder; // Fixes CS0246
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlindIdea.Infrastructure.Extensions
{
    public static class ApplyExtensions
    {
        // Extension method for IApplicationBuilder
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
    }
}
