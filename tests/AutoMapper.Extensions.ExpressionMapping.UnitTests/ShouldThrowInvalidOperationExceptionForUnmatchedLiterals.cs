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
        [InlineData(nameof(ProductModel.DateOnly), typeof(DateOnly?))]
        [InlineData(nameof(ProductModel.TimeSpan), typeof(TimeSpan?))]
        [InlineData(nameof(ProductModel.TimeOfDay), typeof(TimeOfDay?))]
        [InlineData(nameof(ProductModel.TimeOnly), typeof(TimeOnly?))]
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
            var mapper = GetMapper();
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
        [InlineData(nameof(Product.DateOnly), typeof(DateOnly))]
        [InlineData(nameof(Product.TimeSpan), typeof(TimeSpan))]
        [InlineData(nameof(Product.TimeOfDay), typeof(TimeOfDay))]
        [InlineData(nameof(Product.TimeOnly), typeof(TimeOnly))]
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
            var mapper = GetMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(Product), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(Product), memberName));

            var ex = Assert.Throws<InvalidOperationException>
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

        [Theory]
        [InlineData(nameof(ProductModel.Bool), typeof(bool))]
        [InlineData(nameof(ProductModel.DateTime), typeof(DateTime))]
        [InlineData(nameof(ProductModel.DateTimeOffset), typeof(DateTimeOffset))]
        [InlineData(nameof(ProductModel.Date), typeof(Date))]
        [InlineData(nameof(ProductModel.DateOnly), typeof(DateOnly))]
        [InlineData(nameof(ProductModel.TimeSpan), typeof(TimeSpan))]
        [InlineData(nameof(ProductModel.TimeOfDay), typeof(TimeOfDay))]
        [InlineData(nameof(ProductModel.TimeOnly), typeof(TimeOnly))]
        [InlineData(nameof(ProductModel.Guid), typeof(Guid))]
        [InlineData(nameof(ProductModel.Decimal), typeof(decimal))]
        [InlineData(nameof(ProductModel.Byte), typeof(byte))]
        [InlineData(nameof(ProductModel.Short), typeof(short))]
        [InlineData(nameof(ProductModel.Int), typeof(int))]
        [InlineData(nameof(ProductModel.Long), typeof(long))]
        [InlineData(nameof(ProductModel.Float), typeof(float))]
        [InlineData(nameof(ProductModel.Double), typeof(double))]
        [InlineData(nameof(ProductModel.Char), typeof(char))]
        [InlineData(nameof(ProductModel.SByte), typeof(sbyte))]
        [InlineData(nameof(ProductModel.UShort), typeof(ushort))]
        [InlineData(nameof(ProductModel.ULong), typeof(ulong))]
        public void ThrowsmappingExpressionWithMismatchedOperands(string memberName, Type constantType)
        {
            var mapper = GetMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(ProductModel), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(ProductModel), memberName));

            Expression<Func<ProductModel, bool>> expression = Expression.Lambda<Func<ProductModel, bool>>
            (
                Expression.Equal
                (
                    property,
                    Expression.Constant(Activator.CreateInstance(constantType), constantType)
                ),
                productParam
            );

            var exception = Assert.Throws<InvalidOperationException>
            (
                () => mapper.MapExpression<Expression<Func<Product, bool>>>(expression)
            );

            Assert.StartsWith
            (
                "The source and destination types must be the same for expression mapping between literal types.", 
                exception.Message
            );
        }

        [Theory]
        [InlineData(nameof(ProductModel.Bool), typeof(bool))]
        [InlineData(nameof(ProductModel.DateTime), typeof(DateTime))]
        [InlineData(nameof(ProductModel.DateTimeOffset), typeof(DateTimeOffset))]
        [InlineData(nameof(ProductModel.Date), typeof(Date))]
        [InlineData(nameof(ProductModel.DateOnly), typeof(DateOnly))]
        [InlineData(nameof(ProductModel.TimeSpan), typeof(TimeSpan))]
        [InlineData(nameof(ProductModel.TimeOfDay), typeof(TimeOfDay))]
        [InlineData(nameof(ProductModel.TimeOnly), typeof(TimeOnly))]
        [InlineData(nameof(ProductModel.Guid), typeof(Guid))]
        [InlineData(nameof(ProductModel.Decimal), typeof(decimal))]
        [InlineData(nameof(ProductModel.Byte), typeof(byte))]
        [InlineData(nameof(ProductModel.Short), typeof(short))]
        [InlineData(nameof(ProductModel.Int), typeof(int))]
        [InlineData(nameof(ProductModel.Long), typeof(long))]
        [InlineData(nameof(ProductModel.Float), typeof(float))]
        [InlineData(nameof(ProductModel.Double), typeof(double))]
        [InlineData(nameof(ProductModel.Char), typeof(char))]
        [InlineData(nameof(ProductModel.SByte), typeof(sbyte))]
        [InlineData(nameof(ProductModel.UShort), typeof(ushort))]
        [InlineData(nameof(ProductModel.ULong), typeof(ulong))]
        public void MappingExpressionWorksUsingCustomExpressionToResolveBinaryOperators(string memberName, Type constantType)
        {
            var mapper = GetMapperWithCustomExpressions();
            ParameterExpression productParam = Expression.Parameter(typeof(ProductModel), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(ProductModel), memberName));

            Expression<Func<ProductModel, bool>> expression = Expression.Lambda<Func<ProductModel, bool>>
            (
                Expression.Equal
                (
                    property,
                    Expression.Constant(Activator.CreateInstance(constantType), constantType)
                ),
                productParam
            );

            var mappedExpression = mapper.MapExpression<Expression<Func<Product, bool>>>(expression);
            Assert.NotNull(mappedExpression);
        }

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<Product, ProductModel>();
            });
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }

        private static IMapper GetMapperWithCustomExpressions()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<Product, ProductModel>()
                    .ForMember(d => d.Bool, o => o.MapFrom(s => s.Bool.Value))
                    .ForMember(d => d.DateTime, o => o.MapFrom(s => s.DateTime.Value))
                    .ForMember(d => d.DateTimeOffset, o => o.MapFrom(s => s.DateTimeOffset.Value))
                    .ForMember(d => d.Date, o => o.MapFrom(s => s.Date.Value))
                    .ForMember(d => d.DateOnly, o => o.MapFrom(s => s.DateOnly.Value))
                    .ForMember(d => d.TimeSpan, o => o.MapFrom(s => s.TimeSpan.Value))
                    .ForMember(d => d.TimeOfDay, o => o.MapFrom(s => s.TimeOfDay.Value))
                    .ForMember(d => d.TimeOnly, o => o.MapFrom(s => s.TimeOnly.Value))
                    .ForMember(d => d.Guid, o => o.MapFrom(s => s.Guid.Value))
                    .ForMember(d => d.Decimal, o => o.MapFrom(s => s.Decimal.Value))
                    .ForMember(d => d.Byte, o => o.MapFrom(s => s.Byte.Value))
                    .ForMember(d => d.Short, o => o.MapFrom(s => s.Short.Value))
                    .ForMember(d => d.Int, o => o.MapFrom(s => s.Int.Value))
                    .ForMember(d => d.Long, o => o.MapFrom(s => s.Long.Value))
                    .ForMember(d => d.Float, o => o.MapFrom(s => s.Float.Value))
                    .ForMember(d => d.Double, o => o.MapFrom(s => s.Double.Value))
                    .ForMember(d => d.Char, o => o.MapFrom(s => s.Char.Value))
                    .ForMember(d => d.SByte, o => o.MapFrom(s => s.SByte.Value))
                    .ForMember(d => d.UShort, o => o.MapFrom(s => s.UShort.Value))
                    .ForMember(d => d.UInt, o => o.MapFrom(s => s.UInt.Value))
                    .ForMember(d => d.ULong, o => o.MapFrom(s => s.ULong.Value));
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }

        class Product
        {
            public bool? Bool { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
            public DateTime? DateTime { get; set; }
            public Date? Date { get; set; }
            public DateOnly? DateOnly { get; set; }
            public TimeSpan? TimeSpan { get; set; }
            public TimeOfDay? TimeOfDay { get; set; }
            public TimeOnly? TimeOnly { get; set; }
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
            public DateOnly DateOnly { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public TimeOfDay TimeOfDay { get; set; }
            public TimeOnly TimeOnly { get; set; }
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
