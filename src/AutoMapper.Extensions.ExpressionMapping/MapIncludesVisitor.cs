using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Extensions.ExpressionMapping.Extensions;
using AutoMapper.Extensions.ExpressionMapping.Structures;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class MapIncludesVisitor : XpressionMapperVisitor
    {
        public MapIncludesVisitor(IMapper mapper, IConfigurationProvider configurationProvider, Dictionary<Type, Type> typeMappings)
            : base(mapper, configurationProvider, typeMappings)
        {
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (!node.Body.Type.IsLiteralType())
                return base.VisitLambda(node);

            var ex = this.Visit(node.Body);

            var mapped = Expression.Lambda(ex, node.GetDestinationParameterExpressions(this.InfoDictionary, this.TypeMappings));
            this.TypeMappings.AddTypeMapping(ConfigurationProvider, node.Type, mapped.Type);
            return mapped;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!node.Type.IsLiteralType())
                return base.VisitMember(node);

            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);
            return GetMappedMemberExpression(node.GetBaseOfMemberExpression(), new List<PropertyMapInfo>());

            Expression GetMappedMemberExpression(Expression parentExpression, List<PropertyMapInfo> propertyMapInfoList)
            {
                Expression mappedParentExpression = this.Visit(parentExpression);
                FindDestinationFullName(parentExpression.Type, mappedParentExpression.Type, node.GetPropertyFullName(), propertyMapInfoList);

                if (propertyMapInfoList.Any(x => x.CustomExpression != null))//CustomExpression takes precedence over DestinationPropertyInfo
                {
                    return GetMemberExpression
                    (
                        new FindMemberExpressionsVisitor(mappedParentExpression),
                        GetMemberExpressionFromCustomExpression
                        (
                            propertyMapInfoList,
                            propertyMapInfoList.Last(x => x.CustomExpression != null),
                            mappedParentExpression
                        )
                    );
                }

                return GetExpressionForInclude
                (
                    GetMemberExpressionFromMemberMaps
                    (
                        BuildFullName(propertyMapInfoList),
                        mappedParentExpression
                    )
                );
            }

            Expression GetExpressionForInclude(MemberExpression memberExpression) 
                => memberExpression.Type.IsLiteralType() ? memberExpression.Expression : memberExpression;

            MemberExpression GetMemberExpression(FindMemberExpressionsVisitor visitor, Expression mappedExpression)
            {
                visitor.Visit(mappedExpression);
                return visitor.Result;
            }
        }
    }
}
