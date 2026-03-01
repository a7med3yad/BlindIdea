using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Team
    {
        public Guid Id { get;  set; }
        public string Name { get;  set; }

        public string AdminId { get; set; }
        public User Admin { get; private set; }

        private readonly List<User> _members = new();
        public IReadOnlyCollection<User> Members => _members;

        private readonly List<Idea> _ideas = new();
        public IReadOnlyCollection<Idea> Ideas => _ideas;
    }


}
