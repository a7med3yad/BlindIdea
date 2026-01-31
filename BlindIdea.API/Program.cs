using BlindIdea.Core.Entities;
using BlindIdea.Infrastructure.Data;
using BlindIdea.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------------- Controllers & Swagger ----------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ---------------- DbContext ----------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
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

            app.MapControllers();

            app.Run();
        }
    }
}
