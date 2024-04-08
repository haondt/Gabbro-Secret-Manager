using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class SettingsModel : IPageModel
    {
        public List<ViewApiKey> ApiKeys { get; set; } = [];
        public bool ShowNewKeyWarning { get; set; } = false;
        public string? NameError { get; set; }
        public string Name { get; set; } = "";
    }

    public class ViewApiKey
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Value { get; set; }
    }
}
