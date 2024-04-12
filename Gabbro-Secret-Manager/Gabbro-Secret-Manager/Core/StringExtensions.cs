using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public static class StringExtensions
    {
        public static StorageKey GetStorageKey<T>(this string key) => new StorageKey(typeof(T) + "___" + key);
    }
}
