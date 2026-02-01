using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var authResponse = await _authService.RegisterAsync(dto);
            if (authResponse == null)
            {
                return BadRequest("Registration failed. User may already exist.");
            }
            return Ok(authResponse);
        }
    }
}
