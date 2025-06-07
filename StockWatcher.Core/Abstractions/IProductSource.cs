namespace StockWatcher.Core;

public interface IProductSource
{
    Task<IReadOnlyList<Product>> Fetch(string url);
}