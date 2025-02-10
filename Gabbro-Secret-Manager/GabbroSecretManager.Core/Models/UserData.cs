namespace GabbroSecretManager.Core.Models
{
    public class UserData
    {
        public required string Username { get; set; }
        public required string NormalizedUsername { get; set; }
        public EncryptionKeySettings EncryptionKeySettings { get; set; }
    }
}
