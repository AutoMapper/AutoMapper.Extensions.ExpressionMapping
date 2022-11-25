using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapParameterBodyFromChildReferenceWithoutMemberExpression
    {
        [Fact]
        public void Can_map_parameter_body_from_child_reference_without_member_expression()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<TestCategory, TestProductDTO>()
                   .ForMember(p => p.Brand, c => c.MapFrom(p => EF.Property<int>(p, "BrandId"))); ;

                c.CreateMap<TestProduct, TestProductDTO>()
                  .IncludeMembers(p => p.Category);
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
            Assert.Equal("x => (Property(x.Category, \"BrandId\") == 2)", mappedExpression.ToString());
        }

        public class TestCategory
        {
            // Has FK BrandId
        }
        public class TestProduct
        {
            public TestCategory? Category { get; set; }
        }

        public class TestProductDTO
        {
            public int Brand { get; set; }
        }
    }
}
