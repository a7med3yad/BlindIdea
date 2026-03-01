using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers
{
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

        // ================= Rate an Idea =================
        [HttpPost("{ideaId}")]
        public async Task<IActionResult> RateIdea(Guid ideaId, [FromQuery] int value)
        {
            var userId = User.Identity!.Name!; // assuming Name = UserId

            if (value < 1 || value > 5)
                return BadRequest(new { Message = "Rating must be between 1 and 5." });

            try
            {
                await _ratingService.RateIdeaAsync(ideaId, value, userId);
                return Ok(new { Message = "Idea rated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
