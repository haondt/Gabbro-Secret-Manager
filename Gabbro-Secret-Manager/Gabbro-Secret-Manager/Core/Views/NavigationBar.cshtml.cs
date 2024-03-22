namespace Gabbro_Secret_Manager.Core.Views
{
    public class NavigationBarModel 
    {
        public required string CurrentPage { get; set; }
        public List<string> Pages { get; set; } = new();
    }
}
