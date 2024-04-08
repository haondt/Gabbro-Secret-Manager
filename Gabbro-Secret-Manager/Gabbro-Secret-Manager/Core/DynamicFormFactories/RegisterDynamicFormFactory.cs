using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Core.DynamicFormFactories
{
    public class RegisterDynamicFormFactory(
        string username,
        string? usernameError = null,
        string? passwordError = null) : IDynamicFormFactory
    {
        public DynamicFormModel Create()
        {
            return new DynamicFormModel
            {
                Title = "log in",
                HxPost = "account/login",
                Style = "width: 80%; max-width: 250px",
                Items =
                [
                    new DynamicFormInput
                    {
                        Name = "username",
                        Type = DynamicFormInputType.Text,
                        Autocomplete = "username",
                        Value = username,
                        Label = "username",
                        Error = usernameError
                    },
                    new DynamicFormInput
                    {
                        Name = "password",
                        Type = DynamicFormInputType.Password,
                        Autocomplete = "current-password",
                        Label = "password",
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
                           Text = "use existing account",
                           Type = DynamicFormButtonType.Button,
                           HxGet = "partials/login"
                    }
                ],

            };
        }
    }
}
