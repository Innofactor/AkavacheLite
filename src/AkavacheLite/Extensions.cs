using System;

namespace AkavacheLite
{
    public static class Extensions
    {
        public static string GenerateKey(this IBlobCache cache) =>
            Guid.NewGuid().ToString("N");
    }
}
