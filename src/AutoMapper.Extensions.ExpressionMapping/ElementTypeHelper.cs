using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class ElementTypeHelper
    {
        public static Type GetElementType(Type enumerableType) => GetElementTypes(enumerableType, null)[0];

        public static Type[] GetElementTypes(Type enumerableType, ElementTypeFlags flags = ElementTypeFlags.None) =>
            GetElementTypes(enumerableType, null, flags);

        public static Type[] GetElementTypes(Type enumerableType, System.Collections.IEnumerable enumerable,
            ElementTypeFlags flags = ElementTypeFlags.None)
        {
            if (enumerableType.HasElementType)
            {
                return new[] { enumerableType.GetElementType() };
            }

            var iDictionaryType = enumerableType.GetDictionaryType();
            if (iDictionaryType != null && flags.HasFlag(ElementTypeFlags.BreakKeyValuePair))
            {
                return iDictionaryType.GetTypeInfo().GenericTypeArguments;
            }

            var iReadOnlyDictionaryType = enumerableType.GetReadOnlyDictionaryType();
            if (iReadOnlyDictionaryType != null && flags.HasFlag(ElementTypeFlags.BreakKeyValuePair))
            {
                return iReadOnlyDictionaryType.GetTypeInfo().GenericTypeArguments;
            }

            var iEnumerableType = enumerableType.GetIEnumerableType();
            if (iEnumerableType != null)
            {
                return iEnumerableType.GetTypeInfo().GenericTypeArguments;
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return new[] { first?.GetType() ?? typeof(object) };
            }

            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.",
                nameof(enumerableType));
        }

        public static Type GetReadOnlyDictionaryType(this Type type) => type.GetGenericInterface(typeof(IReadOnlyDictionary<,>));
        public static Type GetDictionaryType(this Type type) => type.GetGenericInterface(typeof(IDictionary<,>));

        public static System.Linq.Expressions.Expression ToType(System.Linq.Expressions.Expression expression, Type type) => expression.Type == type ? expression : System.Linq.Expressions.Expression.Convert(expression, type);
    }
    public enum ElementTypeFlags
    {
        None = 0,
        BreakKeyValuePair = 1
    }
}
