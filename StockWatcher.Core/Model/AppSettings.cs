namespace StockWatcher.Core.Model
{
    public sealed class AppSettings
    {
        public string[] Pages { get; init; } = [];
        
        public Watch[] Watches { get; init; } = [];
        
        public TimeSpan PollingInterval { get; init; } = TimeSpan.FromHours(1);
        
        public MailSettings? Mail { get; init; } = null;
    }
}