namespace Gabbro_Secret_Manager.Core.Views
{
    public class NavigationBarModel : IPageModel
    {
        public static string CurrentViewKey = "currentView";
        public List<(string Name, bool IsCurrent)> Pages { get; set; } = [];
        public List<(string Name, string Location)> Actions { get; set; } = [];
    }
}
