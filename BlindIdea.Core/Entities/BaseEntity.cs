using System;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Base entity class providing common properties for all domain entities.
    /// Implements audit trail pattern with CreatedAt, UpdatedAt, and soft delete support (IsDeleted).
    /// All entities in the domain should inherit from this class.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Primary key - unique identifier for the entity.
        /// Uses GUID for distributed system compatibility.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Timestamp when the entity was created (UTC).
        /// Set automatically by the database interceptor.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the entity was last updated (UTC).
        /// Updated automatically by the database interceptor.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag for logical deletion without removing data.
        /// Queries automatically filter these unless explicitly included.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp when the entity was soft deleted (UTC).
        /// Null if entity is not deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
