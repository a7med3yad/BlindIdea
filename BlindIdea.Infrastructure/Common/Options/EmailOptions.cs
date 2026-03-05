using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Common.Options
{
    
    public class EmailOptions
    {
        
        public required string From { get; set; }

        public required string SmtpServer { get; set; }

        public int Port { get; set; } = 587;

        public required string Username { get; set; }

        public required string Password { get; set; }

        public bool EnableSsl { get; set; } = true;
    }
}