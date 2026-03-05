using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Team entity representing a collaborative group.
    /// Teams can house multiple ideas and members with role-based permissions.
    /// </summary>
    public class Team : BaseEntity
    {
        /// <summary>
        /// Team name - unique identifier within the system.
        /// Required, maximum 100 characters.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Foreign key to Admin user (User ID).
        /// The admin user who created/owns this team.
        /// </summary>
        public string AdminId { get; set; } = null!;

        /// <summary>
        /// Navigation property to Admin user.
        /// The user who owns/administrates this team.
        /// </summary>
        public virtual User Admin { get; set; } = null!;

        /// <summary>
        /// Optional team description explaining purpose/goals.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Team members collection (backing field for internal access).
        /// Use AddMember/RemoveMember methods to maintain invariants.
        /// </summary>
        private readonly List<User> _members = new();

        /// <summary>
        /// Read-only collection of team members.
        /// </summary>
        public IReadOnlyCollection<User> Members => _members.AsReadOnly();

        /// <summary>
        /// Team ideas collection (backing field for internal access).
        /// Use AddIdea/RemoveIdea methods to maintain invariants.
        /// </summary>
        private readonly List<Idea> _ideas = new();

        /// <summary>
        /// Read-only collection of ideas posted to this team.
        /// </summary>
        public IReadOnlyCollection<Idea> Ideas => _ideas.AsReadOnly();

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        public Team() { }

        /// <summary>
        /// Creates a new team with provided details.
        /// </summary>
        /// <param name="name">Team name</param>
        /// <param name="adminId">User ID of the admin</param>
        /// <param name="description">Optional description</param>
        public Team(string name, string adminId, string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            AdminId = adminId;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a member to the team (domain invariant: prevent duplicates).
        /// </summary>
        /// <param name="user">User to add</param>
        public void AddMember(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (_members.Any(m => m.Id == user.Id))
                return; // Already a member, silently ignore

            _members.Add(user);
        }

        /// <summary>
        /// Removes a member from the team.
        /// </summary>
        /// <param name="userId">User ID to remove</param>
        public void RemoveMember(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var member = _members.FirstOrDefault(m => m.Id == userId);
            if (member != null)
                _members.Remove(member);
        }

        /// <summary>
        /// Adds an idea to the team.
        /// </summary>
        /// <param name="idea">Idea to add</param>
        public void AddIdea(Idea idea)
        {
            if (idea == null)
                throw new ArgumentNullException(nameof(idea));

            if (_ideas.Any(i => i.Id == idea.Id))
                return; // Already added

            _ideas.Add(idea);
        }

        /// <summary>
        /// Removes an idea from the team.
        /// </summary>
        /// <param name="ideaId">Idea ID to remove</param>
        public void RemoveIdea(Guid ideaId)
        {
            var idea = _ideas.FirstOrDefault(i => i.Id == ideaId);
            if (idea != null)
                _ideas.Remove(idea);
        }

        /// <summary>
        /// Checks if a user is the admin of this team.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is admin</returns>
        public bool IsAdmin(string userId)
        {
            return AdminId == userId;
        }

        /// <summary>
        /// Checks if a user is a member of this team.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is a member</returns>
        public bool IsMember(string userId)
        {
            return _members.Any(m => m.Id == userId);
        }
    }
}
