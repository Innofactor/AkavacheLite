namespace Akavache.Backend.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Akavache.Backend.Implementations;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;
    using Xrm.Json.Serialization;
    using Xunit;

    public class EntityDictionaryTests
    {
        #region Public Methods

        [Fact]
        public void Can_Store_And_Retrieve_Value()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>() { new EntityConverter() }
            };

            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(EntityDictionaryTests)}.db");

            // Act
            var dictionary = new PersistentEntityDictionary<string, Entity>(path)
            {
                ["test"] = entity
            };

            var result = dictionary["test"];

            // Assert
        }

        #endregion Public Methods
    }
}