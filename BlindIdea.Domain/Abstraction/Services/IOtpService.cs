using BlindIdea.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Domain.Abstraction.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        bool CanRequestOtp(ApplicationUser user);
        DateTime GetExpiration();
        bool IsExpired(DateTime? expiration);


    }
}
