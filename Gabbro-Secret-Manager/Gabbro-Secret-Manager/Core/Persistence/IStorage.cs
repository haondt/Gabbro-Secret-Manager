namespace Gabbro_Secret_Manager.Core.Persistence
{
    public interface IStorage
    {
        public Task<T> Get<T>(string key);
        public Task<(bool Success, T? Value)> TryGet<T>(string key);
        public Task<bool> ContainsKey(string key);
        public Task Set<T>(string key, T value);
        public Task Delete(string key);
    }
}
