namespace GabbroSecretManager.Domain.Cryptography.Models
{
    internal class CryptoConstants
    {
        public const int PasswordSaltBytes = 16;
        public const int PasswordHashIterations = 10000;
        public const int PasswordHashBytes = 32;

        public const int EncryptionKeyGenerationDefaultSaltSize = 16;
        public const int EncryptionKeyGenerationDefaultIterations = 10000;
    }
}
