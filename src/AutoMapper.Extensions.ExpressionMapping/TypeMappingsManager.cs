using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class TypeMappingsManager : ITypeMappingsManager
    {
        public TypeMappingsManager(IConfigurationProvider configurationProvider, Type typeSourceFunc, Type typeDestFunc)
        {
            if (!typeof(Delegate).IsAssignableFrom(typeSourceFunc))
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.argumentMustBeDelegateFormat, nameof(typeSourceFunc)), nameof(typeSourceFunc));
            if (!typeof(Delegate).IsAssignableFrom(typeDestFunc))
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.argumentMustBeDelegateFormat, nameof(typeDestFunc)), nameof(typeDestFunc));

            ConfigurationProvider = configurationProvider;
            InfoDictionary = new MapperInfoDictionary(new ParameterExpressionEqualityComparer());
            TypeMappings = [];
            
            AddTypeMappingsFromDelegates(typeSourceFunc, typeDestFunc);
        }

        public MapperInfoDictionary InfoDictionary { get; }

        public Dictionary<Type, Type> TypeMappings { get; }

        private IConfigurationProvider ConfigurationProvider { get; }

        public void AddTypeMapping(Type sourceType, Type destType)
        {
            if (sourceType.GetTypeInfo().IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Expression<>))
            {
                sourceType = sourceType.GetGenericArguments()[0];
                destType = destType.GetGenericArguments()[0];
            }

            if (!TypeMappings.ContainsKey(sourceType) && sourceType != destType)
            {
                TypeMappings.Add(sourceType, destType);
                if (typeof(Delegate).IsAssignableFrom(sourceType))
                    AddTypeMappingsFromDelegates(sourceType, destType);
                else
                {
                    AddUnderlyingTypes(sourceType, destType);
                    FindChildPropertyTypeMaps(sourceType, destType);
                    AddIncludedTypeMaps(sourceType, destType);
                }
            }
        }

        public void AddTypeMappingsFromDelegates(Type sourceType, Type destType)
        {
            DoAddTypeMappingsFromDelegates
            (
                [.. sourceType.GetGenericArguments()],
                [.. destType.GetGenericArguments()]
            );
        }

        public List<ParameterExpression> GetDestinationParameterExpressions(LambdaExpression expression)
        {
            foreach (var p in expression.Parameters.Where(p => !InfoDictionary.ContainsKey(p)))
            {
                InfoDictionary.Add(p, TypeMappings);
            }

            return [.. expression.Parameters.Select(p => InfoDictionary[p].NewParameter)];
        }

        public Type ReplaceType(Type sourceType)
        {
            if (sourceType.IsArray)
            {
                if (TypeMappings.TryGetValue(sourceType, out Type destType))
                    return destType;

                if (TypeMappings.TryGetValue(sourceType.GetElementType(), out Type destElementType))
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
                return TypeMappings.TryGetValue(sourceType, out Type destType)
                    ? destType
                    : sourceType.GetGenericTypeDefinition().MakeGenericType
                    (
                        [.. sourceType
                        .GetGenericArguments()
                        .Select(ReplaceType)]
                    );
            }
            else
            {
                return TypeMappings.TryGetValue(sourceType, out Type destType) ? destType : sourceType;
            }
        }

        private void AddIncludedTypeMaps(Type source/*model*/, Type dest/*data*/)//model to data
        {
            //Stay with the existing design of using configured data to model maps to retrieve the type mappings.
            //This is needed for property map custom expressions.
            AddTypeMaps(ConfigurationProvider.Internal().ResolveTypeMap(sourceType: dest/*data*/, destinationType: source/*model*/));

            void AddTypeMaps(TypeMap typeMap)
            {
                if (typeMap == null)
                    return;

                foreach (TypePair baseTypePair in typeMap.IncludedBaseTypes)
                    AddTypeMapping(baseTypePair.DestinationType/*model*/, baseTypePair.SourceType/*data*/);

                foreach (TypePair derivedTypePair in typeMap.IncludedDerivedTypes)
                    AddTypeMapping(derivedTypePair.DestinationType/*model*/, derivedTypePair.SourceType/*data*/);
            }
        }

        private void AddUnderlyingTypes(Type sourceType, Type destType)
        {
            if ((sourceType.IsGenericType() && typeof(System.Collections.IEnumerable).IsAssignableFrom(sourceType)) || sourceType.IsArray)
            {
                DoAddTypeMappings
                (
                    !HasUnderlyingType(sourceType) ? [] : [.. ElementTypeHelper.GetElementTypes(sourceType)],
                    !HasUnderlyingType(destType) ? [] : [.. ElementTypeHelper.GetElementTypes(destType)]
                );
            }
            else if (sourceType.IsGenericType() && destType.IsGenericType())
            {
                DoAddTypeMappings
                (
                    !HasUnderlyingType(sourceType) ? [] : [.. sourceType.GetGenericArguments()],
                    !HasUnderlyingType(destType) ? [] : [.. destType.GetGenericArguments()]
                );
            }
        }

        private void DoAddTypeMappings(List<Type> sourceArguments, List<Type> destArguments)
        {
            if (sourceArguments.Count != destArguments.Count)
                return;

            for (int i = 0; i < sourceArguments.Count; i++)
            {
                if (!TypeMappings.ContainsKey(sourceArguments[i]) && sourceArguments[i] != destArguments[i])
                    AddTypeMapping(sourceArguments[i], destArguments[i]);
            }
        }

        private void DoAddTypeMappingsFromDelegates(List<Type> sourceArguments, List<Type> destArguments)
        {
            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Properties.Resources.invalidArgumentCount);

            for (int i = 0; i < sourceArguments.Count; i++)
            {
                if (!TypeMappings.ContainsKey(sourceArguments[i]) && sourceArguments[i] != destArguments[i])
                    AddTypeMapping(sourceArguments[i], destArguments[i]);
            }
        }

        private void FindChildPropertyTypeMaps(Type source, Type dest)
        {
            //The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination
            var typeMap = ConfigurationProvider.Internal().ResolveTypeMap(sourceType: dest, destinationType: source);

            if (typeMap == null)
                return;

            FindMaps([.. typeMap.PropertyMaps]);
            void FindMaps(List<PropertyMap> maps)
            {
                foreach (PropertyMap pm in maps.Where(p => p.SourceMembers.Length > 0 || p.CustomMapExpression != null))
                {
                    AddChildMappings
                    (
                        source.GetFieldOrProperty(pm.DestinationMember.Name).GetMemberType(),
                        GetSourceMemberType(pm)
                    );
                    void AddChildMappings(Type sourcePropertyType, Type destPropertyType)
                    {
                        if (sourcePropertyType.IsLiteralType() || destPropertyType.IsLiteralType())
                            return;

                        AddTypeMapping(sourcePropertyType, destPropertyType);
                    }
                }
            }
        }

        private static Type GetSourceMemberType(PropertyMap propertyMap)
            => propertyMap.CustomMapExpression != null
                ? propertyMap.CustomMapExpression.ReturnType
                : propertyMap.SourceMembers.Last().GetMemberType();

        private static bool HasUnderlyingType(Type type)
        {
            return (type.IsGenericType() && type.GetGenericArguments().Length > 0) || type.IsArray;
        }
    }
}
