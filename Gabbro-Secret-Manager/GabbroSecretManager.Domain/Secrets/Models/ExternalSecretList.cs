namespace GabbroSecretManager.Domain.Secrets.Models
{
    public class ExternalSecretList
    {
        public required int Version { get; set; } = -1;
        public List<ExternalSecret> Secrets { get; set; } = [];
    }

    public class ExternalSecret
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required string Comments { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
