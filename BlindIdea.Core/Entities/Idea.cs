using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    public class Idea
    {
        public Guid Id { get;  set; }
        public string Title { get;  set; }
        public string Description { get;  set; }

        public bool IsAnonymous { get;  set; }

        public string UserId { get;  set; }
        public User User { get; set; }

        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        private readonly List<Rating> _ratings = new();
        public IReadOnlyCollection<Rating> Ratings => _ratings;

        public double AverageRating =>
            _ratings.Any() ? _ratings.Average(r => r.Value) : 0;
    }

}
