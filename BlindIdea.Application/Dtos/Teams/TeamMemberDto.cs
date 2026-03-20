namespace BlindIdea.Application.Dtos.Teams
{
    public class TeamMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
