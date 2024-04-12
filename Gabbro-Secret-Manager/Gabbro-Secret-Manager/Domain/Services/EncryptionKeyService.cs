using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class EncryptionKeyService(IOptions<EncryptionKeyServiceSettings> options)
    {
        private readonly Dictionary<string, byte[]> _keys = new();
        private readonly Queue<string> _keyQueue = new();
        private readonly object _dictLock = new();

        private byte[] GenerateEncryptionKey(StorageKey userKey, string password, EncryptionKeySettings encryptionKeySettings)
        {
            var uHash = CryptoService.GenerateHash(userKey.ToString());
            var pHash = CryptoService.GenerateHash(password);
            var keyBytes = CryptoService.GenerateHash(
                Convert.ToBase64String(uHash.Concat(pHash).ToArray()),
                Convert.FromBase64String(encryptionKeySettings.Salt),
                encryptionKeySettings.Iterations,
                32);
            return keyBytes;
        }

        public bool TryGet(string sessionToken, [NotNullWhen(true)] out byte[]? key)
        {
            return _keys.TryGetValue(sessionToken, out key);
        }

        public byte[] CreateApiEncryptionKey(StorageKey userKey, string password, EncryptionKeySettings encryptionKeySettings) => GenerateEncryptionKey(userKey, password, encryptionKeySettings);

        public byte[] Get(string sessionToken) => _keys[sessionToken];
        public bool Contains(string sessionToken) => _keys.ContainsKey(sessionToken);

        public byte[] GetOrCreateEncryptionKey(string sessionToken, StorageKey userKey, string password, EncryptionKeySettings encryptionKeySettings)
        {
            if (TryGet(sessionToken, out var existingKey))
                return existingKey!;

            return UpsertEncryptionKey(sessionToken, userKey, password, encryptionKeySettings);
        }

        public byte[] UpsertEncryptionKey(string sessionToken, StorageKey userKey, string password, EncryptionKeySettings encryptionKeySettings)
        {
            var newKey = GenerateEncryptionKey(userKey, password, encryptionKeySettings);
            Set(sessionToken, newKey);
            return newKey;
        }

        public void Set(string sessionToken, byte[] value)
        {
            lock(_dictLock)
            {
                _keys[sessionToken] = value;
                _keyQueue.Enqueue(sessionToken);
                while (_keys.Count > options.Value.Capacity)
                {
                    if (_keyQueue.TryDequeue(out var removedKey))
                        _keys.Remove(removedKey);
                }
            }
        }
    }
}
