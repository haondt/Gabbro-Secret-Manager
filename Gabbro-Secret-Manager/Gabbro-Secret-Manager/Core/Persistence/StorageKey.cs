using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core.Persistence
{
    [TypeConverter(typeof(StorageKeyConverter))]
    public readonly struct StorageKey : IEquatable<StorageKey>, IParsable<StorageKey>
    {
        private const string INTERNAL_VALUE_PREFIX = "StorageKey:";

        public StorageKey(string value)
        {
            Value = $"{INTERNAL_VALUE_PREFIX}{value}";
        }

        private string Value { get; }

        private static string GetRawValue(string internalValue)
        {
            if (!internalValue.StartsWith(INTERNAL_VALUE_PREFIX))
                throw new ArgumentException($"{nameof(Value)} in unexpected format");
            return internalValue[INTERNAL_VALUE_PREFIX.Length..];
        }
        private static bool TryGetRawValue(string? internalValue, out string rawValue)
        {
            rawValue = "";
            if (string.IsNullOrEmpty(internalValue))
                return false;
            if (!internalValue.StartsWith(INTERNAL_VALUE_PREFIX))
                return false;
            rawValue = internalValue[INTERNAL_VALUE_PREFIX.Length..];
            return true;
        }

        public override readonly int GetHashCode() => Value.GetHashCode();
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is StorageKey sko && Equals(sko);
        public readonly bool Equals(StorageKey other)
        {
            return Value.Equals(other.Value);
        }

        public static bool operator ==(StorageKey left, StorageKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StorageKey left, StorageKey right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Value;
        }

        public static StorageKey Empty { get; } = new StorageKey("");

        public readonly StorageKey Extend(StorageKey extension)
            => new(GetRawValue(Value) + "+" + GetRawValue(extension.Value));

        public static StorageKey Parse(string s, IFormatProvider? provider = null) => new(GetRawValue(s));

        public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out StorageKey result)
            => TryParse(s, null, out result);

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out StorageKey result)
        {
            if (!TryGetRawValue(s, out var rawValue))
            {
                result = StorageKey.Empty;
                return false;
            }
            result = new(rawValue);
            return true;
        }
    }
}
