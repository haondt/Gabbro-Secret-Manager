namespace Gabbro_Secret_Manager.Views.Shared
{
    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Error { get; set; }
    }
}
