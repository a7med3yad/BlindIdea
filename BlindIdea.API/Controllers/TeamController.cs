using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers
{

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

        // ================= Create Team =================
        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam([FromBody] string teamName)
        {
            var userId = User.Identity!.Name!; // assuming Name = UserId
            var teamId = await _teamService.CreateTeamAsync(teamName, userId);
            return Ok(new { TeamId = teamId });
        }

        // ================= Add User to Team =================
        [HttpPost("{teamId}/add-user/{userId}")]
        public async Task<IActionResult> AddUserToTeam(Guid teamId, string userId)
        {
            var adminId = User.Identity!.Name!;
            await _teamService.AddUserToTeamAsync(teamId, adminId, userId);
            return Ok(new { Message = "User added successfully" });
        }

        // ================= Get All Teams =================
        [HttpGet]
        public async Task<IActionResult> GetAllTeams()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }
    }

}
