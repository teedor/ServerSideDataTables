
namespace ServerSideDatatables.Datatables.Repository
{
    public class PeopleDatatablesRepository : DatatablesRepository<Person>
    {
        public PeopleDatatablesRepository(SkippyEntities context)
            : base(context)
        {
        }
    }
}
