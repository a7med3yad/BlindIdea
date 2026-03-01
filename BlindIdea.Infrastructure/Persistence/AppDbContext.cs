using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Idea> Ideas => Set<Idea>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("dbo");

            ConfigureUser(modelBuilder);
            ConfigureTeam(modelBuilder);
            ConfigureIdea(modelBuilder);
            ConfigureRating(modelBuilder);
            ConfigureRefreshToken(modelBuilder);
            ConfigureEmailVerificationToken(modelBuilder);
        }

        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.TeamId)
                      .IsRequired(false); // Guid?

                entity.Property(u => u.IsDeleted)
                      .HasDefaultValue(false);

                entity.HasOne(u => u.Team)
                      .WithMany(t => t.Members)
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(u => !u.IsDeleted);
            });
        }

        private static void ConfigureTeam(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(t => t.Name)
                      .IsUnique();

                // Admin relationship
                entity.Property(t => t.AdminId)
                      .IsRequired();

                entity.HasOne(t => t.Admin)
                      .WithMany()
                      .HasForeignKey(t => t.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Navigation(t => t.Members)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);

                entity.Navigation(t => t.Ideas)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
        }

        private static void ConfigureIdea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Idea>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(i => i.Description)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(i => i.IsAnonymous)
                      .IsRequired();

                entity.Property(i => i.UserId)
                      .IsRequired();

                entity.HasOne(i => i.User)
                      .WithMany()
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Team)
                      .WithMany(t => t.Ideas)
                      .HasForeignKey(i => i.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Navigation(i => i.Ratings)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
        }

        private static void ConfigureRating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Value)
                      .IsRequired();

                entity.HasCheckConstraint(
                    "CK_Rating_Value",
                    "Value >= 1 AND Value <= 5"
                );

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.IdeaId)
                      .IsRequired();

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
            });
        }

        private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.TokenHash)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(rt => rt.JwtId)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(rt => rt.UserId)
                      .IsRequired();

                entity.Property(rt => rt.ExpiresAt)
                      .IsRequired();

                entity.Property(rt => rt.CreatedAt)
                      .IsRequired();

                entity.Property(rt => rt.CreatedByIp)
                      .HasMaxLength(45); // IPv6 max length

                entity.Property(rt => rt.RevokedByIp)
                      .HasMaxLength(45);

                entity.Property(rt => rt.IsUsed)
                      .IsRequired()
                      .HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.JwtId);
                entity.HasIndex(rt => rt.ExpiresAt);
            });
        }

        private static void ConfigureEmailVerificationToken(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(evt => evt.Id);

                entity.Property(evt => evt.TokenHash)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(evt => evt.UserId)
                      .IsRequired();

                entity.Property(evt => evt.ExpiresAt)
                      .IsRequired();

                entity.Property(evt => evt.CreatedAt)
                      .IsRequired();

                entity.HasOne(evt => evt.User)
                      .WithMany()
                      .HasForeignKey(evt => evt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(evt => evt.UserId);
                entity.HasIndex(evt => evt.ExpiresAt);
            });
        }
    }
}
