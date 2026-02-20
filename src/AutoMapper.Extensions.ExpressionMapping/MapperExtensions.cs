using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps an expression from typeof(Expression&lt;TSource&gt;) to typeof(Expression&lt;TTarget&gt;)
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <param name="sourceExpressionType"></param>
        /// <param name="destExpressionType"></param>
        /// <returns></returns>
        public static LambdaExpression MapExpression(this IMapper mapper, LambdaExpression expression, Type sourceExpressionType, Type destExpressionType)
        {
            if (expression == null)
                return default;

            Type sourceDeledateType = sourceExpressionType.GetGenericArguments()[0];
            Type destDelegateType = destExpressionType.GetGenericArguments()[0];
            return GetLambdaExpression(new TypeMappingsManager(mapper.ConfigurationProvider, sourceDeledateType, destDelegateType));

            LambdaExpression GetLambdaExpression(ITypeMappingsManager typeMappingsManager)
            {
                Expression mappedBody = new XpressionMapperVisitor(mapper, typeMappingsManager).Visit(expression.Body) ?? throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

                return Lambda
                (
                    destDelegateType,
                    mappedBody,
                    typeMappingsManager.GetDestinationParameterExpressions(expression)
                );
            }
        }

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestExpression"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestExpression MapExpression<TDestExpression>(this IMapper mapper,
            LambdaExpression expression)
            where TDestExpression : LambdaExpression
        {
            if (expression == null)
                return default;

            return (TDestExpression)MapExpression
            (
                mapper,
                expression,
                expression.GetType(),
                typeof(TDestExpression)
            );
        }

        /// <summary>
        /// Maps an expression given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceExpression"></typeparam>
        /// <typeparam name="TDestExpression"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDestExpression MapExpression<TSourceExpression, TDestExpression>(this IMapper mapper, TSourceExpression expression)
            where TSourceExpression : LambdaExpression
            where TDestExpression : LambdaExpression
            => mapper.MapExpression<TDestExpression>(expression);

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TSourceExpression"></typeparam>
        /// <typeparam name="TDestExpression"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestExpression> MapExpressionList<TSourceExpression, TDestExpression>(this IMapper mapper, ICollection<TSourceExpression> collection)
            where TSourceExpression : LambdaExpression
            where TDestExpression : LambdaExpression
            => collection?.Select(mapper.MapExpression<TSourceExpression, TDestExpression>).ToList();

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination type is the value.
        /// </summary>
        /// <typeparam name="TDestExpression"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ICollection<TDestExpression> MapExpressionList<TDestExpression>(this IMapper mapper, IEnumerable<LambdaExpression> collection)
            where TDestExpression : LambdaExpression
            => collection?.Select(mapper.MapExpression<TDestExpression>).ToList();
    }
}
