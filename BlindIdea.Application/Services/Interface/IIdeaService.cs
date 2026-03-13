using BlindIdea.Application.Dtos.Ideas.Requests;
using BlindIdea.Application.Dtos.Ideas.Responses;

namespace BlindIdea.Application.Services.Interfaces;

public interface IIdeaService
{
    Task<IdeaResponse> CreateIdeaAsync(string userId, CreateIdeaRequest request, CancellationToken ct = default);
    Task<IdeaResponse> GetIdeaAsync(int ideaId, string requestingUserId, CancellationToken ct = default);
    Task<IEnumerable<IdeaResponse>> GetTeamIdeasAsync(int teamId, string requestingUserId, CancellationToken ct = default);
    Task<IdeaResponse> UpdateIdeaAsync(int ideaId, string userId, UpdateIdeaRequest request, CancellationToken ct = default);
    Task DeleteIdeaAsync(int ideaId, string userId, CancellationToken ct = default);
}
