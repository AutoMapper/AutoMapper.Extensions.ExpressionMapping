using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    public class PropertyMapInfo(LambdaExpression customExpression, List<MemberInfo> destinationPropertyInfos)
    {
        public LambdaExpression CustomExpression { get; set; } = customExpression;
        public List<MemberInfo> DestinationPropertyInfos { get; set; } = destinationPropertyInfos;
    }
}
