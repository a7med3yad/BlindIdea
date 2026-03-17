using BlindIdea.API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BlindIdea.API.Core
{
    public class AppDbContext: IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions options)
       : base(options)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
