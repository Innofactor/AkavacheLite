using System.Collections.Generic;
using System.Linq;

namespace AkavacheLite
{
    internal static class InternalExtensions
    {
        const int ChunkSize = 950;

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items) =>
            Chunk<T>(items, ChunkSize);

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> items, int size)
        {
            return items
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / size)
                .Select(x => x.Select(v => v.Value));
        }

        public static string GetDatabasePath(this IStorageProvider storageProvider, string applicationName, StorageLocation location)
        {
            string basePath;
            switch (location)
            {
                case StorageLocation.User:
                    basePath = storageProvider.GetPersistentCacheDirectory();
                    break;
                case StorageLocation.Secure:
                    basePath = storageProvider.GetSecretCacheDirectory();
                    break;
                case StorageLocation.Temporary:
                default:
                    basePath = storageProvider.GetTemporaryCacheDirectory();
                    break;
            }
            return System.IO.Path.Combine(basePath, $"{applicationName}.db");
        }

        //public static IEnumerable<List<T>> Chunk2<T>(List<T> items, int size)
        //{
        //    var count = items.Count;
        //    for (int i = 0; i < count; i += size)
        //    {
        //        yield return items.GetRange(i, Math.Min(size, items.Count - i));
        //    }
        //}
    }
}
