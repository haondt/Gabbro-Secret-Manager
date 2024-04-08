using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Core.DynamicForm
{
    public interface IDynamicFormFactory
    {
        DynamicFormModel Create();
    }
}
