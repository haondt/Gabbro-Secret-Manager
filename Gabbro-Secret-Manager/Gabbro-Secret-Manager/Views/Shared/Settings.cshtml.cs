using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class SettingsModel : IPageModel
    {
        public List<string> ApiKeys { get; set; } = [];
    }
}
