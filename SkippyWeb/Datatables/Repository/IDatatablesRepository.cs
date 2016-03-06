using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ServerSideDatatables.Datatables.Repository
{
    public interface IDatatablesRepository<TEntity>
    {
        Task<IEnumerable<TEntity>> GetPagedSortedFilteredListAsync(int start, int length, string orderColumnName, ListSortDirection order, string searchValue);

        Task<int> GetRecordsTotalAsync();

        Task<int> GetRecordsFilteredAsync(string searchValue);

        string GetSearchPropertyName();
    }
}
