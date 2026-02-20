using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public interface ITypeMappingsManager
    {
        MapperInfoDictionary InfoDictionary { get; }
        Dictionary<Type, Type> TypeMappings { get; }

        void AddTypeMapping(Type sourceType, Type destType);
        List<ParameterExpression> GetDestinationParameterExpressions(LambdaExpression expression);
        Type ReplaceType(Type sourceType);
    }
}
