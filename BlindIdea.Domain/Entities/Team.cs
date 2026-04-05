namespace BlindIdea.Domain.Entities
{
    public class Team
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AdminId { get; set; } = string.Empty;
        public ApplicationUser Admin { get; set; } = null!;
        public ICollection<UserTeam> UserTeams { get; set; }
           = new List<UserTeam>();
        public IEnumerable<ApplicationUser> Members
          => UserTeams.Select(ut => ut.User);
        public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    }
}
