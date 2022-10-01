using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Extensions.ExpressionMapping
{
    using MemberPaths = IEnumerable<IEnumerable<MemberInfo>>;

    internal static class ReflectionExtensions
    {
        public static object MapMember(this ResolutionContext context, MemberInfo member, object value, object destination = null)
            => ReflectionHelper.MapMember(context, member, value, destination);

        public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
            => ReflectionHelper.SetMemberValue(propertyOrField, target, value);

        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
            => ReflectionHelper.GetMemberValue(propertyOrField, target);

        public static IEnumerable<MemberInfo> GetMemberPath(Type type, string fullMemberName)
            => ReflectionHelper.GetMemberPath(type, fullMemberName);

        public static MemberPaths GetMemberPaths(Type type, string[] membersToExpand) =>
            membersToExpand.Select(m => ReflectionHelper.GetMemberPath(type, m));

        public static MemberPaths GetMemberPaths<TResult>(Expression<Func<TResult, object>>[] membersToExpand) =>
            membersToExpand.Select(expr => MemberVisitor.GetMemberPath(expr));

        public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
            => ReflectionHelper.FindProperty(lambdaExpression);

        public static Type GetMemberType(this MemberInfo memberInfo)
            => ReflectionHelper.GetMemberType(memberInfo);

        public static IEnumerable<TypeInfo> GetDefinedTypes(this Assembly assembly) =>
            assembly.DefinedTypes;

        public static bool GetHasDefaultValue(this ParameterInfo info) =>
            info.HasDefaultValue;

        public static bool GetIsConstructedGenericType(this Type type) =>
            type.IsConstructedGenericType;
    }
}
