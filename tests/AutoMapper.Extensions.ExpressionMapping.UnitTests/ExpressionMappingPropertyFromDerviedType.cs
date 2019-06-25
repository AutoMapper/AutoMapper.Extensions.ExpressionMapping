using Shouldly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ExpressionMappingPropertyFromDerviedType : NonValidatingSpecBase
    {
        private List<BaseEntity> _source;

        public class BaseDTO
        {
            public Guid Id { get; set; }
        }

        public class BaseEntity
        {
            public Guid Id { get; set; }
        }

        public class DTO : BaseDTO
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Entity : BaseEntity
        {
            public string Name { get; set; }
            public string Desc { get; set; }
        }

        protected override MapperConfiguration Configuration =>
            new MapperConfiguration(cfg =>
                {
                    cfg.AddExpressionMapping();

                    cfg.CreateMap<BaseEntity, BaseDTO>()
                        .Include<Entity, DTO>()
                        .ReverseMap();

                    cfg.CreateMap<Entity, DTO>()
                        .IncludeBase<BaseEntity, BaseDTO>()
                        .ForMember(dto => dto.Description, opt => opt.MapFrom(entity => entity.Desc))
                        .ReverseMap();
                });

        protected override void Because_of()
        {
            //Arrange
            _source = new List<BaseEntity> {
                new Entity { Id = Guid.NewGuid(), Name = "Sofia", Desc = "Rafael's Spouse" },
                new Entity { Id = Guid.NewGuid(), Name = "Rafael", Desc = "Sofia's Spouse" },
                new BaseEntity { Id = Guid.NewGuid() }
            };

            var typeMap = new Dictionary<Type, Type>().AddTypeMapping<BaseDTO, BaseEntity>(this.ConfigProvider);
            var expressionVisitor = new XpressionMapperVisitor(this.Mapper, this.ConfigProvider, typeMap);
        }

        [Fact]
        public void Should_support_propertypath_expressions_with_properties_from_derived_types()
        {
            var parameterContainer = new
            {
                P1 = "Sofia"
            };

            Expression<Func<BaseDTO, bool>> filter = r => (r as DTO == null ? null : (r as DTO).Name) == parameterContainer.P1;
            var mappedFilter = this.Mapper.MapExpression<Expression<Func<BaseEntity, bool>>>(filter);

            // Assert
            _source
                .AsQueryable()
                .Where(mappedFilter)
                .Count()
                .ShouldBe(1);
        }


        [Fact]
        public void Should_support_custom_expressions_with_properties_from_derived_types()
        {
            Expression<Func<BaseDTO, bool>> filter = r => (r is DTO ? ((DTO)r).Description : "").Contains("Sofia");
            var mappedFilter = this.Mapper.MapExpression<Expression<Func<BaseEntity, bool>>>(filter);

            // Assert
            _source
                .AsQueryable()
                .Where(mappedFilter)
                .Count()
                .ShouldBe(1);
        }
    }
}
