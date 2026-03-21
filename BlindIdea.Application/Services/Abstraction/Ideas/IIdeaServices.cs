using BlindIdea.Application.Dtos.Ideas;

namespace BlindIdea.Application.Services.Abstraction.Ideas
{
    public interface IIdeaService
    {
        Task<IdeaResponseDto> SubmitIdeaAsync(string userId, SubmitIdeaDto dto);
        Task<IEnumerable<IdeaResponseDto>> GetTeamIdeasAsync(string userId);
        Task<IdeaResponseDto> GetIdeaAsync(string userId, string ideaId);
        Task DeleteIdeaAsync(string userId, string ideaId);
        Task<IdeaResponseDto> RateIdeaAsync(string userId, string ideaId, RateIdeaDto dto);
        Task DeleteRatingAsync(string userId, string ideaId);
    }
}