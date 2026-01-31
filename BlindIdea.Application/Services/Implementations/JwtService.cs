using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlindIdea.Application.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateAcessToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("JWT:AccessToken")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescription = new JwtSecurityToken(
                issuer: _config.GetValue<string>("JWT:Issuer"),
                audience: _config.GetValue<string>("JWT:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescription);
        }

      

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public string HashToken()
        {
            throw new NotImplementedException();
        }
    }
}
