using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Application.Services.Implementations;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;

    public RatingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RatingResponse?> CreateRatingAsync(CreateRatingRequest request, string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User must be authenticated");

        var (isValid, error) = await ValidateRatingAsync(request.IdeaId, userId);
        if (!isValid) throw new InvalidOperationException(error ?? "Cannot rate this idea");

        var idea = await _unitOfWork.Ideas.GetByIdAsync(request.IdeaId);
        if (idea == null) throw new KeyNotFoundException("Idea not found");

        var existing = await _unitOfWork.Ratings.FirstOrDefaultAsync(r => r.IdeaId == request.IdeaId && r.UserId == userId);
        if (existing != null)
        {
            existing.Update(request.Value, request.Comment);
            _unitOfWork.Ratings.Update(existing);
        }
        else
        {
            var rating = new Rating(request.IdeaId, userId, request.Value, request.Comment);
            await _unitOfWork.Ratings.AddAsync(rating);
        }

        await _unitOfWork.CommitAsync();

        var ratingEntity = await _unitOfWork.Ratings.FirstOrDefaultAsync(r => r.IdeaId == request.IdeaId && r.UserId == userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return ratingEntity != null && user != null ? MapToResponse(ratingEntity, user, userId) : null;
    }

    public async Task<RatingResponse?> GetRatingAsync(Guid ratingId, string? userId = null)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(ratingId);
        if (rating == null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(rating.UserId);
        return user != null ? MapToResponse(rating, user, userId ?? "") : null;
    }

    public async Task<RatingListResponse> GetIdeaRatingsAsync(ListRatingsRequest request)
    {
        var query = _unitOfWork.Ratings.AsQueryable().Where(r => r.IdeaId == request.IdeaId);

        if (request.RatingValue.HasValue)
            query = query.Where(r => r.Value == request.RatingValue.Value);

        var totalCount = await query.CountAsync();

        query = request.SortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "value_asc" => query.OrderBy(r => r.Value),
            "value_desc" => query.OrderByDescending(r => r.Value),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var ratings = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var userIds = ratings.Select(r => r.UserId).Distinct().ToList();
        var users = (await _unitOfWork.Users.GetAllAsync()).Where(u => userIds.Contains(u.Id!)).ToDictionary(u => u.Id!);

        var stats = await GetRatingStatisticsAsync(request.IdeaId);

        return new RatingListResponse
        {
            Ratings = ratings.Select(r => new RatingSummaryResponse
            {
                Id = r.Id,
                RaterName = users.GetValueOrDefault(r.UserId)?.FullName ?? "Unknown",
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Statistics = stats
        };
    }

    public async Task<RatingResponse?> GetUserRatingAsync(Guid ideaId, string userId)
    {
        var rating = await _unitOfWork.Ratings.FirstOrDefaultAsync(r => r.IdeaId == ideaId && r.UserId == userId);
        if (rating == null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user != null ? MapToResponse(rating, user, userId) : null;
    }

    public async Task<RatingResponse?> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, string userId)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(ratingId);
        if (rating == null) return null;
        if (rating.UserId != userId) throw new UnauthorizedAccessException("You can only edit your own ratings");

        rating.Update(request.Value, request.Comment);
        _unitOfWork.Ratings.Update(rating);
        await _unitOfWork.CommitAsync();

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user != null ? MapToResponse(rating, user, userId) : null;
    }

    public async Task<bool> DeleteRatingAsync(Guid ratingId, string userId)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(ratingId);
        if (rating == null) return false;
        if (rating.UserId != userId) return false;

        _unitOfWork.Ratings.Delete(rating);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> CanUserEditAsync(Guid ratingId, string userId) =>
        (await _unitOfWork.Ratings.FirstOrDefaultAsync(r => r.Id == ratingId && r.UserId == userId)) != null;

    public async Task<bool> CanUserDeleteAsync(Guid ratingId, string userId) =>
        (await _unitOfWork.Ratings.FirstOrDefaultAsync(r => r.Id == ratingId && r.UserId == userId)) != null;

    public async Task<(bool isValid, string? errorMessage)> ValidateRatingAsync(Guid ideaId, string userId)
    {
        var idea = await _unitOfWork.Ideas.GetByIdAsync(ideaId);
        if (idea == null) return (false, "Idea not found");
        if (idea.UserId == userId) return (false, "You cannot rate your own idea");
        return (true, null);
    }

    public async Task<RatingStatisticsResponse?> GetRatingStatisticsAsync(Guid ideaId)
    {
        var ratings = (await _unitOfWork.Ratings.FindAsync(r => r.IdeaId == ideaId)).ToList();
        if (ratings.Count == 0) return new RatingStatisticsResponse { TotalRatings = 0 };

        var distribution = ratings.GroupBy(r => r.Value).ToDictionary(g => g.Key, g => g.Count());
        var avg = ratings.Average(r => r.Value);
        var total = ratings.Count;
        var fiveStars = distribution.GetValueOrDefault(5, 0);
        var positive = distribution.GetValueOrDefault(4, 0) + distribution.GetValueOrDefault(5, 0);

        return new RatingStatisticsResponse
        {
            AverageRating = Math.Round(avg, 2),
            TotalRatings = total,
            RatingDistribution = distribution,
            OneStar = distribution.GetValueOrDefault(1, 0),
            TwoStars = distribution.GetValueOrDefault(2, 0),
            ThreeStars = distribution.GetValueOrDefault(3, 0),
            FourStars = distribution.GetValueOrDefault(4, 0),
            FiveStars = fiveStars,
            PercentFiveStars = total > 0 ? Math.Round(100.0 * fiveStars / total, 2) : 0,
            PercentPositive = total > 0 ? Math.Round(100.0 * positive / total, 2) : 0
        };
    }

    public async Task<RatingListResponse> GetUserRatingsAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Ratings.AsQueryable().Where(r => r.UserId == userId);
        var totalCount = await query.CountAsync();
        var ratings = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new RatingListResponse
        {
            Ratings = ratings.Select(r => new RatingSummaryResponse
            {
                Id = r.Id,
                RaterName = "",
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<int> GetRatingCountAsync(Guid ideaId) =>
        await _unitOfWork.Ratings.CountAsync(r => r.IdeaId == ideaId);

    public async Task<double> GetAverageRatingAsync(Guid ideaId)
    {
        var ratings = await _unitOfWork.Ratings.FindAsync(r => r.IdeaId == ideaId);
        return ratings.Any() ? Math.Round(ratings.Average(r => r.Value), 2) : 0;
    }

    private static RatingResponse MapToResponse(Rating rating, User user, string currentUserId) => new()
    {
        Id = rating.Id,
        IdeaId = rating.IdeaId,
        Rater = new UserBasicResponse { Id = user.Id!, Name = user.FullName },
        Value = rating.Value,
        Comment = rating.Comment,
        CreatedAt = rating.CreatedAt,
        UpdatedAt = rating.UpdatedAt,
        CanEdit = rating.UserId == currentUserId,
        CanDelete = rating.UserId == currentUserId
    };
}
