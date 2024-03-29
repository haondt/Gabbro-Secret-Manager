using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class SecretListModel : IPageModel
    {
        public List<ViewSecret> Values { get; set; } = [];
        public TagSelectModel CreateTagSelectModel()
        {
            return new TagSelectModel
            {
                Options = Values
                    .SelectMany(s => s.Tags)
                    .ToDictionary(s => s, _ => true)
            };
        }
    }

    public class ViewSecret
    {
        public required  string Name { get; set; }
        public required string Value { get; set; }
        public required HashSet<string> Tags { get; set; } = [];
    }
}
