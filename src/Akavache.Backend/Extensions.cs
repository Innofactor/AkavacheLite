namespace Innofactor.Xrm.Akavache.Backend
{
    using System;
    using Akavache.Backend.Interfaces;

    public static class Extensions
    {
        #region Public Methods

        public static string GenerateKey(this IBlobCache cache) =>
            Guid.NewGuid().ToString("N");

        #endregion Public Methods
    }
}