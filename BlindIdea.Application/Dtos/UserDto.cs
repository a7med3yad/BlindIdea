using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Data Transfer Object for user information.
    /// Returned in authentication responses and used in API responses.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's full name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Optional: Team ID if user belongs to a team.
        /// Null if user has not joined a team.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// Indicates if the user's email has been verified.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Indicates if the user's account is active.
        /// </summary>
        public bool IsDeleted { get; set; }
    }

}
