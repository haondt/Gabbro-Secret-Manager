using GabbroSecretManager.Core.Models;
using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Cryptography.Services
{
    public interface IEncryptionKeyCacheService
    {
        void ClearCachedEncryptionKey(string noramlizedUsername);
        byte[] GenerateAndCacheEncryptionKey(string noramlizedUsername, string password, EncryptionKeySettings keySettings);
        byte[] GenerateEncryptionKey(string normalizedUsername, string password, EncryptionKeySettings keySettings);
        Optional<byte[]> TryGetCachedEncryptionKey(string normalizedUsername);
    }
}
