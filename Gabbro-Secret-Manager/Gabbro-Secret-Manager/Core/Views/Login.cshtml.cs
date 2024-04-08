namespace Gabbro_Secret_Manager.Core.Views
{
    public class LoginModel : IPageModel
    {
        public string Username { get; set; } = "";
        public string? Error { get; set; }
    }
}
