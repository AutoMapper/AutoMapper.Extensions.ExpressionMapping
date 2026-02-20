using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal class PrependParentNameVisitor(ParameterExpression currentParameter, string parentFullName, Expression newParameter) : ExpressionVisitor
    {
        public ParameterExpression CurrentParameter { get; } = currentParameter;
        public string ParentFullName { get; } = parentFullName;
        public Expression NewParameter { get; } = newParameter;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (object.ReferenceEquals(CurrentParameter, node))
            {
                return string.IsNullOrEmpty(ParentFullName)
                        ? NewParameter
                        : ExpressionHelpers.MemberAccesses(ParentFullName, NewParameter);
            }

            return base.VisitParameter(node);
        }
    }
}
