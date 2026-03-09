using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = null!;

    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}
