namespace Akavache.Backend.Implementations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
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

        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public ICollection<TKey> Keys => throw new NotImplementedException();
        public ICollection<TValue> Values => throw new NotImplementedException();

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

        public void Clear() => throw new NotImplementedException();

        public bool Contains(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public bool ContainsKey(TKey key) => throw new NotImplementedException();

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public void Dispose() => cache.Dispose();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public bool Remove(TKey key) => throw new NotImplementedException();

        public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

            try
            {
                value = this[key];
                return value != null;
            }
            catch (KeyNotFoundException)
            {
                // Will this ever happen?
                // PersistentBlobCache.GetOrDefault returns empty byte array if the key wasn't found
                return false;
            }
        }

        #endregion Public Methods
    }
}