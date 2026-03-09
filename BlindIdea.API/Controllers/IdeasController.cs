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

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchIdeasRequest request, CancellationToken cancellationToken)
    {
        var ideas = await _ideaService.SearchIdeasAsync(request);
        return Ok(ideas);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var idea = await _ideaService.GetIdeaAsync(id, userId);
        if (idea == null) return NotFound();
        return Ok(idea);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateIdeaRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var idea = await _ideaService.CreateIdeaAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = idea!.Id }, idea);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIdeaRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        try
        {
            var idea = await _ideaService.UpdateIdeaAsync(id, request, userId);
            if (idea == null) return NotFound();
            return Ok(idea);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _ideaService.DeleteIdeaAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("team/{teamId:guid}")]
    public async Task<IActionResult> GetByTeam(Guid teamId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var ideas = await _ideaService.GetTeamIdeasAsync(teamId, pageNumber, pageSize);
        return Ok(ideas);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var ideas = await _ideaService.GetUserIdeasAsync(userId, pageNumber, pageSize);
        return Ok(ideas);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var stats = await _ideaService.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("top")]
    public async Task<IActionResult> GetTop([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var ideas = await _ideaService.GetTopIdeasAsync(count);
        return Ok(ideas);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var ideas = await _ideaService.GetRecentIdeasAsync(count);
        return Ok(ideas);
    }
}
