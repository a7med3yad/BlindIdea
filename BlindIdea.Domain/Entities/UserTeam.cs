using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Domain.Entities
{
    public class UserTeam
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string TeamId { get; set; } = string.Empty;
        public Team Team { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
    }
}
