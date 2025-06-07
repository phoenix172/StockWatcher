using System.Text.Json;

namespace StockWatcher.Core
{
    public sealed class FileStateStore<T> : IStateStore<T>
    {
        private readonly string _path;
        private readonly Dictionary<string, T> _cache;

        public FileStateStore(string fileName = "state.json")
        {
            _path = fileName;
            _cache = File.Exists(_path)
                ? JsonSerializer.Deserialize<Dictionary<string, T>>(File.ReadAllText(_path)) ?? new()
                : new();
        }

        public Task<T?> Get(string label)
            => Task.FromResult(_cache.TryGetValue(label, out var v) ? (T?)v : default);

        public async Task Set(string label, T state)
        {
            _cache[label] = state;
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(_cache));
        }
    }
}
