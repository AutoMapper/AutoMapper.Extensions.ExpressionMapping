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
    public class ExpressionMappingPropertyFromDerviedType : AutoMapperSpecBase
    {
        private List<BaseEntity> _source;
        private IQueryable<BaseEntity> entityQuery;

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
        }

        public class Entity : BaseEntity
        {
            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddExpressionMapping();

                    cfg.CreateMap<BaseEntity, BaseDTO>();
                    cfg.CreateMap<BaseDTO, BaseEntity>();

                    cfg.CreateMap<Entity, DTO>()
                        .IncludeBase<BaseEntity, BaseDTO>();
                    cfg.CreateMap<DTO, Entity>()
                        .IncludeBase<BaseDTO, BaseEntity>();
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

            // Act
            Expression<Func<BaseDTO, bool>> dtoQueryExpression = r => (r is DTO ? ((DTO)r).Name : "") == "Sofia";
            var entityQueryExpression = Mapper.Map<Expression<Func<BaseEntity, bool>>>(dtoQueryExpression);
            entityQuery = _source.AsQueryable().Where(entityQueryExpression);
        }

        [Fact]
        public void Should_support_propertypath_expressions_with_properties_from_assignable_types()
        {
            // Assert
            entityQuery.ToList().Count().ShouldBe(1);
        }
    }
}
