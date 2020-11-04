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
        [Fact]
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
        public void When_Apply_Where_Clause_Over_Queryable_As_Data_Source()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<Model> {new Model {Value = 1}, new Model {Value = 2}};

            var queryable = models.AsQueryable();

            Expression<Func<DTO, bool>> dtoPropertyFilter = (dto) => dto.Value == 1;

            // Act
            var result = queryable.UseAsDataSource(mapper).For<DTO>().Where(dtoPropertyFilter).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
        }

        [Fact]
        public void When_Apply_Where_Clause_Over_Queryable_As_Data_Source_DateTimeOffset()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<Model>
            {
                new Model {Value = 1, Timestamp = DateTimeOffset.Now},
                new Model {Value = 2, Timestamp = DateTimeOffset.Now.AddDays(1)}
            };

            var queryable = models.AsQueryable();

            Expression<Func<DTO, bool>> dtoPropertyFilter = (dto) => dto.Timestamp > DateTimeOffset.Now;

            // Act
            var result = queryable.UseAsDataSource(mapper).For<DTO>().Where(dtoPropertyFilter).ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
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
            public DateTimeOffset Timestamp { get; set; }
            public int Value { get; set; }
        }

        private class DTO
        {
            public DateTimeOffset? Timestamp { get; set; }
            public int? Value { get; set; }
        }

    }
}