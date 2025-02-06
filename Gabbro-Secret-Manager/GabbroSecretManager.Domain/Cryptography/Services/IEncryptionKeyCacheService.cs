using GabbroSecretManager.Core.Models;
using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Cryptography.Services
{
    public interface IEncryptionKeyCacheService
    {
        void ClearEncryptionKey(string noramlizedUsername);
        byte[] CreateEncryptionKey(string noramlizedUsername, string password, EncryptionKeySettings keySettings);
        Optional<byte[]> TryGetEncryptionKey(string normalizedUsername);
    }
}
