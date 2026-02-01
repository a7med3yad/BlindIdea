using BlindIdea.Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using BlindIdea.Application.Dtos;

namespace BlindIdea.Application.Services.Interfaces
{
    public interface IAuthService
    {

        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    }
}
