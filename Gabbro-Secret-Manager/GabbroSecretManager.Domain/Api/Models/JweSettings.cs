using Microsoft.Extensions.Options;

namespace GabbroSecretManager.Domain.Api.Models
{
    public class JweSettings
    {
        public string EncryptionKey { get; set; } = "/fVaB6qcSINgq0Gr0iqb2RMZJ5io0fBLRu9ktlYh5DQ=";
        public string SigningKey { get; set; } = "DDz61VQyU1OgXVnA8MW2KpGT4NXKPYNfY+sPeSOZhM8=";
        public string Issuer { get; set; } = "GabbroSecretManager";
        public string Audience { get; set; } = "GabbroSecretManager";

        public static OptionsBuilder<JweSettings> Validate(OptionsBuilder<JweSettings> builder)
        {
            builder.Validate(o => !string.IsNullOrWhiteSpace(o.EncryptionKey), "Encryption key cannot be empty");
            builder.Validate(o =>
            {
                try
                {
                    Convert.FromBase64String(o.EncryptionKey);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }, "Encryption key is not a valid base64 string.");
            builder.Validate(o => Convert.FromBase64String(o.EncryptionKey).Length == 32, "Encryption key must be 32 bytes in length.");
            builder.Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "Signing key cannot be empty");
            builder.Validate(o =>
            {
                try
                {
                    Convert.FromBase64String(o.SigningKey);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }, "Signing key is not a valid base64 string.");
            builder.Validate(o => Convert.FromBase64String(o.SigningKey).Length == 32, "Signing key must be 32 bytes in length.");

            return builder;
        }
    }
}
