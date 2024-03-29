namespace Gabbro_Secret_Manager.Core
{
    public static class AuthenticationExtensions
    {
        public const string SESSION_TOKEN_COOKIE_KEY = "SessionToken";

        public static string GetAuthentication(this IRequestData request)
        {
            return request.Cookies[SESSION_TOKEN_COOKIE_KEY] ?? throw new InvalidOperationException();
        }

        public static void AddAuthentication(this IResponseCookies cookies, string sessionToken, DateTime sessionExpiry)
        {
            cookies.Append(SESSION_TOKEN_COOKIE_KEY, sessionToken, new CookieOptions
            {
                Expires = sessionExpiry,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
        public static void ExpireAuthentication(this IResponseCookies cookies)
        {
            cookies.Append(SESSION_TOKEN_COOKIE_KEY, "", new CookieOptions
            {
                Expires = DateTime.MinValue,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
