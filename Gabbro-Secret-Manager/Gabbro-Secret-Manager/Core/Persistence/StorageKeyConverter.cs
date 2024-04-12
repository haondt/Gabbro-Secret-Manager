using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class StorageKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is StorageKey storageKey)
            {
                return storageKey.ToString();
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
                return StorageKey.Parse(str);
            throw new NotSupportedException($"Cannot convert {value?.GetType()} to {typeof(StorageKey)}");
        }
    }
}
