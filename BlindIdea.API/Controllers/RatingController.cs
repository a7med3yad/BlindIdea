using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRatingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var rating = await _ratingService.CreateRatingAsync(request, userId);
            return Created("", rating);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{ideaId:guid}")]
    public async Task<IActionResult> RateIdea(Guid ideaId, [FromQuery] int value, [FromQuery] string? comment, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (value < 1 || value > 5) return BadRequest("Rating must be between 1 and 5");
        try
        {
            var request = new CreateRatingRequest { IdeaId = ideaId, Value = value, Comment = comment };
            var rating = await _ratingService.CreateRatingAsync(request, userId);
            return Ok(rating);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rating = await _ratingService.GetRatingAsync(id, userId);
        if (rating == null) return NotFound();
        return Ok(rating);
    }

    [HttpGet("idea/{ideaId:guid}")]
    public async Task<IActionResult> GetByIdea(Guid ideaId, [FromQuery] ListRatingsRequest request, CancellationToken cancellationToken)
    {
        request.IdeaId = ideaId;
        var result = await _ratingService.GetIdeaRatingsAsync(request);
        return Ok(result);
    }

    [HttpGet("idea/{ideaId:guid}/user")]
    public async Task<IActionResult> GetUserRating(Guid ideaId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var rating = await _ratingService.GetUserRatingAsync(ideaId, userId);
        if (rating == null) return NotFound();
        return Ok(rating);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRatingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var rating = await _ratingService.UpdateRatingAsync(id, request, userId);
            if (rating == null) return NotFound();
            return Ok(rating);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _ratingService.DeleteRatingAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("idea/{ideaId:guid}/statistics")]
    public async Task<IActionResult> GetStatistics(Guid ideaId, CancellationToken cancellationToken)
    {
        var stats = await _ratingService.GetRatingStatisticsAsync(ideaId);
        return Ok(stats ?? new RatingStatisticsResponse());
    }
}
