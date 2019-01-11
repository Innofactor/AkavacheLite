namespace Akavache.Backend.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        public void Can_Check_If_Dictionary_Contains_Key()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);

            // Act
            dictionary["test"] = entity;

            var firstSearch = dictionary.ContainsKey("test");
            var secondSearch = dictionary.ContainsKey("test1");

            // Assert
            Assert.True(firstSearch);
            Assert.False(secondSearch);
        }

        [Fact]
        public void Can_Check_If_Dictionary_Contains_Value()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);

            // Act
            dictionary["test"] = entity;

            var existing = new KeyValuePair<string, Entity>("test", entity);
            var nonExisting = new KeyValuePair<string, Entity>("test1", new Entity());

            var firstSearch = dictionary.Contains(existing);
            var secondSearch = dictionary.Contains(nonExisting);

            // Assert
            Assert.True(firstSearch);
            Assert.False(secondSearch);
        }

        [Fact]
        public void Can_Get_Keys()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);

            // Act
            dictionary["test"] = entity;

            var result = dictionary.Keys;

            // Assert
            Assert.Equal("test", result.SingleOrDefault());
            Assert.Equal(typeof(string), result.SingleOrDefault().GetType());
        }

        [Fact]
        public void Can_Get_Values()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var entity1 = new Entity("test1", id1);
            var entity2 = new Entity("test2", id2);

            // Act
            dictionary["test1"] = entity1;
            dictionary["test2"] = entity2;

            var result = dictionary.Values.ToList();

            // Assert
            Assert.Equal("test1", result[0].LogicalName);
            Assert.Equal("test2", result[1].LogicalName);
            Assert.Equal(id1, result[0].Id);
            Assert.Equal(id2, result[1].Id);
        }

        [Fact]
        public void Can_Remove_By_Key()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);

            // Act
            dictionary["test"] = entity;

            var result = dictionary.Remove("test");

            // Assert
            Assert.True(result);
            Assert.False(dictionary.ContainsKey("test"));
        }

        [Fact]
        public void Can_Remove_By_KeyValuePair()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Entity("test", id);

            // Act
            dictionary["test"] = entity;

            var pair = new KeyValuePair<string, Entity>("test", entity);
            var result = dictionary.Remove(pair);

            // Assert
            Assert.True(result);
            Assert.False(dictionary.ContainsKey("test"));
        }

        [Fact]
        public void Can_Store_And_Retrieve_Value()
        {
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

        [Fact]
        public void Dictionaty_Gets_Cleared()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var entity1 = new Entity("test1", id1);
            var entity2 = new Entity("test2", id2);

            // Act
            dictionary["test1"] = entity1;
            dictionary["test2"] = entity2;

            dictionary.Clear();

            // Assert
            Assert.True(dictionary.Count == 0);
        }

        public void Dispose()
        {
            // Cleanup here
            dictionary.Dispose();
            File.Delete(dbPath);
        }

        [Fact]
        public void Returns_Correct_Number_Of_Items()
        {
            // Arrange
            var rnd = new Random();
            var count = rnd.Next(0, 11);

            for (var i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                var entityName = $"test{i}";
                var entity = new Entity(entityName, id);
                dictionary[entityName] = entity;
            }

            // Act
            var result = dictionary.Count;

            // Assert
            Assert.Equal(count, result);
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