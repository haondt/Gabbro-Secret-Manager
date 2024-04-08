using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Domain.DynamicFormFactories
{
    public class RefreshEncryptionKeyDynamicFormFactory(string? error = null) : IDynamicFormFactory
    {
        public DynamicFormModel Create() => new DynamicFormModel
        {
            Title = "",
            HxPost = "actions/refresh-encryption-key",
            Items =
                [
                    new DynamicFormText
                    {
                        Value = "please re-enter password to continue"
                    },
                    new DynamicFormInput
                    {
                        Name = "password",
                        Type = DynamicFormInputType.Password,
                        Autocomplete = "current-password",
                        Error = error
                    }
                ],
            Buttons =
                [
                    new DynamicFormButton
                    {
                           Text = "submit",
                           Type = DynamicFormButtonType.Submit
                    },
                    new DynamicFormButton
                    {
                           Text = "log out",
                           Type = DynamicFormButtonType.Button,
                           HxGet = "account/logout"
                    }
                ],

        };
    }
}
