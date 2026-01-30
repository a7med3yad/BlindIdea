using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Idea
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public List<Rating> Ratings { get; set; }
    }
}
