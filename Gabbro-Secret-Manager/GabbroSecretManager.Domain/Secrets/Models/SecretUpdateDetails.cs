namespace GabbroSecretManager.Domain.Secrets.Models
{
    public class SecretUpdateDetails
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required string Comments { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
