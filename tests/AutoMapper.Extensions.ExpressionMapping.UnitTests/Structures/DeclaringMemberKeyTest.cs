using System;
using System.Reflection;
using AutoMapper.Extensions.ExpressionMapping.Structures;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests.Structures
{
    public class DeclaringMemberKeyTest
    {
        private readonly MemberInfo _testMemberInfo1;
        private readonly MemberInfo _testMemberInfo2;
        private const string TestFullName1 = "TestNamespace.TestClass.TestMember1";
        private const string TestFullName2 = "TestNamespace.TestClass.TestMember2";

        public DeclaringMemberKeyTest()
        {
            _testMemberInfo1 = typeof(TestClass).GetProperty(nameof(TestClass.Property1));
            _testMemberInfo2 = typeof(TestClass).GetProperty(nameof(TestClass.Property2));
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Act
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Assert
            Assert.Equal(_testMemberInfo1, key.DeclaringMemberInfo);
            Assert.Equal(TestFullName1, key.DeclaringMemberFullName);
        }

        #endregion

        #region Equals Tests

        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var result = key.Equals((object)null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_NullDeclaringMemberKey_ReturnsFalse()
        {
            // Arrange
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var result = key.Equals((DeclaringMemberKey)null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var result = key1.Equals(key2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_DifferentMemberInfo_ReturnsFalse()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo2, TestFullName1);

            // Act
            var result = key1.Equals(key2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_DifferentFullName_ReturnsFalse()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo1, TestFullName2);

            // Act
            var result = key1.Equals(key2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_DifferentMemberInfoAndFullName_ReturnsFalse()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo2, TestFullName2);

            // Act
            var result = key1.Equals(key2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_ObjectOverload_SameValues_ReturnsTrue()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            object key2 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var result = key1.Equals(key2);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_SameMemberInfo_ReturnsSameHashCode()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo1, TestFullName2);

            // Act
            var hash1 = key1.GetHashCode();
            var hash2 = key2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_EqualObjects_ReturnsSameHashCode()
        {
            // Arrange
            var key1 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);
            var key2 = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var hash1 = key1.GetHashCode();
            var hash2 = key2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_IsConsistent()
        {
            // Arrange
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var hash1 = key.GetHashCode();
            var hash2 = key.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ReturnsDeclaringMemberFullName()
        {
            // Arrange
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName1);

            // Act
            var result = key.ToString();

            // Assert
            Assert.Equal(TestFullName1, result);
        }

        [Fact]
        public void ToString_WithDifferentFullName_ReturnsCorrectValue()
        {
            // Arrange
            var key = new DeclaringMemberKey(_testMemberInfo1, TestFullName2);

            // Act
            var result = key.ToString();

            // Assert
            Assert.Equal(TestFullName2, result);
        }

        #endregion

        #region Helper Class

        private class TestClass
        {
            public string Property1 { get; set; }
            public int Property2 { get; set; }
        }

        #endregion
    }
}