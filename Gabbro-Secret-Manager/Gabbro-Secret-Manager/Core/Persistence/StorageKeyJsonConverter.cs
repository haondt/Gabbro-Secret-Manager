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
            JObject obj = JObject.Load(reader);
            var keydata = obj.ToObject<StorageKeyRepresentation>() ?? throw new JsonSerializationException("Unable to deserialize storage key");
            var storageKeyType = typeof(StorageKey<>).MakeGenericType(keydata.Type);
            return Activator.CreateInstance(storageKeyType, keydata.Value, keydata.Parent?.AsStorageKey());
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull(); 
                return;
            }
            if (value is not StorageKey sk)
                throw new JsonSerializationException($"object was of unexpected type {value.GetType()}");
            JObject obj = JObject.FromObject(StorageKeyRepresentation.FromStorageKey(sk), serializer);
            obj.WriteTo(writer);
        }
    }
}
