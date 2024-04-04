using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class PasswordReentryModalModel : IPageModel
    {
        public string Text { get; set; } = "";
        public string? Error { get; set; }
    }
}
