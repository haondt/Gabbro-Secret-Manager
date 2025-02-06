namespace GabbroSecretManager.Domain.Authentication.Models
{
    public class AuthenticateUserResult
    {
        public required bool Success { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
