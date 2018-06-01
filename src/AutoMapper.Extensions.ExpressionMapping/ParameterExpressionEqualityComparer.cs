using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class ParameterExpressionEqualityComparer : IEqualityComparer<ParameterExpression>
    {
        public bool Equals(ParameterExpression x, ParameterExpression y) => ReferenceEquals(x, y);

        public int GetHashCode(ParameterExpression obj) => obj.GetHashCode();
    }
}
