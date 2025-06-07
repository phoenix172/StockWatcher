using StockWatcher.Core.Model;

namespace StockWatcher.Core;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        var appSettings = builder.Configuration.Get<AppSettings>() ??
                          throw new ArgumentException("App Settings not found");
        if (Path.GetDirectoryName(appSettings.StateFilePath) is { } directory)
            Directory.CreateDirectory(directory);
        
        builder.Services
            .AddHostedService<Worker>()
            .AddSingleton(appSettings)
            .AddSingleton(appSettings.Mail ?? MailSettings.Empty)
            .AddHttpClient()
            .AddSingleton<IProductSource, ProductSource>()
            .AddSingleton<INotifier<WatchState>, EmailNotifier>()
            .AddSingleton<IStateStore<WatchState>, FileStateStore<WatchState>>(_ => new FileStateStore<WatchState>(appSettings.StateFilePath))
            .AddSingleton<Worker>();
        
        var host = builder.Build();
        host.Run();
    }
}