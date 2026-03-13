using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    
    public class Idea
    {

        public int Id { get; set; }
        
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public int? TeamId { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Team? Team { get; set; }

        private readonly List<Rating> _ratings = new();
      
    }

}