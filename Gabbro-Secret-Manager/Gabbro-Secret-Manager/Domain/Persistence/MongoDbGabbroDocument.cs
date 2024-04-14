using Gabbro_Secret_Manager.Core.Persistence;
using MongoDB.Bson.Serialization.Attributes;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    [BsonIgnoreExtraElements]
    public class MongoDbGabbroDocument<T>
    {
        public required StorageKey Key { get; set; }
        public required T Value { get; set; }
    }
}
