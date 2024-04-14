using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class MongoDbGabbroStorage : IGabbroStorage
    {
        private readonly IMongoDatabase _database;
        private readonly HashSet<string> _accessedCollections = [];

        public MongoDbGabbroStorage(IOptions<MongoDbSettings> options, IMongoClient mongoClient)
        {
            var settings = options.Value;
            _database = mongoClient.GetDatabase(settings.DatabaseName);
        }

        private Task InitializeIndex<T>(IMongoCollection<T> collection, string name)
        {
            if (_accessedCollections.Contains(name))
                return Task.CompletedTask;

            var keys = Builders<T>.IndexKeys.Ascending("$**");

            _accessedCollections.Add(name);
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keys));
        }

        private async Task<IMongoCollection<MongoDbGabbroDocument<T>>> GetCollection<T>(StorageKey<T> key)
        {
            var name = key.Type.FullName ?? key.Type.Name;
            var collection = _database.GetCollection<MongoDbGabbroDocument<T>>(name);
            await InitializeIndex(collection, name);
            return collection;
        }

        private async Task<IMongoCollection<MongoDbGabbroDocument<dynamic>>> GetCollection(StorageKey key)
        {
            var name = key.Type.FullName ?? key.Type.Name;
            var collection = _database.GetCollection<MongoDbGabbroDocument<dynamic>>(name);
            await InitializeIndex(collection, name);
            return collection;
        }

        private async Task<IMongoCollection<MongoDbGabbroDocument<T>>> GetCollection<T>()
        {
            var name = typeof(T).FullName ?? typeof(T).Name;
            var collection = _database.GetCollection<MongoDbGabbroDocument<T>>(name);
            await InitializeIndex(collection, name);
            return collection;
        }

        public async Task<bool> ContainsKey(StorageKey key)
        {
            var collection = await GetCollection(key);
            return await collection.AsQueryable()
                .Where(x => x.Key == key)
                .AnyAsync();
        }

        public async Task Delete(StorageKey key)
        {
            var collection = await GetCollection(key);
            await collection.DeleteOneAsync(d => d.Key == key);
        }

        public async Task<T> Get<T>(StorageKey<T> key)
        {
            var collection = await GetCollection(key);
            var result = await collection.AsQueryable()
                .Where(d => d.Key == key).
                SingleAsync();
            return result.Value ?? throw new KeyNotFoundException(key.ToString());
        }

        public async Task<Dictionary<StorageKey<ApiKey>, ApiKey>> GetApiKeys(StorageKey<User> userKey)
        {
            var collection = await GetCollection<ApiKey>();
            var  apiKeys = await collection.AsQueryable()
                .Where(s => s.Value.Owner == userKey)
                .ToListAsync();
            return apiKeys.ToDictionary(d => d.Key.As<ApiKey>(), d => d.Value);
        }

        public async Task<List<Secret>> GetSecrets(StorageKey<User> userKey)
        {
            var collection = await GetCollection<Secret>();
            return await collection.AsQueryable()
                .Where(s => s.Value.Owner == userKey)
                .Select(d => d.Value)
                .ToListAsync();
        }

        public async Task Set<T>(StorageKey<T> key, T value)
        {
            var collection = await GetCollection(key);
            await collection.FindOneAndReplaceAsync<MongoDbGabbroDocument<T>>(d => d.Key == key, new MongoDbGabbroDocument<T>
            {
                Key = key,
                Value = value
            }, new FindOneAndReplaceOptions<MongoDbGabbroDocument<T>> { IsUpsert = true });
        }

        public async Task<(bool Success, T? Value)> TryGet<T>(StorageKey<T> key)
        {
            var collection = await GetCollection(key);
            var result = await collection.AsQueryable()
                .SingleOrDefaultAsync(d => d.Key == key);
            if (result != null)
                return (true, result.Value);
            return (false, default);
        }
    }
}
