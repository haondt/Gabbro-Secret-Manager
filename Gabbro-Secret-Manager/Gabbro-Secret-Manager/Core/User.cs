namespace Gabbro_Secret_Manager.Core
{
    public class User
    {
        public required string Username { get; set; } 
        public required string PasswordHash { get; set; } 
        public required string PasswordSalt { get; set; }
    }
}
