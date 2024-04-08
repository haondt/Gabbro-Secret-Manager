using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Domain.DynamicFormFactories
{
    public class ApiKeyPasswordConfirmationDynamicFormFactory(string name, string? passwordError = null) : IDynamicFormFactory
    {
        public DynamicFormModel Create() => new()
        {
            HxPost = "actions/create-api-key/complete",
            HxVals = $"{{\"name\":\"{name}\"}}",
            Items =
            [
                new DynamicFormText
                {
                    Value = "please confirm password to continue"
                },
                new DynamicFormInput
                {
                    Name = "password",
                    Type = DynamicFormInputType.Password,
                    Autocomplete = "current-password",
                    Error = passwordError
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
                       Text = "cancel",
                       Type = DynamicFormButtonType.Button,
                       HyperTrigger = "closeModal"
                }
            ],
        };
    }
}
