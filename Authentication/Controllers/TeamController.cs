using BlindIdea.Application.Dtos.Teams;
using BlindIdea.Application.Services.Implementation.Teams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly TeamService _teamService;
        public TeamController(TeamService teamService)
        {
            _teamService= teamService;
        }

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            try
            {
                var team = await _teamService.CreateTeamAsync(GetUserId(), dto);
                return Ok(team);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTeam(JoinTeamDto dto)
        {
            try
            {
                var team = await _teamService.JoinTeamAsync(GetUserId(), dto);
                return Ok(team);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my-team")]
        public async Task<IActionResult> GetMyTeam()
        {
            try
            {
                var team = await _teamService.GetMyTeamAsync(GetUserId());
                return Ok(team);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetMembers()
        {
            try
            {
                var members = await _teamService.GetMembersAsync(GetUserId());
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("leave")]
        public async Task<IActionResult> LeaveTeam()
        {
            try
            {
                await _teamService.LeaveTeamAsync(GetUserId());
                return Ok("Left team successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTeam()
        {
            try
            {
                await _teamService.DeleteTeamAsync(GetUserId());
                return Ok("Team deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("regenerate-invite")]
        public async Task<IActionResult> RegenerateInviteCode()
        {
            try
            {
                var code = await _teamService.RegenerateInviteCodeAsync(GetUserId());
                return Ok(new { inviteCode = code });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("remove-member/{memberId}")]
        public async Task<IActionResult> RemoveMember(string memberId)
        {
            try
            {
                await _teamService.RemoveMemberAsync(GetUserId(), memberId);
                return Ok("Member removed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
