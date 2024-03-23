namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class PersistenceSettings
    {
        public bool UseReadCaching { get; set; } = true;
        public TimeSpan CacheLifetime { get; set; } = TimeSpan.FromHours(1);
    }
}
