using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Ideas
{
    public class RateIdeaDto
    {
        [Range(1, 5, ErrorMessage = "Score must be between 1 and 5")]
        public int Score { get; set; }
    }
}
