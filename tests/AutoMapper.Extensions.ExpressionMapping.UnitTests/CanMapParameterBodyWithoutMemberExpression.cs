using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapParameterBodyWithoutMemberExpression
    {
        [Fact]
        public void Can_map_parameter_body_without_member_expression()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<TestProduct, TestProductDTO>()
                  .ForMember(p => p.Brand, c => c.MapFrom(p => EF.Property<int>(p, "BrandId")));
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var products = new List<TestProduct>() {
                new TestProduct { }
              }.AsQueryable();

            //Act
            Expression<Func<TestProductDTO, bool>> expr = x => x.Brand == 2;
            var mappedExpression = mapper.MapExpression<Expression<Func<TestProduct, bool>>>(expr);

            //Assert
            Assert.Equal("x => (Property(x, \"BrandId\") == 2)", mappedExpression.ToString());
        }

        public class TestProduct
        {
            // Empty, has shadow key named BrandId
        }

        public class TestProductDTO
        {
            public int Brand { get; set; }
        }
    }
}
