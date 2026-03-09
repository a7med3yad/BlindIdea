using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var team = await _teamService.CreateTeamAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = team!.Id }, team);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var team = await _teamService.GetTeamAsync(id);
        if (team == null) return NotFound();
        return Ok(team);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var (teams, totalCount) = await _teamService.GetTeamsAsync(pageNumber, pageSize);
        return Ok(new { teams, totalCount });
    }

    [HttpGet("my-teams")]
    public async Task<IActionResult> GetMyTeams(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var teams = await _teamService.GetUserTeamsAsync(userId);
        return Ok(teams);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var team = await _teamService.UpdateTeamAsync(id, request, userId);
            if (team == null) return NotFound();
            return Ok(team);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _teamService.DeleteTeamAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        try
        {
            var result = await _teamService.AddMemberAsync(id, request, adminId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}/members")]
    public async Task<IActionResult> RemoveMember(Guid id, [FromBody] RemoveTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(requesterId)) return Unauthorized();
        try
        {
            var result = await _teamService.RemoveMemberAsync(id, request, requesterId);
            if (!result) return NotFound();
            return Ok(new { message = "Member removed successfully" });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teamService.GetTeamMembersAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:guid}/transfer-admin")]
    public async Task<IActionResult> TransferAdmin(Guid id, [FromBody] TransferAdminRequest request, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();
        try
        {
            var team = await _teamService.TransferAdminAsync(id, request, adminId);
            if (team == null) return NotFound();
            return Ok(team);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var (teams, totalCount) = await _teamService.SearchTeamsAsync(searchTerm ?? "", pageNumber, pageSize);
        return Ok(new { teams, totalCount });
    }

    [HttpGet("{id:guid}/statistics")]
    public async Task<IActionResult> GetStatistics(Guid id, CancellationToken cancellationToken)
    {
        var stats = await _teamService.GetTeamStatisticsAsync(id);
        if (stats == null) return NotFound();
        return Ok(stats);
    }
}
