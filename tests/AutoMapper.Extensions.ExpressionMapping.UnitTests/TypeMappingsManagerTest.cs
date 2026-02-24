using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class TypeMappingsManagerTest
    {
        #region Test Models

        private class SourceModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public SourceChild Child { get; set; }
        }

        private class DestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DestChild Child { get; set; }
        }

        private class SourceChild
        {
            public string Value { get; set; }
        }

        private class DestChild
        {
            public string Value { get; set; }
        }

        private class SourceItem
        {
            public int ItemId { get; set; }
        }

        private class DestItem
        {
            public int ItemId { get; set; }
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ValidDelegateTypes_CreatesInstance()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Assert
            Assert.NotNull(manager);
            Assert.NotNull(manager.InfoDictionary);
            Assert.NotNull(manager.TypeMappings);
        }

        [Fact]
        public void Constructor_NonDelegateSourceType_ThrowsArgumentException()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                new TypeMappingsManager(config, typeof(int), typeof(Func<DestModel, bool>)));

            Assert.Contains("must be a delegate type", exception.Message);
        }

        [Fact]
        public void Constructor_NonDelegateDestType_ThrowsArgumentException()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                new TypeMappingsManager(config, typeof(Func<SourceModel, bool>), typeof(string)));

            Assert.Contains("must be a delegate type", exception.Message);
        }

        [Fact]
        public void Constructor_AddsMappingsFromDelegateTypes()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceModel), typeof(DestModel)), manager.TypeMappings);
        }

        #endregion

        #region AddTypeMapping Tests

        [Fact]
        public void AddTypeMapping_SimpleTypes_AddsMapping()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMapping(typeof(SourceChild), typeof(DestChild));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceChild), typeof(DestChild)), manager.TypeMappings);
        }

        [Fact]
        public void AddTypeMapping_SameTypes_DoesNotAddMapping()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            var initialCount = manager.TypeMappings.Count;

            // Act
            manager.AddTypeMapping(typeof(int), typeof(int));

            // Assert
            Assert.Equal(initialCount, manager.TypeMappings.Count);
        }

        [Fact]
        public void AddTypeMapping_ExpressionTypes_UnwrapsAndAddsMapping()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMapping(
                typeof(Expression<Func<SourceChild, bool>>),
                typeof(Expression<Func<DestChild, bool>>));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(Func<SourceChild, bool>), typeof(Func<DestChild, bool>)), manager.TypeMappings);
        }

        [Fact]
        public void AddTypeMapping_DuplicateMapping_DoesNotAddAgain()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMapping(typeof(SourceChild), typeof(DestChild));
            var countAfterFirst = manager.TypeMappings.Count;
            manager.AddTypeMapping(typeof(SourceChild), typeof(DestChild));

            // Assert
            Assert.Equal(countAfterFirst, manager.TypeMappings.Count);
        }

        [Fact]
        public void AddTypeMapping_ExpressionSourceNonExpressionDest_ThrowsArgumentException()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manager.AddTypeMapping(
                    typeof(Expression<Func<SourceChild, bool>>),
                    typeof(Func<DestChild, bool>)));

            Assert.Contains("Invalid type mappings", exception.Message);
        }

        [Fact]
        public void AddTypeMapping_ListTypes_AddsUnderlyingTypeMappings()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMapping(typeof(List<SourceItem>), typeof(List<DestItem>));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceItem), typeof(DestItem)), manager.TypeMappings);
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(List<SourceItem>), typeof(List<DestItem>)), manager.TypeMappings);
        }

        [Fact]
        public void AddTypeMapping_ArrayTypes_AddsUnderlyingTypeMappings()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMapping(typeof(SourceItem[]), typeof(DestItem[]));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceItem), typeof(DestItem)), manager.TypeMappings);
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceItem[]), typeof(DestItem[])), manager.TypeMappings);
        }

        #endregion

        #region ReplaceType Tests

        [Fact]
        public void ReplaceType_MappedType_ReturnsDestinationType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            var result = manager.ReplaceType(typeof(SourceModel));

            // Assert
            Assert.Equal(typeof(DestModel), result);
        }

        [Fact]
        public void ReplaceType_UnmappedType_ReturnsOriginalType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            var result = manager.ReplaceType(typeof(string));

            // Assert
            Assert.Equal(typeof(string), result);
        }

        [Fact]
        public void ReplaceType_ArrayTypeWithMapping_ReturnsReplacedArrayType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(SourceItem), typeof(DestItem));

            // Act
            var result = manager.ReplaceType(typeof(SourceItem[]));

            // Assert
            Assert.Equal(typeof(DestItem[]), result);
        }

        [Fact]
        public void ReplaceType_MultiDimensionalArrayWithMapping_ReturnsReplacedArrayType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(SourceItem), typeof(DestItem));

            // Act
            var result = manager.ReplaceType(typeof(SourceItem[,]));

            // Assert
            Assert.Equal(typeof(DestItem[,]), result);
            Assert.Equal(2, result.GetArrayRank());
        }

        [Fact]
        public void ReplaceType_GenericTypeWithMapping_ReturnsReplacedGenericType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(SourceItem), typeof(DestItem));

            // Act
            var result = manager.ReplaceType(typeof(List<SourceItem>));

            // Assert
            Assert.Equal(typeof(List<DestItem>), result);
        }

        [Fact]
        public void ReplaceType_GenericTypeWithMultipleArguments_ReplacesAllArguments()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceItem, DestItem>();
                cfg.CreateMap<SourceChild, DestChild>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(SourceItem), typeof(DestItem));
            manager.AddTypeMapping(typeof(SourceChild), typeof(DestChild));

            // Act
            var result = manager.ReplaceType(typeof(Dictionary<SourceItem, SourceChild>));

            // Assert
            Assert.Equal(typeof(Dictionary<DestItem, DestChild>), result);
        }

        [Fact]
        public void ReplaceType_DirectlyMappedArrayType_ReturnsDirectMapping()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(SourceItem[]), typeof(List<DestItem>));

            // Act
            var result = manager.ReplaceType(typeof(SourceItem[]));

            // Assert
            Assert.Equal(typeof(List<DestItem>), result);
        }

        [Fact]
        public void ReplaceType_DirectlyMappedGenericType_ReturnsDirectMapping()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));
            manager.AddTypeMapping(typeof(List<SourceItem>), typeof(HashSet<DestItem>));

            // Act
            var result = manager.ReplaceType(typeof(List<SourceItem>));

            // Assert
            Assert.Equal(typeof(HashSet<DestItem>), result);
        }

        #endregion

        #region GetDestinationParameterExpressions Tests

        [Fact]
        public void GetDestinationParameterExpressions_SingleParameter_ReturnsDestinationParameter()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            Expression<Func<SourceModel, bool>> expression = s => s.Id > 0;

            // Act
            var result = manager.GetDestinationParameterExpressions(expression);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(DestModel), result[0].Type);
        }

        [Fact]
        public void GetDestinationParameterExpressions_MultipleParameters_ReturnsAllDestinationParameters()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceChild, DestChild>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, SourceChild, bool>),
                typeof(Func<DestModel, DestChild, bool>));

            var param1 = Expression.Parameter(typeof(SourceModel), "s");
            var param2 = Expression.Parameter(typeof(SourceChild), "c");
            var body = Expression.Constant(true);
            var expression = Expression.Lambda<Func<SourceModel, SourceChild, bool>>(
                body,
                param1,
                param2);

            // Act
            var result = manager.GetDestinationParameterExpressions(expression);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(typeof(DestModel), result[0].Type);
            Assert.Equal(typeof(DestChild), result[1].Type);
        }

        [Fact]
        public void GetDestinationParameterExpressions_UnmappedParameter_ReturnsOriginalParameterType()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            Expression<Func<int, bool>> expression = x => x > 0;

            // Act
            var result = manager.GetDestinationParameterExpressions(expression);

            // Assert
            Assert.Single(result);
            Assert.Equal(typeof(int), result[0].Type);
        }

        [Fact]
        public void GetDestinationParameterExpressions_CalledTwiceWithSameParameter_ReturnsSameInstance()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            var param = Expression.Parameter(typeof(SourceModel), "s");
            var body = Expression.Constant(true);
            var expression = Expression.Lambda<Func<SourceModel, bool>>(body, param);

            // Act
            var result1 = manager.GetDestinationParameterExpressions(expression);
            var result2 = manager.GetDestinationParameterExpressions(expression);

            // Assert
            Assert.Same(result1[0], result2[0]);
        }

        [Fact]
        public void GetDestinationParameterExpressions_PreservesParameterName()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            Expression<Func<SourceModel, bool>> expression = myCustomName => myCustomName.Id > 0;

            // Act
            var result = manager.GetDestinationParameterExpressions(expression);

            // Assert
            Assert.Single(result);
            Assert.Equal("myCustomName", result[0].Name);
        }

        #endregion

        #region AddTypeMappingsFromDelegates Tests

        [Fact]
        public void AddTypeMappingsFromDelegates_MatchingArgumentCounts_AddsMappings()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceChild, DestChild>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMappingsFromDelegates(
                typeof(Func<SourceChild, string>),
                typeof(Func<DestChild, string>));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceChild), typeof(DestChild)), manager.TypeMappings);
        }

        [Fact]
        public void AddTypeMappingsFromDelegates_MismatchedArgumentCounts_ThrowsArgumentException()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                manager.AddTypeMappingsFromDelegates(
                    typeof(Func<SourceModel, bool>),
                    typeof(Func<DestModel, int, bool>)));
        }

        [Fact]
        public void AddTypeMappingsFromDelegates_ActionTypes_AddsMappings()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
                cfg.CreateMap<SourceChild, DestChild>();
            });
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Act
            manager.AddTypeMappingsFromDelegates(
                typeof(Action<SourceChild>),
                typeof(Action<DestChild>));

            // Assert
            Assert.Contains(new KeyValuePair<Type, Type>(typeof(SourceChild), typeof(DestChild)), manager.TypeMappings);
        }

        #endregion

        #region InfoDictionary Tests

        [Fact]
        public void InfoDictionary_IsInitialized()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Assert
            Assert.NotNull(manager.InfoDictionary);
            Assert.IsType<MapperInfoDictionary>(manager.InfoDictionary);
        }

        #endregion

        #region TypeMappings Tests

        [Fact]
        public void TypeMappings_IsInitialized()
        {
            // Arrange
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceModel, DestModel>();
            });

            // Act
            var manager = new TypeMappingsManager(
                config,
                typeof(Func<SourceModel, bool>),
                typeof(Func<DestModel, bool>));

            // Assert
            Assert.NotNull(manager.TypeMappings);
            Assert.IsType<Dictionary<Type, Type>>(manager.TypeMappings);
        }

        #endregion
    }
}