using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<User> Members { get; set; } = new List<User>();
        public bool IsDeleted { get; set; }
        public virtual List<Idea> Ideas { get; set; } = new List<Idea>();
    }
}
