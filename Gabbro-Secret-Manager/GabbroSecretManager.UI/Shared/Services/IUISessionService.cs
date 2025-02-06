using Haondt.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Shared.Services
{
    public interface IUISessionService
    {
        Task<Result<(string NormalizedUsername, byte[] EncryptionKey), IResult>> GetUserSessionDataAsync(ControllerBase controller);
    }
}
