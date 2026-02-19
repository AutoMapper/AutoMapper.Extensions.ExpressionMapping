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

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestDelegate MapExpression<TDestDelegate>(this IMapper mapper,
            LambdaExpression expression)
            where TDestDelegate : LambdaExpression
        {
            if (expression == null)
                return default;

            return MapExpression<TDestDelegate>
            (
                mapper,
                expression,
                expression.GetType().GetGenericArguments()[0],
                typeof(TDestDelegate).GetGenericArguments()[0]
            );
        }

        private static TDestDelegate MapExpression<TDestDelegate>(IMapper mapper,
            LambdaExpression expression,
            Type typeSourceFunc,
            Type typeDestFunc)
            where TDestDelegate : LambdaExpression
        {
            return CreateVisitor(new Dictionary<Type, Type>().AddTypeMappingsFromDelegates(mapper.ConfigurationProvider, typeSourceFunc, typeDestFunc));

            TDestDelegate CreateVisitor(Dictionary<Type, Type> typeMappings)
                => MapBody(typeMappings, new XpressionMapperVisitor(mapper, typeMappings));

            TDestDelegate MapBody(Dictionary<Type, Type> typeMappings, XpressionMapperVisitor visitor)
                => GetLambda(typeMappings, visitor, visitor.Visit(expression.Body));

            TDestDelegate GetLambda(Dictionary<Type, Type> typeMappings, XpressionMapperVisitor visitor, Expression mappedBody)
            {
                if (mappedBody == null)
                    throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

                return (TDestDelegate)Lambda
                (
                    typeDestFunc,
                    mappedBody,
                    expression.GetDestinationParameterExpressions(visitor.InfoDictionary, typeMappings)
                );
            }
        }


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
        /// Takes a list of parameters from the source lamda expression and returns a list of parameters for the destination lambda expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        [Obsolete("This method will be moved to a public class meant for internal use.")]
        public static List<ParameterExpression> GetDestinationParameterExpressions(this LambdaExpression expression, MapperInfoDictionary infoDictionary, Dictionary<Type, Type> typeMappings)
        {
            foreach (var p in expression.Parameters.Where(p => !infoDictionary.ContainsKey(p)))
            {
                infoDictionary.Add(p, typeMappings);
            }

            return expression.Parameters.Select(p => infoDictionary[p].NewParameter).ToList();
        }

        private static bool HasUnderlyingType(this Type type)
        {
            return (type.IsGenericType() && type.GetGenericArguments().Length > 0) || type.IsArray;
        }

        private static void AddUnderlyingTypes(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            if ((sourceType.IsGenericType() && typeof(System.Collections.IEnumerable).IsAssignableFrom(sourceType)) || sourceType.IsArray)
            {
                typeMappings.DoAddTypeMappings
                (
                    configurationProvider,
                    !sourceType.HasUnderlyingType() ? new List<Type>() : ElementTypeHelper.GetElementTypes(sourceType).ToList(),
                    !destType.HasUnderlyingType() ? new List<Type>() : ElementTypeHelper.GetElementTypes(destType).ToList()
                );
            }
            else if (sourceType.IsGenericType() && destType.IsGenericType())
            {
                typeMappings.DoAddTypeMappings
                (
                    configurationProvider,
                    !sourceType.HasUnderlyingType() ? new List<Type>() : sourceType.GetGenericArguments().ToList(),
                    !destType.HasUnderlyingType() ? new List<Type>() : destType.GetGenericArguments().ToList()
                );
            }
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the arguments.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="configurationProvider"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        [Obsolete("This method will be moved to a public class meant for internal use.")]
        public static Dictionary<Type, Type> AddTypeMapping(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

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
        [Obsolete("This method will be moved to a public class meant for internal use.")]
        public static Type ReplaceType(this Dictionary<Type, Type> typeMappings, Type sourceType)
        {
            if (sourceType.IsArray)
            {
                if (typeMappings.TryGetValue(sourceType, out Type destType))
                    return destType;

                if (typeMappings.TryGetValue(sourceType.GetElementType(), out Type destElementType))
                {
                    int rank = sourceType.GetArrayRank();
                    return rank == 1 
                        ? destElementType.MakeArrayType()
                        : destElementType.MakeArrayType(rank);
                }

                return sourceType;
            }
            else if (sourceType.IsGenericType)
            {
                if (typeMappings.TryGetValue(sourceType, out Type destType))
                    return destType;
                else
                {
                    return sourceType.GetGenericTypeDefinition().MakeGenericType
                    (
                        sourceType
                        .GetGenericArguments()
                        .Select(typeMappings.ReplaceType)
                        .ToArray()
                    );
                }
            }
            else
            {
                return typeMappings.TryGetValue(sourceType, out Type destType) ? destType : sourceType;
            }
        }

        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates(this Dictionary<Type, Type> typeMappings, IConfigurationProvider configurationProvider, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

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
                throw new ArgumentException(Properties.Resources.invalidArgumentCount);

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
                : propertyMap.SourceMembers.Last().GetMemberType();

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
