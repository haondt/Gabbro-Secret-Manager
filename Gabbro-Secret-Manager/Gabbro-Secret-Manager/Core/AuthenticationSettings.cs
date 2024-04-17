namespace Gabbro_Secret_Manager.Core
{
    public class AuthenticationSettings
    {
        public TimeSpan SessionDuration { get; set; } = TimeSpan.FromDays(1);

        public bool UseSecureCookies { get; set; } = true;
    }
}
