using BlindIdea.Core.Entities;
using System.Linq.Expressions;

namespace BlindIdea.Core.Interfaces;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    // ── Repositories ──
    IRepository<User>         Users         { get; }
    IRepository<Team>         Teams         { get; }
    IRepository<Idea>         Ideas         { get; }
    IRepository<Rating>       Ratings       { get; }
    IRepository<RefreshToken> RefreshTokens { get; }

    // ── Persistence ──
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task      RollbackAsync();

    // ── Transactions (for multi-step operations that need an explicit transaction) ──
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
