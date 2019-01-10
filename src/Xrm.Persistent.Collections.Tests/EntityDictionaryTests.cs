namespace Akavache.Backend.Tests
{
    using System;
    using System.IO;
    using Akavache.Backend.Implementations;
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class EntityDictionaryTests : IDisposable
    {
        #region Private Fields

        private readonly string dbPath;
        private readonly LocalDictionary<string, Entity> dictionary;

        #endregion Private Fields

        #region Public Constructors

        public EntityDictionaryTests()
        {
            var suffix = Guid.NewGuid();
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(EntityDictionaryTests)}-{suffix}.db");

            dictionary = new LocalDictionary<string, Entity>(dbPath);
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        public void Can_Add_And_TryGet_Value()
        {
            var p = dbPath;

            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);
            
            // Act
            dictionary.Add("test", entity);
            var retrieved = dictionary.TryGetValue("test", out Entity result);

            // Assert
            Assert.True(retrieved);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.LogicalName, result.LogicalName);
        }

        [Fact]
        public void Can_Store_And_Retrieve_Value()
        {
            var p = dbPath;

            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);
            
            // Act
            dictionary["test"] = entity;
            
            var result = dictionary["test"];

            // Assert
            Assert.Equal(entity.LogicalName, result.LogicalName);
            Assert.Equal(entity.Id, result.Id);
        }

        public void Dispose()
        {
            // Cleanup here
            dictionary.Dispose();
            File.Delete(dbPath);
        }

        [Fact]
        public void TryGet_Returns_Default_If_Key_Not_Found()
        {
            // Arrange
            
            // Act
            var p = dbPath;

            var retrieved = dictionary.TryGetValue("test", out Entity result);

            // Assert
            Assert.False(retrieved);
            Assert.Equal(default(Entity), result);
        }

        #endregion Public Methods
    }
}