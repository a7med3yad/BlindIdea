namespace BlindIdea.Application.Dtos.Ideas.Requests;

public class CreateIdeaRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int TeamId { get; set; }
}

public class UpdateIdeaRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}
