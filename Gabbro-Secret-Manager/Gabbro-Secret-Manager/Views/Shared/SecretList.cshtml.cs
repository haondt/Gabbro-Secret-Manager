using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;

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
                    .Distinct()
                    .ToDictionary(s => s, _ => false)
            };
        }
    }
}
