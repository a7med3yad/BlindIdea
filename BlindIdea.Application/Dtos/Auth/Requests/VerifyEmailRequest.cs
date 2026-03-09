using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
