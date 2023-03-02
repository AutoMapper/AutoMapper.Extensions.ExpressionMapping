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

        private class ComplexEntity
        {
            //Named in the direction of conversion for entity -> dto
            public int intToEnum { get; set; }
            public string stringToEnum { get; set; }
            public SimpleEnum enumToInt { get; set; }
            public SimpleEnum enumToString { get; set; }
            //Untranslated properties
            public SimpleEnum enumToEnum { get; set; }
            public int intToInt { get; set; }
            public string stringToString { get; set; }
        }

        private class ComplexEntityDto
        {
            public SimpleEnum intToEnum { get; set; }
            public SimpleEnum stringToEnum { get; set; }
            public int enumToInt { get; set; }
            public string enumToString { get; set; }

            public SimpleEnum enumToEnum { get; set; }
            public int intToInt { get; set; }
            public string stringToString { get; set; }
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

                    config.CreateMap<ComplexEntity, ComplexEntityDto>()
                    .ForMember(dest => dest.intToEnum, config => config.MapFrom(src => src.intToEnum))
                    .ForMember(dest => dest.stringToEnum, config => config.MapFrom(src => src.stringToEnum))
                    .ForMember(dest => dest.enumToEnum, config => config.MapFrom(src => src.enumToEnum))
                    .ForMember(dest => dest.enumToInt, config => config.MapFrom(src => src.enumToInt))
                    .ForMember(dest => dest.enumToString, config => config.MapFrom(src => src.enumToString.ToString()))
                    .ForMember(dest => dest.intToInt, config => config.MapFrom(src => src.intToInt))
                    .ForMember(dest => dest.stringToString, config => config.MapFrom(src => src.stringToString))
                    .ReverseMap();

                    //config.CreateMap<SimpleEnum, string>().ConvertUsing(e => e.ToString());
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

        //TODO: Make the a Theory.
        [Fact]
        public void BinaryExpressionPartialTranslation()
        {

            //x => x.IntToEnum == 1 && x.OtherInt == 2;
            Expression<Func<ComplexEntityDto, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(ComplexEntity), "x");
                var property1 = Expression.Property(param, nameof(ComplexEntity.intToEnum));
                var property2 = Expression.Property(param, nameof(ComplexEntity.intToInt));
                var property3 = Expression.Property(param, nameof(ComplexEntity.stringToString));
                var property4 = Expression.Property(param, nameof(ComplexEntity.stringToEnum));
                var property5 = Expression.Property(param, nameof(ComplexEntity.enumToEnum));
                var property6 = Expression.Property(param, nameof(ComplexEntity.enumToInt));
                var property7 = Expression.Property(param, nameof(ComplexEntity.enumToString));

                var constant1 = Expression.Constant(2, typeof(int));
                var constant2 = Expression.Constant(1, typeof(int));
                var constant3 = Expression.Constant(SimpleEnum.Value2.ToString(), typeof(string));
                var constant4 = Expression.Constant(SimpleEnum.Value1.ToString(), typeof(string));
                var constant5 = Expression.Constant(SimpleEnum.Value3, typeof(SimpleEnum));
                var constant6 = Expression.Constant(SimpleEnum.Value2, typeof(SimpleEnum));
                var constant7 = Expression.Constant(SimpleEnum.Value1, typeof(SimpleEnum));

                Expression[] equals = new Expression[]{
                    Expression.Equal(property1, constant1),
                    Expression.Equal(property2, constant2),
                    Expression.Equal(property3, constant3),
                    Expression.Equal(property4, constant4),
                    Expression.Equal(property5, constant5),
                    Expression.Equal(property6, constant6),
                    Expression.Equal(property7, constant7)
                };

                //var equals1 = Expression.Equal(property1, constant1);
                //var equals2 = Expression.Equal(property2, constant2);
                //var equals3 = Expression.Equal(property3, constant3);
                //var equals4 = Expression.Equal(property4, constant4);
                //var equals5 = Expression.Equal(property5, constant5);
                //var equals6 = Expression.Equal(property6, constant6);
                //var equals7 = Expression.Equal(property7, constant7);
                Expression andExpression = equals[0];
                    for (int i = 1; i < equals.Length; i++)
                {
                    andExpression = Expression.And(andExpression, equals[i]);
                }
                var lambdaExpression = Expression.Lambda(andExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<ComplexEntityDto, bool>>>(lambdaExpression);
            }

            Expression<Func<ComplexEntityDto, bool>> translatedExpression =
                translatedExpression = x =>
                x.intToEnum == SimpleEnum.Value2
                && x.intToInt == (int)SimpleEnum.Value1
                && x.stringToString == SimpleEnum.Value2.ToString()
                && x.stringToEnum == SimpleEnum.Value1
                && x.enumToEnum == SimpleEnum.Value3
                && x.enumToInt == (int)SimpleEnum.Value2
                && x.enumToString == SimpleEnum.Value1.ToString()
                ;

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new ComplexEntityDto { intToEnum = SimpleEnum.Value2, intToInt = 1 };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
        }
    }
}
