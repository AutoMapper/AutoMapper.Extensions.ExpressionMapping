using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ShouldOnlyMapExistingTypeMaps
    {
        [Fact]
        public void Issue85()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Source, SourceDto>()
                    .ForMember(o => o.Items, config => config.MapFrom(p => p.Items.Select(s => s.Name)));

                cfg.CreateMap<SourceDto, Source>()
                    .ForMember(o => o.Items, config => config.MapFrom(p => p.Items.Select(s => new SubSource { Name = s })));
            });

            var mapper = config.CreateMapper();

            Expression<Func<Source, bool>> expression1 = o => string.Equals("item1", "item2");
            var mapped1 = mapper.MapExpression<Expression<Func<SourceDto, bool>>>(expression1);

            Expression<Func<SourceDto, bool>> expression2 = o => string.Equals("item1", "item2");
            var mapped2 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression2);

            Assert.NotNull(mapped1);
            Assert.NotNull(mapped2);
        }

        [Fact]
        public void Issue93()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Source, SourceDto>()
                    .ForMember(o => o.Items, config => config.MapFrom(p => p.Items.Select(s => s.Name)));
            });

            var mapper = config.CreateMapper();

            Expression<Func<SourceDto, bool>> expression1 =
                src => ((src != null ? src : null) != null) && src.Items.Any(x => x == "item1");
 
            var mapped1 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression1);

            Assert.NotNull(mapped1);
        }

        public class Source { public ICollection<SubSource> Items { get; set; } }

        public class SubSource { public int ID { get; set; } public string Name { get; set; } }

        public class SourceDto { public string[] Items { get; set; } }
    }
}
