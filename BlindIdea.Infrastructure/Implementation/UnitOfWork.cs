using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Implementation
{
    
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        private IRepository<User>? _users;
        private IRepository<Team>? _teams;
        private IRepository<Idea>? _ideas;
        private IRepository<Rating>? _ratings;
        private bool _disposed = false;

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);

        public IRepository<Team> Teams => _teams ??= new Repository<Team>(_context);

        public IRepository<Idea> Ideas => _ideas ??= new Repository<Idea>(_context);

        public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);


        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task RollbackAsync()
        {
            _context.ChangeTracker.Clear();
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _context?.Dispose();
            _disposed = true;
        }

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