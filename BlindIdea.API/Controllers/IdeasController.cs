using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Ideas.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/teams/{teamId:int}/ideas")]
[ApiController]
[Authorize]
public class IdeasController : ControllerBase
{
    private readonly IIdeaService _ideaService;

    public IdeasController(IIdeaService ideaService)
    {
        _ideaService = ideaService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found.");

    /// <summary>
    /// Get all ideas for a team.  
    /// Author identity is hidden — only <c>isOwnIdea</c> reveals if you submitted it.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> GetTeamIdeas(int teamId, CancellationToken ct)
    {
        var ideas = await _ideaService.GetTeamIdeasAsync(teamId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(ideas));
    }

    /// <summary>Get a single idea by ID (team members only).</summary>
    [HttpGet("{ideaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetIdea(int teamId, int ideaId, CancellationToken ct)
    {
        var idea = await _ideaService.GetIdeaAsync(ideaId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(idea));
    }

    /// <summary>Submit a new idea to the team.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> CreateIdea(int teamId, [FromBody] CreateIdeaRequest request, CancellationToken ct)
    {
        // Bind the teamId from route to the request so callers don't duplicate it
        request.TeamId = teamId;
        var idea = await _ideaService.CreateIdeaAsync(CurrentUserId, request, ct);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(idea, "Idea submitted successfully.", 201));
    }

    /// <summary>Update your own idea (author only).</summary>
    [HttpPut("{ideaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateIdea(int teamId, int ideaId, [FromBody] UpdateIdeaRequest request, CancellationToken ct)
    {
        var idea = await _ideaService.UpdateIdeaAsync(ideaId, CurrentUserId, request, ct);
        return Ok(ApiResponse<object>.SuccessResponse(idea, "Idea updated successfully."));
    }

    /// <summary>Delete your own idea (author only).</summary>
    [HttpDelete("{ideaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteIdea(int teamId, int ideaId, CancellationToken ct)
    {
        await _ideaService.DeleteIdeaAsync(ideaId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { deleted = true }, "Idea deleted successfully."));
    }
}
