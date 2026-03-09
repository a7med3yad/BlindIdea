using BlindIdea.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlindIdea.Infrastructure.Extensions
{
    public static class ApplyExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
    }
}