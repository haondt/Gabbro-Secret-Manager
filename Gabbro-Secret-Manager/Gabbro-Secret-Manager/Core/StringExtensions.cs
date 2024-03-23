namespace Gabbro_Secret_Manager.Core
{
    public static class StringExtensions
    {
        public static string GetStorageKey<T>(this string key) => typeof(T) + "___" + key;

    }
}
