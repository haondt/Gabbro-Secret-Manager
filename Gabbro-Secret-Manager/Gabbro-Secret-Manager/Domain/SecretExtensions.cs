using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain
{
    public static class SecretExtensions
    {
        public static DecryptedSecret AsDecrypted(this Secret secret, string decryptedValue) => new()
        {
            Comments = secret.Comments,
            Name = secret.Name,
            Tags = secret.Tags,
            Value = decryptedValue
        };
    }
}
