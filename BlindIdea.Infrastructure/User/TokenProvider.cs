
using Microsoft.Extensions.Configuration;
using BlindIdea.Core.Entities;
using UserEntity = BlindIdea.Core.Entities.User;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BlindIdea.Infrastructure.User;

internal sealed class TokenProvider(IConfiguration configuration)
{
       public string Create(UserEntity user)
   {
       string secretKey = configuration["Jwt:Secret"]!;
       var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

       var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

       var tokenDescriptor = new SecurityTokenDescriptor
       {
           Subject = new ClaimsIdentity(
           [
               new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
               new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email),
               new Claim("email_verified", user.EmailVerified.ToString())
           ]),
           Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
           SigningCredentials = credentials,
           Issuer = configuration["Jwt:Issuer"],
           Audience = configuration["Jwt:Audience"]
       };

       var handler = new JsonWebTokenHandler();

       string token = handler.CreateToken(tokenDescriptor);

       return token;
   }
}
