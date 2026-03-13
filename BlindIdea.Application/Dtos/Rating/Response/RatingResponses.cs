namespace BlindIdea.Application.Dtos.Ratings.Responses;

public class RatingResponse
{
    public int IdeaId { get; set; }
    public int Value { get; set; }
    public string? Comment { get; set; }
    // Author is intentionally hidden — blind rating system
}
