using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/teams")]
[ApiController]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Create a new team. The authenticated user becomes the admin.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var team = await _teamService.CreateTeamAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(team!, "Team created successfully", 201));
    }

    /// <summary>
    /// Get a team by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var team = await _teamService.GetTeamAsync(id);
        if (team == null)
            return NotFound(ApiResponse<object>.FailureResponse("Team not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(team));
    }

    /// <summary>
    /// Get all teams with pagination.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (teams, totalCount) = await _teamService.GetTeamsAsync(pageNumber, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(new { teams, totalCount, pageNumber, pageSize }));
    }

    /// <summary>
    /// Get teams the authenticated user is a member of.
    /// </summary>
    [HttpGet("my-teams")]
    public async Task<IActionResult> GetMyTeams()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var teams = await _teamService.GetUserTeamsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(teams));
    }

    /// <summary>
    /// Update a team. Only the team admin can update.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var team = await _teamService.UpdateTeamAsync(id, request, userId);
        if (team == null)
            return NotFound(ApiResponse<object>.FailureResponse("Team not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(team, "Team updated successfully"));
    }

    /// <summary>
    /// Soft-delete a team. Only the team admin can delete.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _teamService.DeleteTeamAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Team deleted successfully"));
    }

    /// <summary>
    /// Join a team (self-join). The authenticated user joins the team.
    /// </summary>
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> JoinTeam(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _teamService.JoinTeamAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(result!, "Joined team successfully"));
    }

    /// <summary>
    /// Leave a team. The authenticated user leaves the team. Admin cannot leave.
    /// </summary>
    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> LeaveTeam(Guid id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _teamService.LeaveTeamAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Left team successfully"));
    }

    /// <summary>
    /// Add a member to the team. Only the team admin can add members.
    /// </summary>
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddTeamMemberRequest request)
    {
        var adminId = GetUserId();
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var result = await _teamService.AddMemberAsync(id, request, adminId);
        return Ok(ApiResponse<object>.SuccessResponse(result!, "Member added successfully"));
    }

    /// <summary>
    /// Remove a member from the team. Admin can remove anyone; members can remove themselves.
    /// </summary>
    [HttpDelete("{id:guid}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(Guid id, string memberId)
    {
        var requesterId = GetUserId();
        if (string.IsNullOrEmpty(requesterId)) return Unauthorized();

        await _teamService.RemoveMemberAsync(id, memberId, requesterId);
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Member removed successfully"));
    }

    /// <summary>
    /// Get all members of a team.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var result = await _teamService.GetTeamMembersAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.FailureResponse("Team not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    /// <summary>
    /// Transfer admin role to another team member. Only the current admin can do this.
    /// </summary>
    [HttpPost("{id:guid}/transfer-admin")]
    public async Task<IActionResult> TransferAdmin(Guid id, [FromBody] TransferAdminRequest request)
    {
        var adminId = GetUserId();
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var team = await _teamService.TransferAdminAsync(id, request, adminId);
        return Ok(ApiResponse<object>.SuccessResponse(team!, "Admin role transferred successfully"));
    }

    /// <summary>
    /// Search teams by name or description.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (teams, totalCount) = await _teamService.SearchTeamsAsync(searchTerm ?? "", pageNumber, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(new { teams, totalCount, pageNumber, pageSize }));
    }

    /// <summary>
    /// Get team statistics (member count, idea count, ratings).
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatistics(Guid id)
    {
        var stats = await _teamService.GetTeamStatisticsAsync(id);
        if (stats == null)
            return NotFound(ApiResponse<object>.FailureResponse("Team not found", statusCode: 404));

        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }
}
