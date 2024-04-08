namespace Gabbro_Secret_Manager.Core.Views
{
    public class RegisterModel : IPageModel
    {
        public string Username { get; set; } = "";
        public string? UsernameError { get; set; }
        public string? PasswordError { get; set; }
    }
}
