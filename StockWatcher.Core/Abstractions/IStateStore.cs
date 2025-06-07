namespace StockWatcher.Core
{
    public interface IStateStore<T>
    {
        Task<T?> Get(string label);
        Task Set(string label, T available);
    }
}
