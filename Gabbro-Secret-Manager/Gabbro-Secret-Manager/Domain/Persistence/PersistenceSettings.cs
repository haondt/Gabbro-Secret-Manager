namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class PersistenceSettings : Core.Persistence.PersistenceSettings
    {
        public GabbroPersistenceDrivers Driver { get; set; } = GabbroPersistenceDrivers.Memory;
    }

    public enum GabbroPersistenceDrivers
    {
        Memory,
        File,
        MongoDb,
        Postgres
    }
}
