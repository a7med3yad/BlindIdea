using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Implementation
{
    /// <summary>
    /// Unit of Work implementation coordinating multiple repositories.
    /// Manages database transactions and savepoints for transactional integrity.
    /// Implements lazy loading of repositories through nullable fields.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        // Repository instances (lazy-loaded)
        private IRepository<User>? _users;
        private IRepository<Team>? _teams;
        private IRepository<Idea>? _ideas;
        private IRepository<Rating>? _ratings;
        private IRepository<RefreshToken>? _refreshTokens;
        private IRepository<EmailVerificationToken>? _emailVerificationTokens;

        /// <summary>
        /// Track if disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Constructor injecting the database context.
        /// </summary>
        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ===== REPOSITORY PROPERTIES (Lazy-loaded) =====

        /// <summary>
        /// User repository.
        /// </summary>
        public IRepository<User> Users => _users ??= new Repository<User>(_context);

        /// <summary>
        /// Team repository.
        /// </summary>
        public IRepository<Team> Teams => _teams ??= new Repository<Team>(_context);

        /// <summary>
        /// Idea repository.
        /// </summary>
        public IRepository<Idea> Ideas => _ideas ??= new Repository<Idea>(_context);

        /// <summary>
        /// Rating repository.
        /// </summary>
        public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);

        /// <summary>
        /// Refresh Token repository for token management and rotation.
        /// </summary>
        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

        /// <summary>
        /// Email Verification Token repository for email verification flow.
        /// </summary>
        public IRepository<EmailVerificationToken> EmailVerificationTokens => _emailVerificationTokens ??= new Repository<EmailVerificationToken>(_context);

        // ===== TRANSACTION MANAGEMENT =====

        /// <summary>
        /// Begins a database transaction.
        /// All changes must be explicitly committed/rolled back.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits current database transaction.
        /// Use after BeginTransactionAsync for explicit transaction control.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        /// <summary>
        /// Rolls back current database transaction.
        /// Undoes all changes since BeginTransactionAsync.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        // ===== SAVEPOINT MANAGEMENT =====
        // Savepoint methods removed - use database-level transactions instead
        // .NET 8 doesn't support SavePoint API through IDbContextTransaction

        // ===== PERSIST OPERATIONS =====

        /// <summary>
        /// Saves all changes to the database.
        /// Executes all pending Add/Update/Delete operations.
        /// Handles audit field updates automatically via AppDbContext.
        /// </summary>
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Rolls back all pending changes without saving.
        /// </summary>
        public async Task RollbackAsync()
        {
            _context.ChangeTracker.Clear();
            await Task.CompletedTask;
        }

        // ===== DISPOSAL =====

        /// <summary>
        /// Disposes the DbContext and releases database connection.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _context?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Async disposal of DbContext.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            if (_context != null)
                await _context.DisposeAsync();

            _disposed = true;
        }
    }
}
