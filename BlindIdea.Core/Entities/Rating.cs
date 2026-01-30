using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Rating
    {
        public int Id { get; set; }
        public int Score { get; set; } 
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public bool IsDeleted { get; set; }
        public int IdeaId { get; set; }
        public virtual Idea Idea { get; set; }
    }
}
