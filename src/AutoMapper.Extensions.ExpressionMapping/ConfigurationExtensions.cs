using AutoMapper.Mappers;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class ConfigurationExtensions
    {
        public static IMapperConfigurationExpression AddExpressionMapping(this IMapperConfigurationExpression config)
        {
            config.Mappers.Insert(0, new ExpressionMapper());
            return config;
        }
    }
}