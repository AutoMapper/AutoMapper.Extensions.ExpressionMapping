using System;
using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    public class MapperInfo(ParameterExpression newParameter, Type sourceType, Type destType)
    {
        public Type SourceType { get; set; } = sourceType;
        public Type DestType { get; set; } = destType;
        public ParameterExpression NewParameter { get; set; } = newParameter;
    }
}
