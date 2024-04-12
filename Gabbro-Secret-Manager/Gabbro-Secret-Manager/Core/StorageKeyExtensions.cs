using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public static class StorageKeyExtensions
    {
        public static StorageKey Extend<T>(this StorageKey key) => key.Extend(new StorageKey(typeof(T).ToString()));
        public static StorageKey Extend<T>(this StorageKey key, string extension) => key.Extend(extension.GetStorageKey<T>());
    }
}
