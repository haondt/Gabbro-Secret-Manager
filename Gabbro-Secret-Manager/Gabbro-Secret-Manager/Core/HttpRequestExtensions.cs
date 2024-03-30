using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public static class HttpRequestExtensions
    {
        public static IRequestData AsRequestData(this HttpRequest request)
        {
            return new TransientRequestData(
                () => request.Form,
                () => request.Query,
                () => request.Cookies);
        }

        public static T GetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var uncastedValue = values.Single(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value.Last(s => !string.IsNullOrEmpty(s));
            return (T)(Convert.ChangeType(uncastedValue, typeof(T)) ?? new InvalidOperationException(typeof(T).FullName));
        }

        public static T GetValueOrDefault<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, T defaultValue)
        {
            if (TryGetValue<T>(values, key, out var gotValue))
                return gotValue;
            return defaultValue;
        }

        public static bool TryGetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key, [NotNullWhen(true)]  out T? castedValue)
        {
            castedValue = default;

            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValue = kvp?.Value.Where(s => !string.IsNullOrEmpty(s)).LastOrDefault(s => !string.IsNullOrEmpty(s), null);
            if (stringValue == null)
                return false;

            try
            {
                var result = Convert.ChangeType(stringValue, typeof(T));
                if (result == null || result is not T tResult)
                    return false;
                castedValue = tResult;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<T> GetValues<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var kvp = values
                .Cast<KeyValuePair<string, StringValues>?>()
                .FirstOrDefault(kvp => kvp?.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ?? false, null);

            var stringValues = kvp?.Value.Where(s => !string.IsNullOrEmpty(s)).Where(s => !string.IsNullOrEmpty(s));
            if (stringValues == null)
                return [];

            return stringValues
                .Select(stringValue =>
                {
                    try
                    {
                        var result = Convert.ChangeType(stringValue, typeof(T));
                        if (result == null || result is not T tResult)
                            return (false, default(T?));
                        return (true, tResult);
                    }
                    catch
                    {
                        return (false, default(T?));
                    }

                })
                .Where(t => t.Item1 && t.Item2 != null)
                .Select(t => t.Item2)
                .Cast<T>();
        }
    }
}
