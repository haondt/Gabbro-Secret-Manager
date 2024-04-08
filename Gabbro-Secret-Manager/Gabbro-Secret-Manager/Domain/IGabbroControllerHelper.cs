using Gabbro_Secret_Manager.Controllers;
using Gabbro_Secret_Manager.Core;
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Domain
{
    public interface IGabbroControllerHelper : IControllerHelper
    {
        public Task<ViewResult> GetRefreshEncryptionKeyView(BaseController controller, string? error = null);
        public Task<IActionResult> GetRefreshEncryptionKeyModalView(BaseController controller);
        public Task<(bool IsValid, IActionResult? InvalidSessionResponse, byte[] encryptionKey)> VerifyEncryptedSession(BaseController controller);
    }
}
