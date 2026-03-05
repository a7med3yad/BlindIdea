using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Core.Entities
{
    
    public class User : IdentityUser
    {
        
        public string Name { get; set; } = null!;

        public Guid? TeamId { get; set; }

        public virtual Team? Team { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public User() { }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
            UserName = email;
        }
    }
}