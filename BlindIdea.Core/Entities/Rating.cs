using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    
    public class Rating : BaseEntity
    {
        
        public int Value { get; set; }

        public string? Comment { get; set; }

        public Guid IdeaId { get; set; }

        public virtual Idea Idea { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public Rating() { }

        public Rating(Guid ideaId, string userId, int value, string? comment = null)
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Rating value must be between 1 and 5", nameof(value));

            Id = Guid.NewGuid();
            IdeaId = ideaId;
            UserId = userId;
            Value = value;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool Validate()
        {
            if (Value < 1 || Value > 5)
                throw new InvalidOperationException("Rating value must be between 1 and 5");

            return true;
        }

        public void Update(int newValue, string? newComment = null)
        {
            if (newValue < 1 || newValue > 5)
                throw new ArgumentException("Rating value must be between 1 and 5", nameof(newValue));

            Value = newValue;
            Comment = newComment;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}