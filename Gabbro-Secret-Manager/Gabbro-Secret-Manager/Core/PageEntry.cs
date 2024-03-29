namespace Gabbro_Secret_Manager.Core
{
    public class PageEntry
    {
        public string? SetUrl { get; set; }
        public required string Page { get; set; }
        public required string ViewPath { get; set; }
        public required object Model { get; set; }
    }
}