using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapExpressionWithLocalExpressionConstant
    {
        [Fact]
        public void Map_expression_wchich_includes_local_constant()
        {
            //Arrange
            var config = ConfigurationHelper.GetMapperConfiguration
            (
                cfg =>
                {
                    cfg.CreateMap<EntityModel, Entity>();
                    cfg.CreateMap<Entity, EntityModel>();
                }
            );
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            List<Entity> source = [
                new Entity { Id = 1 },
                new Entity { Id = 3 }
            ];

            //act
            Expression<Func<EntityModel, bool>> filter = f => f.Id > 2;
            Expression<Func<IQueryable<EntityModel>, IQueryable<EntityModel>>> queryableExpression = q => q.Where(filter);
            Expression<Func<IQueryable<Entity>, IQueryable<Entity>>> queryableExpressionMapped = mapper.MapExpression<Expression<Func<IQueryable<Entity>, IQueryable<Entity>>>>(queryableExpression);

            //assert
            Assert.Equal(1, queryableExpressionMapped.Compile()(source.AsQueryable()).Count());
        }

        public record Entity
        {
            public int Id { get; init; }
        }

        public record EntityModel
        {
            public int Id { get; init; }
        }
    }
}
