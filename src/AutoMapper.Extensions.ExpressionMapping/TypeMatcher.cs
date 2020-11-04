using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class TypeMatcher
    {
        public static Type[] InferGenericArguments(MethodInfo method, IEnumerable<Type> constructedArgumentTypes)
        {
            return new TypeInference(method, constructedArgumentTypes).Result;
        }

        private class TypeInference
        {
            public Type[] Result { get; }
            private readonly HashSet<Type> _matched;

            public TypeInference(MethodInfo method, IEnumerable<Type> constructedArgumentTypes)
            {
                Result = method.GetGenericArguments();
                _matched = new HashSet<Type>();

                foreach (var pair in constructedArgumentTypes.Zip(method.GetParameters().Select(p => p.ParameterType),
                    (arg, param) => new {Argument = arg, Parameter = param}))
                {
                    MatchGenericParameter(pair.Argument, pair.Parameter);
                    if (_matched.Count == Result.Length) return;
                }

                throw new Exception($"Failed to infer generic arguments for {method} using argument types {string.Join(", ", constructedArgumentTypes)}.");
            }

            private void MatchGenericParameter(Type argumentType, Type parameterType)
            {
                if (parameterType.IsGenericParameter && _matched.Add(parameterType))
                {
                    for (var i = 0; i < Result.Length; i++)
                    {
                        if (Result[i] == parameterType)
                        {
                            Result[i] = argumentType;
                            return;
                        }
                    }
                }
                else if (parameterType.IsGenericType && argumentType.IsGenericType)
                {
                    foreach (var pair in argumentType.GetGenericArguments().Zip(parameterType.GetGenericArguments(),
                        (arg, param) => new {Argument = arg, Parameter = param}))
                    {
                        MatchGenericParameter(pair.Argument, pair.Parameter);
                    }
                }
            }
        }
    }
}
