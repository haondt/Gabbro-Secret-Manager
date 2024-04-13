using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class StorageKeyStringConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is StorageKey storageKey)
            {
                return Serialize(storageKey);
            }

            throw new NotSupportedException($"Cannot convert {value?.GetType()} to {destinationType}");
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
                return Deserialize(str);
            throw new NotSupportedException($"Cannot convert {value?.GetType()} to {typeof(StorageKey)}");
        }

        private static string Serialize(StorageKey storageKey)
        {
            return JsonConvert.SerializeObject(StorageKeyRepresentation.FromStorageKey(storageKey));
        }

        private static StorageKey Deserialize(string str)
        {
            return JsonConvert.DeserializeObject<StorageKeyRepresentation>(str)?.AsStorageKey()
                ?? throw new InvalidOperationException("Unable to deserialize storage key");
        }

    }
}
