using BlindIdea.Application.Dtos.Teams;
using BlindIdea.Application.Services.Abstraction.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        // -- Multi-team endpoints ----------------------------

        [HttpGet("my-teams")]
        public async Task<IActionResult> GetMyTeams()
        {
            try
            {
                var teams = await _teamService.GetMyTeamsAsync(GetUserId());
                return Ok(teams);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTeam()
        {
            try
            {
                var team = await _teamService.GetActiveTeamAsync(GetUserId());
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("switch")]
        public async Task<IActionResult> SwitchTeam(SwitchTeamDto dto)
        {
            try
            {
                var team = await _teamService.SwitchTeamAsync(GetUserId(), dto);
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Create / Join -----------------------------------

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            try
            {
                var team = await _teamService.CreateTeamAsync(GetUserId(), dto);
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTeam(JoinTeamDto dto)
        {
            try
            {
                var team = await _teamService.JoinTeamAsync(GetUserId(), dto);
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Members -----------------------------------------

        [HttpGet("members")]
        public async Task<IActionResult> GetMembers()
        {
            try
            {
                var members = await _teamService.GetMembersAsync(GetUserId());
                return Ok(members);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Leave -------------------------------------------

        [HttpPost("leave/{teamId}")]
        public async Task<IActionResult> LeaveTeam(string teamId)
        {
            try
            {
                await _teamService.LeaveTeamAsync(GetUserId(), teamId);
                return Ok("Left team successfully");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Delete (admin only) -----------------------------

        [HttpDelete("delete/{teamId}")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            try
            {
                await _teamService.DeleteTeamAsync(GetUserId(), teamId);
                return Ok("Team deleted successfully");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Regenerate invite -------------------------------

        [HttpPost("regenerate-invite/{teamId}")]
        public async Task<IActionResult> RegenerateInvite(string teamId)
        {
            try
            {
                var inviteCode = await _teamService.RegenerateInviteCodeAsync(GetUserId(), teamId);
                return Ok(new { inviteCode });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // -- Remove member -----------------------------------

        [HttpDelete("remove-member/{teamId}/{memberId}")]
        public async Task<IActionResult> RemoveMember(string teamId, string memberId)
        {
            try
            {
                await _teamService.RemoveMemberAsync(GetUserId(), memberId, teamId);
                return Ok("Member removed successfully");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}
