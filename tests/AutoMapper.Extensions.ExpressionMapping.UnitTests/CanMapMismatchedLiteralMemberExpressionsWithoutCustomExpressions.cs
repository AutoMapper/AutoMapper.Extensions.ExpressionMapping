using Microsoft.OData.Edm;
using System;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapMismatchedLiteralMemberExpressionsWithoutCustomExpressions
    {
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
        public void CanMapNonNullableToNullableWithoutCustomExpression(string memberName, Type constantType)
        {
            //arrange
            var mapper = GetDataToModelMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(ProductModel), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(ProductModel), memberName));
            object constantValue = Activator.CreateInstance(constantType);
            Expression<Func<ProductModel, bool>> expression = Expression.Lambda<Func<ProductModel, bool>>
            (
                Expression.Equal
                (
                    property,
                    Expression.Constant(constantValue, constantType)
                ),
                productParam
            );
            Product product = new();
            typeof(Product).GetProperty(memberName).SetValue(product, constantValue);

            //act
            var mappedExpression = mapper.MapExpression<Expression<Func<Product, bool>>>(expression);

            //assert
            Assert.True(mappedExpression.Compile()(product));
        }

        [Theory]
        [InlineData(nameof(Product.Bool), typeof(bool?))]
        [InlineData(nameof(Product.DateTime), typeof(DateTime?))]
        [InlineData(nameof(Product.DateTimeOffset), typeof(DateTimeOffset?))]
        [InlineData(nameof(Product.Date), typeof(Date?))]
        [InlineData(nameof(Product.DateOnly), typeof(DateOnly?))]
        [InlineData(nameof(Product.TimeSpan), typeof(TimeSpan?))]
        [InlineData(nameof(Product.TimeOfDay), typeof(TimeOfDay?))]
        [InlineData(nameof(Product.TimeOnly), typeof(TimeOnly?))]
        [InlineData(nameof(Product.Guid), typeof(Guid?))]
        [InlineData(nameof(Product.Decimal), typeof(decimal?))]
        [InlineData(nameof(Product.Byte), typeof(byte?))]
        [InlineData(nameof(Product.Short), typeof(short?))]
        [InlineData(nameof(Product.Int), typeof(int?))]
        [InlineData(nameof(Product.Long), typeof(long?))]
        [InlineData(nameof(Product.Float), typeof(float?))]
        [InlineData(nameof(Product.Double), typeof(double?))]
        [InlineData(nameof(Product.Char), typeof(char?))]
        [InlineData(nameof(Product.SByte), typeof(sbyte?))]
        [InlineData(nameof(Product.UShort), typeof(ushort?))]
        [InlineData(nameof(Product.ULong), typeof(ulong?))]
        public void CanMapNullableToNonNullableWithoutCustomExpression(string memberName, Type constantType)
        {
            //arrange
            var mapper = GetModelToDataMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(Product), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(Product), memberName));
            object constantValue = Activator.CreateInstance(Nullable.GetUnderlyingType(constantType));
            Expression<Func<Product, bool>> expression = Expression.Lambda<Func<Product, bool>>
            (
                Expression.Equal
                (
                    property,
                    Expression.Constant(constantValue, constantType)
                ),
                productParam
            );
            ProductModel product = new();
            typeof(ProductModel).GetProperty(memberName).SetValue(product, constantValue);

            //act
            var mappedExpression = mapper.MapExpression<Expression<Func<ProductModel, bool>>>(expression);

            //assert
            Assert.True(mappedExpression.Compile()(product));
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
        public void CanMapNonNullableSelectorToNullableelectorWithoutCustomExpression(string memberName, Type memberType)
        {
            var mapper = GetDataToModelMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(ProductModel), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(ProductModel), memberName));
            Type sourceType = typeof(Func<,>).MakeGenericType(typeof(ProductModel), memberType);
            Type destType = typeof(Func<,>).MakeGenericType(typeof(Product), memberType);
            Type sourceExpressionype = typeof(Expression<>).MakeGenericType(sourceType);
            Type destExpressionType = typeof(Expression<>).MakeGenericType(destType);
            var expression = Expression.Lambda
            (
                sourceType,
                property,
                productParam
            );
            object constantValue = Activator.CreateInstance(memberType);
            Product product = new();
            typeof(Product).GetProperty(memberName).SetValue(product, constantValue);

            //act
            var mappedExpression = mapper.MapExpression(expression, sourceExpressionype, destExpressionType);

            //assert
            Assert.Equal(constantValue, mappedExpression.Compile().DynamicInvoke(product));
        }

        [Theory]
        [InlineData(nameof(Product.Bool), typeof(bool?))]
        [InlineData(nameof(Product.DateTime), typeof(DateTime?))]
        [InlineData(nameof(Product.DateTimeOffset), typeof(DateTimeOffset?))]
        [InlineData(nameof(Product.Date), typeof(Date?))]
        [InlineData(nameof(Product.DateOnly), typeof(DateOnly?))]
        [InlineData(nameof(Product.TimeSpan), typeof(TimeSpan?))]
        [InlineData(nameof(Product.TimeOfDay), typeof(TimeOfDay?))]
        [InlineData(nameof(Product.TimeOnly), typeof(TimeOnly?))]
        [InlineData(nameof(Product.Guid), typeof(Guid?))]
        [InlineData(nameof(Product.Decimal), typeof(decimal?))]
        [InlineData(nameof(Product.Byte), typeof(byte?))]
        [InlineData(nameof(Product.Short), typeof(short?))]
        [InlineData(nameof(Product.Int), typeof(int?))]
        [InlineData(nameof(Product.Long), typeof(long?))]
        [InlineData(nameof(Product.Float), typeof(float?))]
        [InlineData(nameof(Product.Double), typeof(double?))]
        [InlineData(nameof(Product.Char), typeof(char?))]
        [InlineData(nameof(Product.SByte), typeof(sbyte?))]
        [InlineData(nameof(Product.UShort), typeof(ushort?))]
        [InlineData(nameof(Product.ULong), typeof(ulong?))]
        public void CanMapNullableSelectorToNonNullableelectorWithoutCustomExpression(string memberName, Type memberType)
        {
            var mapper = GetModelToDataMapper();
            ParameterExpression productParam = Expression.Parameter(typeof(Product), "x");
            MemberExpression property = Expression.MakeMemberAccess(productParam, AutoMapper.Internal.TypeExtensions.GetFieldOrProperty(typeof(Product), memberName));
            Type sourceType = typeof(Func<,>).MakeGenericType(typeof(Product), memberType);
            Type destType = typeof(Func<,>).MakeGenericType(typeof(ProductModel), memberType);
            Type sourceExpressionype = typeof(Expression<>).MakeGenericType(sourceType);
            Type destExpressionType = typeof(Expression<>).MakeGenericType(destType);
            var expression = Expression.Lambda
            (
                sourceType,
                property,
                productParam
            );

            object constantValue = Activator.CreateInstance(Nullable.GetUnderlyingType(memberType));
            ProductModel product = new();
            typeof(ProductModel).GetProperty(memberName).SetValue(product, constantValue);

            //act
            var mappedExpression = mapper.MapExpression(expression, sourceExpressionype, destExpressionType);

            //assert
            Assert.Equal(constantValue, mappedExpression.Compile().DynamicInvoke(product));
        }

        private static IMapper GetModelToDataMapper()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ProductModel, Product>();
            });
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }

        private static IMapper GetDataToModelMapper()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<Product, ProductModel>();
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
