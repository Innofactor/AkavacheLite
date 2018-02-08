using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkavacheLite
{
    public interface IStorageProvider
    {
        ///// <summary>
        ///// Create a directory and its parents. If the directory already
        ///// exists, this method does nothing (i.e. it does not throw if a
        ///// directory exists)
        ///// </summary>
        ///// <param name="path">The path to create.</param>
        //Task CreateRecursive(string path);

        ///// <summary>
        ///// Deletes a file.
        ///// </summary>
        ///// <param name="path">The path to the file</param>
        //Task Delete(string path);

        /// <summary>
        /// Gets the default local machine cache directory (i.e. the one for temporary data)
        /// </summary>
        /// <returns>The default local machine cache directory.</returns>
        string GetTemporaryCacheDirectory();

        /// <summary>
        /// Gets the default roaming cache directory (i.e. the one for user settings)
        /// </summary>
        /// <returns>The default roaming cache directory.</returns>
        string GetPersistentCacheDirectory();

        /// <summary>
        /// Gets the default roaming cache directory (i.e. the one for user settings)
        /// </summary>
        /// <returns>The default roaming cache directory.</returns>
        string GetSecretCacheDirectory();
    }
}
