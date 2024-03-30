using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class UpsertSecretFormModel : IPageModel
    {
        public required bool ShouldOverwriteExisting { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public HashSet<string> Tags { get; set; } = [];
        public string Comments { get; set; } = "";
        public string? Error { get; set; }
    }
}
