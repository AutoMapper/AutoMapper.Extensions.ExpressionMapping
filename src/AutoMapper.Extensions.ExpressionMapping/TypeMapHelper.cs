using AutoMapper.Internal;
using System;
using System.Linq;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal static class TypeMapHelper
    {
        public static bool CanMapConstant(this IConfigurationProvider config, Type sourceType, Type destType)
        {
            if (sourceType == destType)
                return false;

            if (BothTypesAreDictionary())
            {
                Type[] sourceGenericTypes = sourceType.GetGenericArguments();
                Type[] destGenericTypes = destType.GetGenericArguments();
                if (sourceGenericTypes.SequenceEqual(destGenericTypes))
                    return false;
                else if (sourceGenericTypes[0] == destGenericTypes[0])
                    return config.CanMapConstant(sourceGenericTypes[1], destGenericTypes[1]);
                else if (sourceGenericTypes[1] == destGenericTypes[1])
                    return config.CanMapConstant(sourceGenericTypes[0], destGenericTypes[0]);
                else
                    return config.CanMapConstant(sourceGenericTypes[0], destGenericTypes[0]) && config.CanMapConstant(sourceGenericTypes[1], destGenericTypes[1]);
            }
            else if (BothTypesAreEnumerable())
                return config.CanMapConstant(sourceType.GetGenericArguments()[0], destType.GetGenericArguments()[0]);
            else
                return config.Internal().ResolveTypeMap(sourceType, destType) != null;

            bool BothTypesAreEnumerable()
            {
                Type enumerableType = typeof(System.Collections.IEnumerable);
                return sourceType.IsGenericType
                    && destType.IsGenericType
                    && enumerableType.IsAssignableFrom(sourceType)
                    && enumerableType.IsAssignableFrom(destType);
            }

            bool BothTypesAreDictionary()
            {
                Type dictionaryType = typeof(System.Collections.IDictionary);
                return sourceType.IsGenericType
                    && destType.IsGenericType
                    && dictionaryType.IsAssignableFrom(sourceType)
                    && dictionaryType.IsAssignableFrom(destType)
                    && sourceType.GetGenericArguments().Length == 2
                    && destType.GetGenericArguments().Length == 2;
            }
        }

        public static MemberMap GetMemberMapByDestinationProperty(this TypeMap typeMap, string destinationPropertyName)
        {
            var propertyMap = typeMap.PropertyMaps.SingleOrDefault(item => item.DestinationName == destinationPropertyName);
            if (propertyMap != null)
                return propertyMap;

            var memberMap = typeMap.MemberMaps.OfType<ConstructorParameterMap>().SingleOrDefault(mm => string.Compare(mm.Parameter.Name, destinationPropertyName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (memberMap != null)
                return memberMap;

            throw PropertyConfigurationException(typeMap, destinationPropertyName);
        }

        public static TypeMap CheckIfTypeMapExists(this IConfigurationProvider config, Type sourceType, Type destinationType)
        {
            var typeMap = config.Internal().ResolveTypeMap(sourceType, destinationType);
            if (typeMap == null)
            {
                throw MissingMapException(sourceType, destinationType);
            }
            return typeMap;
        }

        public static string GetDestinationName(this MemberMap memberMap)
        {
            if (memberMap is PropertyMap propertyMap)
                return propertyMap.DestinationMember.Name;

            if (memberMap is ConstructorParameterMap constructorMap)
                return constructorMap.Parameter.Name;
            
            throw new ArgumentException(nameof(memberMap));
        }

        public static PathMap FindPathMapByDestinationFullPath(this TypeMap typeMap, string destinationFullPath) =>
            typeMap.PathMaps.SingleOrDefault(item => string.Join(".", item.MemberPath.Members.Select(m => m.Name)) == destinationFullPath);

        private static Exception PropertyConfigurationException(TypeMap typeMap, params string[] unmappedPropertyNames)
            => new AutoMapperConfigurationException(new[] { new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames, true) });

        private static Exception MissingMapException(Type sourceType, Type destinationType)
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    }
}
