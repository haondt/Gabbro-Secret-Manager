using Gabbro_Secret_Manager.Core.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public class SessionService : ISessionService
    {
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private Lazy<Task<UserSession?>> _userSessionLazy;
        public async Task<bool> IsAuthenticatedAsync() => await _userSessionLazy.Value != null;
        public async Task<StorageKey<User>> GetUserKeyAsync() => (await _userSessionLazy.Value)?.Owner ?? throw new InvalidOperationException(nameof(GetUserKeyAsync));

        private Lazy<string?> _sessionTokenLazy;
        public string? SessionToken => _forcedSessionToken ?? _sessionTokenLazy.Value;
        private string? _forcedSessionToken;

        [MemberNotNull(nameof(_sessionTokenLazy))]
        [MemberNotNull(nameof(_userSessionLazy))]
        public void Reset(string? sessionToken = null)
        {
            _forcedSessionToken = sessionToken;
            _sessionTokenLazy = new(() =>
            {
                var cookies = _httpContextAccessor.HttpContext?.Request?.Cookies;
                if (cookies == null)
                    return null;
                if (!cookies.TryGetValue(AuthenticationExtensions.SESSION_TOKEN_COOKIE_KEY, out var sessionToken))
                    return null;
                if (string.IsNullOrEmpty(sessionToken))
                    return null;
                return sessionToken;
            });

            _userSessionLazy = new(async () =>
            {
                if (string.IsNullOrEmpty(SessionToken))
                    return null;

                var (success, session) = await _userService.TryGetSession(SessionToken);
                if (!success)
                    return null;
                return session;
            });

        }

        public SessionService(UserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            Reset();
        }
    }
}
