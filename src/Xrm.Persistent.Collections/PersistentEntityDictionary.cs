namespace Akavache.Backend.Implementations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class PersistentEntityDictionary<TKey, Entity> : IDictionary<TKey, Entity>
    {
        #region Private Fields

        private PersistentBlobCache cache;

        #endregion Private Fields

        #region Public Constructors

        public PersistentEntityDictionary(string databasePath)
        {
            cache = new PersistentBlobCache(databasePath);
        }

        #endregion Public Constructors

        #region Public Properties

        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public ICollection<TKey> Keys => throw new NotImplementedException();
        public ICollection<Entity> Values => throw new NotImplementedException();

        #endregion Public Properties

        #region Public Indexers

        public Entity this[TKey key]
        {
            get
            {
                var task = cache.GetObject<Entity>(key.ToString());
                task.Wait();

                return task.Result;
            }
            set
            {
                var task = cache.Insert(key.ToString(), value);
                task.Wait();
            }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(TKey key, Entity value) =>
            cache.Insert(key.ToString(), value).Wait();

        public void Add(KeyValuePair<TKey, Entity> item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(KeyValuePair<TKey, Entity> item) => throw new NotImplementedException();

        public bool ContainsKey(TKey key) => throw new NotImplementedException();

        public void CopyTo(KeyValuePair<TKey, Entity>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, Entity>> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public bool Remove(TKey key) => throw new NotImplementedException();

        public bool Remove(KeyValuePair<TKey, Entity> item) => throw new NotImplementedException();

        public bool TryGetValue(TKey key, out Entity value) => throw new NotImplementedException();

        #endregion Public Methods
    }
}