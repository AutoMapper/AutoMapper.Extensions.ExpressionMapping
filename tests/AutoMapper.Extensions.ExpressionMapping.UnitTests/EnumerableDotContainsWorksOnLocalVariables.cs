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
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Source, SourceDto>()
                    .ForMember(o => o.Items, config => config.MapFrom(p => p.Items.Select(s => s.Name)));
            });

            var mapper = config.CreateMapper();

            var items = new string[] { "item1", "item2" };
            Expression<Func<SourceDto, bool>> expression1 = o => items.Contains("item1");
            Expression<Func<SourceDto, bool>> expression2 = o => items.Contains("");
            Expression<Func<SourceDto, bool>> expression3 = o => o.Items.Contains("item1");
            Expression<Func<SourceDto, bool>> expression4 = o => o.Items.Contains("B");

            var mapped1 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression1);
            var mapped2 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression2);
            var mapped3 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression3);
            var mapped4 = mapper.MapExpression<Expression<Func<Source, bool>>>(expression4);

            Assert.Equal(1, new Source[] { new Source { } }.AsQueryable().Where(mapped1).Count());
            Assert.Equal(0, new Source[] { new Source { } }.AsQueryable().Where(mapped2).Count());
            Assert.Equal(1, new Source[] { new Source { Items = new List<SubSource> { new SubSource { Name = "item1" } } } }.AsQueryable().Where(mapped3).Count());
            Assert.Equal(0, new Source[] { new Source { Items = new List<SubSource> { new SubSource { Name = "" } } } }.AsQueryable().Where(mapped4).Count());
        }

        public class Source { public ICollection<SubSource> Items { get; set; } }

        public class SubSource { public int ID { get; set; } public string Name { get; set; } }

        public class SourceDto { public string[] Items { get; set; } }
    }
}
