namespace BlindIdea.Application.Dtos.Ideas.Responses;

public class IdeaResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int TeamId { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? CurrentUserRating { get; set; }   // the requesting user's own rating, null if not rated
    public bool IsOwnIdea { get; set; }            // true only to the idea's author (for edit/delete)
}
