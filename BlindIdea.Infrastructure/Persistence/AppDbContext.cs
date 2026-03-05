using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace BlindIdea.Infrastructure.Data
{
    /// <summary>
    /// Application DbContext for Entity Framework Core.
    /// Configures all entities, relationships, indexes, and constraints.
    /// Implements audit trail pattern with automatic timestamp tracking.
    /// </summary>
    public class AppDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Constructor for DbContext with options.
        /// Initializes base IdentityDbContext with User entity.
        /// </summary>
        /// <param name="options">Options for this context</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ===== DBSETS =====

        /// <summary>
        /// Teams collection.
        /// </summary>
        public DbSet<Team> Teams => Set<Team>();

        /// <summary>
        /// Ideas collection.
        /// </summary>
        public DbSet<Idea> Ideas => Set<Idea>();

        /// <summary>
        /// Ratings collection.
        /// </summary>
        public DbSet<Rating> Ratings => Set<Rating>();

        /// <summary>
        /// Refresh tokens collection.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        /// <summary>
        /// Email verification tokens collection.
        /// </summary>
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

        /// <summary>
        /// Configures the database schema and entity relationships.
        /// Called when model is being created.
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure</param>
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

        /// <summary>
        /// Override SaveChangesAsync to handle audit trail.
        /// Automatically sets CreatedAt, UpdatedAt, and soft delete fields.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities affected</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Override SaveChanges (synchronous) to handle audit trail.
        /// </summary>
        /// <returns>Number of entities affected</returns>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Updates audit fields (CreatedAt, UpdatedAt, DeletedAt) for all changed entities.
        /// Called automatically before SaveChanges.
        /// </summary>
        private void UpdateAuditFields()
        {
            var now = DateTime.UtcNow;

            // Update BaseEntity audit fields
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        
                        // If entity is being marked as deleted
                        if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                        {
                            entry.Entity.DeletedAt = now;
                        }
                        // If entity is being restored from soft delete
                        else if (!entry.Entity.IsDeleted && entry.Entity.DeletedAt != null)
                        {
                            entry.Entity.DeletedAt = null;
                        }
                        break;
                }
            }

            // Update User audit fields (User doesn't inherit BaseEntity but should have audit fields)
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                        {
                            entry.Entity.DeletedAt = now;
                        }
                        else if (!entry.Entity.IsDeleted && entry.Entity.DeletedAt != null)
                        {
                            entry.Entity.DeletedAt = null;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Configures the User entity mapping and relationships.
        /// </summary>
        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                // Properties
                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.TeamId)
                      .IsRequired(false);

                entity.Property(u => u.IsDeleted)
                      .HasDefaultValue(false);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValue(DateTime.UtcNow);

                entity.Property(u => u.UpdatedAt)
                      .HasDefaultValue(DateTime.UtcNow);

                // Relationships
                entity.HasOne(u => u.Team)
                      .WithMany()
                      .HasForeignKey(u => u.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Global query filter (soft delete)
                entity.HasQueryFilter(u => !u.IsDeleted);

                // Indexes
                entity.HasIndex(u => u.Email)
                      .IsUnique();
                entity.HasIndex(u => u.IsDeleted);
            });
        }

        /// <summary>
        /// Configures the Team entity mapping and relationships.
        /// </summary>
        private static void ConfigureTeam(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity =>
            {
                // Key
                entity.HasKey(t => t.Id);

                // Properties
                entity.Property(t => t.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(t => t.Description)
                      .HasMaxLength(500);

                entity.Property(t => t.AdminId)
                      .IsRequired();

                entity.Property(t => t.IsDeleted)
                      .HasDefaultValue(false);

                // Relationships
                entity.HasOne(t => t.Admin)
                      .WithMany()
                      .HasForeignKey(t => t.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Navigation property configuration
                entity.Navigation(t => t.Members)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);

                entity.Navigation(t => t.Ideas)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);

                // Global query filter
                entity.HasQueryFilter(t => !t.IsDeleted);

                // Indexes
                entity.HasIndex(t => t.Name)
                      .IsUnique(false);
                entity.HasIndex(t => t.AdminId);
                entity.HasIndex(t => t.IsDeleted);
            });
        }

        /// <summary>
        /// Configures the Idea entity mapping and relationships.
        /// </summary>
        private static void ConfigureIdea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Idea>(entity =>
            {
                // Key
                entity.HasKey(i => i.Id);

                // Properties
                entity.Property(i => i.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(i => i.Description)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(i => i.IsAnonymous)
                      .HasDefaultValue(false);

                entity.Property(i => i.UserId)
                      .IsRequired();

                entity.Property(i => i.TeamId)
                      .IsRequired(false);

                entity.Property(i => i.IsDeleted)
                      .HasDefaultValue(false);

                // Relationships
                entity.HasOne(i => i.User)
                      .WithMany()
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Team)
                      .WithMany(t => t.Ideas)
                      .HasForeignKey(i => i.TeamId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Navigation property configuration
                entity.Navigation(i => i.Ratings)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);

                // Global query filter
                entity.HasQueryFilter(i => !i.IsDeleted);

                // Indexes
                entity.HasIndex(i => i.UserId);
                entity.HasIndex(i => i.TeamId);
                entity.HasIndex(i => i.CreatedAt);
                entity.HasIndex(i => i.IsDeleted);
            });
        }

        /// <summary>
        /// Configures the Rating entity mapping and relationships.
        /// </summary>
        private static void ConfigureRating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>(entity =>
            {
                // Key
                entity.HasKey(r => r.Id);

                // Properties
                entity.Property(r => r.Value)
                      .IsRequired();

                entity.Property(r => r.Comment)
                      .HasMaxLength(500);

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.IdeaId)
                      .IsRequired();

                entity.Property(r => r.IsDeleted)
                      .HasDefaultValue(false);

                // Check constraint - using .NET 8 fluent API
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Rating_Value",
                    "Value >= 1 AND Value <= 5"
                ));

                // Relationships
                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Idea)
                      .WithMany(i => i.Ratings)
                      .HasForeignKey(r => r.IdeaId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Global query filter
                entity.HasQueryFilter(r => !r.IsDeleted);

                // Indexes - ensure one rating per user per idea
                entity.HasIndex(r => new { r.UserId, r.IdeaId })
                      .IsUnique(false);

                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.IdeaId);
                entity.HasIndex(r => r.IsDeleted);
            });
        }

        /// <summary>
        /// Configures the RefreshToken entity mapping and relationships.
        /// </summary>
        private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                // Key
                entity.HasKey(rt => rt.Id);

                // Properties
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

                entity.Property(rt => rt.CreatedByIp)
                      .HasMaxLength(45);

                entity.Property(rt => rt.RevokedByIp)
                      .HasMaxLength(45);

                entity.Property(rt => rt.IsUsed)
                      .HasDefaultValue(false);

                // Relationships
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.JwtId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.TokenHash);
                entity.HasIndex(rt => rt.IsRevoked);
            });
        }

        /// <summary>
        /// Configures the EmailVerificationToken entity mapping and relationships.
        /// </summary>
        private static void ConfigureEmailVerificationToken(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                // Key
                entity.HasKey(evt => evt.Id);

                // Properties
                entity.Property(evt => evt.TokenHash)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(evt => evt.UserId)
                      .IsRequired();

                entity.Property(evt => evt.ExpiresAt)
                      .IsRequired();

                // Relationships
                entity.HasOne(evt => evt.User)
                      .WithMany()
                      .HasForeignKey(evt => evt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(evt => evt.UserId);
                entity.HasIndex(evt => evt.ExpiresAt);
                entity.HasIndex(evt => evt.TokenHash);
                entity.HasIndex(evt => evt.IsUsed);
            });
        }
    }
}
