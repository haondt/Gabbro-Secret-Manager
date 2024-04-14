using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gabbro_Secret_Manager.Core.Persistence
{
    /// <summary>
    /// Converter for <see cref="StorageKey{T}"/>
    /// </summary>
    public class StorageKeyJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StorageKey<>);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String
                || reader.Value is not string keyString
                || keyString == null)
                throw new JsonSerializationException("Unable to deserialize storage key: input string is null");

            return StorageKeyConvert.Deserialize(keyString).AsGeneric();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not StorageKey storageKey)
                throw new JsonSerializationException($"Unexpected value when trying to serialize StorageKey. Expected StorageKey, got {value.GetType().FullName}");

            var serializedKey = StorageKeyConvert.Serialize(storageKey);
            writer.WriteValue(serializedKey);
        }
    }
}
