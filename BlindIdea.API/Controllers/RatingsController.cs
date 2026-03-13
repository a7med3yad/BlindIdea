using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Ratings.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/teams/{teamId:int}/ideas/{ideaId:int}/ratings")]
[ApiController]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found.");

    /// <summary>
    /// Get all ratings for an idea.  
    /// Rater identity is intentionally hidden — this is a blind rating system.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetRatings(int teamId, int ideaId, CancellationToken ct)
    {
        var ratings = await _ratingService.GetIdeaRatingsAsync(ideaId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(ratings));
    }

    /// <summary>Rate an idea (1–5). You cannot rate your own idea.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> RateIdea(int teamId, int ideaId, [FromBody] RateIdeaRequest request, CancellationToken ct)
    {
        var rating = await _ratingService.RateIdeaAsync(ideaId, CurrentUserId, request, ct);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(rating, "Rating submitted.", 201));
    }

    /// <summary>Update your existing rating for an idea.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateRating(int teamId, int ideaId, [FromBody] RateIdeaRequest request, CancellationToken ct)
    {
        var rating = await _ratingService.UpdateRatingAsync(ideaId, CurrentUserId, request, ct);
        return Ok(ApiResponse<object>.SuccessResponse(rating, "Rating updated."));
    }

    /// <summary>Remove your rating from an idea.</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteRating(int teamId, int ideaId, CancellationToken ct)
    {
        await _ratingService.DeleteRatingAsync(ideaId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { deleted = true }, "Rating removed."));
    }
}
