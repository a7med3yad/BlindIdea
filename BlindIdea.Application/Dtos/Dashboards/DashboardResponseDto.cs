namespace BlindIdea.Application.Dtos
{
    public class DashboardResponseDto
    {
        public TeamSummaryDto Team { get; set; } = new();
        public IdeaSummaryDto Ideas { get; set; } = new();
        public List<TopIdeaDto> TopIdeas { get; set; } = new();
        public List<RecentIdeaDto> RecentIdeas { get; set; } = new();
    }
}