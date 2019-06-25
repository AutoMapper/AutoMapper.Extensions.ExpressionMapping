using System;
using System.Linq.Expressions;
using AutoMapper.Internal;
using AutoMapper.Extensions.ExpressionMapping.Extensions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal class PrependParentNameVisitor : ExpressionVisitor
    {
        public PrependParentNameVisitor(Type currentParameterType, string parentFullName, Expression baseExpression)
        {
            CurrentParameterType = currentParameterType;
            ParentFullName = parentFullName;
            this.baseExpression = baseExpression;
        }

        public Type CurrentParameterType { get; }
        public string ParentFullName { get; }
        public Expression baseExpression { get; } 

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath;

            var parameterExpression = node.GetParameterExpression();
            var sType = parameterExpression?.Type;
            if (sType != null && sType == CurrentParameterType && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            var fullName = string.IsNullOrEmpty(ParentFullName)
                            ? sourcePath
                            : string.Concat(ParentFullName, ".", sourcePath);

            var me = ExpressionHelpers.MemberAccesses(fullName, baseExpression);

            return me;
        }
    }
}
