using GabbroSecretManager.Core.Models;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Domain.Secrets.Models;
using GabbroSecretManager.Persistence.Data;
using GabbroSecretManager.Persistence.Models;
using Haondt.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    internal class SecretService(SecretsDbContext secretsDb) : ISecretService
    {
        public const int EXTERNAL_SECRET_LIST_VERSION = 1;
        private static SecretSurrogate EncryptSecret(Secret secret, string owner, byte[] encryptionKey)
        {
            var (encryptedValue, iv) = EncryptSecret(secret.Value, encryptionKey);

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
        private static (byte[] EncryptedValue, byte[] InitializationVector) EncryptSecret(string value, byte[] encryptionKey)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Crypto.AesEncrypt(valueBytes, encryptionKey);
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
        public async Task DeleteSecret(long id, string owner)
        {
            var surrogate = await secretsDb.Secrets
                .Where(s => s.Id == id && s.Owner == owner)
                .FirstOrDefaultAsync();

            if (surrogate == null)
                return;

            secretsDb.Secrets.Remove(surrogate);
            await secretsDb.SaveChangesAsync();
        }
        public Task DeleteAllSecrets(string owner)
        {
            return secretsDb.Secrets.Where(s => s.Owner == owner)
                .ExecuteDeleteAsync();
        }

        public Task<List<(long Id, Secret Secret)>> GetSecrets(string owner, byte[] encryptionKey)
        {
            return secretsDb.Secrets
                .Where(s => s.Owner == owner)
                .Include(s => s.Tags)
                .Select(s => new { Id = s.Id, Secret = DecryptSecret(s, encryptionKey) })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(q => (q.Id, q.Secret)).ToList());
        }

        private static string EscapeLikeTerm(string s)
        {
            return s
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        public Task<List<(long Id, Secret Secret)>> SearchSecrets(string owner, byte[] encryptionKey, string partialKey, HashSet<string>? withTags = null)
        {
            Console.WriteLine($"%{EscapeLikeTerm(partialKey)}%");
            var search = secretsDb.Secrets
                .Where(s => s.Owner == owner && EF.Functions.Like(s.Key, $"%{EscapeLikeTerm(partialKey)}%", "\\"));

            if (withTags != null)
                search = search.Where(s => s.Tags.Select(t => t.Tag).Intersect(withTags).Count() == withTags.Count);

            search = search.Include(s => s.Tags);

            return search
                .Select(s => new { Id = s.Id, Secret = DecryptSecret(s, encryptionKey) })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(q => (q.Id, q.Secret)).ToList());
        }

        public async Task<Optional<Secret>> TryGetSecretAsync(long id, string owner, byte[] encryptionKey)
        {
            var surrogate = await secretsDb.Secrets
                .Where(s => s.Id == id && s.Owner == owner)
                .Include(s => s.Tags)
                .FirstOrDefaultAsync();

            if (surrogate == null)
                return new();

            return DecryptSecret(surrogate, encryptionKey);
        }
        public async Task<Result> TryUpdateSecretAsync(long id, SecretUpdateDetails secret, string owner, byte[] encryptionKey)
        {
            var surrogate = await secretsDb.Secrets
                .Where(s => s.Id == id && s.Owner == owner)
                .Include(s => s.Tags)
                .FirstOrDefaultAsync();

            if (surrogate == null)
                return Result.Fail();

            var (encryptedValue, iv) = EncryptSecret(secret.Value, encryptionKey);

            surrogate.Key = secret.Key;
            surrogate.EncryptedValue = encryptedValue;
            surrogate.InitializationVector = iv;
            surrogate.Comments = secret.Comments;
            surrogate.Tags = secret.Tags.Select(t => new TagSurrogate { Tag = t }).ToList();

            await secretsDb.SaveChangesAsync();
            return Result.Succeed();
        }

        public async Task<ExternalSecretList> ExportSecrets(string owner, byte[] encryptionKey)
        {
            var secrets = await GetSecrets(owner, encryptionKey);
            return new ExternalSecretList
            {
                Version = EXTERNAL_SECRET_LIST_VERSION,
                Secrets = secrets.Select(s => new ExternalSecret
                {
                    Comments = s.Secret.Comments,
                    Key = s.Secret.Key,
                    Tags = s.Secret.Tags,
                    Value = s.Secret.Value
                }).ToList()
            };
        }
        public async Task ImportSecrets(ExternalSecretList secrets, string owner, byte[] encryptionKey)
        {
            if (secrets.Version != EXTERNAL_SECRET_LIST_VERSION)
                throw new InvalidOperationException($"Cannot import secrets. Found model version {secrets.Version} but was expecting {EXTERNAL_SECRET_LIST_VERSION}.");

            var decryptedSecrets = secrets.Secrets.Select(s => new Secret
            {
                Comments = s.Comments,
                Key = s.Key,
                Tags = s.Tags,
                Value = s.Value
            });

            var encryptedSecrets = decryptedSecrets
                .Select(s => EncryptSecret(s, owner, encryptionKey));

            secretsDb.Secrets.AddRange(encryptedSecrets);
            await secretsDb.SaveChangesAsync();
        }
    }
}
