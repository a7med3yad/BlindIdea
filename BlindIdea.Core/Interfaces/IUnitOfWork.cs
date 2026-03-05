using BlindIdea.Core.Entities;

namespace BlindIdea.Core.Interfaces
{
    /// <summary>
    /// Unit of Work pattern implementation for transaction management.
    /// Coordinates multiple repository operations and ensures atomicity.
    /// All repositories share the same DbContext for consistent transactions.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>
        /// Repository for User entity operations.
        /// </summary>
        IRepository<User> Users { get; }

        /// <summary>
        /// Repository for Team entity operations.
        /// </summary>
        IRepository<Team> Teams { get; }

        /// <summary>
        /// Repository for Idea entity operations.
        /// </summary>
        IRepository<Idea> Ideas { get; }

        /// <summary>
        /// Repository for Rating entity operations.
        /// </summary>
        IRepository<Rating> Ratings { get; }

        /// <summary>
        /// Repository for RefreshToken entity operations.
        /// Handles secure token storage and retrieval.
        /// </summary>
        IRepository<RefreshToken> RefreshTokens { get; }

        /// <summary>
        /// Repository for EmailVerificationToken entity operations.
        /// Handles email verification token storage and expiry.
        /// </summary>
        IRepository<EmailVerificationToken> EmailVerificationTokens { get; }

        /// <summary>
        /// Commits all pending changes to the database.
        /// Wraps operations in a transaction for atomicity.
        /// Must be called after Add/Update/Delete operations to persist changes.
        /// </summary>
        /// <returns>Number of entities affected by the transaction</returns>
        Task<int> CommitAsync();

        /// <summary>
        /// Rolls back all pending changes without saving to database.
        /// Useful for undoing changes on error or cancellation.
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Begins an explicit transaction.
        /// Use this for transactions spanning multiple CommitAsync() calls.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}

