namespace BlindIdea.API.Dtos
{
    public class RecentIdeaDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;      // ✅ decrypted
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}