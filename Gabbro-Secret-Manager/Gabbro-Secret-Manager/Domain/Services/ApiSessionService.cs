using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Services;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class ApiSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiKeyService _apiKeyService;

        private readonly Lazy<Task<(bool IsValid, StorageKey UserKey, byte[] EncryptionKey)>> _apiKeyLazy;

        public ApiSessionService(
            IHttpContextAccessor httpContextAccessor,
            ApiKeyService apiKeyService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiKeyService = apiKeyService;

            _apiKeyLazy = new(() =>
            {
                var defaultValue = Task.FromResult<(bool, StorageKey, byte[])>((false, StorageKey.Empty, []));

                if (_httpContextAccessor.HttpContext == null)
                    return defaultValue;
                var authorizationHeaders = _httpContextAccessor.HttpContext.Request.Headers.Authorization;
                if (authorizationHeaders.Count != 1
                    || string.IsNullOrEmpty(authorizationHeaders[0])
                    || !authorizationHeaders[0]!.StartsWith("Bearer "))
                    return defaultValue;
                var token = authorizationHeaders[0]!["Bearer ".Length..];
                if (string.IsNullOrEmpty(token))
                    return defaultValue;

                return _apiKeyService.ValidateApiTokenAsync(token);
            });
        }

        public async Task<bool> IsAuthenticatedAsync() => (await _apiKeyLazy.Value).IsValid;
        public async Task<(StorageKey UserKey, byte[] EncryptionKey)> GetUserDataAsync()
        {
            var (isValid, userKey, encryptionKey) = await _apiKeyLazy.Value;
            if (!isValid)
                throw new InvalidOperationException("api key not valid");

            return (userKey, encryptionKey);
        }
    }
}
