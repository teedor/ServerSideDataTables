#JQuery datatables with server-side processing

![datatables](https://raw.githubusercontent.com/stevenalexander/ServerSideDataTables/master/datatables-server-side.png)

## Introduction

JQuery [Datatables](https://datatables.net) is a great tool, attach it to a table of results and it gives you quick and 
easy sorting/searching. Against a small dataset this works fine, but once you start to have >1000 records your page load 
is going to take a long time. To solve this Datatables recommend [server-side processing](https://datatables.net/manual/server-side).

This code is an example of implementing server-side processing for an ASP.MVC web appliction, using a generic approach with
Linq so that you can re-use it for different entities easily with little code repetition. It also shows an implementation of 
full word search across all columns, which is something that the Javascript processing version offers but is very tricky to 
implement on the database side with decent performance. It's a C# .NET implementation but you can take the interfaces and 
calls from the controllers and convert the approach for Java or Ruby (missing the nice Linq stuff tho).

## Details

I'll skip the basic view/js details as that is easily available on the [datatables documentation](https://datatables.net/examples/basic_init/zero_configuration.html).

The request comes into the controller as a GET with all the sort/search details as query parameters (see
[here](https://datatables.net/manual/server-side)), it expects a result matching [this interface](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Datatables/Response/IDatatablesResponse.cs):

    public interface IDatatablesResponse<T>
    {
        int draw { get; set; }
        int recordsTotal { get; set; }
        int recordsFiltered { get; set; }
        IEnumerable<T> data { get; set; }
        string error { get; set; }
    }

The [controller](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Controllers/HomeController.cs#L41) extracts the parameters, creates the DB context and repository and makes three calls asynchronously:
- get the total records
- get the total filtered records
- get the searched/sorted/paged data

The data is returned and Datatables Javascript uses it to render the table and controls for the correct searched/sorted/paged results.

The magic happens in the DatatablesRepository objects which handle those calls.

### DatatablesRepository classes

[Interface](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Datatables/Repository/IDatatablesRepository.cs):

    public interface IDatatablesRepository<TEntity>
    {
        Task<IEnumerable<TEntity>> GetPagedSortedFilteredListAsync(int start, int length, string orderColumnName, ListSortDirection order, string searchValue);
        Task<int> GetRecordsTotalAsync();
        Task<int> GetRecordsFilteredAsync(string searchValue);
        string GetSearchPropertyName();
    }

The base class DatatablesRepository has a [default implementation](https://github.com/stevenalexander/ServerSideDataTables/blob/master/SkippyWeb/Datatables/Repository/DatatablesRepository.cs#L57) which provides generic logic for paging, searching and ordering an entity:

    protected virtual IQueryable<TEntity> CreateQueryWithWhereAndOrderBy(string searchValue, string orderColumnName, ListSortDirection order)
    {
        ...
        query = GetWhereQueryForSearchValue(query, searchValue);
        query = AddOrderByToQuery(query, orderColumnName, order);
        ...
    }

    protected virtual IQueryable<TEntity> GetWhereQueryForSearchValue(IQueryable<TEntity> queryable, string searchValue)
    {
        string searchPropertyName = GetSearchPropertyName();
        if (!string.IsNullOrWhiteSpace(searchValue) && !string.IsNullOrWhiteSpace(searchPropertyName))
        {
            var searchValues = Regex.Split(searchValue, "\\s+");
            foreach (string value in searchValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    queryable = queryable.Where(GetExpressionForPropertyContains(searchPropertyName, value));
                }
            }
            return queryable;
        }
        return queryable;
    }

    protected virtual IQueryable<TEntity> AddOrderByToQuery(IQueryable<TEntity> query, string orderColumnName, ListSortDirection order)
    {
        var orderDirectionMethod = order == ListSortDirection.Ascending
                ? "OrderBy"
                : "OrderByDescending";

        var type = typeof(TEntity);
        var property = type.GetProperty(orderColumnName);
        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExp = Expression.Lambda(propertyAccess, parameter);
        var filteredAndOrderedQuery = Expression.Call(typeof(Queryable), orderDirectionMethod, new Type[] { type, property.PropertyType }, query.Expression, Expression.Quote(orderByExp));

        return query.Provider.CreateQuery<TEntity>(filteredAndOrderedQuery);
    }

The default implementation for creating the Where query (for searching) will only work if you provide a SearchPropertyName for a property that exists in the database that is a concatenation of all the values you want to search in the format displayed.

You can implement and override to use a custom method if your Entity does not support this, here is an example from the Person Entity:




for these, but unless you provide details on how to search in an explicit Entity repository class it will be unable to search. You will also get errors if you attempt to sort on a column which does not exist in the database (i.e. a formated date or ). To 

## Notes

- If you are displaying date values be aware that you will need to format the date for display before returning in JSON, and the date format will affect how you sort the column on the backend as you will need to identify the actual date column property rather than the formated string.
- 
- I created the initial example with the help of [Stephen Anderson](https://github.com/teedor/ServerSideDataTables)

