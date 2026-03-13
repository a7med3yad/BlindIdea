using BlindIdea.Application.Dtos.Ideas.Requests;
using BlindIdea.Application.Dtos.Ideas.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;

namespace BlindIdea.Application.Services.Implementations;

public class IdeaService : IIdeaService
{
    private readonly IUnitOfWork _unitOfWork;

    public IdeaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IdeaResponse> CreateIdeaAsync(string userId, CreateIdeaRequest request, CancellationToken ct = default)
    {
        // Verify the user is a member of the team
        await EnsureTeamMemberAsync(request.TeamId, userId);

        var idea = new Idea
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId,
            TeamId = request.TeamId,
            IsDeleted = false
        };

        await _unitOfWork.Ideas.AddAsync(idea);
        await _unitOfWork.CommitAsync(ct);

        return MapToIdeaResponse(idea, userId, new List<Rating>());
    }

    public async Task<IdeaResponse> GetIdeaAsync(int ideaId, string requestingUserId, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        if (idea.TeamId.HasValue)
            await EnsureTeamMemberAsync(idea.TeamId.Value, requestingUserId);

        var ratings = await GetIdeaRatingsAsync(ideaId);
        return MapToIdeaResponse(idea, requestingUserId, ratings);
    }

    public async Task<IEnumerable<IdeaResponse>> GetTeamIdeasAsync(int teamId, string requestingUserId, CancellationToken ct = default)
    {
        await EnsureTeamMemberAsync(teamId, requestingUserId);

        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.TeamId == teamId && !i.IsDeleted);
        var allRatings = await _unitOfWork.Ratings.FindAsync(r => r.Idea.TeamId == teamId && !r.IsDeleted);

        return ideas.Select(idea =>
        {
            var ideaRatings = allRatings.Where(r => r.IdeaId == idea.Id).ToList();
            return MapToIdeaResponse(idea, requestingUserId, ideaRatings);
        });
    }

    public async Task<IdeaResponse> UpdateIdeaAsync(int ideaId, string userId, UpdateIdeaRequest request, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        // Only the author can update their own idea
        if (idea.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own ideas.");

        if (request.Title != null) idea.Title = request.Title;
        if (request.Description != null) idea.Description = request.Description;

        _unitOfWork.Ideas.Update(idea);
        await _unitOfWork.CommitAsync(ct);

        var ratings = await GetIdeaRatingsAsync(ideaId);
        return MapToIdeaResponse(idea, userId, ratings);
    }

    public async Task DeleteIdeaAsync(int ideaId, string userId, CancellationToken ct = default)
    {
        var idea = await GetActiveIdeaAsync(ideaId);

        if (idea.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own ideas.");

        idea.IsDeleted = true;
        _unitOfWork.Ideas.Update(idea);
        await _unitOfWork.CommitAsync(ct);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<Idea> GetActiveIdeaAsync(int ideaId)
    {
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.Id == ideaId && !i.IsDeleted);
        return ideas.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Idea {ideaId} not found.");
    }

    private async Task<List<Rating>> GetIdeaRatingsAsync(int ideaId)
    {
        var ratings = await _unitOfWork.Ratings.FindAsync(r => r.IdeaId == ideaId && !r.IsDeleted);
        return ratings.ToList();
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

    /// <summary>
    /// Maps an Idea to its response DTO.  
    /// Author identity is NEVER exposed — only <see cref="IdeaResponse.IsOwnIdea"/> 
    /// tells the requesting user whether they submitted this idea.
    /// </summary>
    private static IdeaResponse MapToIdeaResponse(Idea idea, string requestingUserId, List<Rating> ratings) => new()
    {
        Id = idea.Id,
        Title = idea.Title,
        Description = idea.Description,
        TeamId = idea.TeamId ?? 0,
        AverageRating = ratings.Count > 0 ? ratings.Average(r => r.Value) : 0,
        RatingCount = ratings.Count,
        CurrentUserRating = ratings.FirstOrDefault(r => r.UserId == requestingUserId)?.Value,
        IsOwnIdea = idea.UserId == requestingUserId   // true only to the author
    };
}
