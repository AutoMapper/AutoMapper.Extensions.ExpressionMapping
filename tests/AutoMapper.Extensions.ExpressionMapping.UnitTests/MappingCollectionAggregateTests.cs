using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using AutoMapper.Internal;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests;

public class MappingCollectionAggregateTests : AutoMapperSpecBase
{
    private IQueryable<Source> _sources;

    public class Source
    {
        public ICollection<Item> Items { get; set; } = new List<Item> { new Item { Timestamp = DateTime.Now } };
    }

    public class Item
    {
        public DateTime Timestamp { get; set; }
    }

    public class Dest
    {
        public DateTime? ItemsTimestampMax { get; set; }
    }

    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForMember(dst => dst.ItemsTimestampMax, opt => opt.MapFrom(src => src.Items.Max(x => x.Timestamp)));

        cfg.Internal().ForAllPropertyMaps(
            p => p.SourceType == typeof(DateTimeOffset) && p.DestinationType == typeof(DateTimeOffset?) &&
                p.CustomMapExpression != null, (pMap, _) =>
            {
                var resolver = new ExpressionResolver(Expression.Lambda(
                    Expression.Convert(pMap.CustomMapExpression.Body, typeof(DateTimeOffset?)),
                    pMap.CustomMapExpression.Parameters));

                pMap.SetResolver(resolver);
            });
    });

    protected override void Because_of()
    {
        _sources = new[] { new Source() }.AsQueryable();
    }

    [Fact]
    public void Maps_Date_Filter()
    {
        _sources.UseAsDataSource(Configuration).For<Dest>()
            .Where(d => d.ItemsTimestampMax > DateTime.Today).ToList();
    }

    [Fact]
    public void Maps_Date_Value_Filter()
    {
        _sources.UseAsDataSource(Configuration).For<Dest>()
            .Where(d => d.ItemsTimestampMax.Value.Date == DateTime.Today).ToList();
    }
}