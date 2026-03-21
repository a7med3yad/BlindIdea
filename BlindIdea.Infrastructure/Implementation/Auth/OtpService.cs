using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Entities;

namespace BlindIdea.Infrastructure.Implementation.Auth
{
    public class OtpService:IOtpService
    {
        private const int MaxOtpRequests = 3;       
        private const int WindowMinutes = 10;
        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        public DateTime GetExpiration()
        {
            return DateTime.Now.AddMinutes(5);
        }
        public bool IsExpired(DateTime? expiration)
        {
            return expiration == null || DateTime.Now > expiration;
        }

        public bool CanRequestOtp(ApplicationUser user)
        {
            var now = DateTime.UtcNow;
            if(user.OtpRequestWindowStart==null||
                now > user.OtpRequestWindowStart.Value.AddMinutes(WindowMinutes))
            {
                user.OtpRequestCount = 0;
                user.OtpRequestWindowStart = now;
            }
            if(user.OtpRequestCount >= MaxOtpRequests)
            {
                return false;
            }
            user.OtpRequestCount++;
            return true;
        }
    }
}
