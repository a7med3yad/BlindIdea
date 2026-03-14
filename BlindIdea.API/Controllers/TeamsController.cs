using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Teams.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found.");

    /// <summary>Get all teams the current user belongs to or administers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Greeting()
    {
        return Ok(new
        {
            message = "Welcome! I'm Ahmed Ayad ??",
            description = "Thanks for visiting my API. Everything is up and running!",
            status = "online",
            timestamp = DateTime.UtcNow
        });
    }
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetMyTeams(CancellationToken ct)
    {
        var teams = await _teamService.GetUserTeamsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(teams));
    }

    /// <summary>Get a specific team by ID (must be a member).</summary>
    [HttpGet("{teamId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetTeam(int teamId, CancellationToken ct)
    {
        var team = await _teamService.GetTeamAsync(teamId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(team));
    }

    /// <summary>Create a new team. The caller becomes the admin.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken ct)
    {
        var team = await _teamService.CreateTeamAsync(CurrentUserId, request, ct);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(team, "Team created successfully.", 201));
    }

    /// <summary>Update a team's name or description (admin only).</summary>
    [HttpPut("{teamId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] UpdateTeamRequest request, CancellationToken ct)
    {
        var team = await _teamService.UpdateTeamAsync(teamId, CurrentUserId, request, ct);
        return Ok(ApiResponse<object>.SuccessResponse(team, "Team updated successfully."));
    }

    /// <summary>Soft-delete a team (admin only).</summary>
    [HttpDelete("{teamId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteTeam(int teamId, CancellationToken ct)
    {
        await _teamService.DeleteTeamAsync(teamId, CurrentUserId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { deleted = true }, "Team deleted successfully."));
    }

    /// <summary>Add a member to the team (admin only).</summary>
    [HttpPost("{teamId:int}/members")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> AddMember(int teamId, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
        await _teamService.AddMemberAsync(teamId, CurrentUserId, request, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { added = true }, "Member added successfully."));
    }

    /// <summary>Remove a member from the team (admin only).</summary>
    [HttpDelete("{teamId:int}/members/{memberId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> RemoveMember(int teamId, string memberId, CancellationToken ct)
    {
        await _teamService.RemoveMemberAsync(teamId, CurrentUserId, memberId, ct);
        return Ok(ApiResponse<object>.SuccessResponse(new { removed = true }, "Member removed successfully."));
    }
}
