using BlindIdea.Application.Dtos.Ratings.Requests;
using BlindIdea.Application.Dtos.Ratings.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;

namespace BlindIdea.Application.Services.Implementations;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;

    public RatingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RatingResponse> RateIdeaAsync(int ideaId, string userId, RateIdeaRequest request, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        // Users cannot rate their own ideas
        if (idea.UserId == userId)
            throw new InvalidOperationException("You cannot rate your own idea.");

        await EnsureTeamMemberAsync(idea.TeamId!.Value, userId);

        var existing = await GetExistingRatingAsync(ideaId, userId);
        if (existing != null)
            throw new InvalidOperationException("You have already rated this idea. Use PUT to update your rating.");

        ValidateRatingValue(request.Value);

        var rating = new Rating
        {
            IdeaId = ideaId,
            UserId = userId,
            Value = request.Value,
            Comment = request.Comment,
            IsDeleted = false
        };

        await _unitOfWork.Ratings.AddAsync(rating);
        await _unitOfWork.CommitAsync(ct);

        return MapToRatingResponse(rating);
    }

    public async Task<RatingResponse> UpdateRatingAsync(int ideaId, string userId, RateIdeaRequest request, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        if (idea.UserId == userId)
            throw new InvalidOperationException("You cannot rate your own idea.");

        var rating = await GetExistingRatingAsync(ideaId, userId)
            ?? throw new KeyNotFoundException("No existing rating found. Use POST to create one.");

        ValidateRatingValue(request.Value);

        rating.Value = request.Value;
        rating.Comment = request.Comment;

        _unitOfWork.Ratings.Update(rating);
        await _unitOfWork.CommitAsync(ct);

        return MapToRatingResponse(rating);
    }

    public async Task DeleteRatingAsync(int ideaId, string userId, CancellationToken ct = default)
    {
        var rating = await GetExistingRatingAsync(ideaId, userId)
            ?? throw new KeyNotFoundException("No rating found to delete.");

        rating.IsDeleted = true;
        _unitOfWork.Ratings.Update(rating);
        await _unitOfWork.CommitAsync(ct);
    }

    public async Task<IEnumerable<RatingResponse>> GetIdeaRatingsAsync(int ideaId, string requestingUserId, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        if (idea.TeamId.HasValue)
            await EnsureTeamMemberAsync(idea.TeamId.Value, requestingUserId);

        var ratings = await _unitOfWork.Ratings.FindAsync(r => r.IdeaId == ideaId && !r.IsDeleted);

        // Return ratings without exposing UserId — blind system
        return ratings.Select(MapToRatingResponse);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<Idea> GetActiveIdeaAsync(int ideaId)
    {
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.Id == ideaId && !i.IsDeleted);
        return ideas.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Idea {ideaId} not found.");
    }

    private async Task<Rating?> GetExistingRatingAsync(int ideaId, string userId)
    {
        var ratings = await _unitOfWork.Ratings.FindAsync(
            r => r.IdeaId == ideaId && r.UserId == userId && !r.IsDeleted);
        return ratings.FirstOrDefault();
    }

    private async Task EnsureTeamMemberAsync(int teamId, string userId)
    {
        var teams = await _unitOfWork.Teams.FindAsync(t => t.Id == teamId && t.IsDeleted == false);
        var team = teams.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        bool isMember = team.AdminId == userId || team.Members.Any(m => m.Id == userId);
        if (!isMember)
            throw new UnauthorizedAccessException("You are not a member of this team.");
    }

    private static void ValidateRatingValue(int value)
    {
        if (value < 1 || value > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");
    }

    /// <summary>
    /// Maps Rating to response. UserId is deliberately omitted to keep idea authorship blind.
    /// </summary>
    private static RatingResponse MapToRatingResponse(Rating rating) => new()
    {
        IdeaId = rating.IdeaId,
        Value = rating.Value,
        Comment = rating.Comment
    };
}
