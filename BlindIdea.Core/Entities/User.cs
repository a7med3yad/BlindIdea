using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TeamId { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Team Team { get; set; }
    }
}
