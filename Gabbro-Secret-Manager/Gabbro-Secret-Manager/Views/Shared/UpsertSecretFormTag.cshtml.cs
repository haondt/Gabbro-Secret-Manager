using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class UpsertSecretFormTagModel : IPageModel
    {
        public required string Value { get; set; }
    }
}
