namespace BlindIdea.Application.Dtos.Ratings.Requests;

public class RateIdeaRequest
{
    /// <summary>Rating value, e.g. 1–5.</summary>
    public int Value { get; set; }
    public string? Comment { get; set; }
}
