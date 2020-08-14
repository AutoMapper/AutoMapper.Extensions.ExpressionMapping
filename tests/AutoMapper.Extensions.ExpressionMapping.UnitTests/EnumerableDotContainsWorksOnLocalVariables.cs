using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class EnumerableDotContainsWorksOnLocalVariables
    {
        [Fact]
        public void Issue87()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Source, SourceDto>()
                    .ForMember(o => o.Items, config => config.MapFrom(p => p.Items.Select(s => s.Name)));
            });

            var mapper = config.CreateMapper();

            var items = new string[] { "item1", "item2" };
            Expression<Func<SourceDto, bool>> expression1 = o => items.Contains("");
            Expression<Func<SourceDto, bool>> expression2 = o => o.Items.Contains("");

            var mapped1 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression1);
            var mapped2 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression2);

            Assert.NotNull(mapped1);
            Assert.NotNull(mapped2);
        }

        public class Source { public ICollection<SubSource> Items { get; set; } }

        public class SubSource { public int ID { get; set; } public string Name { get; set; } }

        public class SourceDto { public string[] Items { get; set; } }
    }
}
