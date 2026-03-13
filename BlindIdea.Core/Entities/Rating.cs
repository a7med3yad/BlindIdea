using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    
    public class Rating 
    {
        
        public int Value { get; set; }

        public string? Comment { get; set; }

        public int IdeaId { get; set; }

        public virtual Idea Idea { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public bool IsDeleted { get; set; }




    }
}