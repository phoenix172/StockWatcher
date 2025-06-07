using StockWatcher.Core.Model;

namespace StockWatcher.Core;

public record WatchState(Watch Watch, IReadOnlyList<Product> AvailableProducts, DateTimeOffset LastChecked)
{
    public bool IsAvailable => AvailableProducts.Any();
};

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IProductSource _productSource;
    private readonly IStateStore<WatchState> _store;
    private readonly INotifier<WatchState> _notifier;
    private readonly AppSettings _settings;

    public Worker(ILogger<Worker> logger, IProductSource productSource, IStateStore<WatchState> store, INotifier<WatchState> notifier, AppSettings settings)
    {
        _logger = logger;
        _productSource = productSource;
        _store = store;
        _notifier = notifier;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            }
            
            var watchedProducts = await WatchProducts();
            _logger.LogInformation("Products available: {AvailableProducts}", string.Join(",", watchedProducts.Select(x=>x.Watch.Label)));
            
            var stateChanges = await GetChangedWatchStates(watchedProducts);

            await _notifier.NotifyAsync(stateChanges.Select(x=>x.New));
            _logger.LogInformation("Sent notifications for {Count} watch changes", stateChanges.Count);

            await Task.Delay(_settings.PollingInterval, stoppingToken);
        }
    }

    private async Task<List<(WatchState? Old, WatchState New)>> GetChangedWatchStates(IReadOnlyList<WatchState> watchedProducts)
    {
        var stateChanges = new List<(WatchState? Old, WatchState New)>();
        foreach (var watchState in watchedProducts)
        {
            var oldState = await _store.Get(watchState.Watch.Label);
            if (oldState?.IsAvailable != watchState.IsAvailable)
            {
                stateChanges.Add((oldState, watchState));
                _logger.LogInformation("New state for watch '{WatchLabel}': {IsAvailable}", watchState.Watch.Label, watchState.IsAvailable);
            }
            await _store.Set(watchState.Watch.Label, watchState);
        }

        return stateChanges;
    }

    private async Task<IReadOnlyList<WatchState>> WatchProducts()
    {
        var fetchTasks = _settings.Pages.Select(async page => await _productSource.Fetch(page));
        var availableProducs = (await Task.WhenAll(fetchTasks)).SelectMany(x => x).Where(x=>x.IsAvailable);
        var watchedProducts = _settings.Watches.SelectMany(watch => availableProducs
            .Where(product => watch.Keywords.All(k => product.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .Select(product => new { watch, product }))
            .GroupBy(x => x.watch, x => x.product)
            .Select(x => new WatchState(x.Key, x.ToList(), DateTimeOffset.Now))
            .ToList();
        var emptyWatches = _settings.Watches.Except(watchedProducts.Select(x => x.Watch)).Select(x => new WatchState(x, [], DateTimeOffset.Now));
        return watchedProducts.Concat(emptyWatches).ToList();
    }
}