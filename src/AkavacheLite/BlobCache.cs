using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkavacheLite
{
    // based on the excelent work of Akavache
    public static class BlobCache
    {
        static string _applicationName;
        static IStorageProvider _storageProvider;

        static Lazy<IBlobCache> _localMachine;
        static Lazy<IBlobCache> _userAccount;
        //static Lazy<ISecureBlobCache> _secure;

        static BlobCache()
        {
            _localMachine = new Lazy<IBlobCache>(() => 
                new SQLitePersistentBlobCache(GetDatabasePath(ApplicationName, StorageLocation.Temporary)));
            _userAccount = new Lazy<IBlobCache>(() =>
                new SQLitePersistentBlobCache(GetDatabasePath(ApplicationName, StorageLocation.User)));
            //_secure = new Lazy<ISecureBlobCache>(() =>
            //    new SQLitePersistentBlobCache(GetDatabasePath(ApplicationName, StorageLocation.Secure)));
        }


        /// <summary>
        /// Your application's name. Set this at startup, this defines where
        /// your data will be stored (usually at %AppData%\[ApplicationName])
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_applicationName))
                    throw new Exception($"You must set {nameof(BlobCache)}.{nameof(ApplicationName)} on startup.");
                return _applicationName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _applicationName = value;
                    return;
                }
                var invalidChars = System.IO.Path.GetInvalidFileNameChars();
                if (value.Any(o => invalidChars.Contains(o)))
                    throw new Exception($"{nameof(BlobCache)}.{nameof(ApplicationName)} cannot have any of these characters: " + string.Join(" ", invalidChars));
                _applicationName = value;
            }
        }

        public static IStorageProvider StorageProvider
        {
            get => _storageProvider ?? throw new Exception($"You must set {nameof(BlobCache)}.{nameof(StorageProvider)} on startup.");
            set => _storageProvider = value;
        }

        static IBlobCache localMachine;
        static IBlobCache userAccount;
        //static ISecureBlobCache secure;
        static bool shutdownRequested;

        /// <summary>
        /// The local machine cache. Store data here that is unrelated to the
        /// user account or shouldn't be uploaded to other machines (i.e.
        /// image cache data)
        /// </summary>
        public static IBlobCache LocalMachine => _localMachine.Value;

        /// <summary>
        /// The user account cache. Store data here that is associated with
        /// the user; in large organizations, this data will be synced to all
        /// machines via NT Roaming Profiles.
        /// </summary>
        public static IBlobCache UserAccount => _userAccount.Value;

        ///// <summary>
        ///// An IBlobCache that is encrypted - store sensitive data in this
        ///// cache such as login information.
        ///// </summary>
        //public static ISecureBlobCache Secure => _secure.Value;
        
        public static string GetDatabasePath(string applicationName, StorageLocation location)
        {
            var storageProvider = StorageProvider;
            return storageProvider.GetDatabasePath(applicationName, location);
        }   
    }
}