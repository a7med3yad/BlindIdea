using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Core.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int TeamId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Team Team { get; set; }
    }

}
