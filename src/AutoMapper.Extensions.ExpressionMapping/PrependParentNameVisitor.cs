using System.Linq.Expressions;

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
