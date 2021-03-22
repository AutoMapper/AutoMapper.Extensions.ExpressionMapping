using AutoMapper.Internal;
using AutoMapper.Mappers;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class ConfigurationExtensions
    {
        public static IMapperConfigurationExpression AddExpressionMapping(this IMapperConfigurationExpression config)
        {
            config.Internal().Mappers.Insert(0, new ExpressionMapper());
            return config;
        }
    }
}