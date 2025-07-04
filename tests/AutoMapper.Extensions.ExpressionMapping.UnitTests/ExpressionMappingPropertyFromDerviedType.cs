using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ExpressionMappingPropertyFromDerviedType : AutoMapperSpecBase
    {
        private List<BaseEntity> _source;
        private IQueryable<BaseEntity> entityQuery;

        protected override MapperConfiguration Configuration
        {
            get
            {
                var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
                {
                    cfg.AddExpressionMapping();
                    cfg.AddProfile(typeof(DerivedTypeProfile));
                });
                return config;
            }
        }

        protected override void Because_of()
        {
            //Arrange
            _source = new List<BaseEntity> {
                new Entity { Id = Guid.NewGuid(), Name = "Sofia" },
                new Entity { Id = Guid.NewGuid(), Name = "Rafael" },
                new BaseEntity { Id = Guid.NewGuid() }
            };
        }

        [Fact]
        public void Should_support_propertypath_expressions_with_properties_from_sub_types_using_explicit_cast()
        {
            // Act
            Expression<Func<BaseDTO, bool>> dtoQueryExpression = r => (r is DTO ? ((DTO)r).Name : "") == "Sofia";
            Expression<Func<BaseEntity, bool>> entityQueryExpression = Mapper.MapExpression<Expression<Func<BaseEntity, bool>>>(dtoQueryExpression);
            entityQuery = _source.AsQueryable().Where(entityQueryExpression);

            // Assert
            entityQuery.ToList().Count().ShouldBe(1);
        }

        [Fact]
        public void Should_support_propertypath_expressions_with_properties_from_sub_types_using_as_keyword()
        {
            // Act
            Expression<Func<BaseDTO, bool>> dtoQueryExpression = r => (r is DTO ? (r as DTO).Name : "") == "Sofia";
            Expression<Func<BaseEntity, bool>> entityQueryExpression = Mapper.MapExpression<Expression<Func<BaseEntity, bool>>>(dtoQueryExpression);
            entityQuery = _source.AsQueryable().Where(entityQueryExpression);

            // Assert
            entityQuery.ToList().Count().ShouldBe(1);
        }

        public class DerivedTypeProfile : Profile
        {
            public DerivedTypeProfile()
            {
                CreateMap<BaseEntity, BaseDTO>();

                CreateMap<Entity, DTO>()
                    .ForMember(dest => dest.Description, opts => opts.MapFrom(src => string.Concat(src.Id.ToString(), " - ", src.Name)))
                .IncludeBase<BaseEntity, BaseDTO>();
            }
        }

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
        }
    }
}
