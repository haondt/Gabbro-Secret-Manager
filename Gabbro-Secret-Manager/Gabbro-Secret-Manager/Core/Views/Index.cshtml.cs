namespace Gabbro_Secret_Manager.Core.Views
{
    public class IndexModel 
    {
        public required string Title { get; set; }
        public required PageEntry NavigationBar { get; set; }
        public required PageEntry Content { get; set; }
    }
}
