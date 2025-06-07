namespace StockWatcher.Core.Model;

public sealed class MailSettings
{
    public required string SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public required string SmtpUser { get; init; }
    public required string SmtpPassword { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
        
    public static MailSettings Empty => new()
    {
        SmtpHost = string.Empty,
        SmtpUser = string.Empty,
        SmtpPassword = string.Empty,
        From     = string.Empty,
        To       = string.Empty
    };
}