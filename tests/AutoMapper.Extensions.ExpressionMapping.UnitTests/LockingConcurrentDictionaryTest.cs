using System;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class LockingConcurrentDictionaryTest
    {
        [Fact]
        public void GetOrAdd_WithKey_ReturnsValue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var result = dictionary.GetOrAdd(1);

            // Assert
            Assert.Equal("Value1", result);
        }

        [Fact]
        public void GetOrAdd_WithSameKey_ReturnsSameValue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var result1 = dictionary.GetOrAdd(1);
            var result2 = dictionary.GetOrAdd(1);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal("Value1", result1);
        }

        [Fact]
        public void GetOrAdd_WithValueFactory_ReturnsValue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var result = dictionary.GetOrAdd(1, k => new Lazy<string>(() => "CustomValue"));

            // Assert
            Assert.Equal("CustomValue", result);
        }

        [Fact]
        public void GetOrAdd_ValueFactoryCalledOnlyOnce()
        {
            // Arrange
            var callCount = 0;
            var dictionary = new LockingConcurrentDictionary<int, string>(key =>
            {
                callCount++;
                return $"Value{key}";
            });

            // Act
            var result1 = dictionary.GetOrAdd(1);
            var result2 = dictionary.GetOrAdd(1);

            // Assert
            Assert.Equal(1, callCount);
            Assert.Equal("Value1", result1);
            Assert.Equal("Value1", result2);
        }

        [Fact]
        public void Indexer_Get_ReturnsValue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");
            dictionary.GetOrAdd(1);

            // Act
            var result = dictionary[1];

            // Assert
            Assert.Equal("Value1", result);
        }

        [Fact]
        public void Indexer_Set_UpdatesValue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");
            dictionary.GetOrAdd(1);

            // Act
            dictionary[1] = "UpdatedValue";
            var result = dictionary[1];

            // Assert
            Assert.Equal("UpdatedValue", result);
        }

        [Fact]
        public void TryGetValue_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");
            dictionary.GetOrAdd(1);

            // Act
            var exists = dictionary.TryGetValue(1, out var value);

            // Assert
            Assert.True(exists);
            Assert.Equal("Value1", value);
        }

        [Fact]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var exists = dictionary.TryGetValue(1, out var value);

            // Assert
            Assert.False(exists);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ReferenceType_DefaultIsNull()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var exists = dictionary.TryGetValue(99, out var value);

            // Assert
            Assert.False(exists);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ValueType_DefaultIsZero()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<string, int>(key => key.Length);

            // Act
            var exists = dictionary.TryGetValue("nonexistent", out var value);

            // Assert
            Assert.False(exists);
            Assert.Equal(0, value);
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");
            dictionary.GetOrAdd(1);

            // Act
            var contains = dictionary.ContainsKey(1);

            // Assert
            Assert.True(contains);
        }

        [Fact]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var contains = dictionary.ContainsKey(1);

            // Assert
            Assert.False(contains);
        }

        [Fact]
        public void Keys_ReturnsAllKeys()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");
            dictionary.GetOrAdd(1);
            dictionary.GetOrAdd(2);
            dictionary.GetOrAdd(3);

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.Equal(3, keys.Count);
            Assert.Contains(1, keys);
            Assert.Contains(2, keys);
            Assert.Contains(3, keys);
        }

        [Fact]
        public void Keys_EmptyDictionary_ReturnsEmptyCollection()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public void GetOrAdd_WithMultipleDifferentKeys_ReturnsCorrectValues()
        {
            // Arrange
            var dictionary = new LockingConcurrentDictionary<int, string>(key => $"Value{key}");

            // Act
            var result1 = dictionary.GetOrAdd(1);
            var result2 = dictionary.GetOrAdd(2);
            var result3 = dictionary.GetOrAdd(3);

            // Assert
            Assert.Equal("Value1", result1);
            Assert.Equal("Value2", result2);
            Assert.Equal("Value3", result3);
        }

        [Fact]
        public void Constructor_WithNullValueFactory_ThrowsException()
        {
            // This test verifies the behavior when a null factory is used
            // The actual exception will be thrown when trying to add a value
            var dictionary = new LockingConcurrentDictionary<int, string>(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => dictionary.GetOrAdd(1));
        }
    }
}