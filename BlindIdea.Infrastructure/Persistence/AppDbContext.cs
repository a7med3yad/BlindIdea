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
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureTeam(modelBuilder);
        ConfigureIdea(modelBuilder);
        ConfigureRating(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureTeamMember(modelBuilder);
    }

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

    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.AdminId).IsRequired();
            entity.Property(t => t.IsDeleted).HasDefaultValue(false);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(t => t.Admin)
                .WithMany()
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(t => t.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(t => t.Ideas).UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasQueryFilter(t => !t.IsDeleted);
            entity.HasIndex(t => t.AdminId);
        });
    }

    private static void ConfigureIdea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Idea>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).IsRequired().HasMaxLength(2000);
            entity.Property(i => i.IsAnonymous).HasDefaultValue(false);
            entity.Property(i => i.UserId).IsRequired();
            entity.Property(i => i.IsDeleted).HasDefaultValue(false);
            entity.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(i => i.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Team)
                .WithMany(t => t.Ideas)
                .HasForeignKey(i => i.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(i => i.Ratings).UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasQueryFilter(i => !i.IsDeleted);
            entity.HasIndex(i => i.UserId);
            entity.HasIndex(i => i.TeamId);
            entity.HasIndex(i => i.CreatedAt);
        });
    }

    private static void ConfigureRating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Value).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(500);
            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.IdeaId).IsRequired();
            entity.Property(r => r.IsDeleted).HasDefaultValue(false);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(r => r.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.ToTable(t => t.HasCheckConstraint("CK_Rating_Value", "Value >= 1 AND Value <= 5"));

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Idea)
                .WithMany(i => i.Ratings)
                .HasForeignKey(r => r.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(r => !r.IsDeleted);
            entity.HasIndex(r => new { r.UserId, r.IdeaId }).IsUnique();
        });
    }

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
            entity.HasQueryFilter(rt => !rt.User.IsDeleted);
        });
    }

    private static void ConfigureTeamMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(tm => new { tm.TeamId, tm.UserId });

            entity.HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tm => tm.User)
                .WithMany()
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(tm => !tm.Team.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;

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
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                        entry.Entity.DeletedAt = now;
                    else if (!entry.Entity.IsDeleted && entry.Entity.DeletedAt != null)
                        entry.Entity.DeletedAt = null;
                    break;
            }
        }

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
                    else if (!entry.Entity.IsDeleted && entry.Entity.DeletedAt != null)
                        entry.Entity.DeletedAt = null;
                    break;
            }
        }
    }
}
