using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AkavacheLite
{
    public class SQLitePersistentBlobCache : IBlobCache
    {
        readonly string _databasePath;
        readonly SemaphoreSlim _writeSemaphore;
        readonly SQLiteConnection _db;
        readonly JsonSerializer _serializer;
        static readonly Type _cacheItemType = typeof(CacheItem);

        public SQLitePersistentBlobCache(string databasePath)
        {
            _databasePath = databasePath;
            _writeSemaphore = new SemaphoreSlim(1, 1);

            var directory = System.IO.Path.GetDirectoryName(_databasePath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            _db = new SQLiteConnection(_databasePath);
            _db.ExecuteScalar<string>("PRAGMA journal_mode = WAL");
            CreateTable();

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            _serializer = JsonSerializer.Create(serializerSettings);
        }

        public SQLitePersistentBlobCache(IStorageProvider storageProvider, StorageLocation location, string applicationName)
            : this(storageProvider.GetDatabasePath(applicationName, location))
        { }

        void CreateTable()
        {
            var tableSQL = @"
                CREATE TABLE IF NOT EXISTS CacheItem
                (
                    Key TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    CreatedAt INTEGER NOT NULL,
                    Time INTEGER,
                    Item TEXT,
                    PRIMARY KEY (Key, Type)
                ) WITHOUT ROWID;
            ";
            _db.Execute(tableSQL);
            //var indexSQL = @"
            //    CREATE INDEX IF NOT EXISTS TimeIDX on CacheItem (Key, Type, Time)
            //";
            //_db.RunInTransaction(() =>
            //{
            //    _db.Execute(tableSQL);
            //    _db.Execute(indexSQL);
            //});
        }

        // todo: add to interface
        public Task<IEnumerable<KeyResult>> GetAllKeys()
        {
            var query = @"
                select Key, Type from CacheItem 
                where 
                    (Time is null or Time >= ?)
            ";
            return Read(o =>
            {
                return o.Query<KeyQueryResult>(query, DateTime.UtcNow.Ticks)
                    .Select(p => new KeyResult
                    {
                        Key = p.Key,
                        Type = Type.GetType(p.Type)  // todo: test this
                    });
            });
        }

        public async Task<byte[]> Get(string key)
        {
            var item = await GetObject<BinaryItem>(key).ConfigureAwait(false);
            return item?.Data;
        }

        public async Task<IDictionary<string, byte[]>> Get(IEnumerable<string> keys)
        {
            var items = await GetObjects<BinaryItem>(keys).ConfigureAwait(false);
            return items.ToDictionary(o => o.Key, o => o.Value?.Data);
        }

        public Task Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = null) =>
            InsertObject<BinaryItem>(key, new BinaryItem (data), absoluteExpiration);

        public Task Insert(IDictionary<string, byte[]> keyValuePairs, DateTimeOffset? absoluteExpiration = null) =>
            InsertObjects<BinaryItem>(keyValuePairs?.ToDictionary(o => o.Key, o => new BinaryItem(o.Value)) ?? new Dictionary<string, BinaryItem>(), absoluteExpiration);

        public Task Invalidate(string key) =>
            InvalidateObject<BinaryItem>(key);

        public Task Invalidate(IEnumerable<string> keys) =>
            InvalidateObjects<BinaryItem>(keys);

        public Task<DateTimeOffset?> GetCreatedAt(string key) =>
            GetObjectCreatedAt<BinaryItem>(key);

        public Task<IDictionary<string, DateTimeOffset?>> GetCreatedAt(IEnumerable<string> keys) =>
            GetObjectsCreatedAt<BinaryItem>(keys);

        public Task Flush() => Task.CompletedTask;

        public Task<IEnumerable<T>> GetAllObjects<T>()
        {
            var query = @"
                select * from CacheItem 
                where 
                    Type = ?
                    and (Time is null or Time >= ?)
            ";
            return Read(o =>
            {
                return o.Query<CacheItem>(query, typeof(T).FullName, DateTime.UtcNow.Ticks)
                    .Select(p => Deserialize<T>(p.Item));
            });
        }
        
        public async Task<T> GetObject<T>(string key)
        {
            var query = @"
                select * from CacheItem 
                where 
                    Type = ? 
                    and Key = ? 
                    and (Time is null or Time >= ?)
                limit 1
            ";

            var cacheItem = await Read(o => o.Query<CacheItem>(query, typeof(T).FullName, key, DateTime.UtcNow.Ticks).FirstOrDefault()).ConfigureAwait(false);
            if (cacheItem == null)
                throw new KeyNotFoundException(key);
            return Deserialize<T>(cacheItem.Item);
        }

        public Task<T> GetObjectOrDefault<T>(string key)
        {
            var query = @"
                select * from CacheItem 
                where 
                    Type = ? 
                    and Key = ? 
                    and (Time is null or Time >= ?)
                limit 1
            ";

            return Read(o =>
            {
                var cacheItem = o.Query<CacheItem>(query, typeof(T).FullName, key, DateTime.UtcNow.Ticks).FirstOrDefault();
                if (cacheItem == null)
                    return default(T);
                return Deserialize<T>(cacheItem.Item);
            });
        }

        public Task<DateTimeOffset?> GetObjectCreatedAt<T>(string key)
        {
            var query = @"
                select CreatedAt as UtcTicks from CacheItem 
                where 
                    Key = ? 
                    and Type = ?
                    and (Time is null or Time >= ?)
                limit 1
            ";

            return Read(o =>
            {
                var result = o.Query<DateQueryResult>(query, key, typeof(T).FullName, DateTime.UtcNow.Ticks).FirstOrDefault();
                if (result == null)
                    return default(DateTimeOffset?);
                return new DateTimeOffset(result.UtcTicks, TimeSpan.Zero);
            });
        }

        public async Task<IDictionary<string, DateTimeOffset?>> GetObjectsCreatedAt<T>(IEnumerable<string> keys)
        {
            keys = keys.Distinct();

            var utcTicks = DateTime.UtcNow.Ticks;
            var typeName = typeof(T).FullName;

            var tasks = keys
                .Chunk()
                .Select(chunk =>
                {
                    return Read(o =>
                    {
                        var chunkKeys = chunk.ToArray();
                        var keyParameters = string.Join(", ", Enumerable.Repeat("?", chunkKeys.Length));
                        var sql = $@"
                            select Key, CreatedAt as UtcTicks from CacheItem 
                            where 
                                (Time is null or Time >= ?)
                                and Type = ?
                                and Key in ({keyParameters})
                        ";

                        var args = new object[2 + chunkKeys.Length];
                        args[0] = utcTicks;
                        args[1] = typeName;
                        chunkKeys.CopyTo(args, 2);
                        return o.Query<DateQueryResult>(sql, args);
                    });
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            var foundKeys = tasks
                .SelectMany(o => o.Result)
                .ToDictionary(o => o.Key, o => new DateTimeOffset(o.UtcTicks, TimeSpan.Zero));

            var result = new Dictionary<string, DateTimeOffset?>();
            foreach (var key in keys)
            {
                if (foundKeys.TryGetValue(key, out var dateTimeOffset))
                    result.Add(key, dateTimeOffset);
                else
                    result.Add(key, null);
            }
            return result;
        }

        public async Task<IDictionary<string, T>> GetObjects<T>(IEnumerable<string> keys)
        {
            keys = keys.Distinct();

            var utcTicks = DateTime.UtcNow.Ticks;
            var typeName = typeof(T).FullName;

            var tasks = keys
                .Chunk()
                .Select(chunk =>
                {
                    return Read(o =>
                    {
                        var chunkKeys = chunk.ToArray();
                        var keyParameters = string.Join(", ", Enumerable.Repeat("?", chunkKeys.Length));
                        var sql = $@"
                            select * from CacheItem 
                            where 
                                (Time is null or Time >= ?)
                                and Type = ?
                                and Key in ({keyParameters})
                        ";

                        var args = new object[2 + chunkKeys.Length];
                        args[0] = utcTicks;
                        args[1] = typeName;
                        chunkKeys.CopyTo(args, 2);
                        return o.Query<CacheItem>(sql, args)
                            .Select(p => new GetObjectResult<T>
                            {
                                Key = p.Key,
                                Object = Deserialize<T>(p.Item)
                            });
                    });
                });
            await Task.WhenAll(tasks).ConfigureAwait(false);
            
            return tasks
                .SelectMany(o => o.Result)
                .ToDictionary(o => o.Key, o => o.Object);
        }
        
        public async Task InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = null)
        {
            var item = await Task.Run(() =>
            {
                return new CacheItem
                {
                    Key = key,
                    Type = typeof(T).FullName,
                    CreatedAt = DateTime.UtcNow.Ticks,
                    Time = absoluteExpiration?.UtcTicks,
                    Item = Serialize(value)
                };
            }).ConfigureAwait(false);

            await Write(o =>
            {
                o.InsertOrReplace(item, _cacheItemType);
            });
        }
        
        public async Task InsertObjects<T>(IDictionary<string, T> keyValuePairs, DateTimeOffset? absoluteExpiration = null)
        {
            var items = await Task.Run(() =>
            {
                var typeName = typeof(T).FullName;
                var createdTicks = DateTime.UtcNow.Ticks;
                var expiresTicks = absoluteExpiration?.UtcTicks;
                return keyValuePairs
                    .Select(p => new CacheItem
                    {
                        Key = p.Key,
                        Type = typeName,
                        CreatedAt = createdTicks,
                        Time = expiresTicks,
                        Item = Serialize(p.Value)
                    });
            }).ConfigureAwait(false);

            await Write(db =>
            {
                db.RunInTransaction(() =>
                {
                    var chunks = items.Chunk();
                    var tType = typeof(T).FullName;
                    foreach (var chunk in chunks)
                    {
                        var keys = chunk.Select(o => o.Key).ToArray();
                        var keyParameters = string.Join(", ", Enumerable.Repeat("?", keys.Length));
                        var sql = $@"
                            delete from CacheItem 
                            where
                                Type = ?
                                and Key in ({keyParameters})
                        ";

                        var args = new object[1 + keys.Count()];
                        args[0] = tType;
                        keys.CopyTo(args, 1);
                        db.Execute(sql, args);

                        db.InsertAll(chunk, _cacheItemType, runInTransaction: false);
                    }
                });
            });
        }

        public Task InvalidateAll()
        {
            return Write(o => o.Execute("delete from CacheItem"));
        }

        public Task InvalidateAllObjects<T>()
        {
            var sql = @"
                delete from CacheItem
                where Type = ?
            ";
            return Write(o => o.Execute(sql, typeof(T).FullName));
        }

        public Task InvalidateObject<T>(string key)
        {
            var sql = @"
                delete from CacheItem
                where 
                    Type = ?
                    and Key = ?
            ";
            return Write(o => o.Execute(sql, typeof(T).FullName, key));
        }

        public Task InvalidateObjects<T>(IEnumerable<string> keys)
        {
            return Write(db =>
            {
                db.RunInTransaction(() =>
                {
                    var chunks = keys.Chunk();
                    var tType = typeof(T).FullName;
                    foreach (var chunk in chunks)
                    {
                        var chunkKeys = chunk.ToArray();
                        var keyParameters = string.Join(", ", Enumerable.Repeat("?", chunkKeys.Length));
                        var sql = $@"
                            delete from CacheItem 
                            where
                                Type = ?
                                and Key in ({keyParameters})
                        ";

                        var args = new object[1 + chunkKeys.Length];
                        args[0] = tType;
                        chunkKeys.CopyTo(args, 1);
                        db.Execute(sql, args);
                    }
                });
            });
        }

        public Task Vacuum()
        {
            return Write(o =>
            {
                var sqlDeleteExpired = @"
                    delete from CacheItem
                    where Time < ?
                ";
                o.Execute(sqlDeleteExpired, DateTime.UtcNow.Ticks);
                o.Execute("VACUUM;");
            });
        }

        public void Dispose()
        {
            _db?.Close();
            _db?.Dispose();
            _writeSemaphore?.Dispose();
        }

        Task Write(Action<SQLiteConnection> writeOperation)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await _writeSemaphore.WaitAsync();  // todo: safe to configure await here?  ¯\_(ツ)_/¯
                    writeOperation(_db);
                }
                finally
                {
                    _writeSemaphore.Release();
                }
            });
        }

        Task<T> Read<T>(Func<SQLiteConnection, T> readOperation) =>
            Task.Run(() => readOperation(_db));
        
        T Deserialize<T>(string json)
        {
            using (var reader = new System.IO.StringReader(json))
            using (var jsonReader = new JsonTextReader(reader))
                return _serializer.Deserialize<T>(jsonReader);
        }

        string Serialize(object obj)
        {
            using (var sw = new System.IO.StringWriter())
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                _serializer.Serialize(jsonWriter, obj);
                return sw.ToString();
            }
        }
    }

    class CacheItem
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public long CreatedAt { get; set; }
        public long? Time { get; set; }
        public string Item { get; set; }
    }

    class BinaryItem
    {
        public BinaryItem() { }
        public BinaryItem(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; set; }
    }
    
    class KeyQueryResult
    {
        public string Key { get; set; }
        public string Type { get; set; }
    }

    class DateQueryResult
    {
        public string Key { get; set; }
        public long UtcTicks { get; set; }
    }

    class GetObjectResult<T>
    {
        public string Key { get; set; }
        public T Object { get; set; }
    }

    public class KeyResult
    {
        public string Key { get; set; }
        public Type Type { get; set; }
    }
}
