namespace BlindIdea.API.Services
{
    public class OtpService
    {
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
    }
}
