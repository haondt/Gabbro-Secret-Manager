
using Gabbro_Secret_Manager.Core.DynamicForm;

namespace Gabbro_Secret_Manager.Core.Views
{
    public class DynamicFormModel : IPageModel
    {
        public string? Title { get; set; }
        public List<DynamicFormItem> Items { get; set; } = [];
        public List<DynamicFormButton> Buttons { get; set; } = [new DynamicFormButton { Type = DynamicFormButtonType.Submit }];
        public string? HxVals { get; set; }
        public string? HxTarget { get; set; } = "closest .dynamic-form";
        public string? HxSwap { get; set; } = "outerHTML";
        public string? HxPost { get; set; }
        public string? HxGet { get; set; }
        public string Style { get; set; } = "width: 80%";
    }
}
