namespace Gabbro_Secret_Manager.Domain.Models
{
    public class DecryptedSecret
    {
        public required HashSet<string> Tags { get; set; }
        public required string Name { get; set; }
        public required string Comments { get; set; }
        public required string Value { get; set; }
    }
}
