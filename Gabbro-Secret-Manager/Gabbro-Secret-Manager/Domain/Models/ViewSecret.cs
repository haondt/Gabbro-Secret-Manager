namespace Gabbro_Secret_Manager.Domain.Models
{
    public class ViewSecret
    {
        public required  string Name { get; set; }
        public required string Value { get; set; }
        public string Comments { get; set; } = "";
        public required HashSet<string> Tags { get; set; } = [];
    }
}
