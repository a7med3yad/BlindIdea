using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Team.Requests;

public class CreateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Team name must be between 3 and 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Team description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class UpdateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Team name must be between 3 and 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Team description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class AddTeamMemberRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = null!;
}

public class TransferAdminRequest
{
    [Required(ErrorMessage = "New admin user ID is required")]
    public string NewAdminId { get; set; } = null!;
}