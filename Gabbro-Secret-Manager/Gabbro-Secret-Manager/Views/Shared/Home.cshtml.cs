namespace Gabbro_Secret_Manager.Views.Shared
{
    public class HomeModel
    {
        public string SearchString { get; set; } = "";
        public bool ShouldReRequestPassword { get; set; } = false;
        public SecretListModel? Secrets { get; set; }
    }
}
