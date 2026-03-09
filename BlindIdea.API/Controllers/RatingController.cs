using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/ratings")]
[ApiController]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Create or update a rating for an idea.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRatingRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var rating = await _ratingService.CreateRatingAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(rating!, "Rating submitted successfully", 201));
    }

    /// <summary>
    /// Get a rating by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var rating = await _ratingService.GetRatingAsync(id, userId);
        if (rating == null)
            return NotFound(ApiResponse<object>.FailureResponse("Rating not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(rating));
    }

    /// <summary>
    /// Get all ratings for a specific idea with pagination.
    /// </summary>
    [HttpGet("idea/{ideaId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdea(Guid ideaId, [FromQuery] ListRatingsRequest request)
    {
        request.IdeaId = ideaId;
        var result = await _ratingService.GetIdeaRatingsAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    /// <summary>
    /// Get the current user's rating for a specific idea.
    /// </summary>
    [HttpGet("idea/{ideaId:guid}/user")]
    public async Task<IActionResult> GetUserRating(Guid ideaId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var rating = await _ratingService.GetUserRatingAsync(ideaId, userId);
        if (rating == null)
            return NotFound(ApiResponse<object>.FailureResponse("No rating found for this idea", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(rating));
    }

    /// <summary>
    /// Update an existing rating. Only the author can update.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRatingRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var rating = await _ratingService.UpdateRatingAsync(id, request, userId);
        if (rating == null)
            return NotFound(ApiResponse<object>.FailureResponse("Rating not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(rating, "Rating updated successfully"));
    }

    /// <summary>
    /// Delete a rating. Only the author can delete.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _ratingService.DeleteRatingAsync(id, userId);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Rating not found or access denied", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Rating deleted successfully"));
    }

    /// <summary>
    /// Get rating statistics for an idea (average, distribution, etc.).
    /// </summary>
    [HttpGet("idea/{ideaId:guid}/statistics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatistics(Guid ideaId)
    {
        var stats = await _ratingService.GetRatingStatisticsAsync(ideaId);
        return Ok(ApiResponse<object>.SuccessResponse(stats ?? new RatingStatisticsResponse()));
    }
}
