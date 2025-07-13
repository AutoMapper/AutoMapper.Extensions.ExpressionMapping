using System;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class SetPropertyCalls<TSource>
    {
        public SetPropertyCalls<TSource> SetProperty<TProperty>(
        Func<TSource, TProperty> propertyExpression,
        Func<TSource, TProperty> valueExpression)
        => throw new InvalidOperationException();


        public SetPropertyCalls<TSource> SetProperty<TProperty>(
            Func<TSource, TProperty> propertyExpression,
            TProperty valueExpression)
            => throw new InvalidOperationException();
    }
}
