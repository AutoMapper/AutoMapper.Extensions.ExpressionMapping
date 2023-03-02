
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
                return new MapperConfiguration(config =>
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
                });
            }
        }

        [Fact]
        public void BinaryExpressionOfEnumsToInt()
        {
            Expression<Func<Entity<int>, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto<SimpleEnumInt>), "x");
                var property = Expression.Property(param, nameof(EntityDto<SimpleEnumInt>.Value));
                var constant = Expression.Constant(SimpleEnumInt.Value2, typeof(SimpleEnumInt));
                var binaryExpression = Expression.Equal(property, constant);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<Entity<int>, bool>>>(lambdaExpression);
            }

            Expression<Func<Entity<int>, bool>> translatedExpression = translatedExpression = x => x.Value == 2;

            var mappedExpressionDelegate = mappedExpression.Compile();
            var translatedExpressionDelegate = translatedExpression.Compile();

            var entity = new Entity<int> { Value = (int)SimpleEnumInt.Value2 };
            var mappedResult = mappedExpressionDelegate(entity);
            var translatedResult = translatedExpressionDelegate(entity);

            Assert.True(translatedResult);
            Assert.Equal(mappedResult, translatedResult);
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
        [InlineData(SimpleEnumSByte.Value3, (sbyte)3)]
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

  

        [Theory]
        [InlineData(SimpleEnumSByte.Value2, (sbyte)1)]
        [InlineData(SimpleEnumByte.Value2, (byte)1)]
        [InlineData(SimpleEnumShort.Value2, (short)1)]
        [InlineData(SimpleEnumUShort.Value2, (ushort)1)]
        [InlineData(SimpleEnumInt.Value2, 1)]
        [InlineData(SimpleEnumUInt.Value2, 1U)]
        [InlineData(SimpleEnumLong.Value2, 1L)]
        [InlineData(SimpleEnumULong.Value2, 1UL)]
        public void BinaryExpressionEqualsFalse<TEnum, TNumeric>(TEnum twoAsEnum, TNumeric oneAsNumeric)
    where TEnum : Enum
        {
            Expression<Func<Entity<TNumeric>, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto<TEnum>), "x");
                var property = Expression.Property(param, nameof(EntityDto<TEnum>.Value));
                var constantExp = Expression.Constant(twoAsEnum, typeof(TEnum));
                var binaryExpression = Expression.Equal(property, constantExp);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<Entity<TNumeric>, bool>>>(lambdaExpression);
            }
            var mappedExpressionDelegate = mappedExpression.Compile();
            var entity = new Entity<TNumeric> { Value = oneAsNumeric };
            var result = mappedExpressionDelegate(entity);

            Assert.False(result);
        }

        [Theory]
        [InlineData(SimpleEnumSByte.Value2)]
        [InlineData(SimpleEnumByte.Value2)]
        [InlineData(SimpleEnumShort.Value2)]
        [InlineData(SimpleEnumUShort.Value2)]
        [InlineData(SimpleEnumInt.Value2)]
        [InlineData(SimpleEnumUInt.Value2)]
        [InlineData(SimpleEnumLong.Value2)]
        [InlineData(SimpleEnumULong.Value2)]
        public void BinaryExpressionEqualsWithString<TEnum>(TEnum twoAsEnum)
    where TEnum : Enum
        {

            var constant = typeof(TEnum).GetEnumValues().GetValue(1);
            Expression<Func<Entity<string>, bool>> mappedExpression;
            {
                var param = Expression.Parameter(typeof(EntityDto<TEnum>), "x");
                var property = Expression.Property(param, nameof(EntityDto<TEnum>.Value));
                var constantExp = Expression.Constant(constant);
                var binaryExpression = Expression.Equal(property, constantExp);
                var lambdaExpression = Expression.Lambda(binaryExpression, param);
                mappedExpression = Mapper.Map<Expression<Func<Entity<string>, bool>>>(lambdaExpression);
            }

            var mappedExpressionDelegate = mappedExpression.Compile();

            var entity = new Entity<string> { Value =  constant.ToString()};
            var result = mappedExpressionDelegate(entity);

            Assert.True(result);

            var notEqualConstant = typeof(TEnum).GetEnumValues().GetValue(2);
            entity = new Entity<string> { Value = notEqualConstant.ToString() };
            result = mappedExpressionDelegate(entity);

            Assert.False(result);
        }
    }
}
