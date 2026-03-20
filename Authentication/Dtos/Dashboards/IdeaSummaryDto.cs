namespace BlindIdea.API.Dtos
{
    public class IdeaSummaryDto
    {
        public int TotalIdeas { get; set; }
        public int TotalRatings { get; set; }
        public double OverallAverageRating { get; set; }
        public int IdeasSubmittedByMe { get; set; }
        public int IdeasRatedByMe { get; set; }
    }
}