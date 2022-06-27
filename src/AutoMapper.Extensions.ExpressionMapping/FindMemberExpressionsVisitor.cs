using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;
using AutoMapper.Extensions.ExpressionMapping.Extensions;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal class FindMemberExpressionsVisitor : ExpressionVisitor
    {
        internal FindMemberExpressionsVisitor(Expression newParentExpression) => _newParentExpression = newParentExpression;

        private readonly Expression _newParentExpression;
        private readonly List<MemberExpression> _memberExpressions = new List<MemberExpression>();

        public MemberExpression Result
        {
            get
            {
                const string period = ".";
                var fullNamesGrouped = _memberExpressions.Select(m => m.GetPropertyFullName())
                    .GroupBy(n => n)
                    .Select(grp => grp.Key)
                    .OrderBy(a => a.Length).ToList();

                var member = fullNamesGrouped.Aggregate(string.Empty, (result, next) =>
                {
                    if (string.IsNullOrEmpty(result) || next.Contains(result))
                        result = next;
                    else throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.includeExpressionTooComplex,
                        string.Concat(_newParentExpression.Type.Name, period, result),
                        string.Concat(_newParentExpression.Type.Name, period, next)));

                    return result;
                });

                return ExpressionHelpers.MemberAccesses(member, _newParentExpression);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var parameterExpression = node.GetParameterExpression();
            var sType = parameterExpression?.Type;
            if (sType != null && _newParentExpression.Type == sType && node.IsMemberExpression())
            {
                if (node.Expression.NodeType == ExpressionType.MemberAccess && node.Type.IsLiteralType())
                    _memberExpressions.Add((MemberExpression)node.Expression);
                else if (node.Expression.NodeType == ExpressionType.Parameter && node.Type.IsLiteralType())
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.mappedMemberIsChildOfTheParameterFormat, node.GetPropertyFullName(), node.Type.FullName, sType.FullName));
                else
                    _memberExpressions.Add(node);
            }

            return base.VisitMember(node);
        }
    }
}
