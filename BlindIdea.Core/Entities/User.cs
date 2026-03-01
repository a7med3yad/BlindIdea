using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Core.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public Guid? TeamId { get; set; }  // nullable → user ممكن ميكونش في team

        public bool IsDeleted { get; set; }

        public virtual Team? Team { get; set; }
    }


}
