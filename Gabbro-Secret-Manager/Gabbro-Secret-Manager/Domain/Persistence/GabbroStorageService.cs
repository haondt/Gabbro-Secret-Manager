﻿using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class GabbroStorageService(IGabbroStorage inner, IMemoryCache cache, IOptions<PersistenceSettings> persistenceOptions) : StorageService(inner, cache, persistenceOptions), IGabbroStorageService
    {
        public Task<List<Secret>> GetSecrets(string userKey) => inner.GetSecrets(userKey);
    }
}