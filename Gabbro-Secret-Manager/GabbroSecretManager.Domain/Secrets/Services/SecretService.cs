using GabbroSecretManager.Core.Models;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Persistence.Data;
using GabbroSecretManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    internal class SecretService(SecretsDbContext secretsDb) : ISecretService
    {
        private static SecretSurrogate EncryptSecret(Secret secret, string owner, byte[] encryptionKey)
        {
            var valueBytes = Encoding.UTF8.GetBytes(secret.Value);
            var (encryptedValue, iv) = Crypto.AesEncrypt(valueBytes, encryptionKey);

            return new()
            {
                Comments = secret.Comments,
                Owner = owner,
                Key = secret.Key,
                EncryptedValue = encryptedValue,
                InitializationVector = iv,
                Tags = secret.Tags.Select(t => new TagSurrogate
                {
                    Tag = t
                }).ToList()
            };
        }
        private static Secret DecryptSecret(SecretSurrogate surrogate, byte[] encryptionKey)
        {
            var valueBytes = Crypto.AesDecrypt(surrogate.EncryptedValue, encryptionKey, surrogate.InitializationVector);
            var decryptedValue = Encoding.UTF8.GetString(valueBytes);

            return new()
            {
                Comments = surrogate.Comments,
                Key = surrogate.Key,
                Value = decryptedValue,
                Tags = surrogate.Tags.Select(t => t.Tag).ToList()
            };
        }

        public async Task<long> CreateSecret(Secret secret, string owner, byte[] encryptionKey)
        {
            var surrogate = EncryptSecret(secret, owner, encryptionKey);
            secretsDb.Add(surrogate);
            await secretsDb.SaveChangesAsync();
            return surrogate.Id;
        }

        public Task<List<Secret>> GetSecrets(string owner, byte[] encryptionKey)
        {
            return secretsDb.Secrets
                .Where(s => s.Owner == owner)
                .Include(s => s.Tags)
                .Select(s => DecryptSecret(s, encryptionKey))
                .ToListAsync();
        }

        private static string EscapeLikeTerm(string s)
        {
            return s
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("\\", "[\\]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }

        public Task<List<Secret>> SearchSecrets(string owner, byte[] encryptionKey, string partialKey, HashSet<string>? withTags = null)
        {
            var search = secretsDb.Secrets
                .Where(s => s.Owner == owner && EF.Functions.Like(s.Key, $"%{EscapeLikeTerm(partialKey)}%"));

            if (withTags != null)
                search = search.Where(s => s.Tags.Select(t => t.Tag).Intersect(withTags).Count() == withTags.Count);

            search = search.Include(s => s.Tags);

            return search
                .Select(s => DecryptSecret(s, encryptionKey))
                .ToListAsync();
        }
    }
}
