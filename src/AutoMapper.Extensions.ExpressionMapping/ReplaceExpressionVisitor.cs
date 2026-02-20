using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal class ReplaceExpressionVisitor(Expression oldExpression, Expression newExpression) : ExpressionVisitor
    {
        private readonly Expression _oldExpression = oldExpression;
        private readonly Expression _newExpression = newExpression;

        public override Expression Visit(Expression node)
        {
            if (_oldExpression == node)
                node = _newExpression;

            return base.Visit(node);
        }
    }
}
