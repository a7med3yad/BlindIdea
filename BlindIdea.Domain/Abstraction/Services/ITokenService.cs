using BlindIdea.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Domain.Abstraction.Services
{
    public interface ITokenService
    {
        Task<string> CreateAccessToken(ApplicationUser user);
        Task<RefreshToken> CreateRefreshToken(ApplicationUser user);


    }
}
