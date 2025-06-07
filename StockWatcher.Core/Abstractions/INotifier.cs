namespace StockWatcher.Core;

public interface INotifier<T>
{
    Task NotifyAsync(IEnumerable<T> states);
}