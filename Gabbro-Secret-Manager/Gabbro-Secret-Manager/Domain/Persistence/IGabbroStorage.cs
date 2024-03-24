using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public interface IGabbroStorage : IStorage
    {
        public Task<List<Secret>> GetSecrets(string userKey);
    }
}
