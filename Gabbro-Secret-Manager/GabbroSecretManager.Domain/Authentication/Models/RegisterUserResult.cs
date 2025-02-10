namespace GabbroSecretManager.Domain.Authentication.Models
{
    public class RegisterUserResult
    {
        public required bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
