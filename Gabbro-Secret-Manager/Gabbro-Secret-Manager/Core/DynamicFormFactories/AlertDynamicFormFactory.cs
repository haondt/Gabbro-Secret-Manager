using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Core.DynamicFormFactories
{
    public class AlertDynamicFormFactory(
        string title, 
        string text,
        string? hxGet = null) : IDynamicFormFactory
    {
        public DynamicFormModel Create()
        {
            var model = new DynamicFormModel
            {
                Title = title,
                HxGet = hxGet,
                Items =
                [
                    new DynamicFormText
                    {
                        Value = text
                    }
                ],
            };

            if (string.IsNullOrEmpty(hxGet))
            {
                model.Buttons =
                [
                    new DynamicFormButton
                    {
                           Text = "ok",
                           Type = DynamicFormButtonType.Button,
                           HyperTrigger = "closeModal"
                    }
                ];
            }
            else
            {
                model.HxGet = hxGet;
                model.Buttons =
                [
                    new DynamicFormButton
                    {
                           Text = "ok",
                           Type = DynamicFormButtonType.Submit
                    }
                ];
            }

            return model;
        }
    }
}
