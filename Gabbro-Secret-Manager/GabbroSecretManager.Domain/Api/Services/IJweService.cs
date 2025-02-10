namespace GabbroSecretManager.Domain.Api.Services
{
    public interface IJweService
    {
        string CreateToken(Dictionary<string, string> claims);
        Task<IReadOnlyDictionary<string, string>> GetClaims(string token);
        Task<bool> IsValid(string token);
    }
}
