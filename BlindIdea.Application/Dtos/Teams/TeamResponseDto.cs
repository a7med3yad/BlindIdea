namespace BlindIdea.Application.Dtos.Teams
{
    public class TeamResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
