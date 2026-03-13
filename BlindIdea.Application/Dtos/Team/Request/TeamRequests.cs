namespace BlindIdea.Application.Dtos.Teams.Requests;

public class CreateTeamRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateTeamRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class AddMemberRequest
{
    public string UserId { get; set; } = null!;
}
