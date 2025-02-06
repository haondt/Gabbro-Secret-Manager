using GabbroSecretManager.Domain.Authentication.Models;
using GabbroSecretManager.Domain.Cryptography.Models;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Persistence.Models;
using Microsoft.AspNetCore.Identity;

namespace GabbroSecretManager.Domain.Authentication.Services
{
    public class UserService(
        SignInManager<UserDataSurrogate> signInManager,
        UserManager<UserDataSurrogate> userManager,
        ISessionService sessionService,
        IEncryptionKeyCacheService keyCacheService) : IUserService
    {
        public async Task SignOutAsync()
        {
            var normalizedUsername = await sessionService.GetNormalizedUsernameAsync();
            if (normalizedUsername.HasValue)
                keyCacheService.ClearEncryptionKey(normalizedUsername.Value);

            await signInManager.SignOutAsync();
        }

        public async Task<AuthenticateUserResult> TrySignIn(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return new()
                {
                    Success = false,
                    Errors = ["Incorrect username or password."]
                };
            var userData = user.ToUserData();

            var result = await signInManager.PasswordSignInAsync(userData.Username, password, true, false);
            if (!result.Succeeded)
                return new()
                {
                    Success = false,
                    Errors = ["Incorrect username or password."]
                };

            keyCacheService.CreateEncryptionKey(userData.NormalizedUsername, password, userData.EncryptionKeySettings);
            return new() { Success = true };
        }

        public async Task<RefreshEncryptionKeyResult> TryRefreshEncryptionKey(string password)
        {
            var normalizedUsername = await sessionService.GetNormalizedUsernameAsync();
            if (!normalizedUsername.HasValue)
                return new()
                {
                    Success = false,
                    FailedToGetUsername = true,
                };
            var user = await userManager.FindByNameAsync(normalizedUsername.Value);
            if (user == null)
                return new()
                {
                    Success = false,
                    FailedToGetUsername = true,
                };

            var userData = user.ToUserData();
            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
                return new()
                {
                    Success = false,
                    IncorrectPassword = true,
                };

            keyCacheService.CreateEncryptionKey(userData.NormalizedUsername, password, userData.EncryptionKeySettings);
            return new() { Success = true };
        }

        public async Task<RegisterUserResult> TryRegister(string username, string password)
        {

            var result = await userManager.CreateAsync(new()
            {
                UserName = username,
                EncryptionKeySettingsIterations = CryptoConstants.EncryptionKeyGenerationDefaultIterations,
                EncryptionKeySettingsSalt = Convert.ToBase64String(Crypto.GenerateSalt(CryptoConstants.EncryptionKeyGenerationDefaultSaltSize))
            }, password);


            if (result.Succeeded)
                return new RegisterUserResult { Success = true };

            return new RegisterUserResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }
    }
}
