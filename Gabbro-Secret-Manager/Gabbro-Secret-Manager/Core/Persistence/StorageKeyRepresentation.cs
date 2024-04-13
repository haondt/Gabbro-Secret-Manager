namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class StorageKeyRepresentation
    {
        public required Type Type { get; set; }
        public required string Value { get; set; }
        public StorageKeyRepresentation? Parent { get; set; }

        public static StorageKeyRepresentation FromStorageKey(StorageKey storageKey)
        {
            var repr = new StorageKeyRepresentation
            {
                Type = storageKey.Type,
                Value = storageKey.Value,
            };

            if (storageKey.Parent is not null)
                repr.Parent = FromStorageKey(storageKey.Parent);
            return repr;
        }

        public StorageKey AsStorageKey()
        {
            return new StorageKey(Type, Value, Parent?.AsStorageKey());
        }
    }
}
