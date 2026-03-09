using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "User name is required")]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = null!;
}
