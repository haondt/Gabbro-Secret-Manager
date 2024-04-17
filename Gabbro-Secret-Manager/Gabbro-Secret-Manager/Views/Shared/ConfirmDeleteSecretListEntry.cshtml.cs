using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class ConfirmDeleteSecretListEntryModel(Guid secretId) : IPageModel
    {
        public Guid SecretId { get; set; } = secretId;
    }
}
