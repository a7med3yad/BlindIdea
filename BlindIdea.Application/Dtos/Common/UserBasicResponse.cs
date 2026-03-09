namespace BlindIdea.Application.Dtos.Common;

public class UserBasicResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
}
