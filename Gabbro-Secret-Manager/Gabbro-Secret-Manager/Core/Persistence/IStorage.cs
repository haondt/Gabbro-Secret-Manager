namespace Gabbro_Secret_Manager.Core.Persistence
{
    public interface IStorage
    {
        public Task<T> Get<T>(StorageKey key);
        public Task<(bool Success, T? Value)> TryGet<T>(StorageKey key);
        public Task<bool> ContainsKey(StorageKey key);
        public Task Set<T>(StorageKey key, T value);
        public Task Delete(StorageKey key);
    }
}
