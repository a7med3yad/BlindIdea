using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/ideas")]
[ApiController]
public class IdeasController : ControllerBase
{
    private readonly IIdeaService _ideaService;

    public IdeasController(IIdeaService ideaService)
    {
        _ideaService = ideaService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Search and list ideas with optional filters and pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchIdeasRequest request)
    {
        var ideas = await _ideaService.SearchIdeasAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get a single idea by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var idea = await _ideaService.GetIdeaAsync(id, userId);
        if (idea == null)
            return NotFound(ApiResponse<object>.FailureResponse("Idea not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(idea));
    }

    /// <summary>
    /// Create a new idea. Requires authentication.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateIdeaRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var idea = await _ideaService.CreateIdeaAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(idea!, "Idea created successfully", 201));
    }

    /// <summary>
    /// Update an existing idea. Only the creator can update.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIdeaRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var idea = await _ideaService.UpdateIdeaAsync(id, request, userId);
        if (idea == null)
            return NotFound(ApiResponse<object>.FailureResponse("Idea not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(idea, "Idea updated successfully"));
    }

    /// <summary>
    /// Delete an idea (soft-delete). Only the creator can delete.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _ideaService.DeleteIdeaAsync(id, userId);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Idea not found or access denied", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Idea deleted successfully"));
    }

    /// <summary>
    /// Get ideas belonging to a specific team.
    /// </summary>
    [HttpGet("team/{teamId:guid}")]
    public async Task<IActionResult> GetByTeam(Guid teamId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var ideas = await _ideaService.GetTeamIdeasAsync(teamId, pageNumber, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get ideas created by a specific user.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var ideas = await _ideaService.GetUserIdeasAsync(userId, pageNumber, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get global idea statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _ideaService.GetStatisticsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    /// <summary>
    /// Get top-rated ideas.
    /// </summary>
    [HttpGet("top")]
    public async Task<IActionResult> GetTop([FromQuery] int count = 10)
    {
        var ideas = await _ideaService.GetTopIdeasAsync(count);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }

    /// <summary>
    /// Get most recently created ideas.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
    {
        var ideas = await _ideaService.GetRecentIdeasAsync(count);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }
}
