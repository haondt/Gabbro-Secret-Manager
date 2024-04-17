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
        
        private delegate bool ParseMethod<TResult>(string? value, out TResult result);
        private static T ConvertValue<T>(string? value)
        {
            if (TryConvertValue<T>(value, out var convertedValue))
                return convertedValue;
            throw new InvalidCastException($"Cannot convert {value} to {typeof(T).FullName}");
        }
        private static bool TryConvertValue<T>(string? value, [NotNullWhen(true)] out T? convertedValue)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);


            (bool, T?) TryParse<TParser>(ParseMethod<TParser> parseMethod)
            {
                var parseResult = parseMethod(value, out var parsedValue);
                T? returnValue = parseResult ? (T?)(object?)parsedValue : default;
                return (parseResult, returnValue);
            }

            (bool, T?) FallBackTryParse()
            {
                if (targetType ==  typeof(Guid))
                    return TryParse<Guid>(Guid.TryParse);
                return (false, default);
            }

            (var result, convertedValue) = Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => TryParse<bool>(bool.TryParse),
                TypeCode.String => (true, (T?)(object?)value),
                TypeCode.Int16 => TryParse<int>(int.TryParse),
                TypeCode.Int32 => TryParse<int>(int.TryParse),
                TypeCode.Int64 => TryParse<int>(int.TryParse),
                TypeCode.UInt16 => TryParse<int>(int.TryParse),
                TypeCode.UInt32 => TryParse<int>(int.TryParse),
                TypeCode.UInt64 => TryParse<int>(int.TryParse),
                TypeCode.Double => TryParse<double>(double.TryParse),
                TypeCode.Decimal => TryParse<decimal>(decimal.TryParse),
                TypeCode.DateTime => TryParse<DateTime>(DateTime.TryParse),
                _ => FallBackTryParse()
            };

            return result;
        } 

        public static T GetValue<T>(this IEnumerable<KeyValuePair<string, StringValues>> values, string key)
        {
            var uncastedValue = values.Single(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value.Last(s => !string.IsNullOrEmpty(s));
            return ConvertValue<T>(uncastedValue!);
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

            return TryConvertValue<T>(stringValue, out castedValue);
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
                .Select(stringValue => (TryConvertValue<T>(stringValue, out var castedValue), castedValue))
                .Where(t => t.Item1 && t.Item2 != null)
                .Select(t => t.Item2)
                .Cast<T>();
        }
    }
}
