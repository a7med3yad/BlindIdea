using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BlindIdea.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// Provides abstraction for CRUD operations and common queries.
    /// Implements the Repository Pattern for clean data access.
    /// </summary>
    /// <typeparam name="T">Entity type managed by this repository</typeparam>
    public interface IRepository<T> where T : class 
    {
        // ===== READ =====

        /// <summary>
        /// Gets entity by primary key ID (Guid).
        /// </summary>
        /// <param name="id">Primary key value (Guid)</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets entity by primary key ID (string, for IdentityUser-based entities).
        /// </summary>
        /// <param name="id">Primary key value (string)</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Gets all entities (applies global filters like IsDeleted).
        /// Returns IEnumerable for async evaluation.
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the specified predicate.
        /// Returns all matches or empty collection.
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets first entity matching predicate or null.
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>First matching entity or null</returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Checks if any entity matches the predicate.
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if any matching entity exists</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Counts entities matching the predicate.
        /// </summary>
        /// <param name="predicate">Optional filter condition</param>
        /// <returns>Count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // ===== CREATE/UPDATE/DELETE =====

        /// <summary>
        /// Adds a new entity to the repository (in-memory only, not saved yet).
        /// Use SaveChangesAsync() to persist changes.
        /// </summary>
        /// <param name="entity">Entity to add</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities at once.
        /// </summary>
        /// <param name="entities">Collection of entities to add</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity.
        /// Changes are tracked but not saved until SaveChangesAsync().
        /// </summary>
        /// <param name="entity">Entity with updated values</param>
        void Update(T entity);

        /// <summary>
        /// Updates multiple entities at once.
        /// </summary>
        /// <param name="entities">Entities with updated values</param>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity from the repository.
        /// For soft-deletable entities, marks as deleted instead of physical deletion.
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        void Delete(T entity);

        /// <summary>
        /// Deletes multiple entities at once.
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        void DeleteRange(IEnumerable<T> entities);

        // ===== PERSIST =====

        /// <summary>
        /// Saves all pending changes to the database.
        /// Should be called after Add/Update/Delete operations.
        /// </summary>
        /// <returns>Number of entities affected</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Gets the underlying IQueryable for advanced filtering/projection.
        /// Only use when simple Find/GetAll are insufficient.
        /// </summary>
        /// <returns>IQueryable of the DbSet</returns>
        IQueryable<T> AsQueryable();
    }
}
