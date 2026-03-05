using BlindIdea.Application.Dtos.Team.Requests;
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
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            var userId = User.Identity!.Name!; // assuming Name = UserId
            var team = await _teamService.CreateTeamAsync(request, userId);
            return Ok(team);
        }

        // ================= Add User to Team =================
        [HttpPost("{teamId}/add-user/{userId}")]
        public async Task<IActionResult> AddUserToTeam(Guid teamId, string userId)
        {
            var adminId = User.Identity!.Name!;
            var request = new AddTeamMemberRequest { UserId = userId };
            await _teamService.AddMemberAsync(teamId, request, adminId);
            return Ok(new { Message = "User added successfully" });
        }

        // ================= Get All Teams =================
        [HttpGet]
        public async Task<IActionResult> GetAllTeams()
        {
            var (teams, totalCount) = await _teamService.GetTeamsAsync();
            return Ok(new { teams, totalCount });
        }
    }

}
