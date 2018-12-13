namespace Akavache.Collections.Implementations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Akavache.Collections.Implementations;

    public class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Private Fields

        private PersistentBlobCache cache;

        #endregion Private Fields

        #region Public Constructors

        public PersistentDictionary(string databasePath)
        {
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
                var task = cache.GetObject<TValue>(key.ToString());
                task.Wait();

                return task.Result;
            }
            set
            {
                var task = cache.InsertObject(key.ToString(), value);
                task.Wait();
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(TKey key, TValue value) => 
            cache.InsertObject(key.ToString(), value).Wait();

        public void Add(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public bool ContainsKey(TKey key) => throw new NotImplementedException();

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public bool Remove(TKey key) => throw new NotImplementedException();

        public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        public bool TryGetValue(TKey key, out TValue value) => throw new NotImplementedException();

        #endregion Public Methods
    }
}