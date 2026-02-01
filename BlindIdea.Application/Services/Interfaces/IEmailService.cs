using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);    
    }
}
