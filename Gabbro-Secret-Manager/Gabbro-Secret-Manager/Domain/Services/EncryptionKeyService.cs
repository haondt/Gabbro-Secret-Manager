using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class EncryptionKeyService(IOptions<EncryptionKeyServiceSettings> options)
    {
        private readonly Dictionary<string, byte[]> _keys = new();
        private readonly Queue<string> _keyQueue = new();
        private readonly object _dictLock = new();

        private byte[] GenerateEncryptionKey(string userKey, string password, EncryptionKeySettings encryptionKeySettings)
        {
            var uHash = CryptoService.GenerateHash(userKey);
            var pHash = CryptoService.GenerateHash(password);
            var keyBytes = CryptoService.GenerateHash(
                Convert.ToBase64String(uHash.Concat(pHash).ToArray()),
                Convert.FromBase64String(encryptionKeySettings.Salt),
                encryptionKeySettings.Iterations);
            return keyBytes;
        }

        public bool TryGet(string sessionToken, out byte[]? key)
        {
            return _keys.TryGetValue(sessionToken, out key);
        }
        public byte[] Get(string sessionToken) => _keys[sessionToken];

        public byte[] GetOrCreateEncryptionKey(string sessionToken, string userKey, string password, EncryptionKeySettings encryptionKeySettings)
        {
            if (TryGet(sessionToken, out var existingKey))
                return existingKey!;

            return UpsertEncryptionKey(sessionToken, userKey, password, encryptionKeySettings);
        }

        public byte[] UpsertEncryptionKey(string sessionToken, string userKey, string password, EncryptionKeySettings encryptionKeySettings)
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
