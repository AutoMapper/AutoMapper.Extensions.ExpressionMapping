using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
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

        protected override Expression VisitMember(MemberExpression node)
        {
            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);
            string sourcePath = node.GetPropertyFullName();
            Expression baseParentExpr = node.GetBaseOfMemberExpression();
            Expression visitedParentExpr = this.Visit(baseParentExpr);
            Type sType = baseParentExpr.Type;
            Type dType = visitedParentExpr.Type;

            var propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, dType, sourcePath, propertyMapInfoList);
            string fullName;

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))//CustomExpression takes precedence over DestinationPropertyInfo
            {
                var last = propertyMapInfoList.Last(x => x.CustomExpression != null);
                var beforeCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) < propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });

                var afterCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) > propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });


                fullName = BuildFullName(beforeCustExpression);

                var visitor = new PrependParentNameVisitor
                (
                    last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, 
                    fullName,
                    visitedParentExpr
                );

                var ex = propertyMapInfoList[propertyMapInfoList.Count - 1] != last
                    ? visitor.Visit(last.CustomExpression.Body.MemberAccesses(afterCustExpression))
                    : visitor.Visit(last.CustomExpression.Body);

                var v = new FindMemberExpressionsVisitor(visitedParentExpr);
                v.Visit(ex);

                return v.Result;
            }
            fullName = BuildFullName(propertyMapInfoList);
            var me = ExpressionHelpers.MemberAccesses(fullName, InfoDictionary[parameterExpression].NewParameter);
            if (me.Expression.NodeType == ExpressionType.MemberAccess && me.Type.IsLiteralType())
            {
                return me.Expression;
            }

            return me;
        }
    }
}
