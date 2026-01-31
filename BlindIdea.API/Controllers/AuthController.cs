using BlindIdea.Application.Dtos;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("redister")]
        public IActionResult<User> Register(UserDto request){
            return Ok("Register successful");
        }
    }
}
