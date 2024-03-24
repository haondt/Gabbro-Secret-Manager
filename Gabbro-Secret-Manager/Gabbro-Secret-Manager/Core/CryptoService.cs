using System;
using System.Security.Cryptography;
using System.Text;

namespace Gabbro_Secret_Manager.Core
{
    public class CryptoService
    {
        private const int _saltSize = 16; // 16 bytes for the salt
        private const int _iterations = 10000; // Number of iterations for the PBKDF2 algorithm

        public (string Salt, string Hash) HashPassword(string password)
        {
            byte[] salt = GenerateSalt(_saltSize);
            byte[] hash = GenerateHash(password, salt, _iterations);

            return (
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public string HashPassword(string password, string salt)
        {
            var hash = GenerateHash(password, Convert.FromBase64String(salt), _iterations);
            return Convert.ToBase64String(hash);
        }

        public static byte[] GenerateSalt(int saltSize)
        {
            byte[] salt = new byte[saltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static byte[] GenerateHash(string input, byte[] salt, int iterations)
        {

            return Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, 256);
        }
        public static byte[] GenerateHash(string input)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(input));
        }

        public static (string EncryptedValue, string InitializationVector) Encrypt(string input, byte[] key)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.GenerateIV();

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using var swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(input);
            }

            return (
                Convert.ToBase64String(msEncrypt.ToArray()),
                Convert.ToBase64String(aesAlg.IV));
        }

        public static string Decrypt(string input, byte[] key, string initializationVector)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = Convert.FromBase64String(initializationVector);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using var swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(input);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }


    }
}
