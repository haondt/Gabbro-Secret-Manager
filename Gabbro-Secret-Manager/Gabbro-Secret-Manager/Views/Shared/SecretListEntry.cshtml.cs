using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class SecretListEntryModel : IPageModel
    {
        public const string SecretNameKey = "name";
        public required ViewSecret Secret { get; set; }
    }
}
