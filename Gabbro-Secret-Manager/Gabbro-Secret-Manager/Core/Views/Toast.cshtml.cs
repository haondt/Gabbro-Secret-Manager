namespace Gabbro_Secret_Manager.Core.Views
{
    public class ToastModel : IPageModel
    {
        public required List<(ToastSeverity Severity, string Message)> Toasts { get; set; }
    }

}
