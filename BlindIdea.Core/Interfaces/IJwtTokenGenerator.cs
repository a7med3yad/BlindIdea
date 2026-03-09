using BlindIdea.Core.Entities;

namespace BlindIdea.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
