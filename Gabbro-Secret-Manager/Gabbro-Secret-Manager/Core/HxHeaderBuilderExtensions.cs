using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Core
{
    public static class HxHeaderBuilderExtensions
    {
        public static HxHeaderBuilder ConfigureForPage(this HxHeaderBuilder builder, string page)
        {
            return builder.PushUrl(page)
                .ReTarget("#content")
                .ReSwap("innerHTML")
                .TriggerAfterSettle("on_navigate", new Dictionary<string, string> { { NavigationBarModel.CurrentViewKey, page } })
                .TriggerAfterSettle("closeModal", "");
        }
    }
}
