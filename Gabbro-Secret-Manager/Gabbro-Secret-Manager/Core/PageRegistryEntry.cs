using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistryEntry
    {
        public required PageEntryType Type { get; set; }
        public required string Page { get; set; }
        public required string ViewPath { get; set; }
        public required Func<IReadOnlyDictionary<string, object?>, object> ModelFactory { get; set; }

        public PageEntry Create(IReadOnlyDictionary<string, object?> parameters)
        {
            return new PageEntry
            {
                Type = Type,
                Page = Page,
                ViewPath = ViewPath,
                Model = ModelFactory(parameters)
            };
        }
    }
}
