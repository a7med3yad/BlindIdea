using BlindIdea.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Interfaces
{
    public interface IJwtService
    {
        string CreateAccessToken(User user);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}
