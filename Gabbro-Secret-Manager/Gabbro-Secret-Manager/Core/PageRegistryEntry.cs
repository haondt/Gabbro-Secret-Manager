using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistryEntry
    {
        public bool RequiresAuthentication { get; set; } = false;
        public required PageEntryType Type { get; set; }
        public required string Page { get; set; }
        public required string ViewPath { get; set; }
        public required Func<IRequestData, object> ModelFactory { get; set; }

        public PageEntry Create(IRequestData data)
        {
            return new PageEntry
            {
                Type = Type,
                Page = Page,
                ViewPath = ViewPath,
                Model = ModelFactory(data)
            };
        }

        public PageEntry Create(object data)
        {
            return new PageEntry
            {
                Type = Type,
                Page = Page,
                ViewPath = ViewPath,
                Model = data
            };
        }
    }
}
