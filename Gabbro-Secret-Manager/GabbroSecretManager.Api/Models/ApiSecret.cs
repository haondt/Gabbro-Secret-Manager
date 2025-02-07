using GabbroSecretManager.Core.Models;

namespace GabbroSecretManager.Api.Models
{
    public class ApiSecret
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required string Comments { get; set; }
        public HashSet<string> Tags { get; set; } = [];

        public static ApiSecret FromSecret(Secret secret)
        {
            return new()
            {
                Key = secret.Key,
                Value = secret.Value,
                Comments = secret.Comments,
                Tags = [.. secret.Tags],
            };
        }
    }
}
