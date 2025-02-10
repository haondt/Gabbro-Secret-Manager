using GabbroSecretManager.Core.Models;

namespace GabbroSecretManager.Persistence.Models
{
    public class ApiKeySurrogate
    {
        public required string Owner { get; set; }
        public required string Name { get; set; }
        public string Id { get; set; } = "";
        public static ApiKeySurrogate FromApiKey(ApiKey apiKey)
        {
            return new()
            {
                Name = apiKey.Name,
                Owner = apiKey.Owner,
                Id = apiKey.Id
            };
        }

        public ApiKey ToApiKey()
        {
            return new()
            {
                Id = Id,
                Owner = Owner,
                Name = Name,
            };
        }
    }
}
