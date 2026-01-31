using BlindIdea.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BlindIdea.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Idea> Ideas { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== User =====================
            modelBuilder.HasDefaultSchema("identity");
            modelBuilder.Entity<User>(entity =>
            {


                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.TeamId)
                      .IsRequired();

                entity.Property(u => u.IsDeleted)
                      .HasDefaultValue(false);

                entity.HasOne(u => u.Team)
                      .WithMany(t => t.Members)
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Global filter to ignore deleted users
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // ===================== Team =====================
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(t => t.Name)
                      .IsUnique();

                entity.Property(t => t.IsDeleted)
                      .HasDefaultValue(false);

                // Global filter to ignore deleted teams
                entity.HasQueryFilter(t => !t.IsDeleted);
            });

            // ===================== Idea =====================
            modelBuilder.Entity<Idea>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(i => i.Description)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(i => i.TeamId)
                      .IsRequired();

                entity.Property(i => i.IsDeleted)
                      .HasDefaultValue(false);

                entity.HasOne(i => i.Team)
                      .WithMany(t => t.Ideas)
                      .HasForeignKey(i => i.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Global filter to ignore deleted ideas
                entity.HasQueryFilter(i => !i.IsDeleted);
            });

            // ===================== Rating =====================
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Score)
                      .IsRequired();

                entity.HasCheckConstraint(
                    "CK_Ratings_Score",
                    "[Score] BETWEEN 1 AND 5"
                );

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.IdeaId)
                      .IsRequired();

                entity.Property(r => r.IsDeleted)
                      .HasDefaultValue(false);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Idea)
                      .WithMany(i => i.Ratings)
                      .HasForeignKey(r => r.IdeaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.UserId, r.IdeaId })
                      .IsUnique();

                // Global filter to ignore deleted ratings
                entity.HasQueryFilter(r => !r.IsDeleted);
            });
        }
    }
}
