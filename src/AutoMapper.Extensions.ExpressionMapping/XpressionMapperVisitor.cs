﻿using AutoMapper.Extensions.ExpressionMapping.Extensions;
using AutoMapper.Extensions.ExpressionMapping.Structures;
using AutoMapper.QueryableExtensions.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class XpressionMapperVisitor : ExpressionVisitor
    {
        private readonly bool _ignoreValidations;

        public XpressionMapperVisitor(IMapper mapper, IConfigurationProvider configurationProvider, Dictionary<Type, Type> typeMappings, bool ignoreValidations)
        {
            Mapper = mapper;
            TypeMappings = typeMappings;
            InfoDictionary = new MapperInfoDictionary(new ParameterExpressionEqualityComparer());
            ConfigurationProvider = configurationProvider;
            _ignoreValidations = ignoreValidations;
        }

        public MapperInfoDictionary InfoDictionary { get; }

        public Dictionary<Type, Type> TypeMappings { get; }

        protected IConfigurationProvider ConfigurationProvider { get; }

        protected IMapper Mapper { get; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            InfoDictionary.Add(node, TypeMappings);
            var pair = InfoDictionary.SingleOrDefault(a => a.Key.Equals(node));
            return !pair.Equals(default(KeyValuePair<Type, MapperInfo>)) ? pair.Value.NewParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            string sourcePath;

            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);

            var sType = parameterExpression.Type;
            if (InfoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            var propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, InfoDictionary[parameterExpression].DestType, sourcePath, propertyMapInfoList);
            string fullName;

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))
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
                var visitor = new PrependParentNameVisitor(last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, fullName, InfoDictionary[parameterExpression].NewParameter);

                var ex = propertyMapInfoList[propertyMapInfoList.Count - 1] != last
                    ? visitor.Visit(last.CustomExpression.Body.MemberAccesses(afterCustExpression))
                    : visitor.Visit(last.CustomExpression.Body);

                this.TypeMappings.AddTypeMapping(ConfigurationProvider, node.Type, ex.Type);
                return ex;
            }
            fullName = BuildFullName(propertyMapInfoList);
            var me = ExpressionHelpers.MemberAccesses(fullName, InfoDictionary[parameterExpression].NewParameter);

            this.TypeMappings.AddTypeMapping(ConfigurationProvider, node.Type, me.Type);
            return me;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var ex = this.Visit(node.Body);

            var mapped = Expression.Lambda(ex, node.GetDestinationParameterExpressions(this.InfoDictionary, this.TypeMappings));
            this.TypeMappings.AddTypeMapping(ConfigurationProvider, node.Type, mapped.Type);
            return mapped;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return DoVisitBinary(this.Visit(node.Left), this.Visit(node.Right), this.Visit(node.Conversion));

            Expression DoVisitBinary(Expression newLeft, Expression newRight, Expression conversion)
            {
                if (newLeft != node.Left || newRight != node.Right || conversion != node.Conversion)
                {
                    if (node.NodeType == ExpressionType.Coalesce && node.Conversion != null)
                        return Expression.Coalesce(newLeft, newRight, conversion as LambdaExpression);
                    else
                        return Expression.MakeBinary
                        (
                            node.NodeType,
                            newLeft,
                            newRight,
                            node.IsLiftedToNull,
                            Expression.MakeBinary(node.NodeType, newLeft, newRight).Method
                        );
                }

                return node;
            }
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            return DoVisitConditional(this.Visit(c.Test), this.Visit(c.IfTrue), this.Visit(c.IfFalse));

            Expression DoVisitConditional(Expression test, Expression ifTrue, Expression ifFalse)
            {
                if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
                    return Expression.Condition(test, ifTrue, ifFalse);

                return c;
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    switch (node.Operand.NodeType)
                    {
                        case ExpressionType.Constant:
                            return ProcessConstant((ConstantExpression)node.Operand);
                        default:
                            return _ignoreValidations ? node.Update(Visit(node.Operand)) : base.VisitUnary(node);
                    }
                case ExpressionType.Lambda:
                    var lambdaExpression = (LambdaExpression)node.Operand;
                    var ex = this.Visit(lambdaExpression.Body);

                    var mapped = Expression.Quote(Expression.Lambda(ex, lambdaExpression.GetDestinationParameterExpressions(this.InfoDictionary, this.TypeMappings)));
                    this.TypeMappings.AddTypeMapping(ConfigurationProvider, node.Type, mapped.Type);
                    return mapped;
                default:
                    return _ignoreValidations ? node.Update(Visit(node.Operand)) : base.VisitUnary(node);
            }

            Expression ProcessConstant(ConstantExpression operand)
                                => this.TypeMappings.TryGetValue(operand.Type, out Type newType)
                                    ? Expression.Constant(Mapper.Map(operand.Value, node.Type, newType), newType)
                                    : base.VisitUnary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (this.TypeMappings.TryGetValue(node.Type, out Type newType))
                return base.VisitConstant(Expression.Constant(Mapper.Map(node.Value, node.Type, newType), newType));

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression != null)
                InfoDictionary.Add(parameterExpression, TypeMappings);

            var listOfArgumentsForNewMethod = node.Arguments.Aggregate(new List<Expression>(), (lst, next) =>
            {
                var mappedNext = this.Visit(next);
                TypeMappings.AddTypeMapping(ConfigurationProvider, next.Type, mappedNext.Type);

                lst.Add(mappedNext);
                return lst;
            });//Arguments could be expressions or other objects. e.g. s => s.UserId  or a string "ZZZ".  For extention methods node.Arguments[0] is usually the helper object itself

            //type args are the generic type args e.g. T1 and T2 MethodName<T1, T2>(method arguments);
            var typeArgsForNewMethod = node.Method.IsGenericMethod
                ? node.Method.GetGenericArguments().Select(i => TypeMappings.ContainsKey(i) ? TypeMappings[i] : i).ToList()//not converting the type it is not in the typeMappings dictionary
                : null;

            ConvertTypesIfNecessary(node.Method.GetParameters(), listOfArgumentsForNewMethod, node.Method);

            return node.Method.IsStatic
                    ? GetStaticExpression()
                    : GetInstanceExpression(this.Visit(node.Object));

            MethodCallExpression GetInstanceExpression(Expression instance)
                => node.Method.IsGenericMethod
                    ? Expression.Call(instance, node.Method.Name, typeArgsForNewMethod.ToArray(), listOfArgumentsForNewMethod.ToArray())
                    : Expression.Call(instance, node.Method, listOfArgumentsForNewMethod.ToArray());

            MethodCallExpression GetStaticExpression()
                => node.Method.IsGenericMethod
                    ? Expression.Call(node.Method.DeclaringType, node.Method.Name, typeArgsForNewMethod.ToArray(), listOfArgumentsForNewMethod.ToArray())
                    : Expression.Call(node.Method, listOfArgumentsForNewMethod.ToArray());
        }

        void ConvertTypesIfNecessary(ParameterInfo[] parameters, List<Expression> listOfArgumentsForNewMethod, MethodInfo mInfo)
        {
            if (mInfo.IsGenericMethod)
                return;

            for (int i = 0; i < listOfArgumentsForNewMethod.Count; i++)
            {
                if (listOfArgumentsForNewMethod[i].Type != parameters[i].ParameterType
                    && parameters[i].ParameterType.IsAssignableFrom(listOfArgumentsForNewMethod[i].Type))
                    listOfArgumentsForNewMethod[i] = Expression.Convert(listOfArgumentsForNewMethod[i], parameters[i].ParameterType);
            }
        }

        protected string BuildFullName(List<PropertyMapInfo> propertyMapInfoList)
        {
            var fullName = string.Empty;
            foreach (var info in propertyMapInfoList)
            {
                if (info.CustomExpression != null)
                {
                    fullName = string.IsNullOrEmpty(fullName)
                        ? info.CustomExpression.GetMemberFullName()
                        : string.Concat(fullName, ".", info.CustomExpression.GetMemberFullName());
                }
                else
                {
                    var additions = info.DestinationPropertyInfos.Aggregate(new StringBuilder(fullName), (sb, next) =>
                    {
                        if (sb.ToString() == string.Empty)
                            sb.Append(next.Name);
                        else
                        {
                            sb.Append(".");
                            sb.Append(next.Name);
                        }
                        return sb;
                    });

                    fullName = additions.ToString();
                }
            }

            return fullName;
        }

        private static void AddPropertyMapInfo(Type parentType, string name, List<PropertyMapInfo> propertyMapInfoList)
        {
            var sourceMemberInfo = parentType.GetFieldOrProperty(name);
            switch (sourceMemberInfo)
            {
                case PropertyInfo propertyInfo:
                    propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> { propertyInfo }));
                    break;
                case FieldInfo fieldInfo:
                    propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> { fieldInfo }));
                    break;
            }
        }

        private bool GenericTypeDefinitionsAreEquivalent(Type typeSource, Type typeDestination)
            => typeSource.IsGenericType() && typeDestination.IsGenericType() && typeSource.GetGenericTypeDefinition() == typeDestination.GetGenericTypeDefinition();

        protected void FindDestinationFullName(Type typeSource, Type typeDestination, string sourceFullName, List<PropertyMapInfo> propertyMapInfoList)
        {
            const string period = ".";

            if (typeSource == typeDestination)
            {
                var sourceFullNameArray = sourceFullName.Split(new[] { period[0] }, StringSplitOptions.RemoveEmptyEntries);
                sourceFullNameArray.Aggregate(propertyMapInfoList, (list, next) =>
                {
                    if (list.Count == 0)
                    {
                        AddPropertyMapInfo(typeSource, next, list);
                    }
                    else
                    {
                        var last = list[list.Count - 1];
                        AddPropertyMapInfo(last.CustomExpression == null
                            ? last.DestinationPropertyInfos[last.DestinationPropertyInfos.Count - 1].GetMemberType()
                            : last.CustomExpression.ReturnType, next, list);
                    }
                    return list;
                });
                return;
            }

            if (GenericTypeDefinitionsAreEquivalent(typeSource, typeDestination))
            {
                if (sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    //sourceFullName is a member of the generic type definition so just add the members PropertyMapInfo
                    AddPropertyMapInfo(typeDestination, sourceFullName, propertyMapInfoList);
                    var sourceType = typeSource.GetFieldOrProperty(sourceFullName).GetMemberType();
                    var destType = typeDestination.GetFieldOrProperty(sourceFullName).GetMemberType();

                    TypeMappings.AddTypeMapping(ConfigurationProvider, sourceType, destType);

                    return;
                }
                else
                {
                    //propertyName is a member of the generic type definition so just add the members PropertyMapInfo
                    var propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase));
                    AddPropertyMapInfo(typeDestination, propertyName, propertyMapInfoList);

                    var sourceType = typeSource.GetFieldOrProperty(propertyName).GetMemberType();
                    var destType = typeDestination.GetFieldOrProperty(propertyName).GetMemberType();

                    TypeMappings.AddTypeMapping(ConfigurationProvider, sourceType, destType);

                    var childFullName = sourceFullName.Substring(sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) + 1);
                    FindDestinationFullName(sourceType, destType, childFullName, propertyMapInfoList);
                    return;
                }
            }

            var typeMap = ConfigurationProvider.CheckIfMapExists(sourceType: typeDestination, destinationType: typeSource);//The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination 

            PathMap pathMap = typeMap.FindPathMapByDestinationPath(destinationFullPath: sourceFullName);
            if (pathMap != null)
            {
                propertyMapInfoList.Add(new PropertyMapInfo(pathMap.CustomMapExpression, new List<MemberInfo>()));
                return;
            }


            if (sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) < 0)
            {
                var propertyMap = typeMap.GetPropertyMapByDestinationProperty(sourceFullName);
                var sourceMemberInfo = typeSource.GetFieldOrProperty(propertyMap.DestinationMember.Name);
                if (propertyMap.ValueResolverConfig != null)
                {
                    throw new InvalidOperationException(Resource.customResolversNotSupported);
                }

                if (propertyMap.CustomMapExpression == null && !propertyMap.SourceMembers.Any())
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, sourceFullName));

                CompareSourceAndDestLiterals
                (
                    propertyMap.CustomMapExpression != null ? propertyMap.CustomMapExpression.ReturnType : propertyMap.SourceMember.GetMemberType(),
                    propertyMap.CustomMapExpression != null ? propertyMap.CustomMapExpression.ToString() : propertyMap.SourceMember.Name,
                    sourceMemberInfo.GetMemberType()
                );

                void CompareSourceAndDestLiterals(Type mappedPropertyType, string mappedPropertyDescription, Type sourceMemberType)
                {
                    //switch from IsValueType to IsLiteralType because we do not want to throw an exception for all structs
                    if ((mappedPropertyType.IsLiteralType() || sourceMemberType.IsLiteralType()) && sourceMemberType != mappedPropertyType)
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.expressionMapValueTypeMustMatchFormat, mappedPropertyType.Name, mappedPropertyDescription, sourceMemberType.Name, propertyMap.DestinationMember.Name));
                }

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomMapExpression, propertyMap.SourceMembers.ToList()));
            }
            else
            {
                var propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase));
                var propertyMap = typeMap.GetPropertyMapByDestinationProperty(propertyName);

                var sourceMemberInfo = typeSource.GetFieldOrProperty(propertyMap.DestinationMember.Name);
                if (propertyMap.CustomMapExpression == null && !propertyMap.SourceMembers.Any())//If sourceFullName has a period then the SourceMember cannot be null.  The SourceMember is required to find the ProertyMap of its child object.
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, propertyName));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomMapExpression, propertyMap.SourceMembers.ToList()));
                var childFullName = sourceFullName.Substring(sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) + 1);

                FindDestinationFullName(sourceMemberInfo.GetMemberType(), propertyMap.CustomMapExpression == null
                    ? propertyMap.SourceMember.GetMemberType()
                    : propertyMap.CustomMapExpression.ReturnType, childFullName, propertyMapInfoList);
            }
        }
    }
}