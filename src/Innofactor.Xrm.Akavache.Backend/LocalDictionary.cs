namespace Innofactor.Xrm.Persistent.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Innofactor.Xrm.Persistent.Collections.Backend;
    using Newtonsoft.Json;
    using Xrm.Json.Serialization;

    public class LocalDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
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
                    new EntityCollectionConverter(),
                    new EntityConverter(),
                    new EntityReferenceConverter(),
                    new MoneyConverter(),
                    new OptionSetConvertor()
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

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var task = cache.GetAllKeys();
                task.Wait();

                // Default to string if no key type was set
                return task.Result.Select(v => (TKey)Convert.ChangeType(v.Key, v.Type ?? typeof(string))).ToList();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var task = cache.GetAll();
                task.Wait();

                return task.Result.Select(v => JsonConvert.DeserializeObject<TValue>(Encoding.UTF8.GetString(v))).ToList();
            }
        }

        #endregion Public Properties

        #region Public Indexers

        public TValue this[TKey key]
        {
            get
            {
                var task = cache.Get(key.ToString());
                task.Wait();

                return JsonConvert.DeserializeObject<TValue>(Encoding.UTF8.GetString(task.Result));
            }
            set
            {
                var task = cache.Insert(key.ToString(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
                task.Wait();
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(TKey key, TValue value) =>
            cache.Insert(key.ToString(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value))).Wait();

        public void Add(KeyValuePair<TKey, TValue> item) =>
            Add(item.Key, item.Value);

        public void Clear()
        {
            cache.InvalidateAll().Wait();
            cache.Vacuum().Wait();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var task = cache.Get(item.Key.ToString());
            task.Wait();

            // TODO: Maybe compare as strings instead?
            // TODO: Calculate MD5 for both values and compare those?
            return task.Result.SequenceEqual(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item.Value)));
        }

        public bool ContainsKey(TKey key)
        {
            var task = cache.Get(key.ToString());
            task.Wait();

            return task.Result.Length > 0;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var source = new List<KeyValuePair<TKey, TValue>>();

            foreach (var item in this)
            {
                source.Add(item);
            }

            source.ToArray().CopyTo(array, arrayIndex);
        }

        public void Dispose() => cache.Dispose();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var keys = Keys;

            foreach (var key in keys)
            {
                // This piece should not be modernized for sake of CI server
#pragma warning disable IDE0018 // Inline variable declaration
                var value = default(TValue);
#pragma warning restore IDE0018 // Inline variable declaration

                TryGetValue(key, out value);

                yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(TKey key)
        {
            try
            {
                var task = cache.InvalidateObject(key.ToString());
                task.Wait();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

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