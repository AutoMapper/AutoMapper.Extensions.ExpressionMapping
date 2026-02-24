## OData
AutoMapper extentions for mapping expressions (OData)

[![CI](https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping/actions/workflows/ci.yml/badge.svg)](https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping/actions/workflows/ci.yml)
[![CodeQL](https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping/actions/workflows/github-code-scanning/codeql)
[![NuGet](http://img.shields.io/nuget/v/AutoMapper.Extensions.ExpressionMapping.svg)](https://www.nuget.org/packages/AutoMapper.Extensions.ExpressionMapping/)

To use, configure using the configuration helper method:

```c#
var mapper = new Mapper(new MapperConfiguration(cfg => {
    cfg.AddExpressionMapping();
	// Rest of your configuration
}, loggerFactory));

// or if using the MS Ext DI:

services.AddAutoMapper(cfg => {
    cfg.AddExpressionMapping();
}, /* assemblies with profiles */);
```

## DTO Queries
Expression Mapping also supports writing queries against the mapped objects. Take the following source and destination types:
```csharp
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Request
    {
        public int Id { get; set; }
        public int AssigneeId { get; set; }
        public User Assignee { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RequestDTO
    {
        public int Id { get; set; }
        public UserDTO Assignee { get; set; }
    }
```

We can write LINQ expressions against the DTO collections.
```csharp
ICollection<RequestDTO> requests = [.. context.Request.GetQuery1<RequestDTO, Request>(mapper, r => r.Id > 0 && r.Id < 3, null, [r => r.Assignee])];
ICollection <UserDTO> users = [.. context.User.GetQuery1<UserDTO, User>(mapper, u => u.Id > 0 && u.Id < 4, q => q.OrderBy(u => u.Name))];
int count = await context.Request.Query<RequestDTO, Request, int, int>(mapper, q => q.Count(r => r.Id > 1));
```
The methods below map the DTO query expresions to the equivalent data query expressions. The call to IMapper.Map converts the data query results back to the DTO (or model) object types. The call to IMapper.ProjectTo converts the data query to a DTO (or model) query.
```csharp
    static class Extensions
    {
        internal static async Task<TModelResult> Query<TModel, TData, TModelResult, TDataResult>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelResult>> queryFunc) where TData : class
        {
            //Map the expressions
            Func<IQueryable<TData>, TDataResult> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataResult>>>(queryFunc).Compile();

            //execute the query
            return mapper.Map<TDataResult, TModelResult>(mappedQueryFunc(query));
        }

        //This example compiles the queryable expression.
        internal static IQueryable<TModel> GetQuery1<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = null,
            IEnumerable<Expression<Func<TModel, object>>> expansions = null)
        {
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryDelegate = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryableExpression)?.Compile();
            if (filter != null)
                query = query.Where(mapper.MapExpression<Expression<Func<TData, bool>>>(filter));

            return mappedQueryDelegate != null
                    ? mapper.ProjectTo(mappedQueryDelegate(query), null, GetExpansions())
                    : mapper.ProjectTo(query, null, GetExpansions());

            Expression<Func<TModel, object>>[] GetExpansions() => expansions?.ToArray() ?? [];
        }

        //This example updates IQueryable<TData>.Expression with the mapped queryable expression argument.
        internal static IQueryable<TModel> GetQuery2<TModel, TData>(this IQueryable<TData> query,
            IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression = null,
            IEnumerable<Expression<Func<TModel, object>>> expansions = null)
        {
            Expression<Func<IQueryable<TData>, IQueryable<TData>>> mappedQueryExpression = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryableExpression);
            if (filter != null)
                query = query.Where(mapper.MapExpression<Expression<Func<TData, bool>>>(filter));

            if (mappedQueryExpression != null)
            {
                var queryableExpressionBody = GetUnconvertedExpression(mappedQueryExpression.Body);
                queryableExpressionBody = ReplaceParameter(queryableExpressionBody, mappedQueryExpression.Parameters[0], query.Expression);
                query = query.Provider.CreateQuery<TData>(queryableExpressionBody);
            }

            return mapper.ProjectTo(query, null, GetExpansions());

            Expression<Func<TModel, object>>[] GetExpansions() => expansions?.ToArray() ?? [];
            static Expression GetUnconvertedExpression(Expression expression) => expression.NodeType switch
            {
                ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs => GetUnconvertedExpression(((UnaryExpression)expression).Operand),
                _ => expression,
            };
            Expression ReplaceParameter(Expression expression, ParameterExpression source, Expression target) => new ParameterReplacer(source, target).Visit(expression);
        }

        class ParameterReplacer(ParameterExpression source, Expression target) : ExpressionVisitor
        {
            private readonly ParameterExpression _source = source;
            private readonly Expression _target = target;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
    }
```

## Known Issues
Mapping a single type in the source expression to multiple types in the destination expression is not supported e.g.
```c#
        [Fact]
        public void Can_map_if_source_type_targets_multiple_destination_types_in_the_same_expression()
        {
            var mapper = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceType, TargetType>().ReverseMap();
                cfg.CreateMap<SourceChildType, TargetChildType>().ReverseMap();

                // Same source type can map to different target types. This seems unsupported currently.
                cfg.CreateMap<SourceListItemType, TargetListItemType>().ReverseMap();
                cfg.CreateMap<SourceListItemType, TargetChildListItemType>().ReverseMap();

            }).CreateMapper();

            Expression<Func<SourceType, bool>> sourcesWithListItemsExpr = src => src.Id != 0 && src.ItemList.Any() && src.Child.ItemList.Any(); // Sources with non-empty ItemList
            Expression<Func<TargetType, bool>> target1sWithListItemsExpr = mapper.MapExpression<Expression<Func<TargetType, bool>>>(sourcesWithListItemsExpr);
        }

        private class SourceChildType
        {
            public int Id { get; set; }
            public IEnumerable<SourceListItemType> ItemList { get; set; } // Uses same type (SourceListItemType) for its itemlist as SourceType
        }

        private class SourceType
        {
            public int Id { get; set; }
            public SourceChildType Child { set; get; }
            public IEnumerable<SourceListItemType> ItemList { get; set; }
        }

        private class SourceListItemType
        {
            public int Id { get; set; }
        }

        private class TargetChildType
        {
            public virtual int Id { get; set; }
            public virtual ICollection<TargetChildListItemType> ItemList { get; set; } = [];
        }

        private class TargetChildListItemType
        {
            public virtual int Id { get; set; }
        }

        private class TargetType
        {
            public virtual int Id { get; set; }

            public virtual TargetChildType Child { get; set; }

            public virtual ICollection<TargetListItemType> ItemList { get; set; } = [];
        }

        private class TargetListItemType
        {
            public virtual int Id { get; set; }
        }

```