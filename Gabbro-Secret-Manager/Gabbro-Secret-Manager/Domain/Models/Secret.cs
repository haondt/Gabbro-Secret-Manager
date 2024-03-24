namespace Gabbro_Secret_Manager.Domain.Models
{
    public class Secret
    {
        public List<string> Tags { get; set; } = [];
        public required string Name { get; set; }
        public required string EncryptedValue { get; set; }
        public required string Owner { get; set; }
        public required string InitializationVector { get; set; }
    }
}
