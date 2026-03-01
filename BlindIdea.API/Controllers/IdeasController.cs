using BlindIdea.Application.Dtos;
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
            var ideas = await _ideaService.GetAllAsync();
            return Ok(ideas);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIdeaDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var id = await _ideaService.CreateAsync(dto, userId);
            return Ok(id);
        }
    }

}
