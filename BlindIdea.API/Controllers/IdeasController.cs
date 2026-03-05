using BlindIdea.Application.Dtos;
using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/ideas")]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService _ideaService;

        public IdeasController(IIdeaService ideaService)
        {
            _ideaService = ideaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ideas = await _ideaService.SearchIdeasAsync(new SearchIdeasRequest());
            return Ok(ideas);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIdeaRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var idea = await _ideaService.CreateIdeaAsync(request, userId);
            return Ok(idea);
        }
    }

}