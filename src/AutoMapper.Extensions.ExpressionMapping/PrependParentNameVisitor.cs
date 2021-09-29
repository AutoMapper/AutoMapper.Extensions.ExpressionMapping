using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AutoMapper.Extensions.ExpressionMapping.Extensions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal class PrependParentNameVisitor : ExpressionVisitor
    {
        public PrependParentNameVisitor(ParameterExpression currentParameter, string parentFullName, Expression newParameter)
        {
            CurrentParameter = currentParameter;
            ParentFullName = parentFullName;
            NewParameter = newParameter;
        }

        public ParameterExpression CurrentParameter { get; }
        public string ParentFullName { get; }
        public Expression NewParameter { get; }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            if (!(node.Expression is ParameterExpression))
                return base.VisitTypeBinary(node);

            if (!object.ReferenceEquals(CurrentParameter, node.GetParameterExpression()))
                return base.VisitTypeBinary(node);

            return Expression.TypeIs
            (
                string.IsNullOrEmpty(ParentFullName)
                    ? NewParameter
                    : ExpressionHelpers.MemberAccesses(ParentFullName, NewParameter),
                node.TypeOperand
            );
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            if (!object.ReferenceEquals(CurrentParameter, node.GetParameterExpression()) || !node.IsMemberExpression())
                return base.VisitMember(node);

            return ExpressionHelpers.MemberAccesses
            (
                string.IsNullOrEmpty(ParentFullName)
                        ? node.GetPropertyFullName()
                        : $"{ParentFullName}.{node.GetPropertyFullName()}",
                NewParameter
            );
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!IsParentParameterExpression())
                return base.VisitMethodCall(node);

            if (!object.ReferenceEquals(CurrentParameter, node.GetParameterExpression()))
                return base.VisitMethodCall(node);

            if (node.Method.IsStatic)
            {
                if (!IsExtentionMethod())
                    return base.VisitMethodCall(node);

                if (node.Method.IsGenericMethod)
                    return Expression.Call
                    (
                        node.Method.DeclaringType,
                        node.Method.Name,
                        node.Method.GetGenericArguments(),
                        GetNewArgumentsForExtensionMethod()
                    );
                else
                    return Expression.Call(node.Method, GetNewArgumentsForExtensionMethod());
            }

            //instance method
            if (node.Method.IsGenericMethod)
            {
                return Expression.Call
                (
                    GetNewParent(),
                    node.Method.Name,
                    node.Method.GetGenericArguments(),
                    node.Arguments.ToArray()
                );
            }
            else
            {
                return Expression.Call
                (
                    GetNewParent(),
                    node.Method,
                    node.Arguments
                );
            }

            Expression[] GetNewArgumentsForExtensionMethod()
            {
                Expression[] arguments = node.Arguments.ToArray();
                arguments[0] = GetNewParent();
                return arguments.ToArray();
            }

            Expression GetNewParent()
                => string.IsNullOrEmpty(ParentFullName)
                ? NewParameter
                : ExpressionHelpers.MemberAccesses(ParentFullName, NewParameter);

            bool IsParentParameterExpression()
            {
                if (node.Method.IsStatic)
                    return node.Arguments[0] is ParameterExpression;

                if (!node.Method.IsStatic)
                    return node.Object is ParameterExpression;

                return false;
            }

            bool IsExtentionMethod()
                => node.Method.IsDefined(typeof(ExtensionAttribute), true);
        }
    }
}
