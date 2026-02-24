using AutoMapper.Extensions.ExpressionMapping.Structures;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper.Extensions.ExpressionMapping
{
    using static Expression;

    internal static class ExpressionExtensions
    {
        public static Expression ConvertTypeIfNecessary(this Expression expression, Type memberType)
        {
            if (memberType == expression.Type)
                return expression;

            expression = expression.GetUnconvertedExpression();
            if (memberType != expression.Type)
                return Expression.Convert(expression, memberType);

            return expression;
        }

        /// <summary>
        /// Returns the first ancestor node that is not a MemberExpression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression GetBaseOfMemberExpression(this MemberExpression expression)
        {
            if (expression.Expression == null)
                return null;

            return expression.Expression.NodeType == ExpressionType.MemberAccess
                           ? GetBaseOfMemberExpression((MemberExpression)expression.Expression)
                           : expression.Expression;
        }

        public static MemberExpression GetMemberExpression(this Expression expr)
            => expr?.GetUnconvertedExpression() as MemberExpression;

        public static MemberExpression GetMemberExpression(this LambdaExpression expr)
            => expr.Body.GetUnconvertedExpression() as MemberExpression;

        /// <summary>
        /// For the given a Lambda Expression, returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetMemberFullName(this LambdaExpression expr)
        {
            if (expr.Body.NodeType == ExpressionType.Parameter)
                return string.Empty;
            MemberExpression me = expr.Body.NodeType switch
            {
                ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs => expr.Body.GetUnconvertedExpression() as MemberExpression,
                _ => expr.Body as MemberExpression,
            };
            return me.GetPropertyFullName();
        }

        /// <summary>
        /// Returns the ParameterExpression for the LINQ parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression(this Expression expression)
        {
            if (expression == null)
                return null;

            //the node represents parameter of the expression
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return (ParameterExpression)expression;
                case ExpressionType.Quote:
                    return GetParameterExpression(((UnaryExpression)expression).Operand);
                case ExpressionType.Lambda:
                    return GetParameterExpression(((LambdaExpression)expression).Body);
                case ExpressionType.ConvertChecked:
                case ExpressionType.Convert:
                    var ue = expression as UnaryExpression;
                    return GetParameterExpression(ue?.Operand);
                case ExpressionType.TypeAs:
                    return ((UnaryExpression)expression).Operand.GetParameterExpression();
                case ExpressionType.TypeIs:
                    return ((TypeBinaryExpression)expression).Expression.GetParameterExpression();
                case ExpressionType.MemberAccess:
                    return GetParameterExpression(((MemberExpression)expression).Expression);
                case ExpressionType.Call:
                    var methodExpression = expression as MethodCallExpression;
                    var parentExpression = methodExpression?.Object;//Method is an instance method

                    var isExtension = methodExpression != null && methodExpression.Method.IsDefined(typeof(ExtensionAttribute), true);
                    if (isExtension && parentExpression == null && methodExpression.Arguments.Count > 0)
                        parentExpression = methodExpression.Arguments[0];//Method is an extension method based on the type of methodExpression.Arguments[0].

                    if (parentExpression == null)
                        return null;

                    return GetParameterExpression(parentExpression);
            }

            return null;
        }

        /// <summary>
        /// Returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyFullName(this Expression expression)
        {
            if (expression == null)
                return string.Empty;

            const string period = ".";

            //the node represents parameter of the expression
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    var parentFullName = memberExpression.Expression.GetPropertyFullName();
                    return string.IsNullOrEmpty(parentFullName)
                        ? memberExpression.Member.Name
                        : string.Concat(memberExpression.Expression.GetPropertyFullName(), period, memberExpression.Member.Name);
                default:
                    return string.Empty;
            }
        }

        public static Expression GetUnconvertedExpression(this Expression expression)
        {
            return expression.NodeType switch
            {
                ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs => ((UnaryExpression)expression).Operand.GetUnconvertedExpression(),
                _ => expression,
            };
        }

        /// <summary>
        /// Determines whether the specified type is an enumeration type.
        /// </summary>
        /// <param name="type">The type to evaluate. This can be a nullable type, in which case the underlying type is checked.</param>
        /// <returns>true if the specified type is an enumeration; otherwise, false.</returns>
        public static bool IsEnumType(this Type type)
        {
            if (type.IsNullableType())
                type = Nullable.GetUnderlyingType(type);

            return type.IsEnum();
        }

        /// <summary>
        /// Adds member expressions to an existing expression.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static MemberExpression MemberAccesses(this Expression exp, List<PropertyMapInfo> list) =>
            (MemberExpression)list.SelectMany(propertyMapInfo => propertyMapInfo.DestinationPropertyInfos).MemberAccesses(exp);

        public static Expression MemberAccesses(this IEnumerable<MemberInfo> members, Expression obj) =>
            members.Aggregate(obj, MakeMemberAccess);
    }
}