using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerSideDatatables.Datatables.Repository
{
    /// <summary>
    /// Generic repository for handling data requested needed for server-side processing of JQuery Datatables. 
    /// Works best with an Entity made from a Database View that can return the data as it will be displayed, 
    /// which makes sorting and filtering much easier.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DatatablesRepository<TEntity> : IDatatablesRepository<TEntity> where TEntity : class
    {
        private SkippyEntities _context;
        private DbSet<TEntity> _dbSet;

        public DatatablesRepository(SkippyEntities context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetPagedSortedFilteredListAsync(int start, int length, string orderColumnName, ListSortDirection order, string searchValue)
        {
            return await CreateQueryWithWhereAndOrderBy(searchValue, orderColumnName, order)
                .Skip(start)
                .Take(length)
                .ToListAsync();
        }

        public async Task<int> GetRecordsFilteredAsync(string searchValue)
        {
            IQueryable<TEntity> query = _dbSet;

            return await GetWhereQueryForSearchValue(query, searchValue).CountAsync();
        }

        public async Task<int> GetRecordsTotalAsync()
        {
            IQueryable<TEntity> query = _dbSet;

            return await query.CountAsync();
        }

        public virtual string GetSearchPropertyName()
        {
            return null;
        }

        protected virtual IQueryable<TEntity> CreateQueryWithWhereAndOrderBy(string searchValue, string orderColumnName, ListSortDirection order)
        {
            IQueryable<TEntity> query = _dbSet;

            query = GetWhereQueryForSearchValue(query, searchValue);
            
            query = AddOrderByToQuery(query, orderColumnName, order);

            return query;
        }
        
        /// <summary>
        /// This generic implementation relies on there being a SearchProperty on the Entity which 
        /// contains a concatenation of the data being displayed, e.g. "Adam Ant 21/02/1990 12345".
        /// Override for specific Datatables to handle logic such as searching on formated dates
        /// and concatenations.
        /// </summary>
        /// <param name="queryable">Entity framework query object</param>
        /// <param name="searchValue">text string to search on all displayed columns</param>
        /// <returns></returns>
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

        /// <summary>
        /// This generic implementation of adding the OrderBy clauses to the query will not
        /// handle any properties which do not exist on the DB table, such as concatenations
        /// or formatted dates.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderColumnName"></param>
        /// <param name="order"></param>
        /// <returns></returns>
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
        
        protected Expression<Func<TEntity, bool>> GetExpressionForPropertyContains(string propertyName, string value)
        {
            var parent = Expression.Parameter(typeof(TEntity));
            MethodInfo method = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            var expressionBody = Expression.Call(Expression.Property(parent, propertyName), method, Expression.Constant(value));
            return Expression.Lambda<Func<TEntity, bool>>(expressionBody, parent);
        }
    }
}