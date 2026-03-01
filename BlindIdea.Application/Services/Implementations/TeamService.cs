using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly AppDbContext _context;

        public TeamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateTeamAsync(string name, string adminId)
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = name,
                AdminId = adminId
            };
            _context.Teams.Add(team);

            var adminUser = await _context.Users.FindAsync(adminId);
            if (adminUser == null)
                throw new Exception("Admin user not found.");

            adminUser.TeamId = team.Id;

            await _context.SaveChangesAsync();
            return team.Id;
        }

        public async Task AddUserToTeamAsync(Guid teamId, string adminId, string userId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                throw new Exception("Team not found.");

            if (team.AdminId != adminId)
                throw new Exception("Only admin can add users.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (user.TeamId != null)
                throw new Exception("User already in a team.");

            user.TeamId = teamId;
            await _context.SaveChangesAsync();
        }

        public async Task<List<TeamDto>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    AdminId = t.AdminId,
                    MemberCount = t.Members.Count
                })
                .ToListAsync();
        }
    }
}
