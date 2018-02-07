using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkavacheLite
{
    public static class Extensions
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
