using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests;

public class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
