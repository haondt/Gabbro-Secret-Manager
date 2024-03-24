namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<string, object> _storage = [];

        public Task<bool> ContainsKey(string key) => Task.FromResult(_storage.ContainsKey(key));
        public Task Delete(string key)
        {
            _storage.Remove(key);
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key) => Task.FromResult((T)_storage[key]);

        public Task Set<T>(string key, T value)
        {
            _storage[key] = value ?? throw new ArgumentNullException(nameof(value));
            return Task.CompletedTask;
        }

        public Task<(bool, T?)> TryGet<T>(string key)
        {
            if (_storage.TryGetValue(key, out var valueObj))
            {
                return Task.FromResult((true, (T?)valueObj));
            }
            return Task.FromResult<(bool, T?)>((false, default));
        }
    }
}
