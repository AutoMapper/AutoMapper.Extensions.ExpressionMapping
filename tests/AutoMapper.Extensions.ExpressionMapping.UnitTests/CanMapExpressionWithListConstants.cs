using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapExpressionWithListConstants
    {
        [Fact]
        public void Map_expression_with_constant_array()
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
            List<EntityModel> source1 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value3 }
            };
            List<EntityModel> source2 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value1 }
            };
            Entity[] entities = new Entity[]  { new Entity { SimpleEnum = SimpleEnum.Value1 }, new Entity { SimpleEnum = SimpleEnum.Value2 } };
            Expression<Func<Entity, bool>> filter = e => entities.Any(en => e.SimpleEnum == en.SimpleEnum);

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.False(source1.AsQueryable().Any(mappedFilter));
            Assert.True(source2.AsQueryable().Any(mappedFilter));
        }

        [Fact]
        public void Map_expression_with_constant_list_using_generic_list_dot_contains()
        {
            //Arrange
            var config = ConfigurationHelper.GetMapperConfiguration
            (
                cfg =>
                {
                    cfg.CreateMap<EntityModel, Entity>();
                }
            );
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            List<EntityModel> source1 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value3 }
            };
            List<EntityModel> source2 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value1 }
            };
            List<SimpleEnum> enums = new() { SimpleEnum.Value1, SimpleEnum.Value2 };
            Expression<Func<Entity, bool>> filter = e => enums.Contains(e.SimpleEnum);

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.False(source1.AsQueryable().Any(mappedFilter));
            Assert.True(source2.AsQueryable().Any(mappedFilter));
        }

        [Fact]
        public void Map_expression_with_constant_list_using_generic_enumerable_dot_contains()
        {
            //Arrange
            var config = ConfigurationHelper.GetMapperConfiguration
            (
                cfg =>
                {
                    cfg.CreateMap<EntityModel, Entity>();
                }
            );
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            List<EntityModel> source1 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value3 }
            };
            List<EntityModel> source2 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value1 }
            };
            List<SimpleEnum> enums = new() { SimpleEnum.Value1, SimpleEnum.Value2 };
            Expression<Func<Entity, bool>> filter = e => Enumerable.Contains(enums, e.SimpleEnum);

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.False(source1.AsQueryable().Any(mappedFilter));
            Assert.True(source2.AsQueryable().Any(mappedFilter));
        }

        [Fact]
        public void Map_expression_with_constant_dictionary()
        {
            //Arrange
            var config = ConfigurationHelper.GetMapperConfiguration
            (
                cfg =>
                {
                    cfg.CreateMap<EntityModel, Entity>();
                }
            );
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            List<EntityModel> source1 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value3 }
            };
            List<EntityModel> source2 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value1 }
            };
            Dictionary<string, SimpleEnum> enumDictionary = new() { ["A"] = SimpleEnum.Value1, ["B"] = SimpleEnum.Value2 };
            Expression<Func<Entity, bool>> filter = e => enumDictionary.Any(i => i.Value == e.SimpleEnum);

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.False(source1.AsQueryable().Any(mappedFilter));
            Assert.True(source2.AsQueryable().Any(mappedFilter));
        }

        [Fact]
        public void Map_expression_with_constant_dictionary_mapping_both_Key_and_value()
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
            List<EntityModel> source1 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value3 }
            };
            List<EntityModel> source2 = new() {
                new EntityModel { SimpleEnum = SimpleEnumModel.Value1 }
            };
            Dictionary<SimpleEnum, Entity> enumDictionary = new() { [SimpleEnum.Value1] = new Entity { SimpleEnum = SimpleEnum.Value1 }, [SimpleEnum.Value2] = new Entity { SimpleEnum = SimpleEnum.Value2 } };
            Expression<Func<Entity, bool>> filter = e => enumDictionary.Any(i => i.Key == e.SimpleEnum && i.Value.SimpleEnum == e.SimpleEnum);

            //act
            Expression<Func<EntityModel, bool>> mappedFilter = mapper.MapExpression<Expression<Func<EntityModel, bool>>>(filter);

            //assert
            Assert.False(source1.AsQueryable().Any(mappedFilter));
            Assert.True(source2.AsQueryable().Any(mappedFilter));
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
    }
}
