﻿namespace Innofactor.Xrm.Persistent.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Backend;
    using Newtonsoft.Json;
    using Xrm.Json.Serialization;

    public class LocalDictionary<T> : IDictionary<string, T>, IDisposable
    {
        #region Private Fields

        private PersistentBlobCache cache;

        #endregion Private Fields

        #region Public Constructors

        public LocalDictionary(string databasePath)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>()
                {
                    new DateTimeConverter(),
                    new EntityCollectionConverter(),
                    new EntityConverter(),
                    new EntityReferenceConverter(),
                    new GuidConverter(),
                    new MoneyConverter(),
                    new OptionSetConverter()
                }
            };

            cache = new PersistentBlobCache(databasePath);
        }

        #endregion Public Constructors

        #region Public Properties

        public int Count
        {
            get
            {
                var task = cache.GetAllKeys();
                task.Wait();

                return task.Result.Count();
            }
        }

        public bool IsReadOnly =>
            throw new NotImplementedException();

        public ICollection<string> Keys
        {
            get
            {
                var task = cache.GetAllKeys();
                task.Wait();

                return task.Result.Select(v => v.Key).ToList();
            }
        }

        public ICollection<T> Values
        {
            get
            {
                var task = cache.GetAll();
                task.Wait();

                return task.Result.Select(v => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(v))).ToList();
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public T this[string key]
        {
            get
            {
                var task = cache.Get(key);
                task.Wait();

                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(task.Result));
            }
            set
            {
                var task = cache.Insert(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
                task.Wait();
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(string key, T value) =>
            cache.Insert(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value))).Wait();

        public void Add(KeyValuePair<string, T> item) =>
            Add(item.Key, item.Value);

        public void Clear()
        {
            cache.InvalidateAll().Wait();
            cache.Vacuum().Wait();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            var task = cache.Get(item.Key.ToString());
            task.Wait();

            // TODO: Maybe compare as strings instead?
            // TODO: Calculate MD5 for both values and compare those?
            return task.Result.SequenceEqual(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item.Value)));
        }

        public bool ContainsKey(string key)
        {
            var task = cache.Get(key);
            task.Wait();

            return task.Result.Length > 0;
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            var source = new List<KeyValuePair<string, T>>();

            foreach (var item in this)
            {
                source.Add(item);
            }

            source.ToArray().CopyTo(array, arrayIndex);
        }

        public void Dispose() =>
            cache.Dispose();

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            var keys = Keys;

            foreach (var key in keys)
            {
                var value = default(T);

                if (TryGetValue(key, out var found))
                {
                    value = found;
                }

                yield return new KeyValuePair<string, T>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public bool Remove(string key)
        {
            try
            {
                var task = cache.InvalidateObject(key);
                task.Wait();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, T> item) =>
            Remove(item.Key);

        public bool TryGetValue(string key, out T value)
        {
            value = default(T);

            try
            {
                value = this[key];
                return value != null;
            }
            catch (Backend.KeyNotFoundException)
            {
                // Will this ever happen?
                // PersistentBlobCache.GetOrDefault returns empty byte array if the key wasn't found
                return false;
            }
        }

        #endregion Public Methods
    }
}