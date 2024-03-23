using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gabbro_Secret_Manager.Views.Shared
{
    public class RegisterModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? UsernameError { get; set; }
        public string? PasswordError { get; set; }
    }
}
