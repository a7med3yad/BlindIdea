using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Persistence;

namespace BlindIdea.Infrastructure.Implementation;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private IRepository<User>? _users;
    private IRepository<Team>? _teams;
    private IRepository<Idea>? _ideas;
    private IRepository<Rating>? _ratings;
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<TeamMember>? _teamMembers;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Team> Teams => _teams ??= new Repository<Team>(_context);
    public IRepository<Idea> Ideas => _ideas ??= new Repository<Idea>(_context);
    public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
    public IRepository<TeamMember> TeamMembers => _teamMembers ??= new Repository<TeamMember>(_context);

    public Task BeginTransactionAsync() => _context.Database.BeginTransactionAsync();
    public Task CommitTransactionAsync() => _context.Database.CommitTransactionAsync();
    public Task RollbackTransactionAsync() => _context.Database.RollbackTransactionAsync();
    public Task<int> CommitAsync() => _context.SaveChangesAsync();

    public Task RollbackAsync()
    {
        _context.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _context.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await _context.DisposeAsync();
        _disposed = true;
    }
}