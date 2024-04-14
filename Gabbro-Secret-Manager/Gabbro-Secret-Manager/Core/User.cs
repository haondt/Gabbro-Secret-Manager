using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public class User
    {
        public static StorageKey<User> GetStorageKey(string username) => StorageKey<User>.Create(username.ToLower().Trim());
        public required string Username { get; set; } 
        public required string PasswordHash { get; set; } 
        public required string PasswordSalt { get; set; }
    }
}
