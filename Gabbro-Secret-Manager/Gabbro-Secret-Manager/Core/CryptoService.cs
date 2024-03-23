using System;
using System.Security.Cryptography;
using System.Text;

namespace Gabbro_Secret_Manager.Core
{
    public class CryptoService
    {
        private const int SaltSize = 16; // 16 bytes for the salt
        private const int HashSize = 20; // 20 bytes for the hash
        private const int Iterations = 10000; // Number of iterations for the PBKDF2 algorithm

        public (string Salt, string Hash) Hash(string password)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = GenerateHash(password, salt);

            return (
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public string Hash(string password, string salt)
        {
            var hash = GenerateHash(password, Convert.FromBase64String(salt));
            return Convert.ToBase64String(hash);
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static byte[] GenerateHash(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(HashSize);
        }
    }
}
