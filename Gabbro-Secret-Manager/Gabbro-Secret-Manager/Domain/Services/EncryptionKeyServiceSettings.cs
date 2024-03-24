using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class EncryptionKeyServiceSettings
    {
        public int Capacity { get; set; } = 1000;
        public int DefaultEncryptionKeyIterations { get; set; } = 10000;
    }
}