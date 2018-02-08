using System;
using System.IO;

namespace AkavacheLite.Platforms
{
    public class WindowsStorageProvider : IStorageProvider
    {
        readonly string _userData;
        readonly string _tempData;

        public WindowsStorageProvider()
        {
            _userData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _tempData = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
        }

        public string GetPersistentCacheDirectory() =>
            _userData;

        public string GetSecretCacheDirectory() =>
            _userData;

        public string GetTemporaryCacheDirectory() =>
            _tempData;
    }
}
