using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping.ArgumentMappers
{
    internal class DefaultArgumentMapper : ArgumentMapper
    {
        public DefaultArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression => ExpressionVisitor.Visit(Argument);
    }
}
