using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    public class IdeaDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsAnonymous { get; set; }
        public Guid? TeamId { get; set; }
        public string? UserId { get; set; }
        public double AverageRating { get; set; }
    }
}
