using System;
using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    public class MapperInfo
    {
        [Obsolete("Use MapperInfo(ParameterExpression newParameter, Type sourceType, Type destType).")]
        public MapperInfo()
        {

        }

        public MapperInfo(ParameterExpression newParameter, Type sourceType, Type destType)
        {
            NewParameter = newParameter;
            SourceType = sourceType;
            DestType = destType;
        }

        public Type SourceType { get; set; }
        public Type DestType { get; set; }
        public ParameterExpression NewParameter { get; set; }
    }
}
