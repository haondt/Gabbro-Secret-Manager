using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class UserDataService(IGabbroStorageService storageService, IOptions<EncryptionKeyServiceSettings> encryptionKeyOptions)
    {
        private const int _saltSize = 16; // 16 bytes for the salt
        public async Task<UserData> InitializeUserData(StorageKey userKey)
        {
            var userDataKey = userKey.Extend<UserData>();
            var userData = new UserData
            {
                Owner = userKey,
                EncryptionKeySettings = new EncryptionKeySettings
                {
                    Iterations = encryptionKeyOptions.Value.DefaultEncryptionKeyIterations,
                    Salt = Convert.ToBase64String(CryptoService.GenerateSalt(_saltSize))
                }
            };
            await storageService.Set(userDataKey, userData);
            return userData;
        }

        public Task<UserData> GetUserData(StorageKey userKey)
        {
            return storageService.Get<UserData>(userKey.Extend<UserData>());
        }
    }
}
