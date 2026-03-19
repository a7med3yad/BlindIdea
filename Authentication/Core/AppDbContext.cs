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
        public DbSet<Team> Teams { get; set; }
        public DbSet<Idea> Ideas { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.HasOne(t=>t.Admin)
                      .WithMany()
                      .HasForeignKey(t=>t.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(t=>t.Members)
                      .WithOne(u=>u.Team)
                      .HasForeignKey(u=>u.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(t => t.InviteCode)
                      .IsUnique();
            });

            builder.Entity<Idea>(entity =>
            {
                entity.HasKey(i=>i.Id);

                entity.HasOne(i=>i.Team)
                      .WithMany(t=>t.Ideas)
                      .HasForeignKey(i=>i.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i=>i.User)
                      .WithMany()
                      .HasForeignKey(i=>i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            builder.Entity<Rating>(entity =>
            {
                entity.HasKey(i=>i.Id);

                entity.HasOne(r=>r.Idea)
                      .WithMany(i=>i.Ratings)
                      .HasForeignKey(r=>r.IdeaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r=>r.User)
                      .WithMany()
                      .HasForeignKey(r=>r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r=> new {r.UserId,r.IdeaId}) 
                      .IsUnique();
            });
        }
    }
}
