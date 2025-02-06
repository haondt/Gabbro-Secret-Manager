using GabbroSecretManager.Domain.Cryptography.Models;
using System.Security.Cryptography;
using System.Text;

namespace GabbroSecretManager.Domain.Cryptography.Services
{
    internal class Crypto
    {

        public static (string Salt, string Hash) HashPassword(string password)
        {
            byte[] salt = GenerateSalt(CryptoConstants.PasswordSaltBytes);
            byte[] hash = GenerateHash(password, salt, CryptoConstants.PasswordHashIterations, CryptoConstants.PasswordHashBytes);

            return (
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static string HashPassword(string password, string salt)
        {
            var hash = GenerateHash(password, Convert.FromBase64String(salt), CryptoConstants.PasswordHashIterations, CryptoConstants.PasswordHashBytes);
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

        public static byte[] GenerateHash(string input, byte[] salt, int iterations, int outputLength)
        {

            return Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, HashAlgorithmName.SHA256, outputLength);
        }
        public static byte[] GenerateHash(string input)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(input));
        }

        public static (byte[] EncryptedData, byte[] IV) AesEncrypt(byte[] data, byte[] encryptionKey)
        {
            using Aes aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return (ms.ToArray(), aes.IV);
        }

        public static byte[] AesDecrypt(byte[] data, byte[] encryptionKey, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

    }
}
