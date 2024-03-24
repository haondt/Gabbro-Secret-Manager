using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class MemoryGabbroStorage : MemoryStorage, IGabbroStorage
    {
        public Task<List<Secret>> GetSecrets(string userKey)
        {
            var secrets = _storage
                .Select(kvp => kvp.Value as Secret)
                .Where(s => s != null && s.Owner.Equals(userKey))
                .Cast<Secret>()
                .ToList();
            return Task.FromResult(secrets);
        }
    }
}
