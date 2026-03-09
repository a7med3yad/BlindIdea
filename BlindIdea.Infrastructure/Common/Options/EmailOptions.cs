namespace BlindIdea.Infrastructure.Common.Options;

public class EmailOptions
{
    public const string SectionName = "Email";
    public string? From { get; set; }
    public string? SmtpServer { get; set; }
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string AppBaseUrl { get; set; } = "http://localhost:5000";
}
