using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class ConfirmDeleteSecretListEntryModel(string secretName) : IPageModel
    {
        public string SecretName { get; set; } = secretName;
    }
}
