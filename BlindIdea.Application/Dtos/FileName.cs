using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    public class TeamDto
    {
        public Guid Id { get; set; }            // Team Id
        public string Name { get; set; } = null!;   // Team Name
        public string AdminId { get; set; } = null!; // Id of the Admin
        public int MemberCount { get; set; }    // Number of members in the team
    }
}
