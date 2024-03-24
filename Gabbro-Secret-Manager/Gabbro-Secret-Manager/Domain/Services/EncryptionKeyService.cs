using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class EncryptionKeyService(IOptions<EncryptionKeyServiceSettings> options, UserService userService, UserDataService userDataService)
    {
        private readonly Dictionary<string, byte[]> _keys = new();
        private readonly Queue<string> _keyQueue = new();
        private readonly object _dictLock = new();

        private async Task<byte[]> GenerateEncryptionKey(string sessionToken, string password)
        {
            var userSession = await userService.GetSession(sessionToken);
            var userData = await userDataService.GetUserData(userSession.UserKey);
            var uHash = CryptoService.GenerateHash(userSession.UserKey);
            var pHash = CryptoService.GenerateHash(password);
            var keyBytes = CryptoService.GenerateHash(
                Convert.ToBase64String(uHash.Concat(pHash).ToArray()),
                Convert.FromBase64String(userData.EncryptionKeySettings.Salt),
                userData.EncryptionKeySettings.Iterations);
            return keyBytes;
        }

        public bool TryGet(string sessionToken, out byte[]? key)
        {
            return _keys.TryGetValue(sessionToken, out key);
        }
        public byte[] Get(string sessionToken) => _keys[sessionToken];

        public async Task<byte[]> GetOrCreateEncryptionKey(string sessionToken, string password)
        {
            if (TryGet(sessionToken, out var existingKey))
                return existingKey!;

            var newKey = await GenerateEncryptionKey(sessionToken, password);
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
