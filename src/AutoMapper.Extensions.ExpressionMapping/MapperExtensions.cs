using AutoMapper.Extensions.ExpressionMapping.Extensions;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps an expression from typeof(Expression&lt;TSource&gt;) to typeof(Expression&lt;TTarget&gt;)
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <param name="sourceExpressionType"></param>
        /// <param name="destExpressionType"></param>
        /// <returns></returns>
        public static LambdaExpression MapExpression(this IMapper mapper, LambdaExpression expression, Type sourceExpressionType, Type destExpressionType)
        {
            if (expression == null)
                return default;

            return (LambdaExpression)"_MapExpression".GetMapExpressionMethod().MakeGenericMethod
            (
                sourceExpressionType,
                destExpressionType
            ).Invoke(null, new object[] { mapper, expression });
        }

        private static TDestDelegate _MapExpression<TSourceDelegate, TDestDelegate>(this IMapper mapper, TSourceDelegate expression)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression
            => mapper.MapExpression<TSourceDelegate, TDestDelegate>(expression);

        private static MethodInfo GetMapExpressionMethod(this string methodName)
            => typeof(MapperExtensions).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        public static object MapObject(this IMapper mapper, object obj, Type sourceType, Type destType) 
            => "_MapObject".GetMapObjectMethod().MakeGenericMethod
            (
                sourceType,
                destType
            ).Invoke(null, new object[] { mapper, obj });

        private static TDest _MapObject<TSource, TDest>(IMapper mapper, TSource source) 
            => mapper.Map<TSource, TDest>(source);

        private static MethodInfo GetMapObjectMethod(this string methodName)
           => typeof(MapperExtensions).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpression<TDestDelegate>(this IMapper mapper, LambdaExpression expression)
            where TDestDelegate : LambdaExpression
        {
            if (expression == null)
                return default;

            return mapper.MapExpression<TDestDelegate>
            (
                expression, 
                (config, mappings) => new XpressionMapperVisitor(mapper, config, mappings),
                (desType) => IsFuncType(desType)
            );
        }

        private static TDestDelegate MapExpression<TDestDelegate>(this IMapper mapper, 
            LambdaExpression expression, 
            Func<IConfigurationProvider, Dictionary<Type, Type>, XpressionMapperVisitor> getVisitor, 
            Func<Type, bool> shouldConvertMappedBodyToDestType)
            where TDestDelegate : LambdaExpression
        {
            return MapExpression<TDestDelegate>
            (
                mapper.ConfigurationProvider,
                expression,
                expression.GetType().GetGenericArguments()[0],
                typeof(TDestDelegate).GetGenericArguments()[0],
                getVisitor,
                shouldConvertMappedBodyToDestType
            );
        }

        private static TDestDelegate MapExpression<TDestDelegate>(IConfigurationProvider configurationProvider,
            LambdaExpression expression,
            Type typeSourceFunc,
            Type typeDestFunc,
            Func<IConfigurationProvider, Dictionary<Type, Type>, XpressionMapperVisitor> getVisitor,
            Func<Type, bool> shouldConvertMappedBodyToDestType)
            where TDestDelegate : LambdaExpression
        {
            return CreateVisitor(new Dictionary<Type, Type>().AddTypeMappingsFromDelegates(configurationProvider, typeSourceFunc, typeDestFunc));

            TDestDelegate CreateVisitor(Dictionary<Type, Type> typeMappings)
                => MapBody(typeMappings, getVisitor(configurationProvider, typeMappings));

            TDestDelegate MapBody(Dictionary<Type, Type> typeMappings, XpressionMapperVisitor visitor)
                => GetLambda(typeMappings, visitor, visitor.Visit(expression.Body));

            TDestDelegate GetLambda(Dictionary<Type, Type> typeMappings, XpressionMapperVisitor visitor, Expression mappedBody)
            {
                if (mappedBody == null)
                    throw new InvalidOperationException(Resource.cantRemapExpression);

                return (TDestDelegate)Lambda
                (
                    typeDestFunc,
                    ConvertBody(),
                    expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings)
                );

                Expression ConvertBody()
                {
                    if (!shouldConvertMappedBodyToDestType(typeDestFunc))
                        return mappedBody;

                    mappedBody = mappedBody.GetUnconvertedExpression();

                    return ElementTypeHelper.ToType(mappedBody, typeDestFunc.GetGenericArguments().Last());
                }
            }
        }

        private static bool IsFuncType(this Type type)
            => type.FullName.StartsWith("System.Func");

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpression<TSourceDelegate, TDestDelegate>(this IMapper mapper, TSourceDelegate expression)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression
            => mapper.MapExpression<TDestDelegate>(expression);

        /// <summary>
        /// Maps an expression to be used as an "Include" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpressionAsInclude<TDestDelegate>(this IMapper mapper, LambdaExpression expression)
            where TDestDelegate : LambdaExpression
        {
            if (expression == null)
                return default;

            return mapper.MapExpression<TDestDelegate>
            (
                expression,
                (config, mappings) => new MapIncludesVisitor(mapper, config, mappings),
                desType => false
            );
        }

        /// <summary>
        /// Maps an expression to be used as an "Include" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpressionAsInclude<TSourceDelegate, TDestDelegate>(this IMapper mapper, TSourceDelegate expression)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression
            => mapper.MapExpressionAsInclude<TDestDelegate>(expression);

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapExpressionList<TSourceDelegate, TDestDelegate>(this IMapper mapper, ICollection<TSourceDelegate> collection)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression
            => collection?.Select(mapper.MapExpression<TSourceDelegate, TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapExpressionList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression
            => collection?.Select(mapper.MapExpression<TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapIncludesList<TSourceDelegate, TDestDelegate>(this IMapper mapper, ICollection<TSourceDelegate> collection)
            where TSourceDelegate : LambdaExpression
            where TDestDelegate : LambdaExpression
            => collection?.Select(mapper.MapExpressionAsInclude<TSourceDelegate, TDestDelegate>).ToList();

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestDelegate> MapIncludesList<TDestDelegate>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestDelegate : LambdaExpression
            => collection?.Select(mapper.MapExpressionAsInclude<TDestDelegate>).ToList();

        /// <summary>
        /// Takes a list of parameters from the source lamda expression and returns a list of parameters for the destination lambda expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        public static List<ParameterExpression> GetDestinationParameterExpressions(this LambdaExpression expression, MapperInfoDictionary infoDictionary, Dictionary<Type, Type> typeMappings)
        {
            foreach (var p in expression.Parameters.Where(p => !infoDictionary.ContainsKey(p)))
            {
                infoDictionary.Add(p, typeMappings);
            }

            return expression.Parameters.Select(p => infoDictionary[p].NewParameter).ToList();
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the generic arguments.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="typeMappings"></param>
        /// <param name="configurationProvider"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping<TSource, TDest>(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider)
            => typeMappings == null
                ? throw new ArgumentException(Resource.typeMappingsDictionaryIsNull)
                : typeMappings.AddTypeMapping(configurationProvider, typeof(TSource), typeof(TDest));

        private static bool HasUnderlyingType(this Type type)
        {
            return (type.IsGenericType() && typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) || type.IsArray;
        }

        private static void AddUnderlyingTypes(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            typeMappings.DoAddTypeMappings
            (
                configurationProvider,
                !sourceType.HasUnderlyingType() ? new List<Type>() : ElementTypeHelper.GetElementTypes(sourceType).ToList(),
                !destType.HasUnderlyingType() ? new List<Type>() : ElementTypeHelper.GetElementTypes(destType).ToList()
            );
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the arguments.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="configurationProvider"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            if (sourceType.GetTypeInfo().IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Expression<>))
            {
                sourceType = sourceType.GetGenericArguments()[0];
                destType = destType.GetGenericArguments()[0];
            }

            if (!typeMappings.ContainsKey(sourceType) && sourceType != destType)
            {
                typeMappings.Add(sourceType, destType);
                if (typeof(Delegate).IsAssignableFrom(sourceType))
                    typeMappings.AddTypeMappingsFromDelegates(configurationProvider, sourceType, destType);
                else
                {
                    typeMappings.AddUnderlyingTypes(configurationProvider, sourceType, destType);
                    typeMappings.FindChildPropertyTypeMaps(configurationProvider, sourceType, destType);
                    typeMappings.AddIncludedTypeMaps(configurationProvider, sourceType, destType);
                }
            }

            return typeMappings;
        }

        private static void AddIncludedTypeMaps(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type source/*model*/, Type dest/*data*/)//model to date
        {
            //Stay with the existing design of using configured data to model maps to retrieve the type mappings.
            //This is needed for property map custom expressions.
            AddTypeMaps(configurationProvider.Internal().ResolveTypeMap(sourceType: dest/*data*/, destinationType: source/*model*/));

            void AddTypeMaps(TypeMap typeMap)
            {
                if (typeMap == null)
                    return;

                foreach (TypePair baseTypePair in typeMap.IncludedBaseTypes)
                    typeMappings.AddTypeMapping(configurationProvider, baseTypePair.DestinationType/*model*/, baseTypePair.SourceType/*data*/);

                foreach (TypePair derivedTypePair in typeMap.IncludedDerivedTypes)
                    typeMappings.AddTypeMapping(configurationProvider, derivedTypePair.DestinationType/*model*/, derivedTypePair.SourceType/*data*/);
            }
        }

        /// <summary>
        /// Replaces a type in the source expression with the corresponding destination type.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static Type ReplaceType(this Dictionary<Type, Type> typeMappings, Type sourceType)
        {
            if (!sourceType.IsGenericType)
            {
                return typeMappings.TryGetValue(sourceType, out Type destType) ? destType : sourceType;
            }
            else
            {
                if (typeMappings.TryGetValue(sourceType, out Type destType))
                    return destType;
                else
                    return sourceType.GetGenericTypeDefinition().MakeGenericType
                    (
                        sourceType
                        .GetGenericArguments()
                        .Select(type => typeMappings.ReplaceType(type))
                        .ToArray()
                    );
            }
        }

        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Resource.typeMappingsDictionaryIsNull);

            typeMappings.DoAddTypeMappingsFromDelegates
            (
                configurationProvider,
                sourceType.GetGenericArguments().ToList(),
                destType.GetGenericArguments().ToList()
            );

            return typeMappings;
        }

        private static void DoAddTypeMappingsFromDelegates(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, List<Type> sourceArguments, List<Type> destArguments)
        {
            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Resource.invalidArgumentCount);

            for (int i = 0; i < sourceArguments.Count; i++)
            {
                if (!typeMappings.ContainsKey(sourceArguments[i]) && sourceArguments[i] != destArguments[i])
                    typeMappings.AddTypeMapping(configurationProvider, sourceArguments[i], destArguments[i]);
            }
        }

        private static void DoAddTypeMappings(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, List<Type> sourceArguments, List<Type> destArguments)
        {
            if (sourceArguments.Count != destArguments.Count)
                return;

            for (int i = 0; i < sourceArguments.Count; i++)
            {
                if (!typeMappings.ContainsKey(sourceArguments[i]) && sourceArguments[i] != destArguments[i])
                    typeMappings.AddTypeMapping(configurationProvider, sourceArguments[i], destArguments[i]);
            }
        }

        private static Type GetSourceMemberType(this PropertyMap propertyMap)
            => propertyMap.CustomMapExpression != null
                ? propertyMap.CustomMapExpression.ReturnType
                : propertyMap.SourceMember.GetMemberType();

        private static void FindChildPropertyTypeMaps(this Dictionary<Type, Type> typeMappings, IConfigurationProvider ConfigurationProvider, Type source, Type dest)
        {
            //The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination
            var typeMap = ConfigurationProvider.Internal().ResolveTypeMap(sourceType: dest, destinationType: source);

            if (typeMap == null)
                return;

            FindMaps(typeMap.PropertyMaps.ToList());
            void FindMaps(List<PropertyMap> maps)
            {
                foreach (PropertyMap pm in maps)
                {
                    if (!pm.SourceMembers.Any() && pm.CustomMapExpression == null)
                        continue;

                    AddChildMappings
                    (
                        source.GetFieldOrProperty(pm.DestinationMember.Name).GetMemberType(),
                        pm.GetSourceMemberType()
                    );
                    void AddChildMappings(Type sourcePropertyType, Type destPropertyType)
                    {
                        if (sourcePropertyType.IsLiteralType() || destPropertyType.IsLiteralType())
                            return;

                        typeMappings.AddTypeMapping(ConfigurationProvider, sourcePropertyType, destPropertyType);
                    }
                }
            }
        }
    }
}
