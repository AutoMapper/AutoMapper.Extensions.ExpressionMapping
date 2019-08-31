using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Extensions.ExpressionMapping.Structures;

namespace AutoMapper.Extensions.ExpressionMapping.Extensions
{
    internal static class VisitorExtensions
    {
        /// <summary>
        /// Returns true if the expression is a direct or descendant member expression of the parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsMemberExpression(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                return IsMemberOrParameterExpression(memberExpression.Expression);
            }

            return false;
        }

        private static bool IsMemberOrParameterExpression(Expression expression)
        {
            //the node represents parameter of the expression
            switch (expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return true;
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    return IsMemberOrParameterExpression(memberExpression.Expression);
            }

            return false;
        }

        /// <summary>
        /// Returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyFullName(this Expression expression)
        {
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

        /// <summary>
        /// Returns the ParameterExpression for the LINQ parameter.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression(this Expression expression)
        {
            if (expression == null)
                return null;

            switch (expression)
            {
                case ParameterExpression pe:
                    return pe;
                case UnaryExpression ue:
                    return ue.Operand.GetParameterExpression();
                case BinaryExpression be:
                    return be.Left.GetParameterExpression()
                        ?? be.Right.GetParameterExpression();
                case ConditionalExpression ce:
                    return ce.Test.GetParameterExpression()
                        ?? ce.IfTrue.GetParameterExpression()
                        ?? ce.IfFalse.GetParameterExpression();
                case MemberExpression me:
                    return me.Expression.GetParameterExpression();
                case MethodCallExpression mce:
                    return mce.Method.IsDefined(typeof(ExtensionAttribute), true)
                        ? mce.Arguments.FirstOrDefault()?.GetParameterExpression()
                        : mce.Object?.GetParameterExpression();
                case LambdaExpression le:
                    return le.Body.GetParameterExpression();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Determines if the given expression contains a parameter expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool HasParameterExpression(this Expression expression)
        {
            return expression.GetParameterExpression() != null;
        }

        /// <summary>
        /// Returns the first ancestor node that is not a MemberExpression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression GetBaseOfMemberExpression(this MemberExpression expression)
        {
            switch(expression.Expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return GetBaseOfMemberExpression((MemberExpression)expression.Expression);
                default:
                    return expression.Expression;
            }
        }

        /// <summary>
        /// Adds member expressions to an existing expression.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static MemberExpression MemberAccesses(this Expression exp, List<PropertyMapInfo> list) =>
            (MemberExpression) list.SelectMany(propertyMapInfo => propertyMapInfo.DestinationPropertyInfos).MemberAccesses(exp);

        /// <summary>
        /// For the given a Lambda Expression, returns the fully qualified name of the member starting with the immediate child member of the parameter
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetMemberFullName(this LambdaExpression expr)
        {
            if (expr.Body.NodeType == ExpressionType.Parameter)
                return string.Empty;

            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    me = (expr.Body as UnaryExpression)?.Operand as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            return me.GetPropertyFullName();
        }

        /// <summary>
        /// Returns the underlying type typeof(T) when the type implements IEnumerable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetUnderlyingGenericTypes(this Type type) => 
            type == null || !type.GetTypeInfo().IsGenericType
            ? new List<Type>()
            : type.GetGenericArguments().ToList();
    }
}
