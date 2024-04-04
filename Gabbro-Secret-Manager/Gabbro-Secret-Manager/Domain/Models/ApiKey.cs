namespace Gabbro_Secret_Manager.Domain.Models
{
    public class ApiKey
    {
        public required string Owner { get; set; }
        public required string Name { get; set; }
    }
}
