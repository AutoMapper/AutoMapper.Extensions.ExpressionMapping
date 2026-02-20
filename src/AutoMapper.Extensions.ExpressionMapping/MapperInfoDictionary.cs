using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Extensions.ExpressionMapping.Structures;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class MapperInfoDictionary(ParameterExpressionEqualityComparer comparer) : Dictionary<ParameterExpression, MapperInfo>(comparer)
    {
        public void Add(ParameterExpression key, Dictionary<Type, Type> typeMappings)
        {
            if (ContainsKey(key))
                return;

            Add(key, typeMappings.TryGetValue(key.Type, out Type valueType)
                    ? new MapperInfo(Expression.Parameter(valueType, key.Name), key.Type, valueType)
                    : new MapperInfo(Expression.Parameter(key.Type, key.Name), key.Type, key.Type));
        }
    }
}
