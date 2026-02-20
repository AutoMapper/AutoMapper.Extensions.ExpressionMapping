using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ElementTypeHelperTest
    {
        #region GetElementType Tests

        [Fact]
        public void GetElementType_ArrayType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(int[]);

            // Act
            var result = ElementTypeHelper.GetElementType(type);

            // Assert
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void GetElementType_ListType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = ElementTypeHelper.GetElementType(type);

            // Assert
            Assert.Equal(typeof(string), result);
        }

        [Fact]
        public void GetElementType_IEnumerableType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(IEnumerable<double>);

            // Act
            var result = ElementTypeHelper.GetElementType(type);

            // Assert
            Assert.Equal(typeof(double), result);
        }

        [Fact]
        public void GetElementType_NonEnumerableType_ThrowsArgumentException()
        {
            // Arrange
            var type = typeof(int);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ElementTypeHelper.GetElementType(type));
        }

        #endregion

        #region GetElementTypes with Flags Tests

        [Fact]
        public void GetElementTypes_ArrayType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(string[]);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(string), result[0]);
        }

        [Fact]
        public void GetElementTypes_ListType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(int), result[0]);
        }

        [Fact]
        public void GetElementTypes_DictionaryTypeWithoutFlag_ReturnsKeyValuePairType()
        {
            // Arrange
            var type = typeof(Dictionary<int, string>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, ElementTypeFlags.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(KeyValuePair<int, string>), result[0]);
        }

        [Fact]
        public void GetElementTypes_DictionaryTypeWithBreakFlag_ReturnsKeyAndValueTypes()
        {
            // Arrange
            var type = typeof(Dictionary<int, string>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, ElementTypeFlags.BreakKeyValuePair);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(typeof(int), result[0]);
            Assert.Equal(typeof(string), result[1]);
        }

        [Fact]
        public void GetElementTypes_IDictionaryTypeWithBreakFlag_ReturnsKeyAndValueTypes()
        {
            // Arrange
            var type = typeof(IDictionary<string, double>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, ElementTypeFlags.BreakKeyValuePair);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(typeof(string), result[0]);
            Assert.Equal(typeof(double), result[1]);
        }

        [Fact]
        public void GetElementTypes_ReadOnlyDictionaryTypeWithBreakFlag_ReturnsKeyAndValueTypes()
        {
            // Arrange
            var type = typeof(IReadOnlyDictionary<long, bool>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, ElementTypeFlags.BreakKeyValuePair);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(typeof(long), result[0]);
            Assert.Equal(typeof(bool), result[1]);
        }

        [Fact]
        public void GetElementTypes_ReadOnlyDictionaryTypeWithoutFlag_ReturnsKeyValuePairType()
        {
            // Arrange
            var type = typeof(IReadOnlyDictionary<int, string>);

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, ElementTypeFlags.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(KeyValuePair<int, string>), result[0]);
        }

        [Fact]
        public void GetElementTypes_NonEnumerableType_ThrowsArgumentException()
        {
            // Arrange
            var type = typeof(int);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ElementTypeHelper.GetElementTypes(type));
            Assert.Contains("Unable to find the element type", exception.Message);
        }

        #endregion

        #region GetElementTypes with Enumerable Tests

        [Fact]
        public void GetElementTypes_WithEnumerableContainingIntegers_ReturnsIntType()
        {
            // Arrange
            var type = typeof(ArrayList);
            IEnumerable enumerable = new ArrayList { 1, 2, 3 };

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, enumerable);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(int), result[0]);
        }

        [Fact]
        public void GetElementTypes_WithEnumerableContainingStrings_ReturnsStringType()
        {
            // Arrange
            var type = typeof(ArrayList);
            IEnumerable enumerable = new ArrayList { "test", "data" };

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, enumerable);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(string), result[0]);
        }

        [Fact]
        public void GetElementTypes_WithEmptyEnumerable_ReturnsObjectType()
        {
            // Arrange
            var type = typeof(ArrayList);
            IEnumerable enumerable = new ArrayList();

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, enumerable);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(object), result[0]);
        }

        [Fact]
        public void GetElementTypes_WithNullEnumerable_ReturnsObjectType()
        {
            // Arrange
            var type = typeof(ArrayList);
            IEnumerable enumerable = null;

            // Act
            var result = ElementTypeHelper.GetElementTypes(type, enumerable);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(object), result[0]);
        }

        #endregion

        #region GetReadOnlyDictionaryType Tests

        [Fact]
        public void GetReadOnlyDictionaryType_ReadOnlyDictionaryType_ReturnsGenericInterface()
        {
            // Arrange
            var type = typeof(IReadOnlyDictionary<int, string>);

            // Act
            var result = type.GetReadOnlyDictionaryType();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGenericType);
            Assert.Equal(typeof(IReadOnlyDictionary<,>), result.GetGenericTypeDefinition());
        }

        [Fact]
        public void GetReadOnlyDictionaryType_NonReadOnlyDictionaryType_ReturnsNull()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.GetReadOnlyDictionaryType();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetReadOnlyDictionaryType_RegularDictionaryType_ImplementsIReadOnlyDictionaryDoesNotReturnNull()
        {
            // Arrange
            var type = typeof(Dictionary<int, string>);

            // Act
            var result = type.GetReadOnlyDictionaryType();

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetDictionaryType Tests

        [Fact]
        public void GetDictionaryType_DictionaryType_ReturnsGenericInterface()
        {
            // Arrange
            var type = typeof(Dictionary<int, string>);

            // Act
            var result = type.GetDictionaryType();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGenericType);
            Assert.Equal(typeof(IDictionary<,>), result.GetGenericTypeDefinition());
        }

        [Fact]
        public void GetDictionaryType_IDictionaryType_ReturnsGenericInterface()
        {
            // Arrange
            var type = typeof(IDictionary<string, double>);

            // Act
            var result = type.GetDictionaryType();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsGenericType);
            Assert.Equal(typeof(IDictionary<,>), result.GetGenericTypeDefinition());
        }

        [Fact]
        public void GetDictionaryType_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.GetDictionaryType();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetDictionaryType_ReadOnlyDictionaryType_ReturnsNull()
        {
            // Arrange
            var type = typeof(IReadOnlyDictionary<int, string>);

            // Act
            var result = type.GetDictionaryType();

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ToType Tests

        [Fact]
        public void ToType_SameType_ReturnsOriginalExpression()
        {
            // Arrange
            Expression<Func<int>> expression = () => 42;
            var body = expression.Body;
            var type = typeof(int);

            // Act
            var result = ElementTypeHelper.ToType(body, type);

            // Assert
            Assert.Same(body, result);
        }

        [Fact]
        public void ToType_DifferentType_ReturnsConvertExpression()
        {
            // Arrange
            Expression<Func<int>> expression = () => 42;
            var body = expression.Body;
            var type = typeof(long);

            // Act
            var result = ElementTypeHelper.ToType(body, type);

            // Assert
            Assert.NotSame(body, result);
            Assert.IsType<UnaryExpression>(result);
            Assert.Equal(ExpressionType.Convert, result.NodeType);
            Assert.Equal(typeof(long), result.Type);
        }

        [Fact]
        public void ToType_ConvertIntToObject_ReturnsConvertExpression()
        {
            // Arrange
            Expression<Func<int>> expression = () => 5;
            var body = expression.Body;
            var type = typeof(object);

            // Act
            var result = ElementTypeHelper.ToType(body, type);

            // Assert
            Assert.IsType<UnaryExpression>(result);
            Assert.Equal(typeof(object), result.Type);
        }

        [Fact]
        public void ToType_ConvertStringToObject_ReturnsConvertExpression()
        {
            // Arrange
            Expression<Func<string>> expression = () => "test";
            var body = expression.Body;
            var type = typeof(object);

            // Act
            var result = ElementTypeHelper.ToType(body, type);

            // Assert
            Assert.IsType<UnaryExpression>(result);
            Assert.Equal(typeof(object), result.Type);
        }

        #endregion
    }
}