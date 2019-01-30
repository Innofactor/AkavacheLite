﻿namespace Innofactor.Xrm.Persistent.Collections.Backend.Platforms
{
    using System;
    using System.IO;
    using Interfaces;

    public class GenericApplicationStorageProvider : IStorageProvider
    {
        #region Public Constructors

        public GenericApplicationStorageProvider()
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public string GetPersistentCacheDirectory() =>
            Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public string GetSecretCacheDirectory() =>
            Path.Combine(GetPersistentCacheDirectory(), "Secret");

        public string GetTemporaryCacheDirectory() =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #endregion Public Methods
    }
}