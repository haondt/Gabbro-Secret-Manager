using Gabbro_Secret_Manager.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    internal class DataObject
    {
        public Dictionary<string, object?> Values = [];
    }

    public class FileGabbroStorage : IGabbroStorage
    {
        public string datafile = "./data.json";
        private readonly JsonSerializerSettings _serializerSettings;
        private DataObject? _dataCache;
        private readonly SemaphoreSlim _semaphoreSlim = new (1, 1);

        public FileGabbroStorage()
        {
            _serializerSettings = new JsonSerializerSettings();
            ConfigureSerializerSettings(_serializerSettings);
        }

        private static JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error;
            settings.Formatting = Newtonsoft.Json.Formatting.Indented;
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return settings;
        }

        private async Task TryAcquireSemaphoreAnd(Func<Task> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { await func(); }
            finally { _semaphoreSlim.Release(); }
        }
        private async Task<T> TryAcquireSemaphoreAnd<T>(Func<Task<T>> func)
        {
            if (!await _semaphoreSlim.WaitAsync(1000))
                throw new Exception("Unable to acquire semaphore within the time limit");
            try { return await func(); }
            finally { _semaphoreSlim.Release(); }
        }



        private async Task<DataObject> GetDataAsync()
        {
            if (_dataCache != null)
                return _dataCache;

            if (!File.Exists(datafile))
            {
                _dataCache = new DataObject();
                return _dataCache;
            }

            using var reader = new StreamReader(datafile, new FileStreamOptions
            {
                Access = FileAccess.Read,
                BufferSize = 4096, 
                Mode = FileMode.Open,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = await reader.ReadToEndAsync();

            _dataCache = JsonConvert.DeserializeObject<DataObject>(text, _serializerSettings) ?? new DataObject();
            return _dataCache;
        }

        private async Task SetDataAsync(DataObject data)
        {
            using var writer = new StreamWriter(datafile, new FileStreamOptions
            {
                Access = FileAccess.Write,
                BufferSize = 4096, 
                Mode = FileMode.Create,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

            var text = JsonConvert.SerializeObject(data, _serializerSettings);
            await writer.WriteAsync(text);
            _dataCache = data;
        }

        public Task<bool> ContainsKey(string key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return data.Values.ContainsKey(key);
            });

        public Task Delete(string key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                if (!data.Values.Remove(key))
                    return;
                await SetDataAsync(data);
            });

        public Task<T> Get<T>(string key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return (T)data.Values[key]!;
            });

        public Task<List<Secret>> GetSecrets(string userKey) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                return data.Values
                .Select(kvp => kvp.Value)
                .Where(v => v != null && v is Secret)
                .Cast<Secret>()
                .Where(s => s.Owner.Equals(userKey))
                .ToList();
            });

        public Task Set<T>(string key, T value) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                data.Values[key] = value;
                await SetDataAsync(data);
            });

        public Task<(bool Success, T? Value)> TryGet<T>(string key) =>
            TryAcquireSemaphoreAnd(async () =>
            {
                var data = await GetDataAsync();
                if (data.Values.TryGetValue(key, out var value) && value is T)
                    return (true, (T?)value);
                return (false, default);
            });
    }
}
