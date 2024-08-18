using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System.Text;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class PostgreSQLGabbroStorage : IGabbroStorage
    {
        private readonly PostgreSQLSettings _settings;
        private bool _setup = false;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly SemaphoreSlim _locker = new(1, 1);

        public PostgreSQLGabbroStorage(IOptions<PostgreSQLSettings> options)
        {
            _settings = options.Value;
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

        private async Task ExecuteAsync(Func<NpgsqlConnection, Task> func)
        {
            await SetupDatabaseAsync();
            await ExecuteInternal(func);
        }
        private async Task<T> ExecuteAsync<T>(Func<NpgsqlConnection, Task<T>> func)
        {
            await SetupDatabaseAsync();
            return await ExecuteInternal(func);
        }

        private async Task ExecuteInternal(Func<NpgsqlConnection, Task> func)
        {
            using var conn = new NpgsqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();
            await func(conn);
        }

        private async Task<T> ExecuteInternal<T>(Func<NpgsqlConnection, Task<T>> func)
        {
            using var conn = new NpgsqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();
            return await func(conn);
        }

        private async Task SetupDatabaseAsync()
        {
            if (!_setup)
            {
                await _locker.WaitAsync();
                try
                {
                    if (!_setup)
                    {

                        await ExecuteInternal(async conn =>
                        {
                            using var cmd = new NpgsqlCommand(@"
                                CREATE TABLE IF NOT EXISTS Entities (
                                    id SERIAL PRIMARY KEY,
                                    storageKey TEXT UNIQUE NOT NULL,
                                    data TEXT NOT NULL
                                );
                            ", conn);
                            await cmd.ExecuteNonQueryAsync();

                            using var secretsCmd = new NpgsqlCommand(@"
                                CREATE TABLE IF NOT EXISTS Secrets (
                                    id SERIAL PRIMARY KEY,
                                    storageKey TEXT UNIQUE NOT NULL,
                                    gabbroId UUID NOT NULL,
                                    tags TEXT[],
                                    name TEXT NOT NULL,
                                    encryptedValue TEXT NOT NULL,
                                    owner TEXT REFERENCES Entities(storageKey),
                                    initializationVector TEXT NOT NULL,
                                    comments TEXT NOT NULL
                                );
                            ", conn);
                            await secretsCmd.ExecuteNonQueryAsync();

                            using var apiKeysCmd = new NpgsqlCommand(@"
                                CREATE TABLE IF NOT EXISTS ApiKeys (
                                    id SERIAL PRIMARY KEY,
                                    storageKey TEXT UNIQUE NOT NULL,
                                    gabbroId UUID NOT NULL,
                                    name TEXT NOT NULL,
                                    created timestamp NOT NULL,
                                    owner TEXT REFERENCES Entities(storageKey)
                                );
                            ", conn);
                            await apiKeysCmd.ExecuteNonQueryAsync();
                        });

                        _setup = true;
                    }
                }
                finally
                {
                    _locker.Release();
                }
            }
        }

        private T DeserializeObject<T>(StorageKey<T> key, object? obj) => DeserializeObject<T>((StorageKey)key, obj);
        private T DeserializeObject<T>(StorageKey key, object? obj)
        {
            if (TryDeserializeObject<T>(obj) is not (true, var result))
                throw new PostgreSQLGabbroException($"failed to deserialize object with storage key {key}");
            return result!;
        }
        private (bool, T?) TryDeserializeObject<T>(object? obj)
        {
            if (obj == null)
                return (false, default);
            var objString = obj.ToString();
            if (objString == null)
                return (false, default);
            var result = JsonConvert.DeserializeObject<T>(objString, _serializerSettings);
            if (result == null)
                return (false, default);
            return (true, result);
        }

        public Task<bool> ContainsKey(StorageKey key)
        {
            var table = key.Type == typeof(Secret) ? "Secrets" : "Entities";
            return ExecuteAsync(async conn =>
            {
                using var cmd = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM {table} WHERE storageKey = @storageKey)", conn);
                cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                var result = await cmd.ExecuteScalarAsync();
                return result != null && (bool)result;
            });
        }

        public Task Delete(StorageKey key)
        {
            var table = key.Type == typeof(Secret) ? "Secrets" : "Entities";
            return ExecuteAsync(async conn =>
            {
                using var cmd = new NpgsqlCommand($"DELETE FROM {table} WHERE storageKey = @storageKey)", conn);
                cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                await cmd.ExecuteNonQueryAsync();
            });
        }

        public async Task<T> Get<T>(StorageKey<T> key)
        {
            if (await TryGet(key) is not (true, var value) || value is null)
                throw new KeyNotFoundException(key.ToString());
            return value;
        }

        public Task<Dictionary<StorageKey<ApiKey>, ApiKey>> GetApiKeys(StorageKey<User> userKey) => ExecuteAsync(async conn =>
        {
            using var cmd = new NpgsqlCommand("SELECT gabbroId, name, created, owner, storageKey FROM ApiKeys WHERE owner = @owner", conn);
            cmd.Parameters.AddWithValue("owner", StorageKeyConvert.Serialize(userKey));

            using var reader = await cmd.ExecuteReaderAsync();
            var apiKeys = new Dictionary<StorageKey<ApiKey>, ApiKey>();
            while (await reader.ReadAsync())
            {
                var storageKey = StorageKeyConvert.Deserialize<ApiKey>(reader.GetString(4));
                apiKeys[storageKey] = new ApiKey
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Created = reader.GetDateTime(2),
                    Owner = StorageKeyConvert.Deserialize<User>(reader.GetString(3))
                };
            }

            return apiKeys;
        });

        public Task<List<Secret>> GetSecrets(StorageKey<User> userKey, string? secretName, IReadOnlyCollection<string>? tags = null) => ExecuteAsync(async conn =>
        {
            var sql = new StringBuilder(@"
                SELECT gabbroId, tags, name, encryptedValue, owner, initializationVector, comments 
                FROM Secrets 
                WHERE owner = @owner
            ");

            var parameters = new List<NpgsqlParameter>
            {
                new ("owner", StorageKeyConvert.Serialize(userKey))
            };

            if (!string.IsNullOrEmpty(secretName))
            {
                sql.Append(" AND name = @secretName");
                parameters.Add(new NpgsqlParameter("secretName", secretName));
            }

            if (tags != null && tags.Count > 0)
            {
                sql.Append($"AND tags && @tags");
                parameters.Add(new NpgsqlParameter("tags", tags.ToArray()));
            }

            using var cmd = new NpgsqlCommand(sql.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            var secrets = new List<Secret>();
            while (await reader.ReadAsync())
            {
                secrets.Add(new Secret
                {
                    Id = reader.GetGuid(0),
                    Tags = reader.GetFieldValue<string[]>(1).ToHashSet(),
                    Name = reader.GetString(2),
                    EncryptedValue = reader.GetString(3),
                    Owner = StorageKeyConvert.Deserialize<User>(reader.GetString(4)),
                    InitializationVector = reader.GetString(5),
                    Comments = reader.GetString(6)
                });
            }

            return secrets;
        });

        public Task Set<T>(StorageKey<T> key, T value)
        {
            if (value is Secret secretValue)
                return ExecuteAsync(async conn =>
                {
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO Secrets (gabbroId, tags, name, encryptedValue, owner, initializationVector, comments, storageKey) 
                        VALUES (@gabbroId, @tags, @name, @encryptedValue, @owner, @initializationVector, @comments @storageKey) 
                        ON CONFLICT (storageKey) DO UPDATE
                        SET data = EXCLUDED.Data, owner = EXCLUDED.owner
                    ", conn);

                    cmd.Parameters.AddWithValue("gabbroId", secretValue.Id);
                    cmd.Parameters.AddWithValue("tags", secretValue.Tags.ToArray());
                    cmd.Parameters.AddWithValue("name", secretValue.Name);
                    cmd.Parameters.AddWithValue("encryptedValue", secretValue.EncryptedValue);
                    cmd.Parameters.AddWithValue("owner", StorageKeyConvert.Serialize(secretValue.Owner));
                    cmd.Parameters.AddWithValue("initializationVector", secretValue.InitializationVector);
                    cmd.Parameters.AddWithValue("comments", secretValue.Comments);
                    cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                    await cmd.ExecuteNonQueryAsync();
                });

            if (value is ApiKey apiKeyvalue)
                return ExecuteAsync(async conn =>
                {
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO Secrets (gabbroId, name, created, owner, storageKey) 
                        VALUES (@gabbroId, @name, @creatd, @owner, @storageKey) 
                        ON CONFLICT (storageKey) DO UPDATE
                        SET data = EXCLUDED.Data, owner = EXCLUDED.owner
                    ", conn);

                    cmd.Parameters.AddWithValue("gabbroId", apiKeyvalue.Id);
                    cmd.Parameters.AddWithValue("name", apiKeyvalue.Name);
                    cmd.Parameters.AddWithValue("owner", StorageKeyConvert.Serialize(apiKeyvalue.Owner));
                    cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                    await cmd.ExecuteNonQueryAsync();
                });

            return ExecuteAsync(async conn =>
            {
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO Entities (storageKey, type, data, owner)
                        VALUES (@gabbroId, @tags, @name, @encryptedValue, @owner, @initializationVector, @comments @storageKey) 
                        ON CONFLICT (storageKey) DO UPDATE
                        SET data = EXCLUDED.Data, owner = EXCLUDED.owner
                    ", conn);

                var result = await cmd.ExecuteScalarAsync();
                return DeserializeObject(key, result);
            });
        }

        public Task<(bool Success, T? Value)> TryGet<T>(StorageKey<T> key)
        {
            if (typeof(T) == typeof(Secret))
                return ExecuteAsync(async conn =>
                {
                    using var cmd = new NpgsqlCommand("SELECT gabbroId, tags, name, encryptedValue, owner, initializationVector, comments FROM Secrets WHERE storageKey = @storageKey", conn);
                    cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return (true, (T?)(object)new Secret
                        {
                            Id = reader.GetGuid(0),
                            Tags = reader.GetFieldValue<string[]>(1).ToHashSet(),
                            Name = reader.GetString(2),
                            EncryptedValue = reader.GetString(3),
                            Owner = StorageKeyConvert.Deserialize<User>(reader.GetString(4)),
                            InitializationVector = reader.GetString(5),
                            Comments = reader.GetString(6)
                        });
                    }
                    return (false, default);
                });

            if (typeof(T) == typeof(ApiKey))
                return ExecuteAsync(async conn =>
                {
                    using var cmd = new NpgsqlCommand("SELECT gabbroId, name, created, owner FROM ApiKeys WHERE storageKey = @storageKey", conn);
                    cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return (true, (T?)(object)new ApiKey
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Created = reader.GetDateTime(2),
                            Owner = StorageKeyConvert.Deserialize<User>(reader.GetString(3))
                        });
                    }
                    return (false, default);
                });


            return ExecuteAsync(async conn =>
            {
                using var cmd = new NpgsqlCommand("SELECT data FROM Entities WHERE storageKey = @storageKey", conn);
                cmd.Parameters.AddWithValue("storageKey", StorageKeyConvert.Serialize(key));

                var result = await cmd.ExecuteScalarAsync();
                return TryDeserializeObject<T>(result);
            });
        }
    }
}

