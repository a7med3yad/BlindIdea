using BlindIdea.Application.Dtos.Ratings.Requests;
using BlindIdea.Application.Dtos.Ratings.Responses;

namespace BlindIdea.Application.Services.Interfaces;

public interface IRatingService
{
    Task<RatingResponse> RateIdeaAsync(int ideaId, string userId, RateIdeaRequest request, CancellationToken ct = default);
    Task<RatingResponse> UpdateRatingAsync(int ideaId, string userId, RateIdeaRequest request, CancellationToken ct = default);
    Task DeleteRatingAsync(int ideaId, string userId, CancellationToken ct = default);
    Task<IEnumerable<RatingResponse>> GetIdeaRatingsAsync(int ideaId, string requestingUserId, CancellationToken ct = default);
}
