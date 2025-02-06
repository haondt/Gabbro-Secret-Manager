using GabbroSecretManager.Core.Models;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    public interface ISecretService
    {
        Task<long> CreateSecret(Secret secret, string owner, byte[] encryptionKey);
        Task<List<Secret>> GetSecrets(string owner, byte[] encryptionKey);
        Task<List<Secret>> SearchSecrets(
            string owner,
            byte[] encryptionKey,
            string partialKey,
            HashSet<string>? withTags = null);
    }
}