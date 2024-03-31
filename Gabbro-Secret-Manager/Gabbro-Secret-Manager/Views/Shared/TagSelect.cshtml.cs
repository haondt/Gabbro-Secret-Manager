using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class TagSelectModel : IPageModel
    {
        public Dictionary<string, bool> Options { get; set; } = [];
        public bool HasActiveTags() => Options.Any(o => o.Value);
    }
}
