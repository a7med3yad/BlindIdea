using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    
    public class Idea : BaseEntity
    {
        
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsAnonymous { get; set; } = false;

        public string UserId { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public Guid? TeamId { get; set; }

        public virtual Team? Team { get; set; }

        private readonly List<Rating> _ratings = new();

        public IReadOnlyCollection<Rating> Ratings => _ratings.AsReadOnly();

        public double AverageRating
        {
            get
            {
                if (!_ratings.Any())
                    return 0;
                return Math.Round(_ratings.Average(r => r.Value), 2);
            }
        }

        public int RatingCount => _ratings.Count;

        public Idea() { }

        public Idea(string title, string description, string userId, bool isAnonymous = false, Guid? teamId = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            UserId = userId;
            IsAnonymous = isAnonymous;
            TeamId = teamId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool AddRating(Rating rating)
        {
            if (rating == null)
                throw new ArgumentNullException(nameof(rating));

            if (_ratings.Any(r => r.UserId == rating.UserId))
                return false;

            _ratings.Add(rating);
            return true;
        }

        public bool UpdateRating(string userId, int newValue)
        {
            if (newValue < 1 || newValue > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(newValue));

            var existingRating = _ratings.FirstOrDefault(r => r.UserId == userId);
            if (existingRating == null)
                return false;

            existingRating.Value = newValue;
            existingRating.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public bool RemoveRating(Guid ratingId)
        {
            var rating = _ratings.FirstOrDefault(r => r.Id == ratingId);
            if (rating == null)
                return false;

            _ratings.Remove(rating);
            return true;
        }

        public int? GetUserRating(string userId)
        {
            return _ratings.FirstOrDefault(r => r.UserId == userId)?.Value;
        }

        public bool HasUserRated(string userId)
        {
            return _ratings.Any(r => r.UserId == userId);
        }

        public bool CanUserRate(string userId)
        {
            return UserId != userId;
        }

        public void Update(string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            Title = title;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}