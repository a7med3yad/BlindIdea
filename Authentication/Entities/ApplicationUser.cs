using Microsoft.AspNetCore.Identity;
namespace BlindIdea.API.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsVerified { get; set; }
        public DateTime? OtpExpiration { get; set; }
        public int ?OtpRequestCount { get; set; }
        public DateTime? OtpRequestWindowStart { get; set; }
        public string? TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
