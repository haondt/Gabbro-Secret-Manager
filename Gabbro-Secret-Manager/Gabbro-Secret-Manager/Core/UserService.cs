using Gabbro_Secret_Manager.Core.Persistence;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core
{
    public class UserService(IStorageService storage, CryptoService crypto, IOptions<AuthenticationSettings> authenticationOptions)
    {
        private bool ValidateCredentials(string username, string password, out string usernameReason, out string passwordReason)
        {
            usernameReason = "";
            passwordReason = "";
            var isValid = true;
            if (string.IsNullOrEmpty(username) || username.Length < 3)
            {
                usernameReason = "Username must be at least 3 characters";
                isValid = false;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                passwordReason = "Password must be at least 8 characters";
                isValid =  false;
            }

            return isValid;
        }

        public async Task<(bool Success, string UsernameReason, string PasswordReason, User? user, string userKey)> TryRegisterUser(string username, string password)
        {
            if (!ValidateCredentials(username, password, out var usernameReason, out var passwordReason))
                return (false, usernameReason, passwordReason, default, "");

            var userKey = username.ToLower().Trim().GetStorageKey<User>();

            if (await storage.ContainsKey(userKey))
            {
                usernameReason = "Username not available";
                return (false, usernameReason, "", default, "");
            }

            var (salt, hash) = crypto.HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
            };

            await storage.Set(userKey, user);
            return (true, "", "", user, userKey);
        }

        public async Task<(bool Success, string sessionToken, DateTime expiry, string userKey)> TryAuthenticateUser(string username, string password)
        {
            if (!ValidateCredentials(username, password, out _, out _))
                return (false, "", default, "");

            var userKey = username.ToLower().Trim().GetStorageKey<User>();

            var (foundUser, user) = await storage.TryGet<User>(userKey);
            if (!foundUser)
                return (false, "", default, "");

            var foundHash = crypto.HashPassword(password, user!.PasswordSalt);
            if (!foundHash.Equals(user.PasswordHash)) 
                return (false, "", default, "");

            var sessionToken = GenerateSessionToken();
            var sessionKey = sessionToken.GetStorageKey<UserSession>();
            var now = DateTime.UtcNow;
            var sessionExpiry = now + authenticationOptions.Value.SessionDuration;

            await storage.Set(sessionKey, new UserSession
            {
                Expiry = sessionExpiry,
                UserKey = userKey
            });
            await storage.Set(userKey, user);
            return (true, sessionToken, sessionExpiry, userKey);
        }

        private string GenerateSessionToken()
        {
            var paranoia = 2;
            return Enumerable.Range(0, paranoia)
                .Select(_ => Guid.NewGuid().ToString())
                .Aggregate((a, b) => a + b);
        }

        public async Task<UserSession> GetSession(string sessionToken)
        {
            var sessionKey = sessionToken.GetStorageKey<UserSession>();
            var (foundSession, session) = await storage.TryGet<UserSession>(sessionKey);
            if (!foundSession)
                throw new KeyNotFoundException(sessionToken);

            if (session!.Expiry < DateTime.UtcNow)
            {
                try
                {
                    await storage.Delete(sessionKey);
                }
                catch { }
                throw new InvalidOperationException(sessionToken);
            }

            return session;
        }

        public async Task<User> GetUser(string userKey)
        {
            var (foundUser, user) = await storage.TryGet<User>(userKey);
            if (!foundUser)
                throw new KeyNotFoundException(userKey);
            return user!;
        }

        public async Task<(bool Success, UserSession? Session)> TryGetSession(string sessionToken)
        {
            var sessionKey = sessionToken.GetStorageKey<UserSession>();
            var (foundSession, session) = await storage.TryGet<UserSession>(sessionKey);
            if (!foundSession)
                return (false, default);

            if (session!.Expiry < DateTime.UtcNow)
            {
                try
                {
                    await storage.Delete(sessionKey);
                }
                catch { }
                return (false, default);
            }

            return (true, session!);
        }

        public Task EndSession(string sessionToken)
        {
            var sessionKey = sessionToken.GetStorageKey<UserSession>();
            return storage.Delete(sessionKey);
        }
    }
}
