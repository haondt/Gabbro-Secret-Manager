namespace GabbroSecretManager.Domain.Cryptography.Models
{
    public class EncryptionKeyCacheSettings
    {
        public int LifetimeMinutes { get; set; } = 15;
    }
}