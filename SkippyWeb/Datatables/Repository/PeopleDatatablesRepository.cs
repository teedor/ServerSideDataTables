
using System.Data.Entity.SqlServer;
using System.Linq;

namespace ServerSideDatatables.Datatables.Repository
{
    public class PeopleDatatablesRepository : DatatablesRepository<Person>
    {
        public PeopleDatatablesRepository(SkippyEntities context)
            : base(context)
        {
        }

        protected override IQueryable<Person> GetWhereQueryForSearchValue(IQueryable<Person> queryable, string searchValue)
        {
            return queryable.Where(x =>
                    // id column (int)
                    SqlFunctions.StringConvert((double)x.Id).Contains(searchValue)
                    // name column (string)
                    || x.Name.Contains(searchValue)
                    // date of birth column (datetime, formatted as d/M/yyyy) - limitation of sql prevented us from getting leading zeros in day or month
                    || (SqlFunctions.StringConvert((double)SqlFunctions.DatePart("dd", x.DateOfBirth)) + "/" + SqlFunctions.DatePart("mm", x.DateOfBirth) + "/" + SqlFunctions.DatePart("yyyy", x.DateOfBirth)).Contains(searchValue));
        }
    }
}
