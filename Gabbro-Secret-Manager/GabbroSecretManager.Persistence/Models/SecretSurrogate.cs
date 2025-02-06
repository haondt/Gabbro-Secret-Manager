namespace GabbroSecretManager.Persistence.Models
{
    public class SecretSurrogate
    {
        public long Id { get; set; } = default!;
        public required string Key { get; set; }
        public required string Owner { get; set; }
        public required byte[] EncryptedValue { get; set; }
        public required byte[] InitializationVector { get; set; }

        public required string Comments { get; set; }
        public List<TagSurrogate> Tags { get; set; } = [];

    }

    public class TagSurrogate
    {
        public required string Tag { get; set; }

        public long SecretId { get; set; } = default!;
        public SecretSurrogate Secret { get; set; } = default!;
    }
}
