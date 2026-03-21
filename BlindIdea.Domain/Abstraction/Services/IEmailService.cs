namespace BlindIdea.Domain.Abstraction.Services
{
    public interface IEmailService
    {
        Task SendOtp(string email, string otp);
    }
}
