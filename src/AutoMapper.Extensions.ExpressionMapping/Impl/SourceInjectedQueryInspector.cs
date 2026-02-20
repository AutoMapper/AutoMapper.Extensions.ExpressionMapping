using System;
using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping.Impl
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SourceInjectedQueryInspector
    {
        public SourceInjectedQueryInspector()
        {
            SourceResult = (e,o) => { };
            DestResult = o => { };
            StartQueryExecuteInterceptor = (t, e) => { };
        }
        public Action<Expression, object> SourceResult { get; set; }
        public Action<object> DestResult { get; set; }
        public Action<Type, Expression> StartQueryExecuteInterceptor { get; set; }

    }
}