namespace Gabbro_Secret_Manager.Views.Shared
{
    public class HomeModel
    {
        public List<HomeSecret> Secrets { get; set; } = [];
        public TagSelectModel CreateTagSelectModel()
        {
            return new TagSelectModel
            {
                Options = Secrets
                .SelectMany(s => s.Tags)
                .ToDictionary(s => s, _ => true)
            };
        }
    }

    public class HomeSecret
    {
        public required  string Name { get; set; }
        public required string Value { get; set; }
        public required HashSet<string> Tags { get; set; } = [];
    }
}
