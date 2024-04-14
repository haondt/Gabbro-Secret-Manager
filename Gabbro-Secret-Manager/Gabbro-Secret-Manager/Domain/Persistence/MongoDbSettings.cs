namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public string DatabaseName { get; set; } = "gsm";
    }
}
