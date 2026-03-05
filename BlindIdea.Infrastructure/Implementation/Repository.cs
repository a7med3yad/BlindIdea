using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BlindIdea.Infrastructure.Implementation
{
    /// <summary>
    /// Generic repository implementation for Entity Framework Core.
    /// Provides CRUD operations and common queries for all entities.
    /// Handles soft deletes through global query filters from DbContext.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Database context instance.
        /// </summary>
        protected readonly AppDbContext _context;

        /// <summary>
        /// DbSet for the entity.
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor injecting the database context.
        /// </summary>
        /// <param name="context">App database context</param>
        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        // ===== READ OPERATIONS =====

        /// <summary>
        /// Gets entity by GUID ID.
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Gets entity by string ID (for IdentityUser-based entities).
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Gets all entities (respects global query filters).
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Finds entities matching predicate.
        /// </summary>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Gets first entity matching predicate.
        /// </summary>
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Checks if any entity matches predicate.
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// Counts entities matching predicate.
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
        }

        // ===== CREATE OPERATIONS =====

        /// <summary>
        /// Adds entity (not saved until SaveChangesAsync).
        /// </summary>
        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Adds multiple entities at once.
        /// </summary>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await _dbSet.AddRangeAsync(entities);
        }

        // ===== UPDATE OPERATIONS =====

        /// <summary>
        /// Updates an existing entity.
        /// The entity must already be tracked by the context.
        /// </summary>
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Updates multiple entities.
        /// </summary>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.UpdateRange(entities);
        }

        // ===== DELETE OPERATIONS =====

        /// <summary>
        /// Deletes an entity (soft delete via BaseEntity pattern if applicable).
        /// </summary>
        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Deletes multiple entities.
        /// </summary>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
        }

        // ===== PERSIST OPERATIONS =====

        /// <summary>
        /// Saves all changes to the database.
        /// Must be called after Add/Update/Delete operations.
        /// Handles audit field updates automatically.
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // ===== UTILITIES =====

        /// <summary>
        /// Gets the underlying IQueryable for advanced queries.
        /// </summary>
        public virtual IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
