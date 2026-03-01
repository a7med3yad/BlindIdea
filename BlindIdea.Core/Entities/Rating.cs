using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Rating
    {
        public Guid Id { get; set; }
        public int Value { get; set; } // 1 to 5
        public Guid IdeaId { get; set; }
        public Idea Idea { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }

}
