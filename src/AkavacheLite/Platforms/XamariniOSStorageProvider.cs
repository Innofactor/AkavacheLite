using System;
using System.IO;

namespace AkavacheLite.Platforms
{
    public class XamariniOSStorageProvider : IStorageProvider
    {
        readonly string _baseFolder;

        public XamariniOSStorageProvider()
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
