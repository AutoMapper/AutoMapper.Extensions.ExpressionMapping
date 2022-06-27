using System;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class MemberMappingsOfLiteralParentTypesMustMatch
    {
        [Fact]
        public void MappingMemberOfNullableParentToMemberOfNonNullableParentWithoutCustomExpressionsThrowsException()
        {
            //arrange
            var mapper = GetMapper();
            Expression<Func<ProductModel, bool>> expression = x => x.DateTimeOffset.Value.Day.ToString() == "2";

            //act
            var exception = Assert.Throws<InvalidOperationException>(() => mapper.MapExpression<Expression<Func<Product, bool>>>(expression));

            //assert
            Assert.StartsWith
            (
                "For members of literal types, use IMappingExpression.ForMember() to make the parent property types an exact match.",
                exception.Message
            );
        }

        [Fact]
        public void MappingMemberOfNonNullableParentToMemberOfNullableParentWithoutCustomExpressionsThrowsException()
        {
            //arrange
            var mapper = GetMapper();
            Expression<Func<ProductModel, bool>> expression = x => x.DateTime.Day.ToString() == "2";

            //act
            var exception = Assert.Throws<InvalidOperationException>(() => mapper.MapExpression<Expression<Func<Product, bool>>>(expression));

            //assert
            Assert.StartsWith
            (
                "For members of literal types, use IMappingExpression.ForMember() to make the parent property types an exact match.",
                exception.Message
            );
        }

        [Fact]
        public void MappingMemberOfNullableParentToMemberOfNonNullableParentWorksUsingCustomExpressions()
        {
            //arrange
            var mapper = GetMapperWithCustomExpressions();
            Expression<Func<ProductModel, bool>> expression = x => x.DateTimeOffset.Value.Day.ToString() == "2";

            //act
            var mappedExpression = mapper.MapExpression<Expression<Func<Product, bool>>>(expression);

            //assert
            Assert.NotNull(mappedExpression);
            Func<Product, bool> func = mappedExpression.Compile();
            Assert.False(func(new Product { DateTimeOffset = new DateTimeOffset(new DateTime(2000, 3, 3), TimeSpan.Zero) }));
            Assert.True(func(new Product { DateTimeOffset = new DateTimeOffset(new DateTime(2000, 2, 2), TimeSpan.Zero) }));
        }

        [Fact]
        public void MappingMemberOfNonNullableParentToMemberOfNullableParentWorksUsingCustomExpressions()
        {
            //arrange
            var mapper = GetMapperWithCustomExpressions();
            Expression<Func<ProductModel, bool>> expression = x => x.DateTime.Day.ToString() == "2";

            //act
            var mappedExpression = mapper.MapExpression<Expression<Func<Product, bool>>>(expression);

            //assert
            Assert.NotNull(mappedExpression);
            Func<Product, bool> func = mappedExpression.Compile();
            Assert.False(func(new Product { DateTime = new DateTime(2000, 3, 3) }));
            Assert.True(func(new Product { DateTime = new DateTime(2000, 2, 2) }));
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
                    .ForMember(d => d.DateTime, o => o.MapFrom(s => s.DateTime.Value))
                    .ForMember(d => d.DateTimeOffset, o => o.MapFrom(s => (DateTimeOffset?)s.DateTimeOffset));
            });
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }

        class Product
        {
            public DateTime? DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
        }

        class ProductModel
        {
            public DateTime DateTime { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
        }
    }
}
