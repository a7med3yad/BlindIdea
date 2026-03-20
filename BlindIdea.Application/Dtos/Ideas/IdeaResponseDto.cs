namespace BlindIdea.Application.Dtos.Ideas
{
    public class IdeaResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;      
        public string Content { get; set; } = string.Empty;    
        public DateTime CreatedAt { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int? MyRating { get; set; }                     
    }
}
