using System;
using System.Linq.Expressions;
using AutoMapper.Internal;
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
    }
}
