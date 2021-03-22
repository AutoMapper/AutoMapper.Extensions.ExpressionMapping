using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    using AutoMapper.Extensions.ExpressionMapping;
    using static Expression;

    internal static class ExpressionExtensions
    {
        public static Expression MemberAccesses(this IEnumerable<MemberInfo> members, Expression obj) =>
            members.Aggregate(obj, (expression, member) => MakeMemberAccess(expression, member));

        public static IEnumerable<MemberExpression> GetMembers(this Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if(memberExpression == null)
            {
                return new MemberExpression[0];
            }
            return memberExpression.GetMembers();
        }

        public static IEnumerable<MemberExpression> GetMembers(this MemberExpression expression)
        {
            while(expression != null)
            {
                yield return expression;
                expression = expression.Expression as MemberExpression;
            }
        }

        public static bool IsMemberPath(this LambdaExpression exp)
        {
            return exp.Body.GetMembers().LastOrDefault()?.Expression == exp.Parameters.First();
        }
    }

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