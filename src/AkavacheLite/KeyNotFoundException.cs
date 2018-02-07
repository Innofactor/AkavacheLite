using System;

namespace AkavacheLite
{
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException()
            : base()
        { }

        public KeyNotFoundException(string key)
            : base(GetKeyMessage(key))
        { }

        static string GetKeyMessage(string key) =>
            $"The key `{key}` was not found or has already expired.";
    }
}
