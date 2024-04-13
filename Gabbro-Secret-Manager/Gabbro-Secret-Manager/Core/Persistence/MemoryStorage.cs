namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class MemoryStorage : IStorage
    {
        protected readonly Dictionary<StorageKey, object> _storage = [];

        public Task<bool> ContainsKey(StorageKey key) => Task.FromResult(_storage.ContainsKey(key));
        public Task Delete(StorageKey key)
        {
            _storage.Remove(key);
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(StorageKey<T> key) => Task.FromResult((T)_storage[key]);

        public Task Set<T>(StorageKey<T> key, T value)
        {
            _storage[key] = value ?? throw new ArgumentNullException(nameof(value));
            return Task.CompletedTask;
        }

        public Task<(bool, T?)> TryGet<T>(StorageKey<T> key)
        {
            if (_storage.TryGetValue(key, out var valueObj) && valueObj is T)
            {
                return Task.FromResult((true, (T?)valueObj));
            }
            return Task.FromResult<(bool, T?)>((false, default));
        }
    }
}
