using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class MappingToNullablePropertyUsingUseAsDataSource
    {
        [Fact(Skip = "Not implemented")]
        public void When_Apply_OrderBy_Clause_Over_Queryable_As_Data_Source()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<Model>()
            {
                new Model {Value = 1},
                new Model {Value = 2}
            };

            var queryable = models.AsQueryable();

            Expression<Func<DTO, int?>> dtoPropertySelector = (dto) => dto.Value;

            // Act
            var result = queryable.UseAsDataSource(mapper).For<DTO>().OrderBy(dtoPropertySelector).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
        }

        [Fact]
        public void Should_Create_Mapper()
        {
            CreateMapper();
        }

        private static IMapper CreateMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Model, DTO>();
            });

            var mapper = mapperConfig.CreateMapper();
            return mapper;
        }

        private class Model
        {
            public int Value { get; set; }
        }

        private class DTO
        {
            public int? Value { get; set; }
        }

    }
}