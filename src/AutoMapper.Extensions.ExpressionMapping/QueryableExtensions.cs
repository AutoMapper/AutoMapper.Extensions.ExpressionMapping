using System.Linq;
using AutoMapper.Extensions.ExpressionMapping.Impl;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class QueryableExtensions
    {
        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource, IConfigurationProvider config)
            => dataSource.UseAsDataSource(config.CreateMapper());

        public static IQueryDataSourceInjection<TSource> UseAsDataSource<TSource>(this IQueryable<TSource> dataSource, IMapper mapper)
            => new QueryDataSourceInjection<TSource>(dataSource, mapper);

    }
}