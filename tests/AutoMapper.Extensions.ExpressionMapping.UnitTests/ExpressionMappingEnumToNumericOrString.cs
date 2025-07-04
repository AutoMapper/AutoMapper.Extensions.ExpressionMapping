
using System;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class ExpressionMappingEnumToNumericOrString : AutoMapperSpecBase
    {
        public enum SimpleEnumByte : byte
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumSByte : sbyte
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumShort : short
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumUShort : ushort
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumInt : int
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumUInt : uint
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
        public enum SimpleEnumLong : long
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = long.MaxValue
        }
        public enum SimpleEnumULong : ulong
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = long.MaxValue
        }
        private class EntityDto<TEnum>
            where TEnum : Enum
        {
            internal TEnum Value { get; set; }
        }
        private class Entity<T>
        {
            internal T Value { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                return ConfigurationHelper.GetMapperConfiguration(config =>
                {
                    config.AddExpressionMapping();
                    config.CreateMap<Entity<byte>, EntityDto<SimpleEnumByte>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();                    
                    
                    config.CreateMap<Entity<sbyte>, EntityDto<SimpleEnumSByte>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();                    
                    config.CreateMap<Entity<short>, EntityDto<SimpleEnumShort>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();                    
                    config.CreateMap<Entity<ushort>, EntityDto<SimpleEnumUShort>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();                    
                    config.CreateMap<Entity<int>, EntityDto<SimpleEnumInt>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                     config.CreateMap<Entity<int?>, EntityDto<SimpleEnumInt>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<uint>, EntityDto<SimpleEnumUInt>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<long>, EntityDto<SimpleEnumLong>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<ulong>, EntityDto<SimpleEnumULong>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumByte>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumSByte>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumShort>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumUShort>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumInt>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumUInt>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumLong>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();
                    config.CreateMap<Entity<string>, EntityDto<SimpleEnumULong>>()
                    .ForMember(dest => dest.Value, config => config.MapFrom(src => src.Value))
                    .ReverseMap();

                    config.CreateMap<SimpleEnumByte, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumSByte, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumShort, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumUShort, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumInt, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumUInt, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumLong, string>().ConvertUsing(e => e.ToString());
                    config.CreateMap<SimpleEnumULong, string>().ConvertUsing(e => e.ToString());

                    config.CreateMap<ComplexEntity, ComplexEntityDto>()
.ForMember(dest => dest.intToEnum, config => config.MapFrom(src => src.intToEnum))
.ForMember(dest => dest.enumToEnum, config => config.MapFrom(src => src.enumToEnum))
.ForMember(dest => dest.enumToInt, config => config.MapFrom(src => src.enumToInt))
.ForMember(dest => dest.intToInt, config => config.MapFrom(src => src.intToInt))
.ReverseMap();
                });
            }
        }

        [Theory]
        [InlineData(SimpleEnumByte.Value2, (byte)2)]
        [InlineData(SimpleEnumSByte.Value2, (sbyte)2)]
        [InlineData(SimpleEnumShort.Value2, (short)2)]
        [InlineData(SimpleEnumUShort.Value2, (ushort)2)]
        [InlineData(SimpleEnumInt.Value2, 2)]
        [InlineData(SimpleEnumUInt.Value2, 2U)]
        [InlineData(SimpleEnumLong.Value2, 2L)]
        [InlineData(SimpleEnumULong.Value2, 2UL)]
        [InlineData(SimpleEnumSByte.Value2, (sbyte)1)]
        [InlineData(SimpleEnumByte.Value2, (byte)1)]
        [InlineData(SimpleEnumShort.Value2, (short)1)]
        [InlineData(SimpleEnumUShort.Value2, (ushort)1)]
        [InlineData(SimpleEnumInt.Value2, 1)]
        [InlineData(SimpleEnumUInt.Value2, 1U)]
        [InlineData(SimpleEnumLong.Value2, 1L)]
        [InlineData(SimpleEnumULong.Value2, 1UL)]
        [InlineData(SimpleEnumSByte.Value3, (sbyte)3)]
        [InlineData(SimpleEnumByte.Value3, (byte)1)]
        [InlineData(SimpleEnumShort.Value3, (short)1)]
        [InlineData(SimpleEnumUShort.Value3, (ushort)1)]
        [InlineData(SimpleEnumInt.Value3, 1)]
        [InlineData(SimpleEnumUInt.Value3, 1U)]
        [InlineData(SimpleEnumLong.Value3, 1L)]
        [InlineData(SimpleEnumULong.Value3, 1UL)]
        [InlineData(SimpleEnumByte.Value3, (byte)3)]
        [InlineData(SimpleEnumShort.Value3, (short)3)]
        [InlineData(SimpleEnumUShort.Value3, (ushort)3)]
        [InlineData(SimpleEnumInt.Value3, 3)]
        [InlineData(SimpleEnumUInt.Value3, 3U)]
        [InlineData(SimpleEnumLong.Value3, 3L)]
        [InlineData(SimpleEnumULong.Value3, 3UL)]
        [InlineData(SimpleEnumLong.Value3, long.MaxValue)]
        [InlineData(SimpleEnumULong.Value3, (ulong)long.MaxValue)]
        public void BinaryExpressionEquals<TEnum, TNumeric>(TEnum enumConstant, TNumeric numericConstant)
    where TEnum : Enum
        {
            var correctResult = ((TNumeric)(object)enumConstant).Equals(numericConstant);
            Expression<Func<Entity<TNumeric>, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto<TEnum>), "x");
                var property = Expression.Property(param, nameof(EntityDto<TEnum>.Value));
                var constantExp = Expression.Constant(enumConstant, typeof(TEnum));
                var binaryExpression = Expression.Equal(property, constantExp);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<Entity<TNumeric>, bool>>>(lambdaExpression);
            }

            var mappedExpressionDelegate = mappedExpression.Compile();

            var entity = new Entity<TNumeric> { Value = numericConstant };
            var result = mappedExpressionDelegate(entity);

            Assert.Equal(result, correctResult);
        }

        [Fact]
        public void BinaryExpressionEqualsWithNullable()
        {
            SimpleEnumInt enumConstant = SimpleEnumInt.Value2;
            int? numericConstant = 2;
            Expression<Func<Entity<int?>, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto<SimpleEnumInt>), "x");
                var property = Expression.Property(param, nameof(EntityDto<SimpleEnumInt>.Value));
                var constantExp = Expression.Constant(enumConstant, typeof(SimpleEnumInt));
                var binaryExpression = Expression.Equal(property, constantExp);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<Entity<int?>, bool>>>(lambdaExpression);
            }

            var mappedExpressionDelegate = mappedExpression.Compile();

            var entity = new Entity<int?> { Value = numericConstant };
            var result = mappedExpressionDelegate(entity);

            Assert.True(result);
        }

        private class ComplexEntity
        {
            public int intToEnum { get; set; }
            public SimpleEnumInt enumToInt { get; set; }
            public SimpleEnumInt enumToEnum { get; set; }
            public int intToInt { get; set; }
        }

        private class ComplexEntityDto
        {
            public SimpleEnumInt intToEnum { get; set; }
            public int enumToInt { get; set; }

            public SimpleEnumInt enumToEnum { get; set; }
            public int intToInt { get; set; }
        }

        [Fact]
        public void BinaryExpressionPartialTranslation()
        {
            Expression<Func<ComplexEntityDto, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(ComplexEntity), "x");
                var property1 = Expression.Property(param, nameof(ComplexEntity.intToEnum));
                var property2 = Expression.Property(param, nameof(ComplexEntity.intToInt));
                var property5 = Expression.Property(param, nameof(ComplexEntity.enumToEnum));
                var property6 = Expression.Property(param, nameof(ComplexEntity.enumToInt));

                var constant1 = Expression.Constant(2, typeof(int));
                var constant2 = Expression.Constant(1, typeof(int));
                var constant5 = Expression.Constant(SimpleEnumInt.Value3, typeof(SimpleEnumInt));
                var constant6 = Expression.Constant(SimpleEnumInt.Value2, typeof(SimpleEnumInt));

                Expression[] equals = new Expression[]{
                    Expression.Equal(property1, constant1),
                    Expression.Equal(property2, constant2),
                    Expression.Equal(property5, constant5),
                    Expression.Equal(property6, constant6),
                };

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
                x.intToEnum == SimpleEnumInt.Value2
                && x.intToInt == (int)SimpleEnumInt.Value1
                && x.enumToEnum == SimpleEnumInt.Value3
                && x.enumToInt == (int)SimpleEnumInt.Value2
                ;

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new ComplexEntityDto { intToEnum = SimpleEnumInt.Value2, intToInt = 1, enumToEnum = SimpleEnumInt.Value3, enumToInt = 2 };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
        }
    }
}
