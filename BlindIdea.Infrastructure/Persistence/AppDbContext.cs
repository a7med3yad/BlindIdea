using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureTeam(modelBuilder);
        ConfigureIdea(modelBuilder);
        ConfigureRating(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
    }

    // ?? User ?????????????????????????????????????????????????????????????????
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasQueryFilter(u => !u.IsDeleted);
        });
    }

    // ?? Team ?????????????????????????????????????????????????????????????????
    // Members are a many-to-many with User through the join table "TeamMembers".
    // Team.Members and Team.Ideas are public fields (not properties) — EF needs
    // PropertyAccessMode.Field to access them correctly.
    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.AdminId).IsRequired();

            // IsDeleted is bool? on Team — map it with a default
            entity.Property(t => t.IsDeleted).HasDefaultValue(false);

            // IsAdmin / IsMember are computed view flags, not stored columns
            entity.Ignore(t => t.IsAdmin);
            entity.Ignore(t => t.IsMember);

            entity.HasOne(t => t.Admin)
                .WithMany()
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Members: many-to-many User <-> Team via implicit join table
            entity.HasMany<User>(t => (IEnumerable<User>)t.Members)
                .WithMany()
                .UsingEntity(j => j.ToTable("TeamMembers"));

            // Ideas: one Team -> many Ideas (configured on Idea side too)
            entity.HasMany<Idea>(t => (IEnumerable<Idea>)t.Ideas)
                .WithOne(i => i.Team)
                .HasForeignKey(i => i.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(t => t.IsDeleted == false);
            entity.HasIndex(t => t.AdminId);
        });
    }

    // ?? Idea ?????????????????????????????????????????????????????????????????
    // Idea._ratings is a private backing field — expose it via field accessor.
    private static void ConfigureIdea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Idea>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).IsRequired().HasMaxLength(2000);
            entity.Property(i => i.UserId).IsRequired();
            entity.Property(i => i.IsDeleted).HasDefaultValue(false);

            entity.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ratings navigation uses the private _ratings backing field
            entity.HasMany<Rating>("_ratings")
                .WithOne(r => r.Idea)
                .HasForeignKey(r => r.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(i => !i.IsDeleted);
            entity.HasIndex(i => i.UserId);
            entity.HasIndex(i => i.TeamId);
        });
    }

    // ?? Rating ????????????????????????????????????????????????????????????????
    // Rating has no Id — use composite key (IdeaId, UserId).
    // One user can only rate each idea once (enforced by PK + unique index).
    private static void ConfigureRating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rating>(entity =>
        {
            // Composite PK — no separate Id column needed
            entity.HasKey(r => new { r.IdeaId, r.UserId });

            entity.Property(r => r.Value).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(500);
            entity.Property(r => r.IsDeleted).HasDefaultValue(false);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_Rating_Value", "Value >= 1 AND Value <= 5"));

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Idea side is configured via the private _ratings field above
            entity.HasQueryFilter(r => !r.IsDeleted);
        });
    }

    // ?? RefreshToken ??????????????????????????????????????????????????????????
    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).IsRequired().HasMaxLength(500);
            entity.Property(r => r.UserId).IsRequired();

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.Token).IsUnique();

            // Filter out tokens belonging to soft-deleted users
            entity.HasQueryFilter(rt => !rt.User.IsDeleted);
        });
    }

    // ?? Audit fields ??????????????????????????????????????????????????????????
    // Only User has CreatedAt/UpdatedAt/DeletedAt — other entities don't, so we
    // only auto-update User here. SaveChangesAsync is still overridden so this
    // can be extended easily when you add audit fields to other entities.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateUserAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateUserAuditFields()
    {
        var now = DateTime.UtcNow;

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
                        entry.Entity.DeletedAt = now;
                    else if (!entry.Entity.IsDeleted)
                        entry.Entity.DeletedAt = null;
                    break;
            }
        }
    }
}
