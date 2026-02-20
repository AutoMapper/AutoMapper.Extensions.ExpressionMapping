using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class ConfigurationHelper
    {
        public static MapperConfiguration GetMapperConfiguration(Action<IMapperConfigurationExpression> configure)
        {
            return new MapperConfiguration(configure, NullLoggerFactory.Instance);
        }

        public static MapperConfiguration GetMapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            return new MapperConfiguration(configurationExpression, NullLoggerFactory.Instance);
        }
    }
}
