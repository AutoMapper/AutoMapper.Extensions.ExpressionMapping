## OData
AutoMapper extentions for mapping expressions (OData)

[![NuGet](http://img.shields.io/nuget/v/AutoMapper.Extensions.ExpressionMapping.svg)](https://www.nuget.org/packages/AutoMapper.Extensions.ExpressionMapping/)

To use, configure using the configuration helper method:

```c#
var mapper = new Mapper(new MapperConfiguration(cfg => {
    cfg.AddExpressionMapping();
	// Rest of your configuration
}));

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
ICollection<RequestDTO> requests = await context.Request.GetItemsAsync(mapper, r => r.Id > 0 && r.Id < 3, null, new List<Expression<Func<IQueryable<RequestDTO>, IIncludableQueryable<RequestDTO, object>>>>() { item => item.Include(s => s.Assignee) });
ICollection<UserDTO> users = await context.User.GetItemsAsync<UserDTO, User>(mapper, u => u.Id > 0 && u.Id < 4, q => q.OrderBy(u => u.Name));
int count = await context.Request.Query<RequestDTO, Request, int, int>(mapper, q => q.Count(r => r.Id > 1));
```
The methods below map the DTO query expresions to the equivalent data query expressions. The call to IMapper.Map converts the data query results back to the DTO (or model) object types.
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

        internal static async Task<ICollection<TModel>> GetItemsAsync<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<TModel, bool>> filter = null,
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null,
            ICollection<Expression<Func<IQueryable<TModel>, IIncludableQueryable<TModel, object>>>> includeProperties = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();
            ICollection<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>> includes = mapper.MapIncludesList<Expression<Func<IQueryable<TData>, IIncludableQueryable<TData, object>>>>(includeProperties);

            if (f != null)
                query = query.Where(f);

            if (includes != null)
                query = includes.Select(i => i.Compile()).Aggregate(query, (list, next) => query = next(query));

            //Call the store
            ICollection<TData> result = mappedQueryFunc != null ? await mappedQueryFunc(query).ToListAsync() : await query.ToListAsync();

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(result).ToList();
        }
    }
```
