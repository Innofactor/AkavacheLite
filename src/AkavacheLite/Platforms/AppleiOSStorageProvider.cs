using System;
using System.IO;

namespace AkavacheLite.Platforms
{
    public class AppleiOSStorageProvider : IStorageProvider
    {
        readonly string _baseFolder;

        public AppleiOSStorageProvider()
        {
            _baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string GetPersistentCacheDirectory() =>
            Path.Combine(_baseFolder, "..", "Library");

        public string GetSecretCacheDirectory() =>
            Path.Combine(_baseFolder, "..", "Library");

        public string GetTemporaryCacheDirectory() =>
            Path.Combine(_baseFolder, "..", "Library", "Caches");
    }
}
