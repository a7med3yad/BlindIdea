using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Common.Options
{
    public class EmailOptions
    {
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }

}
