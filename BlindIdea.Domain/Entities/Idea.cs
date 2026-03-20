namespace BlindIdea.Domain.Entities
{
    public class Idea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EncryptedTitle { get; set; } = string.Empty;
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TeamId { get; set; } = string.Empty; 
        public Team Team { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}

