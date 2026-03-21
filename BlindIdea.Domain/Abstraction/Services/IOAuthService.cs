using BlindIdea.Domain.Dtos.Auth;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Domain.Abstraction.Services
{
    public interface IOAuthService
    {
        Task<AuthResponseDto> HandleOAuthLogin(AuthenticateResult result);
    }
}
