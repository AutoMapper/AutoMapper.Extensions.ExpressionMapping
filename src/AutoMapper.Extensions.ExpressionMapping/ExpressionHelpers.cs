using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal static class ExpressionHelpers
    {
        public static MemberExpression MemberAccesses(string members, Expression obj) =>
            (MemberExpression)GetMemberPath(obj.Type, members).MemberAccesses(obj);

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
        {
            var replaceExp = exp.Body;
            for (var i = 0; i < Math.Min(replace.Length, exp.Parameters.Count); i++)
                replaceExp = Replace(replaceExp, exp.Parameters[i], replace[i]);
            return replaceExp;
        }

        public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceExpressionVisitor(old, replace).Visit(exp);

        private static IEnumerable<MemberInfo> GetMemberPath(Type type, string fullMemberName)
        {
            MemberInfo property = null;
            foreach (var memberName in fullMemberName.Split('.'))
            {
                var currentType = GetCurrentType(property, type);
                yield return property = currentType.GetFieldOrProperty(memberName);
            }
        }

        private static Type GetCurrentType(MemberInfo member, Type type)
            => member?.GetMemberType() ?? type;
    }
}
