namespace GabbroSecretManager.Core.Models
{
    public class ApiKey
    {
        public required string Owner { get; set; }
        public required string Name { get; set; }
        public required string Id { get; set; }
    }
}
