namespace Gabbro_Secret_Manager.Core
{
    public class UserSession
    {
        public required DateTime Expiry { get; set; }
        public required string UserKey { get; set; }
    }
}
