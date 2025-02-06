namespace GabbroSecretManager.Domain.Authentication.Models
{
    public class RefreshEncryptionKeyResult
    {
        public bool Success { get; set; }
        public bool FailedToGetUsername { get; set; }
        public bool IncorrectPassword { get; set; }
    }
}
