
using MailKit.Net.Smtp;
using MimeKit;
using StockWatcher.Core.Model;

namespace StockWatcher.Core;

public sealed class EmailNotifier : INotifier<WatchState>
{
    private readonly MailSettings _cfg;
    
    public EmailNotifier(MailSettings cfg)
    {
        _cfg = cfg;
    }

    public async Task NotifyAsync(IEnumerable<WatchState> states)
    {
        var statesList = states.ToList();
        if (!statesList.Any()) return;
        
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_cfg.From));
        msg.To.Add(MailboxAddress.Parse(_cfg.To));
        msg.Subject = $"ArzneiPrivat Stock Watcher Notification - {statesList.Count(x=>x.IsAvailable)} out of {statesList.Count()} products available";
        string text = string.Join("\n\n", statesList.Select(state => state.IsAvailable ? 
            @$"{state.LastChecked:yyyy-MM-dd HH:mm}
{state.Watch.Label} is now AVAILABLE 🎉
Products available:
{string.Join("\n", state.AvailableProducts.Select(p => $"\t{p.Name} {p.Url}"))}" :
            $"{state.LastChecked:yyyy-MM-dd HH:mm}: {state.Watch.Label} is no longer available"));
        msg.Body = new TextPart("plain")
        {
            Text = text
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_cfg.SmtpHost, _cfg.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_cfg.SmtpUser, _cfg.SmtpPassword);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }
}