﻿using GabbroSecretManager.Core.Models;
using GabbroSecretManager.Domain.Secrets.Models;
using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    public interface ISecretService
    {
        Task<long> CreateSecret(Secret secret, string owner, byte[] encryptionKey);
        Task DeleteSecret(long id, string owner);
        Task<List<(long Id, Secret Secret)>> GetSecrets(string owner, byte[] encryptionKey);
        Task<Optional<Secret>> TryGetSecretAsync(long id, string owner, byte[] encryptionKey);
        Task<List<(long Id, Secret Secret)>> SearchSecrets(
            string owner,
            byte[] encryptionKey,
            string partialKey,
            HashSet<string>? withTags = null);
        Task<Result> TryUpdateSecretAsync(long id, SecretUpdateDetails secret, string owner, byte[] encryptionKey);
    }
}