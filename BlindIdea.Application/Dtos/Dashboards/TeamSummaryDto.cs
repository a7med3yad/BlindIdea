namespace BlindIdea.Application.Dtos
{
    public class TeamSummaryDto
    {
        public string TeamName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}