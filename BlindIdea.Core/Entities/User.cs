using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// User entity extending ASP.NET Identity IdentityUser.
    /// Represents an authenticated user in the system.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// User's full name or display name.
        /// Required field, maximum 100 characters.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Foreign key to the Team entity.
        /// Nullable - user can exist without being in a team.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// Navigation property to the Team entity.
        /// Null if user is not part of any team.
        /// </summary>
        public virtual Team? Team { get; set; }

        /// <summary>
        /// Soft delete flag - user is logically deleted but data remains.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp when user was created (audit trail).
        /// Set by database interceptor.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when user was last updated (audit trail).
        /// Updated by database interceptor.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Timestamp when user was soft deleted (audit trail).
        /// Null if user is not deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Initial construction for creating new user.
        /// </summary>
        public User() { }

        /// <summary>
        /// Creates a new user with provided name and email.
        /// </summary>
        /// <param name="name">User's full name</param>
        /// <param name="email">User's email address</param>
        public User(string name, string email)
        {
            Name = name;
            Email = email;
            UserName = email;
        }
    }
}
