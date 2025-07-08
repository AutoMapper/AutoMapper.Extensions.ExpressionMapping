using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ShouldUseDeclaringTypeForInstanceMethodCalls
    {
        [Fact]
        public void MethodInfoShouldRetainDeclaringTypeInMappedExpression()
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
            Expression<Func<Entity, bool>> filter = e => e.SimpleEnum.HasFlag(SimpleEnum.Value3);
            EntityModel entityModel1 = new() { SimpleEnum = SimpleEnumModel.Value3 };
            EntityModel entityModel2 = new() { SimpleEnum = SimpleEnumModel.Value2 };

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.Equal(typeof(Enum), HasFlagVisitor.GetasFlagReflectedType(mappedFilter));
            Assert.Single(new List<EntityModel> { entityModel1 }.AsQueryable().Where(mappedFilter));
            Assert.Empty(new List<EntityModel> { entityModel2 }.AsQueryable().Where(mappedFilter));
        }

        public enum SimpleEnum
        {
            Value1,
            Value2,
            Value3
        }

        public record Entity
        {
            public int Id { get; init; }
            public SimpleEnum SimpleEnum { get; init; }
        }

        public enum SimpleEnumModel
        {
            Value1,
            Value2,
            Value3
        }

        public record EntityModel
        {
            public int Id { get; init; }
            public SimpleEnumModel SimpleEnum { get; init; }
        }

        public class HasFlagVisitor : ExpressionVisitor
        {
            public static Type GetasFlagReflectedType(Expression expression)
            {
                HasFlagVisitor hasFlagVisitor = new();
                hasFlagVisitor.Visit(expression);
                return hasFlagVisitor.HasFlagReflectedType;
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "HasFlag")
                    HasFlagReflectedType = node.Method.ReflectedType;

                return base.VisitMethodCall(node);
            }

            public Type HasFlagReflectedType { get; private set; }
        }
    }
}
