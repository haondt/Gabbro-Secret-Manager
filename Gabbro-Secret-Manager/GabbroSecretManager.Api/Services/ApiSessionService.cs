using GabbroSecretManager.Domain.Api.Services;
using Haondt.Core.Extensions;
using Haondt.Core.Models;

namespace GabbroSecretManager.Api.Services
{
    public class ApiSessionService : IApiSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiKeyService _apiKeyService;
        private readonly Task<Optional<(string NormalizedUsername, byte[] EncryptionKey)>> _apiKeyTask;

        public ApiSessionService(
            IHttpContextAccessor httpContextAccessor,
            IApiKeyService apiKeyService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiKeyService = apiKeyService;

            _apiKeyTask = Task.Run(async () =>
            {

                if (_httpContextAccessor.HttpContext == null)
                    return new();
                var authorizationHeaders = _httpContextAccessor.HttpContext.Request.Headers.Authorization;
                if (authorizationHeaders.Count != 1
                    || string.IsNullOrEmpty(authorizationHeaders[0])
                    || !authorizationHeaders[0]!.StartsWith("Bearer "))
                    return new();
                var token = authorizationHeaders[0]!["Bearer ".Length..];
                if (string.IsNullOrEmpty(token))
                    return new();

                var result = await _apiKeyService.ValidateToken(token);
                return result.As(q => (q.ApiKey.Owner, q.EncryptionKey));
            });
        }

        public async Task<bool> IsAuthenticatedAsync() => (await _apiKeyTask).HasValue;
        public async Task<(string NormalizedUsername, byte[] EncryptionKey)> GetUserDataAsync()
        {
            var result = await _apiKeyTask;
            if (!result.HasValue)
                throw new InvalidOperationException("api key not valid");

            return result.Value;
        }
    }
}
