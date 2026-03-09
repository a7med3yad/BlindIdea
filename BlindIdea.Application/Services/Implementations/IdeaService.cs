using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Dtos.Idea.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Application.Services.Implementations;

public class IdeaService : IIdeaService
{
    private readonly IUnitOfWork _unitOfWork;

    public IdeaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IdeaResponse?> CreateIdeaAsync(CreateIdeaRequest request, string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User must be authenticated");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        Team? team = null;
        if (request.TeamId.HasValue)
        {
            team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
            if (team == null) throw new KeyNotFoundException("Team not found");
            var isMember = await _unitOfWork.TeamMembers.AnyAsync(tm => tm.TeamId == team.Id && tm.UserId == userId);
            if (!isMember && team.AdminId != userId)
                throw new UnauthorizedAccessException("You must be a team member to add ideas");
        }

        var idea = new Idea(request.Title, request.Description, userId, request.IsAnonymous, request.TeamId);
        await _unitOfWork.Ideas.AddAsync(idea);
        await _unitOfWork.CommitAsync();

        return MapToResponse(idea, user, team, userId);
    }

    public async Task<IdeaResponse?> GetIdeaAsync(Guid ideaId, string? userId = null)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null) return null;

        var creator = await _unitOfWork.Users.GetByIdAsync(idea.UserId);
        var team = idea.TeamId.HasValue ? await _unitOfWork.Teams.GetByIdAsync(idea.TeamId.Value) : null;

        var ratings = (await _unitOfWork.Ratings.FindAsync(r => r.IdeaId == ideaId)).ToList();
        var avgRating = ratings.Count > 0 ? ratings.Average(r => r.Value) : 0;
        var ratingCount = ratings.Count;

        int? currentUserRating = null;
        if (!string.IsNullOrEmpty(userId))
        {
            var userRating = ratings.FirstOrDefault(r => r.UserId == userId);
            currentUserRating = userRating?.Value;
        }

        return new IdeaResponse
        {
            Id = idea.Id,
            Title = idea.Title,
            Description = idea.Description,
            IsAnonymous = idea.IsAnonymous,
            Creator = creator != null && !idea.IsAnonymous ? new UserBasicResponse { Id = creator.Id!, Name = creator.FullName } : null,
            Team = team != null ? new TeamBasicResponse { Id = team.Id, Name = team.Name } : null,
            AverageRating = Math.Round(avgRating, 2),
            RatingCount = ratingCount,
            CreatedAt = idea.CreatedAt,
            UpdatedAt = idea.UpdatedAt,
            CanEdit = userId == idea.UserId,
            CanDelete = userId == idea.UserId,
            CurrentUserRating = currentUserRating
        };
    }

    public async Task<IdeaListResponse> SearchIdeasAsync(SearchIdeasRequest request)
    {
        var query = _unitOfWork.Ideas.AsQueryable();

        if (request.TeamId.HasValue)
            query = query.Where(i => i.TeamId == request.TeamId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(term) || i.Description.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(i => i.CreatedAt),
            "title" => query.OrderBy(i => i.Title),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        var ideas = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var ideaIds = ideas.Select(i => i.Id).ToList();
        var creators = (await _unitOfWork.Users.GetAllAsync()).ToDictionary(u => u.Id!);
        var ratingsByIdea = (await _unitOfWork.Ratings.FindAsync(r => ideaIds.Contains(r.IdeaId)))
            .GroupBy(r => r.IdeaId)
            .ToDictionary(g => g.Key, g => (Avg: g.Average(x => x.Value), Count: g.Count()));

        var summaries = ideas.Select(i =>
        {
            var creatorName = i.IsAnonymous ? "Anonymous" : (creators.GetValueOrDefault(i.UserId)?.FullName ?? "Unknown");
            var (avg, count) = ratingsByIdea.GetValueOrDefault(i.Id, (0.0, 0));
            return new IdeaSummaryResponse
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description.Length > 200 ? i.Description[..200] + "..." : i.Description,
                IsAnonymous = i.IsAnonymous,
                CreatorName = creatorName,
                AverageRating = Math.Round(avg, 2),
                RatingCount = count,
                CreatedAt = i.CreatedAt
            };
        }).ToList();

        return new IdeaListResponse
        {
            Ideas = summaries,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IdeaListResponse> GetTeamIdeasAsync(Guid teamId, int pageNumber = 1, int pageSize = 10)
    {
        return await SearchIdeasAsync(new SearchIdeasRequest { TeamId = teamId, PageNumber = pageNumber, PageSize = pageSize });
    }

    public async Task<IdeaListResponse> GetUserIdeasAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Ideas.AsQueryable().Where(i => i.UserId == userId);
        var totalCount = await query.CountAsync();
        var ideas = await query.OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var creator = await _unitOfWork.Users.GetByIdAsync(userId);
        var creatorName = creator?.FullName ?? "Unknown";
        var ideaIds = ideas.Select(i => i.Id).ToList();
        var ratingsByIdea = (await _unitOfWork.Ratings.FindAsync(r => ideaIds.Contains(r.IdeaId)))
            .GroupBy(r => r.IdeaId)
            .ToDictionary(g => g.Key, g => (Avg: g.Average(x => x.Value), Count: g.Count()));

        var summaries = ideas.Select(i =>
        {
            var (avg, count) = ratingsByIdea.GetValueOrDefault(i.Id, (0.0, 0));
            return new IdeaSummaryResponse
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description.Length > 200 ? i.Description[..200] + "..." : i.Description,
                IsAnonymous = i.IsAnonymous,
                CreatorName = i.IsAnonymous ? "Anonymous" : creatorName,
                AverageRating = Math.Round(avg, 2),
                RatingCount = count,
                CreatedAt = i.CreatedAt
            };
        }).ToList();

        return new IdeaListResponse
        {
            Ideas = summaries,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IdeaResponse?> UpdateIdeaAsync(Guid ideaId, UpdateIdeaRequest request, string userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null) return null;
        if (idea.UserId != userId) throw new UnauthorizedAccessException("You can only edit your own ideas");

        idea.Update(request.Title, request.Description);
        idea.IsAnonymous = request.IsAnonymous;
        _unitOfWork.Ideas.Update(idea);
        await _unitOfWork.CommitAsync();

        var creator = await _unitOfWork.Users.GetByIdAsync(idea.UserId);
        var team = idea.TeamId.HasValue ? await _unitOfWork.Teams.GetByIdAsync(idea.TeamId.Value) : null;
        return MapToResponse(idea, creator!, team, userId);
    }

    public async Task<bool> DeleteIdeaAsync(Guid ideaId, string userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null) return false;
        if (idea.UserId != userId) return false;

        idea.IsDeleted = true;
        idea.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Ideas.Update(idea);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> CanUserEditAsync(Guid ideaId, string userId) =>
        (await _unitOfWork.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId && i.UserId == userId)) != null;

    public async Task<bool> CanUserDeleteAsync(Guid ideaId, string userId) =>
        (await _unitOfWork.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId && i.UserId == userId)) != null;

    public async Task<bool> CanUserRateAsync(Guid ideaId, string userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        return idea != null && idea.UserId != userId;
    }

    public async Task<IdeaStatisticsResponse> GetStatisticsAsync()
    {
        var ideas = await _unitOfWork.Ideas.GetAllAsync();
        var allIdeas = ideas.ToList();
        var ratings = await _unitOfWork.Ratings.GetAllAsync();
        var allRatings = ratings.ToList();

        return new IdeaStatisticsResponse
        {
            TotalIdeas = allIdeas.Count,
            TotalRatings = allRatings.Count,
            AverageRating = allRatings.Any() ? Math.Round(allRatings.Average(r => r.Value), 2) : 0,
            AnonymousIdeas = allIdeas.Count(i => i.IsAnonymous),
            TeamIdeas = allIdeas.Count(i => i.TeamId.HasValue),
            PersonalIdeas = allIdeas.Count(i => !i.TeamId.HasValue)
        };
    }

    public async Task<List<IdeaSummaryResponse>> GetTopIdeasAsync(int count = 10)
    {
        var ideas = await _unitOfWork.Ideas.GetAllAsync();
        var ratings = await _unitOfWork.Ratings.GetAllAsync();
        var creators = (await _unitOfWork.Users.GetAllAsync()).ToDictionary(u => u.Id!);

        var topIdeas = ideas
            .Select(i => new
            {
                Idea = i,
                Ratings = ratings.Where(r => r.IdeaId == i.Id).ToList()
            })
            .Select(x => new
            {
                x.Idea,
                Avg = x.Ratings.Any() ? x.Ratings.Average(r => r.Value) : 0,
                Count = x.Ratings.Count
            })
            .OrderByDescending(x => x.Avg)
            .ThenByDescending(x => x.Count)
            .Take(count)
            .ToList();

        return topIdeas.Select(x => new IdeaSummaryResponse
        {
            Id = x.Idea.Id,
            Title = x.Idea.Title,
            Description = x.Idea.Description.Length > 200 ? x.Idea.Description[..200] + "..." : x.Idea.Description,
            IsAnonymous = x.Idea.IsAnonymous,
            CreatorName = x.Idea.IsAnonymous ? "Anonymous" : (creators.GetValueOrDefault(x.Idea.UserId)?.FullName ?? "Unknown"),
            AverageRating = Math.Round(x.Avg, 2),
            RatingCount = x.Count,
            CreatedAt = x.Idea.CreatedAt
        }).ToList();
    }

    public async Task<List<IdeaSummaryResponse>> GetRecentIdeasAsync(int count = 10)
    {
        var query = _unitOfWork.Ideas.AsQueryable().OrderByDescending(i => i.CreatedAt).Take(count);
        var ideas = await query.ToListAsync();
        var creators = (await _unitOfWork.Users.GetAllAsync()).ToDictionary(u => u.Id!);
        var ideaIds = ideas.Select(i => i.Id).ToList();
        var ratings = (await _unitOfWork.Ratings.FindAsync(r => ideaIds.Contains(r.IdeaId)))
            .GroupBy(r => r.IdeaId)
            .ToDictionary(g => g.Key, g => (Avg: g.Average(x => x.Value), Count: g.Count()));

        return ideas.Select(i => new IdeaSummaryResponse
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description.Length > 200 ? i.Description[..200] + "..." : i.Description,
            IsAnonymous = i.IsAnonymous,
            CreatorName = i.IsAnonymous ? "Anonymous" : (creators.GetValueOrDefault(i.UserId)?.FullName ?? "Unknown"),
            AverageRating = Math.Round(ratings.GetValueOrDefault(i.Id, (Avg: 0.0, Count: 0)).Avg, 2),
            RatingCount = ratings.GetValueOrDefault(i.Id, (Avg: 0.0, Count: 0)).Count,
            CreatedAt = i.CreatedAt
        }).ToList();
    }

    private static IdeaResponse MapToResponse(Idea idea, User creator, Team? team, string? userId)
    {
        var ratings = idea.Ratings;
        var avg = ratings.Any() ? ratings.Average(r => r.Value) : 0;
        var currentRating = ratings.FirstOrDefault(r => r.UserId == userId)?.Value;

        return new IdeaResponse
        {
            Id = idea.Id,
            Title = idea.Title,
            Description = idea.Description,
            IsAnonymous = idea.IsAnonymous,
            Creator = !idea.IsAnonymous ? new UserBasicResponse { Id = creator.Id!, Name = creator.FullName } : null,
            Team = team != null ? new TeamBasicResponse { Id = team.Id, Name = team.Name } : null,
            AverageRating = Math.Round(avg, 2),
            RatingCount = ratings.Count,
            CreatedAt = idea.CreatedAt,
            UpdatedAt = idea.UpdatedAt,
            CanEdit = userId == idea.UserId,
            CanDelete = userId == idea.UserId,
            CurrentUserRating = currentRating
        };
    }
}
