using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class TypeExtensionsTest
    {
        #region Has<TAttribute> Tests

        [AttributeUsage(AttributeTargets.Class)]
        private class TestAttribute : Attribute { }

        [Test]
        private class ClassWithAttribute { }

        private class ClassWithoutAttribute { }

        [Fact]
        public void Has_TypeWithAttribute_ReturnsTrue()
        {
            // Arrange
            var type = typeof(ClassWithAttribute);

            // Act
            var result = type.Has<TestAttribute>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Has_TypeWithoutAttribute_ReturnsFalse()
        {
            // Arrange
            var type = typeof(ClassWithoutAttribute);

            // Act
            var result = type.Has<TestAttribute>();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetDeclaredConstructors Tests

        private class ClassWithMultipleConstructors
        {
            public ClassWithMultipleConstructors() { }
            public ClassWithMultipleConstructors(int value) { }
            public ClassWithMultipleConstructors(string text, int value) { }
        }

        [Fact]
        public void GetDeclaredConstructors_ReturnsAllConstructors()
        {
            // Arrange
            var type = typeof(ClassWithMultipleConstructors);

            // Act
            var constructors = type.GetDeclaredConstructors().ToList();

            // Assert
            Assert.Equal(3, constructors.Count);
        }

        #endregion

        #region GetDeclaredMethod Tests

        private class ClassWithMethods
        {
            public void TestMethod() { }
            public void AnotherMethod(int value) { }
        }

        [Fact]
        public void GetDeclaredMethod_ExistingMethod_ReturnsMethod()
        {
            // Arrange
            var type = typeof(ClassWithMethods);

            // Act
            var method = type.GetDeclaredMethod("TestMethod");

            // Assert
            Assert.NotNull(method);
            Assert.Equal("TestMethod", method.Name);
        }

        [Fact]
        public void GetDeclaredMethod_NonExistingMethod_ReturnsNull()
        {
            // Arrange
            var type = typeof(ClassWithMethods);

            // Act
            var method = type.GetDeclaredMethod("NonExistingMethod");

            // Assert
            Assert.Null(method);
        }

        #endregion

        #region GetDeclaredConstructor Tests

        [Fact]
        public void GetDeclaredConstructor_WithMatchingParameters_ReturnsConstructor()
        {
            // Arrange
            var type = typeof(ClassWithMultipleConstructors);

            // Act
            var constructor = type.GetDeclaredConstructor(new[] { typeof(int) });

            // Assert
            Assert.NotNull(constructor);
            Assert.Single(constructor.GetParameters());
            Assert.Equal(typeof(int), constructor.GetParameters()[0].ParameterType);
        }

        [Fact]
        public void GetDeclaredConstructor_WithNonMatchingParameters_ReturnsNull()
        {
            // Arrange
            var type = typeof(ClassWithMultipleConstructors);

            // Act
            var constructor = type.GetDeclaredConstructor(new[] { typeof(double) });

            // Assert
            Assert.Null(constructor);
        }

        [Fact]
        public void GetDeclaredConstructor_WithMultipleParameters_ReturnsCorrectConstructor()
        {
            // Arrange
            var type = typeof(ClassWithMultipleConstructors);

            // Act
            var constructor = type.GetDeclaredConstructor(new[] { typeof(string), typeof(int) });

            // Assert
            Assert.NotNull(constructor);
            Assert.Equal(2, constructor.GetParameters().Length);
        }

        #endregion

        #region GetAllMethods Tests

        [Fact]
        public void GetAllMethods_ReturnsAllRuntimeMethods()
        {
            // Arrange
            var type = typeof(ClassWithMethods);

            // Act
            var methods = type.GetAllMethods().ToList();

            // Assert
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "TestMethod");
            Assert.Contains(methods, m => m.Name == "AnotherMethod");
        }

        #endregion

        #region GetDeclaredProperties Tests

        private class ClassWithProperties
        {
            public int Property1 { get; set; }
            public string Property2 { get; set; }
        }

        [Fact]
        public void GetDeclaredProperties_ReturnsAllDeclaredProperties()
        {
            // Arrange
            var type = typeof(ClassWithProperties);

            // Act
            var properties = type.GetDeclaredProperties().ToList();

            // Assert
            Assert.Equal(2, properties.Count);
            Assert.Contains(properties, p => p.Name == "Property1");
            Assert.Contains(properties, p => p.Name == "Property2");
        }

        #endregion

        #region IsStatic Tests

        private class ClassWithStaticMembers
        {
            public static int StaticField = 0;
            public int InstanceField = 0;
            public static int StaticProperty { get; set; }
            public int InstanceProperty { get; set; }
            public static void StaticMethod() { }
            public void InstanceMethod() { }
        }

        [Fact]
        public void IsStatic_StaticField_ReturnsTrue()
        {
            // Arrange
            var field = typeof(ClassWithStaticMembers).GetField("StaticField");

            // Act
            var result = field.IsStatic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStatic_InstanceField_ReturnsFalse()
        {
            // Arrange
            var field = typeof(ClassWithStaticMembers).GetField("InstanceField");

            // Act
            var result = field.IsStatic();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStatic_NullField_ReturnsFalse()
        {
            // Arrange
            FieldInfo field = null;

            // Act
            var result = field.IsStatic();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStatic_StaticProperty_ReturnsTrue()
        {
            // Arrange
            var property = typeof(ClassWithStaticMembers).GetProperty("StaticProperty");

            // Act
            var result = property.IsStatic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStatic_InstanceProperty_ReturnsFalse()
        {
            // Arrange
            var property = typeof(ClassWithStaticMembers).GetProperty("InstanceProperty");

            // Act
            var result = property.IsStatic();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStatic_NullProperty_ReturnsFalse()
        {
            // Arrange
            PropertyInfo property = null;

            // Act
            var result = property.IsStatic();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStatic_StaticMethod_ReturnsTrue()
        {
            // Arrange
            MemberInfo member = typeof(ClassWithStaticMembers).GetMethod("StaticMethod");

            // Act
            var result = member.IsStatic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStatic_InstanceMethod_ReturnsFalse()
        {
            // Arrange
            MemberInfo member = typeof(ClassWithStaticMembers).GetMethod("InstanceMethod");

            // Act
            var result = member.IsStatic();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStatic_MemberInfoAsStaticField_ReturnsTrue()
        {
            // Arrange
            MemberInfo member = typeof(ClassWithStaticMembers).GetField("StaticField");

            // Act
            var result = member.IsStatic();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStatic_MemberInfoAsStaticProperty_ReturnsTrue()
        {
            // Arrange
            MemberInfo member = typeof(ClassWithStaticMembers).GetProperty("StaticProperty");

            // Act
            var result = member.IsStatic();

            // Assert
            Assert.True(result);
        }

        #endregion

        #region IsEnum Tests

        private enum TestEnum { Value1, Value2 }

        [Fact]
        public void IsEnum_EnumType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(TestEnum);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnum_NonEnumType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsGenericType Tests

        [Fact]
        public void IsGenericType_GenericType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsGenericType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsGenericType_NonGenericType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsGenericType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsPrimitive Tests

        [Fact]
        public void IsPrimitive_PrimitiveType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsPrimitive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPrimitive_NonPrimitiveType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsPrimitive();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValueType Tests

        [Fact]
        public void IsValueType_StructType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsValueType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValueType_ClassType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsValueType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsLiteralType Tests

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(char))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(string))]
        public void IsLiteralType_LiteralType_ReturnsTrue(Type type)
        {
            // Act
            var result = type.IsLiteralType();

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(bool?))]
        [InlineData(typeof(DateTime?))]
        [InlineData(typeof(decimal?))]
        public void IsLiteralType_NullableLiteralType_ReturnsTrue(Type type)
        {
            // Act
            var result = type.IsLiteralType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLiteralType_NonLiteralType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(ClassWithProperties);

            // Act
            var result = type.IsLiteralType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsQueryableType Tests

        [Fact]
        public void IsQueryableType_QueryableType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(IQueryable<int>);

            // Act
            var result = type.IsQueryableType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsQueryableType_QueryableNonGenericType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(IQueryable);

            // Act
            var result = type.IsQueryableType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsQueryableType_NonQueryableType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsQueryableType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetGenericElementType Tests

        [Fact]
        public void GetGenericElementType_ArrayType_ReturnsElementType()
        {
            // Arrange
            var type = typeof(int[]);

            // Act
            var result = type.GetGenericElementType();

            // Assert
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void GetGenericElementType_GenericType_ReturnsGenericArgument()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = type.GetGenericElementType();

            // Assert
            Assert.Equal(typeof(string), result);
        }

        #endregion

        #region IsEnumerableType Tests

        [Fact]
        public void IsEnumerableType_ListType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsEnumerableType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumerableType_IEnumerableType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(IEnumerable<string>);

            // Act
            var result = type.IsEnumerableType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumerableType_NonEnumerableType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnumerableType();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEnumerableType_NonGenericEnumerableType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(System.Collections.IEnumerable);

            // Act
            var result = type.IsEnumerableType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ReplaceItemType Tests

        [Fact]
        public void ReplaceItemType_SimpleTypeMatch_ReturnsNewType()
        {
            // Arrange
            var targetType = typeof(int);
            var oldType = typeof(int);
            var newType = typeof(string);

            // Act
            var result = targetType.ReplaceItemType(oldType, newType);

            // Assert
            Assert.Equal(typeof(string), result);
        }

        [Fact]
        public void ReplaceItemType_SimpleTypeNoMatch_ReturnsOriginalType()
        {
            // Arrange
            var targetType = typeof(int);
            var oldType = typeof(string);
            var newType = typeof(double);

            // Act
            var result = targetType.ReplaceItemType(oldType, newType);

            // Assert
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void ReplaceItemType_GenericType_ReplacesTypeArgument()
        {
            // Arrange
            var targetType = typeof(List<int>);
            var oldType = typeof(int);
            var newType = typeof(string);

            // Act
            var result = targetType.ReplaceItemType(oldType, newType);

            // Assert
            Assert.Equal(typeof(List<string>), result);
        }

        [Fact]
        public void ReplaceItemType_NestedGenericType_ReplacesTypeArgument()
        {
            // Arrange
            var targetType = typeof(Dictionary<int, string>);
            var oldType = typeof(int);
            var newType = typeof(long);

            // Act
            var result = targetType.ReplaceItemType(oldType, newType);

            // Assert
            Assert.Equal(typeof(Dictionary<long, string>), result);
        }

        [Fact]
        public void ReplaceItemType_MultipleGenericArguments_ReplacesAll()
        {
            // Arrange
            var targetType = typeof(Dictionary<int, int>);
            var oldType = typeof(int);
            var newType = typeof(string);

            // Act
            var result = targetType.ReplaceItemType(oldType, newType);

            // Assert
            Assert.Equal(typeof(Dictionary<string, string>), result);
        }

        #endregion
    }
}