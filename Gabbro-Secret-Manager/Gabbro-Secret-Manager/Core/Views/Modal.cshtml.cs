using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gabbro_Secret_Manager.Core.Views
{
    public class ModalModel : IPageModel
    {
        public required PageEntry Content { get; set; }
        public bool AllowClickOut { get; set; } = true;
    }
}
