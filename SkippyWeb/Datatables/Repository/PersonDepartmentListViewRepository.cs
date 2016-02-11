using System.ComponentModel;
using System.Linq;

namespace ServerSideDatatables.Datatables.Repository
{
    public class PersonDepartmentListViewRepository : DatatablesRepository<PersonDepartmentListView>
    {
        public PersonDepartmentListViewRepository(SkippyEntities context)
            : base(context)
        {
        }

        public override string GetSearchPropertyName()
        {
            return "SearchString";
        }

        protected override IQueryable<PersonDepartmentListView> AddOrderByToQuery(IQueryable<PersonDepartmentListView> query, string orderColumnName, ListSortDirection order)
        {
            if (orderColumnName == "DateOfBirthFormatted")
            {
                orderColumnName = "DateOfBirth";
            }

            return base.AddOrderByToQuery(query, orderColumnName, order);
        }
    }
}
