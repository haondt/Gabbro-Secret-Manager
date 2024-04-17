using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class UpsertSecretFormModel : IPageModel
    {
        public string Name { get; set; } = "";
        public Guid? Id { get; set; }
        public string Value { get; set; } = "";
        public HashSet<string> Tags { get; set; } = [];
        public List<string> TagSuggestions { get; set; } = [];
        public string Comments { get; set; } = "";
        public string? Error { get; set; }
    }
}
