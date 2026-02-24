using AutoMapper.Extensions.ExpressionMapping.Structures;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AutoMapper.Extensions.ExpressionMapping
{
    public class XpressionMapperVisitor(IMapper mapper, ITypeMappingsManager typeMappingsManager) : ExpressionVisitor
    {
        private MapperInfoDictionary InfoDictionary { get { return TypeMappingsManager.InfoDictionary; } }

        private Dictionary<Type, Type> TypeMappings { get { return TypeMappingsManager.TypeMappings; } }

        private IConfigurationProvider ConfigurationProvider { get; } = mapper.ConfigurationProvider;

        private IMapper Mapper { get; } = mapper;

        private IConfigurationProvider anonymousTypesConfigurationProvider;
        private readonly MapperConfigurationExpression anonymousTypesBaseMappings = new();

        private ITypeMappingsManager TypeMappingsManager { get; } = typeMappingsManager;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            InfoDictionary.Add(node, TypeMappings);
            var pair = InfoDictionary.SingleOrDefault(a => a.Key.Equals(node));
            return !pair.Equals(default(KeyValuePair<ParameterExpression, MapperInfo>)) ? pair.Value.NewParameter : base.VisitParameter(node);
        }

        private static object GetConstantValue(object constantObject, string fullName, Type parentType)
        {
            return fullName.Split('.').Aggregate(constantObject, (parent, memberName) =>
            {
                MemberInfo memberInfo = parentType.GetFieldOrProperty(memberName);
                parentType = memberInfo.GetMemberType();
                return memberInfo.GetMemberValue(parent);
            });
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
            {
                var baseExpression = node.GetBaseOfMemberExpression();
                if (baseExpression?.NodeType == ExpressionType.Constant)
                {
                    if (node.Type.IsLiteralType())
                        return node;

                    return this.Visit
                    (
                        Expression.Constant
                        (
                            GetConstantValue
                            (
                                ((ConstantExpression)baseExpression).Value,
                                node.GetPropertyFullName(),
                                baseExpression.Type
                            ),
                            node.Type
                        )
                    );
                }

                return base.VisitMember(node);
            }

            InfoDictionary.Add(parameterExpression, TypeMappings);
            return GetMappedMemberExpression(node.GetBaseOfMemberExpression(), []);

            Expression GetMappedMemberExpression(Expression parentExpression, List<PropertyMapInfo> propertyMapInfoList)
            {
                Expression mappedParentExpression = this.Visit(parentExpression);
                FindDestinationFullName(parentExpression.Type, mappedParentExpression.Type, node.GetPropertyFullName(), propertyMapInfoList);

                if (propertyMapInfoList.Any(x => x.CustomExpression != null))
                {
                    var fromCustomExpression = GetMemberExpressionFromCustomExpression
                    (
                        propertyMapInfoList,
                        propertyMapInfoList.Last(x => x.CustomExpression != null),
                        mappedParentExpression
                    );

                    if (ShouldConvertMemberExpression(node.Type, fromCustomExpression.Type))
                        fromCustomExpression = fromCustomExpression.ConvertTypeIfNecessary(node.Type);

                    this.TypeMappingsManager.AddTypeMapping(node.Type, fromCustomExpression.Type);
                    return fromCustomExpression;
                }

                Expression memberExpression = GetMemberExpressionFromMemberMaps(XpressionMapperVisitor.BuildFullName(propertyMapInfoList), mappedParentExpression);
                if (ShouldConvertMemberExpression(node.Type, memberExpression.Type))
                    memberExpression = memberExpression.ConvertTypeIfNecessary(node.Type);

                this.TypeMappingsManager.AddTypeMapping(node.Type, memberExpression.Type);

                return memberExpression;
            }
        }

        protected static MemberExpression GetMemberExpressionFromMemberMaps(string fullName, Expression visitedParentExpr) 
            => ExpressionHelpers.MemberAccesses(fullName, visitedParentExpr);

        private static Expression GetMemberExpressionFromCustomExpression(PropertyMapInfo lastWithCustExpression,
                PropertyMapInfo lastInList,
                List<PropertyMapInfo> beforeCustExpression,
                List<PropertyMapInfo> afterCustExpression,
                Expression visitedParentExpr)
        {
            return PrependParentMemberExpression
            (
                new PrependParentNameVisitor
                (
                    lastWithCustExpression.CustomExpression.Parameters[0]/*Parent parameter of current property*/,
                    BuildFullName(beforeCustExpression),
                    visitedParentExpr
                )
            );

            Expression PrependParentMemberExpression(PrependParentNameVisitor visitor)
                => visitor.Visit
                (
                    lastInList != lastWithCustExpression
                        ? lastWithCustExpression.CustomExpression.Body.MemberAccesses(afterCustExpression)
                        : lastWithCustExpression.CustomExpression.Body
                );
        }

        private static bool ShouldConvertMemberExpression(Type initialType, Type mappedType)
        {
            if (initialType.IsLiteralType())
                return true;

            if (!initialType.IsEnumType())
                return false;

            if (initialType.IsNullableType())
                initialType = Nullable.GetUnderlyingType(initialType);

            if (mappedType.IsNullableType())
                mappedType = Nullable.GetUnderlyingType(mappedType);

            return mappedType == Enum.GetUnderlyingType(initialType);
        }

        protected static Expression GetMemberExpressionFromCustomExpression(List<PropertyMapInfo> propertyMapInfoList, PropertyMapInfo lastWithCustExpression, Expression mappedParentExpr) 
            => GetMemberExpressionFromCustomExpression
            (
                lastWithCustExpression,
                propertyMapInfoList.Last(),
                propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) < propertyMapInfoList.IndexOf(lastWithCustExpression))
                        list.Add(next);
                    return list;
                }),
                propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) > propertyMapInfoList.IndexOf(lastWithCustExpression))
                        list.Add(next);
                    return list;
                }),
                mappedParentExpr
            );

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var ex = this.Visit(node.Body);

            var mapped = Expression.Lambda(this.TypeMappingsManager.ReplaceType(node.Type), ex, this.TypeMappingsManager.GetDestinationParameterExpressions(node));
            this.TypeMappingsManager.AddTypeMapping(node.Type, mapped.Type);
            return mapped;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Type newType = this.TypeMappingsManager.ReplaceType(node.Type);
            if (newType != node.Type && !IsAnonymousType(node.Type))
            {
                return Expression.New(newType);
            }
            else if (node.Arguments.Count > 0 && IsAnonymousType(node.Type))
            {
                ParameterInfo[] parameters = node.Type.GetConstructors()[0].GetParameters();
                Dictionary<string, Expression> bindingExpressions = [];

                for (int i = 0; i < parameters.Length; i++)
                    bindingExpressions.Add(parameters[i].Name, this.Visit(node.Arguments[i]));

                return GetAnonymousTypeMemberInitExpression(bindingExpressions, node.Type);
            }

            return base.VisitNew(node);
        }

        private static bool IsAnonymousType(Type type)
            => type.Name.Contains("AnonymousType")
            &&
            (
                Attribute.IsDefined
                (
                    type,
                    typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),
                    false
                )
                ||
                type.Assembly.IsDynamic
            );

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Type newType = this.TypeMappingsManager.ReplaceType(node.Type);
            if (newType != node.Type && !IsAnonymousType(node.Type))
            {
                var typeMap = ConfigurationProvider.CheckIfTypeMapExists(sourceType: newType, destinationType: node.Type);
                //The destination becomes the source because to map a source expression to a destination expression,
                //we need the expressions used to create the source from the destination

                return GetMemberInit
                (
                    new MemberBindingGroup
                    (
                        declaringMemberKey: null,
                        isRootMemberAssignment: true,
                        newType: newType,
                        memberAssignmentInfos: node.Bindings.OfType<MemberAssignment>().Aggregate(new List<MemberAssignmentInfo>(), (list, binding) =>
                        {
                            var propertyMap = typeMap.PropertyMaps.SingleOrDefault(item => item.DestinationName == binding.Member.Name);
                            if (propertyMap == null)
                                return list;

                            list.Add(new MemberAssignmentInfo(propertyMap, binding));
                            return list;
                        })
                    )
                );
            }
            else if (IsAnonymousType(node.Type))
            {
                return GetAnonymousTypeMemberInitExpression
                (
                    node.Bindings
                        .OfType<MemberAssignment>()
                        .ToDictionary
                        (
                            binding => binding.Member.Name,
                            binding => this.Visit(binding.Expression)
                        ),
                    node.Type
                );
            }

            return base.VisitMemberInit(node);
        }

        private void ConfigureAnonymousTypeMaps(Type oldType, Type newAnonymousType)
        {
            anonymousTypesBaseMappings.CreateMap(newAnonymousType, oldType);
            Dictionary<Type, Type> memberTypeMaps = [];
            newAnonymousType.GetMembers()
                .OfType<PropertyInfo>()
                .ToList()
                .ForEach(member =>
                {
                    Type sourceType = member.PropertyType;
                    Type destMember = oldType.GetProperty(member.Name).PropertyType;
                    if (sourceType == destMember)
                        return;

                    if (!memberTypeMaps.ContainsKey(sourceType))
                    {
                        memberTypeMaps.Add(sourceType, destMember);
                        anonymousTypesBaseMappings.CreateMap(sourceType, destMember);
                    }
                });

            anonymousTypesConfigurationProvider = ConfigurationHelper.GetMapperConfiguration(anonymousTypesBaseMappings);
        }

        private MemberInitExpression GetAnonymousTypeMemberInitExpression(Dictionary<string, Expression> bindingExpressions, Type oldType)
        {
            Type newAnonymousType = AnonymousTypeFactory.CreateAnonymousType(bindingExpressions.ToDictionary(a => a.Key, a => a.Value.Type));
            this.TypeMappingsManager.AddTypeMapping(oldType, newAnonymousType);

            ConfigureAnonymousTypeMaps(oldType, newAnonymousType);
            return Expression.MemberInit
            (
                Expression.New(newAnonymousType),
                bindingExpressions
                    .ToDictionary(be => be.Key, be => newAnonymousType.GetProperty(be.Key))
                    .Select(member => Expression.Bind(member.Value, bindingExpressions[member.Key]))
            );
        }

        private MemberInitExpression GetMemberInit(MemberBindingGroup memberBindingGroup)
        {
            Dictionary<DeclaringMemberKey, List<MemberAssignmentInfo>> includedMembers = [];

            List<MemberBinding> bindings = memberBindingGroup.MemberAssignmentInfos.Aggregate(new List<MemberBinding>(), (list, next) =>
            {
                var propertyMap = next.PropertyMap;
                var binding = next.MemberAssignment;

                var sourceMember = GetSourceMember(propertyMap);//does the corresponding member mapping exist
                if (sourceMember == null)
                    return list;

                DeclaringMemberKey declaringMemberKey = new                (
                    GetParentMember(propertyMap),
                    BuildParentFullName(propertyMap)
                );

                if (ShouldBindPropertyMap(next))
                {
                    list.Add//adding bindings for property maps
                    (
                        DoBind
                        (
                            sourceMember,
                            binding.Expression,
                            this.Visit(binding.Expression)
                        )
                    );
                }
                else
                {
                    if (declaringMemberKey.DeclaringMemberInfo == null)
                        throw new InvalidOperationException("DeclaringMemberInfo is null.");

                    if (!includedMembers.TryGetValue(declaringMemberKey, out List<MemberAssignmentInfo> assignments))
                    {
                        includedMembers.Add
                        (
                            declaringMemberKey,
                            [
                                new MemberAssignmentInfo
                                (
                                    propertyMap,
                                    binding
                                )
                            ]
                        );
                    }
                    else
                    {
                        assignments.Add(new MemberAssignmentInfo(propertyMap, binding));
                    }
                }

                return list;

                bool ShouldBindPropertyMap(MemberAssignmentInfo memberAssignmentInfo)
                    => (memberBindingGroup.IsRootMemberAssignment && sourceMember.ReflectedType.IsAssignableFrom(memberBindingGroup.NewType))
                    || (!memberBindingGroup.IsRootMemberAssignment && declaringMemberKey.Equals(memberBindingGroup.DeclaringMemberKey));
            });

            includedMembers.Select
            (
                kvp => new MemberBindingGroup
                (
                    declaringMemberKey: kvp.Key,
                    isRootMemberAssignment: false,
                    newType: kvp.Key.DeclaringMemberInfo.GetMemberType(),
                    memberAssignmentInfos: [.. includedMembers.Values.SelectMany(m => m)]
                )
            )
            .ToList()
            .ForEach(group =>
            {
                if (ShouldBindChildReference(group))
                    bindings.Add(Expression.Bind(group.DeclaringMemberKey.DeclaringMemberInfo, GetMemberInit(group)));
            });

            bool ShouldBindChildReference(MemberBindingGroup group)
                => (memberBindingGroup.IsRootMemberAssignment
                    && group.DeclaringMemberKey.DeclaringMemberInfo.ReflectedType.IsAssignableFrom(memberBindingGroup.NewType))
                || (!memberBindingGroup.IsRootMemberAssignment
                    && group.DeclaringMemberKey.DeclaringMemberInfo.ReflectedType.IsAssignableFrom(memberBindingGroup.NewType)
                    && group.DeclaringMemberKey.DeclaringMemberFullName.StartsWith(memberBindingGroup.DeclaringMemberKey.DeclaringMemberFullName));

            return Expression.MemberInit(Expression.New(memberBindingGroup.NewType), bindings);
        }

        private MemberAssignment DoBind(MemberInfo sourceMember, Expression initial, Expression mapped)
        {
            mapped = mapped.ConvertTypeIfNecessary(sourceMember.GetMemberType());
            this.TypeMappingsManager.AddTypeMapping(initial.Type, mapped.Type);
            return Expression.Bind(sourceMember, mapped);
        }

        private static MemberInfo GetSourceMember(PropertyMap propertyMap)
            => propertyMap.CustomMapExpression != null
                ? propertyMap.CustomMapExpression.GetMemberExpression()?.Member
                : propertyMap.SourceMembers.Last();

        private static MemberInfo GetParentMember(PropertyMap propertyMap)
            => propertyMap.IncludedMember?.ProjectToCustomSource != null
                            ? propertyMap.IncludedMember.ProjectToCustomSource.GetMemberExpression().Member
                            : GetSourceParentMember(propertyMap);

        private static MemberInfo GetSourceParentMember(PropertyMap propertyMap)
        {
            if (propertyMap.CustomMapExpression != null)
                return propertyMap.CustomMapExpression.GetMemberExpression()?.Expression.GetMemberExpression()?.Member;

            if (propertyMap.SourceMembers.Length > 1)
                return new List<MemberInfo>(propertyMap.SourceMembers)[propertyMap.SourceMembers.Length - 2];

            return null;
        }

        private static string BuildParentFullName(PropertyMap propertyMap)
        {
            List<PropertyMapInfo> propertyMapInfos = [];
            if (propertyMap.IncludedMember?.ProjectToCustomSource != null)
                propertyMapInfos.Add(new PropertyMapInfo(propertyMap.IncludedMember.ProjectToCustomSource, []));

            propertyMapInfos.Add(new PropertyMapInfo(propertyMap.CustomMapExpression, [.. propertyMap.SourceMembers]));

            List<string> fullNameArray = [.. BuildFullName(propertyMapInfos).Split(['.'])];

            fullNameArray.Remove(fullNameArray.Last());

            return string.Join(".", fullNameArray);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return DoVisitBinary(this.Visit(node.Left), this.Visit(node.Right), this.Visit(node.Conversion));

            Expression DoVisitBinary(Expression newLeft, Expression newRight, Expression conversion)
            {
                if (newLeft != node.Left || newRight != node.Right || conversion != node.Conversion)
                {
                    return node.NodeType == ExpressionType.Coalesce && node.Conversion != null
                        ? Expression.Coalesce(newLeft, newRight, conversion as LambdaExpression)
                        : Expression.MakeBinary
                        (
                            node.NodeType,
                            newLeft,
                            newRight,
                            node.IsLiftedToNull,
                            TypesChanged()
                                ? Expression.MakeBinary(node.NodeType, newLeft, newRight).Method
                                : node.Method
                        );
                }

                return node;

                bool TypesChanged() => newLeft.Type != node.Left.Type || newRight.Type != node.Right.Type;
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

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Type mappedType = this.TypeMappingsManager.ReplaceType(node.TypeOperand);
            if (mappedType != node.TypeOperand)
                return MapTypeBinary(this.Visit(node.Expression));

            return base.VisitTypeBinary(node);

            Expression MapTypeBinary(Expression mapped)
            {
                if (mapped == node.Expression)
                    return base.VisitTypeBinary(node);

                return node.NodeType switch
                {
                    ExpressionType.TypeIs => Expression.TypeIs(mapped, mappedType),
                    _ => base.VisitTypeBinary(node),
                };
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return DoVisitUnary(Visit(node.Operand));

            Expression DoVisitUnary(Expression updated)
            {
                Type mappedType = this.TypeMappingsManager.ReplaceType(node.Type);
                if (mappedType != node.Type)
                    return Expression.MakeUnary
                    (
                        node.NodeType,
                        updated != node.Operand
                            ? updated
                            : node.Operand,
                        mappedType
                    );

                return updated != node.Operand
                        ? node.Update(updated)
                        : base.VisitUnary(node);
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Type newType = this.TypeMappingsManager.ReplaceType(node.Type);
            if (newType != node.Type)
            {
                if (node.Value == null)
                    return base.VisitConstant(Expression.Constant(null, newType));

                if (ConfigurationProvider.CanMapConstant(node.Type, newType))
                    return base.VisitConstant(Expression.Constant(Mapper.Map(node.Value, node.Type, newType), newType));
                //Issue 3455 (Non-Generic Mapper.Map failing for structs in v10)
                //return base.VisitConstant(Expression.Constant(Mapper.Map(node.Value, node.Type, newType), newType));

                if (typeof(Expression).IsAssignableFrom(node.Type))
                    return Expression.Constant(this.Visit((Expression)node.Value), newType);
            }
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
                this.TypeMappingsManager.AddTypeMapping(next.Type, mappedNext.Type);

                lst.Add(mappedNext);
                return lst;
            });//Arguments could be expressions or other objects. e.g. s => s.UserId  or a string "ZZZ".  For extention methods node.Arguments[0] is usually the helper object itself

            //type args are the generic type args e.g. T1 and T2 MethodName<T1, T2>(method arguments);
            var typeArgsForNewMethod = node.Method.IsGenericMethod
                ? node.Method.GetGenericArguments().Select(type => this.TypeMappingsManager.ReplaceType(type)).ToList()//not converting the type it is not in the typeMappings dictionary
                : null;

            return node.Method.IsStatic
                    ? GetStaticExpression()
                    : GetInstanceExpression(this.Visit(node.Object));

            MethodCallExpression GetInstanceExpression(Expression instance)
            {
                return node.Method.IsGenericMethod
                            ? Expression.Call(instance, node.Method.Name, [.. typeArgsForNewMethod], [.. listOfArgumentsForNewMethod])
                            : Expression.Call(instance, GetMethodInfoForNonGeneric(), [.. listOfArgumentsForNewMethod]);

                MethodInfo GetMethodInfoForNonGeneric()
                {
                    MethodInfo methodInfo = instance.Type.GetMethod(node.Method.Name, [.. listOfArgumentsForNewMethod.Select(a => a.Type)]);
                    if (methodInfo.DeclaringType != instance.Type)
                        methodInfo = methodInfo.DeclaringType.GetMethod(node.Method.Name, [.. listOfArgumentsForNewMethod.Select(a => a.Type)]);
                    return methodInfo;
                }
            }

            MethodCallExpression GetStaticExpression()
                => node.Method.IsGenericMethod
                    ? Expression.Call(node.Method.DeclaringType, node.Method.Name, [.. typeArgsForNewMethod], [.. listOfArgumentsForNewMethod])
                    : Expression.Call(node.Method, [.. listOfArgumentsForNewMethod]);
        }

        protected static string BuildFullName(List<PropertyMapInfo> propertyMapInfoList)
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
                            sb.Append('.');
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
                    propertyMapInfoList.Add(new PropertyMapInfo(null, [propertyInfo]));
                    break;
                case FieldInfo fieldInfo:
                    propertyMapInfoList.Add(new PropertyMapInfo(null, [fieldInfo]));
                    break;
            }
        }

        private static bool GenericTypeDefinitionsAreEquivalent(Type typeSource, Type typeDestination) 
            => typeSource.IsGenericType() && typeDestination.IsGenericType() && typeSource.GetGenericTypeDefinition() == typeDestination.GetGenericTypeDefinition();

        protected void FindDestinationFullName(Type typeSource, Type typeDestination, string sourceFullName, List<PropertyMapInfo> propertyMapInfoList)
        {
            if (typeSource.IsLiteralType()
                && typeDestination.IsLiteralType()
                && typeSource != typeDestination)
            {
                throw new InvalidOperationException
                (
                    string.Format
                    (
                        CultureInfo.CurrentCulture,
                        Properties.Resources.makeParentTypesMatchForMembersOfLiteralsFormat,
                        typeSource,
                        typeDestination,
                        sourceFullName
                    )
                );
            }

            const string period = ".";
            bool BothTypesAreAnonymous()
                => IsAnonymousType(typeSource) && IsAnonymousType(typeDestination);

            TypeMap GetTypeMap() => BothTypesAreAnonymous() 
                ? anonymousTypesConfigurationProvider.CheckIfTypeMapExists(sourceType: typeDestination, destinationType: typeSource)
                : ConfigurationProvider.CheckIfTypeMapExists(sourceType: typeDestination, destinationType: typeSource);
            //The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination 

            if (typeSource == typeDestination)
            {
                var sourceFullNameArray = sourceFullName.Split([period[0]], StringSplitOptions.RemoveEmptyEntries);

                foreach (string next in sourceFullNameArray)
                {
                    if (propertyMapInfoList.Count == 0)
                    {
                        AddPropertyMapInfo(typeSource, next, propertyMapInfoList);
                    }
                    else
                    {
                        var last = propertyMapInfoList[propertyMapInfoList.Count - 1];
                        AddPropertyMapInfo(last.CustomExpression == null
                            ? last.DestinationPropertyInfos[last.DestinationPropertyInfos.Count - 1].GetMemberType()
                            : last.CustomExpression.ReturnType, next, propertyMapInfoList);
                    }
                }

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

                    this.TypeMappingsManager.AddTypeMapping(sourceType, destType);

                    return;
                }
                else
                {
                    //propertyName is a member of the generic type definition so just add the members PropertyMapInfo
                    var propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase));
                    AddPropertyMapInfo(typeDestination, propertyName, propertyMapInfoList);

                    var sourceType = typeSource.GetFieldOrProperty(propertyName).GetMemberType();
                    var destType = typeDestination.GetFieldOrProperty(propertyName).GetMemberType();

                    this.TypeMappingsManager.AddTypeMapping(sourceType, destType);

                    var childFullName = sourceFullName.Substring(sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) + 1);
                    FindDestinationFullName(sourceType, destType, childFullName, propertyMapInfoList);
                    return;
                }
            }

            var typeMap = GetTypeMap();

            PathMap pathMap = typeMap.FindPathMapByDestinationFullPath(destinationFullPath: sourceFullName);
            if (pathMap != null)
            {
                propertyMapInfoList.Add(new PropertyMapInfo(pathMap.CustomMapExpression, []));
                return;
            }


            if (sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) < 0)
            {
                var propertyMap = typeMap.GetMemberMapByDestinationProperty(sourceFullName);

                if (propertyMap.CustomMapExpression == null && propertyMap.SourceMembers.Length == 0)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, sourceFullName));

                if (propertyMap.IncludedMember?.ProjectToCustomSource != null)
                    propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.IncludedMember.ProjectToCustomSource, []));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomMapExpression, [.. propertyMap.SourceMembers]));
            }
            else
            {
                var propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase));
                var propertyMap = typeMap.GetMemberMapByDestinationProperty(propertyName);

                var sourceMemberInfo = typeSource.GetFieldOrProperty(propertyMap.GetDestinationName());
                if (propertyMap.CustomMapExpression == null && propertyMap.SourceMembers.Length == 0)//If sourceFullName has a period then the SourceMember cannot be null.  The SourceMember is required to find the ProertyMap of its child object.
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, propertyName));

                if (propertyMap.IncludedMember?.ProjectToCustomSource != null)
                    propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.IncludedMember.ProjectToCustomSource, []));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomMapExpression, [.. propertyMap.SourceMembers]));
                var childFullName = sourceFullName.Substring(sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) + 1);

                FindDestinationFullName(sourceMemberInfo.GetMemberType(), propertyMap.CustomMapExpression == null
                    ? propertyMap.SourceMembers.Last().GetMemberType()
                    : propertyMap.CustomMapExpression.ReturnType, childFullName, propertyMapInfoList);
            }
        }
    }
}