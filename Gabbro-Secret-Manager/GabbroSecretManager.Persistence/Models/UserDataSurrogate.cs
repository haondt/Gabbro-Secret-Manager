using GabbroSecretManager.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace GabbroSecretManager.Persistence.Models
{
    public class UserDataSurrogate : IdentityUser
    {
        [PersonalData]
        public required int EncryptionKeySettingsIterations { get; set; }
        [PersonalData]
        public required string EncryptionKeySettingsSalt { get; set; }

        public static UserDataSurrogate FromUserData(UserData userData)
        {
            return new()
            {
                UserName = userData.Username,
                NormalizedUserName = userData.NormalizedUsername,
                EncryptionKeySettingsIterations = userData.EncryptionKeySettings.Iterations,
                EncryptionKeySettingsSalt = userData.EncryptionKeySettings.Salt,
            };

        }

        public UserData ToUserData()
        {
            return new()
            {
                NormalizedUsername = NormalizedUserName
                    ?? throw new InvalidOperationException($"User {Id} has a null normalized username."),
                Username = UserName
                    ?? throw new InvalidOperationException($"User {Id} has a null username."),
                EncryptionKeySettings = new()
                {
                    Iterations = EncryptionKeySettingsIterations,
                    Salt = EncryptionKeySettingsSalt
                }
            };
        }
    }
}
