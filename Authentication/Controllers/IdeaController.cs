using BlindIdea.API.Application.Ideas;
using BlindIdea.API.Dtos;
using BlindIdea.API.Dtos.Ideas;
using BlindIdea.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IdeaController : ControllerBase
    {
        private readonly IdeaService _ideaService;

        public IdeaController(IdeaService ideaService)
        {
            _ideaService = ideaService;
        }

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitIdea(SubmitIdeaDto dto)
        {
            try
            {
                var idea = await _ideaService.SubmitIdeaAsync(GetUserId(), dto);
                return Ok(idea);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("team-ideas")]
        public async Task<IActionResult> GetTeamIdeas()
        {
            try
            {
                var ideas = await _ideaService.GetTeamIdeasAsync(GetUserId());
                return Ok(ideas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{ideaId}")]
        public async Task<IActionResult> GetIdea(string ideaId)
        {
            try
            {
                var idea = await _ideaService.GetIdeaAsync(GetUserId(), ideaId);
                return Ok(idea);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{ideaId}")]
        public async Task<IActionResult> DeleteIdea(string ideaId)
        {
            try
            {
                await _ideaService.DeleteIdeaAsync(GetUserId(), ideaId);
                return Ok("Idea deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{ideaId}/rate")]
        public async Task<IActionResult> RateIdea(string ideaId, RateIdeaDto dto)
        {
            try
            {
                var idea = await _ideaService.RateIdeaAsync(GetUserId(), ideaId, dto);
                return Ok(idea);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{ideaId}/rate")]
        public async Task<IActionResult> DeleteRating(string ideaId)
        {
            try
            {
                await _ideaService.DeleteRatingAsync(GetUserId(), ideaId);
                return Ok("Rating removed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}