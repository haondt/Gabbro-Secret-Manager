using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core
{
    public class AuthenticationService(IOptions<AuthenticationSettings> options, IHttpContextAccessor httpContext)
    {
        private readonly IHttpContextAccessor _httpContext = httpContext;
        private const string SESSION_TOKEN_COOKIE_KEY = "SessionToken";

        private CookieOptions CookieOptionsFactory(DateTime? expiry = null) => new()
        {
            Expires = expiry ?? DateTime.UtcNow + options.Value.SessionDuration,
            HttpOnly = true,
            Secure = options.Value.UseSecureCookies,
            SameSite = SameSiteMode.Strict
        };
        public string GetAuthentication()
        {
            return _httpContext.HttpContext?.Request.Cookies[SESSION_TOKEN_COOKIE_KEY] ?? throw new InvalidOperationException();
        }

        public bool TryGetAuthentication(out string? value)
        {
            value = default;
            if (_httpContext.HttpContext == null)
                return false;
            return _httpContext.HttpContext.Request.Cookies.TryGetValue(SESSION_TOKEN_COOKIE_KEY, out value);
        }


        public void AddAuthentication(string sessionToken, DateTime? expiry = null)
        {
            if (_httpContext.HttpContext == null)
                throw new InvalidOperationException();

            _httpContext.HttpContext.Response.Cookies.Append(SESSION_TOKEN_COOKIE_KEY, sessionToken, CookieOptionsFactory(expiry));
        }

        public void ExpireAuthentication()
        {
            if (_httpContext.HttpContext == null)
                throw new InvalidOperationException();

            _httpContext.HttpContext.Response.Cookies.Append(SESSION_TOKEN_COOKIE_KEY, "", CookieOptionsFactory(DateTime.Now));
        }
    }
}
