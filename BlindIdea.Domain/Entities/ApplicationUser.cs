using Microsoft.AspNetCore.Identity;
namespace BlindIdea.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsVerified { get; set; }
        public DateTime? OtpExpiration { get; set; }
        public int ?OtpRequestCount { get; set; }
        public DateTime? OtpRequestWindowStart { get; set; }
        public ICollection<UserTeam> UserTeams { get; set; }
            = new List<UserTeam>();
        public string? ActiveTeamId { get; set; }

    }   
}
