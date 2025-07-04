
using System;
using System.Linq;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ExpressionConversion
    {
        public class Source
        {
            public int Value { get; set; }
            public int Foo { get; set; }
            public ChildSrc Child { get; set; }
        }

        public class ChildSrc
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
            public int Bar { get; set; }
            public int ChildValue { get; set; }
        }

        public enum SourceEnum
        {
            Foo,
            Bar
        }

        public enum DestEnum
        {
            Foo,
            Bar
        }

        public class SourceWithEnum : Source
        {
            public SourceEnum Enum { get; set; }
        }

        public class DestWithEnum : Dest
        {
            public DestEnum Enum { get; set; }
        }

        [Fact]
        public void Can_map_unary_expression_converting_enum_to_int()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<SourceEnum, DestEnum>();
                cfg.CreateMap<DestWithEnum, SourceWithEnum>();
            });

            Expression<Func<SourceWithEnum, bool>> expr = s => s.Enum == SourceEnum.Bar;

            var mapped = config.CreateMapper().MapExpression<Expression<Func<SourceWithEnum, bool>>, Expression<Func<DestWithEnum, bool>>>(expr);

            var items = new[]
           {
                new DestWithEnum {Enum = DestEnum.Foo},
                new DestWithEnum {Enum = DestEnum.Bar},
                new DestWithEnum {Enum = DestEnum.Bar}
            };

            var items2 = items.AsQueryable().Select(mapped).ToList();
        }

        [Fact]
        public void Can_map_single_properties()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>();
            });

            Expression<Func<Dest, bool>> expr = d => d.Value == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Value = 10},
                new Source {Value = 10},
                new Source {Value = 15}
            };

            items.AsQueryable().Where(mapped).Count().ShouldBe(2);
        }

        [Fact]
        public void Can_map_flattened_properties()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>();
            });

            Expression<Func<Dest, bool>> expr = d => d.ChildValue == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Child = new ChildSrc {Value = 10}},
                new Source {Child = new ChildSrc {Value = 10}},
                new Source {Child = new ChildSrc {Value = 15}}
            };

            items.AsQueryable().Where(mapped).Count().ShouldBe(2);
        }

        [Fact]
        public void Can_map_custom_mapped_properties()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>().ForMember(d => d.Bar, opt => opt.MapFrom(src => src.Foo));
            });

            Expression<Func<Dest, bool>> expr = d => d.Bar == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Foo = 10},
                new Source {Foo = 10},
                new Source {Foo = 15}
            };

            items.AsQueryable().Where(mapped).Count().ShouldBe(2);
        }

        [Fact]
        public void Throw_AutoMapperMappingException_if_expression_types_dont_match()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>();
            });

            Expression<Func<Dest, bool>> expr = d => d.Bar == 10;

            Assert.Throws<AutoMapperMappingException>(() => config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Action<Source, bool>>>(expr));
        }

        [Fact]
        public void Can_map_with_different_destination_types()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>().ForMember(d => d.Bar, opt => opt.MapFrom(src => src.Foo));
            });

            Expression<Func<Dest, Dest>> expr = d => d;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, Dest>>, Expression<Func<Source, Source>>>(expr);

            var items = new[]
            {
                new Source {Foo = 10},
                new Source {Foo = 10},
                new Source {Foo = 15}
            };

            var items2 = items.AsQueryable().Select(mapped).ToList();
        }
    }
}