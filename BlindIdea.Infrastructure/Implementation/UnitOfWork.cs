using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IRepository<User>? _users;
        private IRepository<Team>? _teams;
        private IRepository<Idea>? _ideas;
        private IRepository<Rating>? _ratings;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Team> Teams => _teams ??= new Repository<Team>(_context);
        public IRepository<Idea> Ideas => _ideas ??= new Repository<Idea>(_context);
        public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
