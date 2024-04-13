using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core.Persistence
{

    [JsonConverter(typeof(StorageKeyJsonConverter))]
    [TypeConverter(typeof(StorageKeyStringConverter))]
    public class StorageKey(Type type, string value, StorageKey? parent = null) : IEquatable<StorageKey>
    {
        public Type Type => type;
        public string Value { get; } = value;
        public StorageKey? Parent { get; } = parent;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Type);
            hashCode.Add(Value);
            if (Parent is not null)
                hashCode.Add(Parent.GetHashCode());
            return hashCode.ToHashCode();
        }
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is StorageKey sko && Equals(sko);
        public bool Equals(StorageKey? other)
        {
            if (!Value.Equals(other?.Value))
                return false;
            if (Parent is null)
                return other.Parent is null;
            return Parent.Equals(other.Parent);
        }

        public static bool operator ==(StorageKey left, StorageKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StorageKey left, StorageKey right)
        {
            return !(left == right);
        }


        public static StorageKey Empty { get; } = new StorageKey(typeof(object), "");

        public override string ToString()
        {
            return Value;
        }

    }

    [JsonConverter(typeof(StorageKeyJsonConverter))]
    public class StorageKey<T> : StorageKey
    {
        public StorageKey(string value, StorageKey? parent = null) : base(typeof(T), value, parent)
        {
        }

        public StorageKey<T2> Extend<T2>(string value)
        {
            return new StorageKey<T2>(value, this);
        }

        public static new StorageKey<T> Empty { get; } = new StorageKey<T>("");

    }

    public static class StorageKeyExtensions
    {
        public static StorageKey<T> As<T>(this StorageKey storageKey)
        {
            if (storageKey.Type != typeof(T))
                throw new InvalidCastException($"Cannot convert {storageKey.Type} to {typeof(T)}");
            return new StorageKey<T>(storageKey.Value, storageKey.Parent);
        }
    }
}
