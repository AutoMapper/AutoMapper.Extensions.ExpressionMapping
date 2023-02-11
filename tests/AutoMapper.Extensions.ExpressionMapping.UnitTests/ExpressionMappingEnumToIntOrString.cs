using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ExpressionMappingEnumToIntOrString : AutoMapperSpecBase
    {
        public enum SimpleEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        private class EntityDto
        {
            internal SimpleEnum Value { get; set; }
        }
        private class EntityAsString
        {
            internal string Value { get; set; }
        }
        private class EntityAsInt
        {
            internal int Value { get; set; }
        }
        public class SimpleEnumToStringTypeResolver : ITypeConverter<SimpleEnum, string>
        {
            public string Convert(SimpleEnum source, string destination, ResolutionContext context)
            {
                return source.ToString();
            }
        }
        protected override MapperConfiguration Configuration
        {
            get
            {
                return new MapperConfiguration(config =>
                {
                    config.AddExpressionMapping();
                    config.CreateMap<EntityAsString, EntityDto>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value));
                    config.CreateMap<EntityAsInt, EntityDto>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value));
                    config.CreateMap<SimpleEnum, string>().ConvertUsing<SimpleEnumToStringTypeResolver>();
                });
            }
        }

        [Fact]
        public void BinaryExpressionOfEnumsToInt()
        {
            Expression<Func<EntityAsInt, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto), "x");
                var property = Expression.Property(param, nameof(EntityDto.Value));
                var constant = Expression.Constant(SimpleEnum.Value2, typeof(SimpleEnum));
                var binaryExpression = Expression.Equal(property, constant);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<EntityAsInt, bool>>>(lambdaExpression);
            }

            Expression<Func<EntityAsInt, bool>> translatedExpression= translatedExpression = x => x.Value == 2;

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new EntityAsInt { Value = (int)SimpleEnum.Value2 };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
        }

        [Fact]
        public void BinaryExpressionOfEnumsToString()
        {
            Expression<Func<EntityAsString, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto), "x");
                var property = Expression.Property(param, nameof(EntityDto.Value));
                var constant = Expression.Constant(SimpleEnum.Value2, typeof(SimpleEnum));
                var binaryExpression = Expression.Equal(property, constant);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<EntityAsString, bool>>>(lambdaExpression);
            }

            Expression<Func<EntityAsString, bool>> translatedExpression = x => x.Value == "Value2";

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new EntityAsString { Value = SimpleEnum.Value2.ToString() };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
        }

        [Fact]
        public void BinaryExpressionOfCoalescedEnumToInt()
        {
            Expression<Func<EntityAsInt, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto), "x");
                var property = Expression.Property(param, nameof(EntityDto.Value));
                var usedConstant = Expression.Constant(SimpleEnum.Value2, typeof(SimpleEnum?));
                var otherConstant = Expression.Constant(SimpleEnum.Value1, typeof(SimpleEnum));
                var coalesce = Expression.Coalesce(usedConstant, otherConstant);
                var binaryExpression = Expression.Equal(property, coalesce);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<EntityAsInt, bool>>>(lambdaExpression);
            }

            Expression<Func<EntityAsInt, bool>> translatedExpression = translatedExpression = x => x.Value == ((int?)2 ?? 1);

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new EntityAsInt { Value = (int)SimpleEnum.Value2 };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
        }
    }
}
