using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public class User
    {
        public static StorageKey GetStorageKey(string username) => username.ToLower().Trim().GetStorageKey<User>();
        public required string Username { get; set; } 
        public required string PasswordHash { get; set; } 
        public required string PasswordSalt { get; set; }
    }
}
