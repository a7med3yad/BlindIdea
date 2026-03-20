namespace BlindIdea.Domain.Entities
{
    public class Rating
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Score { get; set; }
        public string IdeaId { get; set; } = string.Empty;
        public Idea Idea { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}