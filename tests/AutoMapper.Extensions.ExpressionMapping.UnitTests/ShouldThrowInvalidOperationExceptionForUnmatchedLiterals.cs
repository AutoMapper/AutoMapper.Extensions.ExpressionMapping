using Microsoft.OData.Edm;
using System;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ShouldThrowInvalidOperationExceptionForUnmatchedLiterals
    {
        [Theory]
        [InlineData(nameof(ProductModel.Bool), typeof(bool?))]
        [InlineData(nameof(ProductModel.DateTime), typeof(DateTime?))]
        [InlineData(nameof(ProductModel.DateTimeOffset), typeof(DateTimeOffset?))]
        [InlineData(nameof(ProductModel.Date), typeof(Date?))]
#if NET8_0_OR_GREATER
        [InlineData(nameof(ProductModel.DateOnly), typeof(DateOnly?))]
#endif
        [InlineData(nameof(ProductModel.TimeSpan), typeof(TimeSpan?))]
        [InlineData(nameof(ProductModel.TimeOfDay), typeof(TimeOfDay?))]
#if NET8_0_OR_GREATER
        [InlineData(nameof(ProductModel.TimeOnly), typeof(TimeOnly?))]
#endif
        [InlineData(nameof(ProductModel.Guid), typeof(Guid?))]
        [InlineData(nameof(ProductModel.Decimal), typeof(decimal?))]
        [InlineData(nameof(ProductModel.Byte), typeof(byte?))]
        [InlineData(nameof(ProductModel.Short), typeof(short?))]
        [InlineData(nameof(ProductModel.Int), typeof(int?))]
        [InlineData(nameof(ProductModel.Long), typeof(long?))]
        [InlineData(nameof(ProductModel.Float), typeof(float?))]
        [InlineData(nameof(ProductModel.Double), typeof(double?))]
        [InlineData(nameof(ProductModel.Char), typeof(char?))]
        [InlineData(nameof(ProductModel.SByte), typeof(sbyte?))]
        [InlineData(nameof(ProductModel.UShort), typeof(ushort?))]
        [InlineData(nameof(ProductModel.ULong), typeof(ulong?))]
        public void ThrowsCreatingBinaryExpressionCombiningNonNullableParameterWithNullableConstant(string memberName, Type constantType)
        {
            ParameterExpression productParam = Expression.Parameter(typeof(ProductModel), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(ProductModel), memberName));

            Assert.Throws<InvalidOperationException>
            (
                () => Expression.Lambda<Func<ProductModel, bool>>
                (
                    Expression.Equal
                    (
                        property,
                        Expression.Constant(Activator.CreateInstance(constantType), constantType)
                    ),
                    productParam
                )
            );
        }

        [Theory]
        [InlineData(nameof(Product.Bool), typeof(bool))]
        [InlineData(nameof(Product.DateTime), typeof(DateTime))]
        [InlineData(nameof(Product.DateTimeOffset), typeof(DateTimeOffset))]
        [InlineData(nameof(Product.Date), typeof(Date))]
#if NET8_0_OR_GREATER
        [InlineData(nameof(Product.DateOnly), typeof(DateOnly))]
#endif
        [InlineData(nameof(Product.TimeSpan), typeof(TimeSpan))]
        [InlineData(nameof(Product.TimeOfDay), typeof(TimeOfDay))]
#if NET8_0_OR_GREATER
        [InlineData(nameof(Product.TimeOnly), typeof(TimeOnly))]
#endif
        [InlineData(nameof(Product.Guid), typeof(Guid))]
        [InlineData(nameof(Product.Decimal), typeof(decimal))]
        [InlineData(nameof(Product.Byte), typeof(byte))]
        [InlineData(nameof(Product.Short), typeof(short))]
        [InlineData(nameof(Product.Int), typeof(int))]
        [InlineData(nameof(Product.Long), typeof(long))]
        [InlineData(nameof(Product.Float), typeof(float))]
        [InlineData(nameof(Product.Double), typeof(double))]
        [InlineData(nameof(Product.Char), typeof(char))]
        [InlineData(nameof(Product.SByte), typeof(sbyte))]
        [InlineData(nameof(Product.UShort), typeof(ushort))]
        [InlineData(nameof(Product.ULong), typeof(ulong))]
        public void ThrowsCreatingBinaryExpressionCombiningNullableParameterWithNonNullableConstant(string memberName, Type constantType)
        {
            ParameterExpression productParam = Expression.Parameter(typeof(Product), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(Product), memberName));

            Assert.Throws<InvalidOperationException>
            (
                () => Expression.Lambda<Func<Product, bool>>
                (
                    Expression.Equal
                    (
                        property,
                        Expression.Constant(Activator.CreateInstance(constantType), constantType)
                    ),
                    productParam
                )
            );
        }

        class Product
        {
            public bool? Bool { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
            public DateTime? DateTime { get; set; }
            public Date? Date { get; set; }
#if NET8_0_OR_GREATER
            public DateOnly? DateOnly { get; set; }
#endif
            public TimeSpan? TimeSpan { get; set; }
            public TimeOfDay? TimeOfDay { get; set; }
#if NET8_0_OR_GREATER
            public TimeOnly? TimeOnly { get; set; }
#endif
            public Guid? Guid { get; set; }
            public decimal? Decimal { get; set; }
            public byte? Byte { get; set; }
            public short? Short { get; set; }
            public int? Int { get; set; }
            public long? Long { get; set; }
            public float? Float { get; set; }
            public double? Double { get; set; }
            public char? Char { get; set; }
            public sbyte? SByte { get; set; }
            public ushort? UShort { get; set; }
            public uint? UInt { get; set; }
            public ulong? ULong { get; set; }
        }

        class ProductModel
        {
            public bool Bool { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public DateTime DateTime { get; set; }
            public Date Date { get; set; }
#if NET8_0_OR_GREATER
            public DateOnly DateOnly { get; set; }
#endif
            public TimeSpan TimeSpan { get; set; }
            public TimeOfDay TimeOfDay { get; set; }
#if NET8_0_OR_GREATER
            public TimeOnly TimeOnly { get; set; }
#endif
            public Guid Guid { get; set; }
            public decimal Decimal { get; set; }
            public byte Byte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public long Long { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public char Char { get; set; }
            public sbyte SByte { get; set; }
            public ushort UShort { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
        }
    }
}
