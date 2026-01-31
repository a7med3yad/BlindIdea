using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BlindIdea.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        public Task ConfirmEmailAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto dto)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAllAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(RegisterDto dto)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
